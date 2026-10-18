using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Utils.Patching;
using BossAuras.Core.Commands;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace BossAuras.Patches;


/// Patches that handle the add and remove logic for BossAuras. <br/>
/// Specifically extracted from SingletonModel to make sure it doesn't happen during hooks.
[HarmonyPatch]
public static class BossAuraHandler
{
    /// All canonical AuraModels that implement IBossAura
    private static List<IBossAura> AllBossAuras => field ??= AuraDb.AllAuras.OfType<IBossAura>().ToList();
    
    // RunManager.EnterAct right before await FadeIn -> Add Auras
    // RunManager.EnterAct right after await FadeOut -> Remove Auras at end of Act
    // After set Boss    AfterCombatEnd   -> CombatManager.EndCombatInternal(CombatTurnState) at the very end. Postfix is fine.
    
    // BeforeBosses | Before set Boss    BeforeRoomEntered

 
    // I will leave this here since if AsyncMethodCall gets fixed it's a better solution. AsyncMethodCall does not support awaits inside a using block.
    // [HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterAct), MethodType.Async)]
    // [HarmonyTranspiler]
    // private static List<CodeInstruction> InsertAsyncCallToAddBossAuras(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
    // {
    //     var setActInternal = AccessTools.Method(typeof(RunManager), nameof(RunManager.SetActInternal));
    //     var modifiedInstructions = AsyncMethodCall.Create(generator, instructions, original,
    //                 AccessTools.Method(typeof(BossAuraHandler), nameof(RemoveBossAurasDuringActBegin)),
    //                 beforeState: setActInternal);
    //     return AsyncMethodCall.Create(generator, modifiedInstructions, original,
    //                 AccessTools.Method(typeof(BossAuraHandler), nameof(AddBossAurasDuringActBegin)),
    //                 afterState: setActInternal);
    //
    // }
    

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetActInternal))] 
    [HarmonyPrefix]
    private static void RemoveBeforeActChange()
    {
        var task = RemoveBossAurasDuringActBegin();
        if (!task.IsCompleted)
            Entry.Logger.Warn("Boss aura removal did not finish synchronously; it will overlap SetActInternal.");
    }
    
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetActInternal), MethodType.Async)]
    [HarmonyTranspiler] 
    private static List<CodeInstruction> AddAfterGenerateMap(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        return AsyncMethodCall.Create(generator, instructions, original,
            AccessTools.Method(typeof(BossAuraHandler), nameof(AddBossAurasDuringActBegin)),
            afterState: AccessTools.Method(typeof(RunManager), "GenerateMap"));
    }

    [HarmonyPatch]
    private static class CombatTurnStateIsAnInternalClass
    {
        static MethodBase TargetMethod()
        {
            var turnState = AccessTools.TypeByName("MegaCrit.Sts2.Core.Combat.CombatTurnState");
            var asyncMethod = AccessTools.Method(typeof(CombatManager), "EndCombatInternal", [turnState]);
            return AccessTools.AsyncMoveNext(asyncMethod);
        }   

        [HarmonyTranspiler]
        private static List<CodeInstruction> RemoveAfterCombatEnd(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return AsyncMethodCall.Create(generator, instructions, original,
                        AccessTools.Method(typeof(BossAuraHandler), nameof(RemoveBossAurasAfterCombat)),
                        afterState: AccessTools.Method(typeof(Hook), nameof(Hook.AfterCombatVictory)));
        }
    }

   
    
    [HarmonyPatch(typeof(RunManager), "EnterRoomInternal", MethodType.Async)]
    [HarmonyTranspiler]
    private static List<CodeInstruction> RemoveBeforeRoomEntered(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        return AsyncMethodCall.Create(generator, instructions, original,
                    AccessTools.Method(typeof(BossAuraHandler), nameof(RemoveBossAurasAfterCombat)),
                    beforeState: AccessTools.Method(typeof(Hook), nameof(Hook.BeforeRoomEntered)));
    }
    
    
    
    
    
    #region Logic
    
    private static async Task AddBossAurasDuringActBegin()
    {
        if (!GameHelper.TryGetRunState(out var runState)) return;
        Entry.Logger.Info("Adding Boss Auras for the Act.");
        // Aggregate all eligible bosses as well as all boss-auras that can come from those.
        var relations = runState.Act.AllActiveBossEncounter
                    .ToDictionary(e => e
                                , e => AllBossAuras.Where(b => b.Bosses.Any(e2 => e2.Id == e.Id))
                                            .Cast<AuraModel>().ToList());

        var rng = runState.Rng.Niche; //TODO: Should this be niche?
        // For each eligible boss, get one random boss aura, and set their Boss Property for cleanup later.
        foreach (var (boss, candidates) in relations)
        {
            if(candidates.Count == 0) continue;
            var canonical = candidates.Count == 1 ? candidates[0] : rng.NextItem(candidates)!;
            var mutable = canonical.ToMutable();
            ((IBossAura)mutable).Boss = boss;
            await AuraCmd.Add(mutable);
        }
    }
    
    private static async Task RemoveBossAurasDuringActBegin()
    {
        if (!GameHelper.TryGetRunState(out var runState)) return;
        // We could in theory just clear all boss auras here, because a new act means out with the old and in with the new.
        // but that would include BossAuraEnd.None auras so I'm not doing it that way.
        var toRemove = runState.Auras.OfType<IBossAura>()
                    .Where(b => b.AuraEnd is BossAuraEnd.EndOfAct)
                    .Cast<AuraModel>().ToList();
        foreach (var bossAura in toRemove)
            await AuraCmd.Remove(bossAura);
    }

    private static async Task RemoveBossAurasAfterCombat()
    {
        if (!GameHelper.TryGetRunState(out var runState)) return;
        var room = (CombatRoom?)runState.CurrentRoom; // This method is only called through CombatManager.EndCombatInternal
        if (room?.RoomType is not RoomType.Boss) return;
        var toRemove = runState.Auras.OfType<IBossAura>()
                    .Where(b => b.AuraEnd is BossAuraEnd.AfterBoss && b.Boss.Id == room.Encounter.Id)
                    .Cast<AuraModel>().ToList();
        foreach (var bossAura in toRemove)
            await AuraCmd.Remove(bossAura);
    }
    
    
    private static async Task RemoveBeforeRoomEntered()
    {
        if (!GameHelper.TryGetRunState(out var runState)) return;
        var room = runState.CurrentRoom;
        if (room?.RoomType is not RoomType.Boss || room is not CombatRoom combatRoom) return;
        
        var toRemove = runState.Auras.OfType<IBossAura>()
                    .Where(b => b.AuraEnd is BossAuraEnd.BeforeBosses ||
                                (b.AuraEnd is BossAuraEnd.BeforeBoss && b.Boss?.Id == combatRoom.Encounter.Id))
                    .Cast<AuraModel>().ToList();
        foreach (var bossAura in toRemove)
            await AuraCmd.Remove(bossAura);
    }
    
    #endregion Logic
    
}