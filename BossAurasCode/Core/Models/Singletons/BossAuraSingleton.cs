using BossAuras.Core.Commands;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace BossAuras.Core.Models.Singletons;

// potential race condition with auras using these hooks!
// This is a quick and dirty solution. If we ever create Auras that need these hooks,
// it will be better to write some dedicated patches that handle removal (and adding) at appropriate times outside of the hook ecosystem.
// e.g. Adding could happen with a transpiler right before "RunState.EnterAct()" calls "await Hook.AfterActEntered(State);"
public class BossAuraSingleton : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => false;
    public BossAuraSingleton()
    {
        ModHelper.SubscribeForRunStateHooks($"{Entry.MOD_ID_PREFIX}-BOSS_AURAS_SINGLETON_HANDLER", IterateBossAurasForRunHooks);
    }
    private IEnumerable<AbstractModel> IterateBossAurasForRunHooks(RunState runState) => [this];

    /// All canonical AuraModels that implement IBossAura
    private List<IBossAura> AllBossAuras => field ??= GetAllBossAuras();
    private static List<IBossAura> GetAllBossAuras() => AuraDb.AllAuras.OfType<IBossAura>().ToList();
    
    
    // TODO:
    // This hook fires while the map is visible when entering Act 2 and 3, so you see the auras appear and disappear on the map.
    // This might be fine, Will need to see how it looks when auras from act 2 and 3 are added.
    
    // Why does this not get the RunState (or at least the ActModel) passed? :(
    public override async Task AfterActEntered()
    {
        var runState = GameHelper.GetRunState();
        // There is no "EndOfAct" hook, so we do it this way.
        await HandleRemoveAurasAfterAct(runState);
        await AddBossAurasAtActBegin(runState);
    }
    
   
    private async Task AddBossAurasAtActBegin(IRunState runState)
    {
        Entry.Logger.Info("Adding Boss Auras for the Act.");
        // Aggregate all eligible bosses as well as all boss-auras that can come from those.
        var relations = runState.Act.AllActiveBossEncounter
                    .ToDictionary(e => e
                                , e => AllBossAuras.Where(b => b.Bosses.Any(e2 => e2.Id == e.Id))
                                            .Cast<AuraModel>().ToList());

        var rng = runState.Rng.Niche;
        // For each eligible boss, get one random boss aura, and set their Boss Property for cleanup later.
        foreach (var relation in relations)
        {
            switch (relation.Value.Count)
            {
                case 0:
                    continue;
                case 1: // Dont call rng if its not necessary.
                    var mutable1 = relation.Value[0].ToMutable();
                    (mutable1 as IBossAura)!.Boss = relation.Key;
                    await AuraCmd.Add(mutable1);
                    continue;
                default:
                    var mutable2 = rng.NextItem(relation.Value)!.ToMutable();
                    (mutable2 as IBossAura)!.Boss = relation.Key;
                    await AuraCmd.Add(mutable2);
                    continue;
            }
        }
        // The game does not save the run after "AfterActEntered" is called.
        // However, if the player reloads the save at this point, the game does not call "AfterActEntered" again.
        // This leads to all Boss-Auras being permanently lost for the act.
        if(runState.CurrentActIndex is 0)
            await SaveManager.Instance.SaveRun(null, saveProgress: false);
    }
    
    // Handle BossAuraEnd.EndOfAct
    private async Task HandleRemoveAurasAfterAct(IRunState runState)
    {
        // We could in theory just clear all boss auras here, because a new act means out with the old and in with the new.
        // but that would include BossAuraEnd.None auras so I'm not doing it that way.
        foreach (var bossAura in runState.Auras.OfType<IBossAura>()
                             .Where(b => b.AuraEnd is BossAuraEnd.EndOfAct)
                             .Cast<AuraModel>().ToList())
        {
            await AuraCmd.Remove(bossAura);
        }
    }
    
    // Handle BossAuraEnd.BeforeBosses and BossAuraEnd.BeforeBoss
    public override async Task BeforeRoomEntered(AbstractRoom room)
    {
        if (room.RoomType is not RoomType.Boss || room is not CombatRoom combatRoom) return;
        
        var runState = GameHelper.GetRunState();
        var toRemove = runState.Auras.OfType<IBossAura>()
                    .Where(b => b.AuraEnd is BossAuraEnd.BeforeBosses ||
                                (b.AuraEnd is BossAuraEnd.BeforeBoss && b.Boss?.Id == combatRoom.Encounter.Id))
                    .Cast<AuraModel>()
                    .ToList();
        foreach (var bossAura in toRemove)
            await AuraCmd.Remove(bossAura);
        var a = runState.Auras;
        Entry.Logger.Info("gfsf");

    }

    // Handle BossAuraEnd.AfterBoss
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (room.RoomType is not RoomType.Boss) return;
        var runState = GameHelper.GetRunState();
        foreach (var bossAura in runState.Auras.OfType<IBossAura>()
                             .Where(b => b.AuraEnd is BossAuraEnd.AfterBoss && b.Boss.Id == room.Encounter.Id)
                             .Cast<AuraModel>())
        {
            await AuraCmd.Remove(bossAura);
        }
    }
    
}