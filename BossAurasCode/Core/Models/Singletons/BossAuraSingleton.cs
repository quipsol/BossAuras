using BossAuras.Core.Commands;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace BossAuras.Core.Models.Singletons;

// TODO:
// Reloading the run at neow does not call AfterActEntered again.
// However the run does not get saved yet, only after choosing a neow option.
// So by starting the run, and leaving-reentering, the auras are gone.
// Might also happen at the other Acts.
public class BossAuraSingleton : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => false;
    public BossAuraSingleton()
    {
        ModHelper.SubscribeForRunStateHooks($"{Entry.MOD_ID_PREFIX}-BOSS_AURAS_SINGLETON_HANDLER", IterateBossAurasForRunHooks);
    }
    private IEnumerable<AbstractModel> IterateBossAurasForRunHooks(RunState runState) => [this];

    private List<IBossAura> AllBossAuras => field ??= GetAllBossAuras();
    private static List<IBossAura> GetAllBossAuras() => AuraDb.AllAuras.OfType<IBossAura>().ToList();
    
    
    // Why does this not get the RunState (or at least the ActModel) passed? :(
    public override async Task AfterActEntered()
    {
        Entry.Logger.Info("Adding Boss Auras for the Act.");
        var runState = GameHelper.GetRunState();
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
                case 1:
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
    
    
    
    // TODO: Handle the end points for the auras
    
}