using BaseLib.Patches.Saves;
using BaseLib.Utils;
using BossAuras.Constants;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using BossAuras.Core.Saves.Runs;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Handler;

/// Connects the AuraModels to the RunState. <br/>
/// Holds SpireField. Registers Save logic. Subscribes Auras to hooks.
[HarmonyPatch]
public class AuraHandler
{
    #region Accessor

    private static readonly SpireField<IRunState, List<AuraModel>> Auras = new(() => []);

    internal static List<AuraModel> GetAuras(IRunState rs) => Auras.Get(rs)!;

    /// Always go through here when setting the entire list!
    internal static void SetAuras(IRunState rs, List<AuraModel> auras)
    {
        Auras.Set(rs, auras); 
        GameHelper.GetNMapScreen()?.AuraContainer.Reload();
    }

#endregion Accessor
    
    
    
    #region Register Extended Save
    
    private static bool _alreadyRegistered = false;
    internal static void Register()
    {
        if (_alreadyRegistered)
        {
            Entry.Logger.Warn("Attempted to register saving auras a second time! Request ignored");
            return;
        }
        _alreadyRegistered = true;
        Entry.Logger.Info("Auras.RegisterSave called");
        
        ExtendedSaveTypes.RegisterObjectSaveType<SerializableAura>(
                    ExtendedSaveTypes.PropertyFunc<SerializableAura, ModelId>(nameof(SerializableAura.Id)),
                    ExtendedSaveTypes.PropertyFunc<SerializableAura, SavedProperties>(nameof(SerializableAura.Props))
                    );
        ExtendedSaveTypes.RegisterListSaveType<SerializableAura>();
        
        ExtendedSaveHandlers<IRunState, SerializableRun>.RegisterSave<IRunState, List<SerializableAura>>(
                    SaveKey.AURAS,
                    GetAurasFromRunState,
                    SetAurasOnRunState,
                    Serializer,
                    Deserializer);
    }
    private static List<SerializableAura>? GetAurasFromRunState(IRunState rs)
    {
        var auras = GetAuras(rs);
        return auras.Count == 0 ? null : auras.Select(aura => aura.ToSerializable()).ToList();
    }

    private static void SetAurasOnRunState(IRunState rs, List<SerializableAura>? saved)
    {
        if (saved == null) return;
        // We circumvent the map reload that would happen on SetAuras on purpose here
        var auras = saved.Select(AuraModel.FromSerializable).ToList();
        foreach (var auraModel in auras) auraModel.AddInternal(rs);
        Auras.Set(rs, auras);
    }

    private static void Serializer(List<SerializableAura> data, PacketWriter writer)
    {
        writer.WriteList(data);
    }

    private static List<SerializableAura> Deserializer(PacketReader reader)
    {
        return reader.ReadList<SerializableAura>();
    }

    #endregion Register Extended Save
    
    
    
    #region Subscribe Auras to Hooks
    
    
    // AuraModels get ALL hooks. Unlike normal models who choose between run and combat, the AuraModel gets ALL.
    private static bool _alreadySubscribed = false;
    internal static void SubscribeForStateHooks()
    {
        if (_alreadySubscribed)
        {
            Entry.Logger.Warn("Attempted to subscribe to combat hooks a second time! Request ignored");
            return;
        }
        _alreadySubscribed = true;

        ModHelper.SubscribeForCombatStateHooks($"{Entry.MOD_ID_PREFIX}-BOSS_AURA_ITERATOR", IterateBossAurasForCombatHooks);
    }
  

    private static IEnumerable<AbstractModel> IterateBossAurasForCombatHooks(CombatState combatState) => combatState.RunState.Auras.ToList();

    
    // Some RunHooks pass null as the child CombatState, even though the RunState has a CombatState. So we do this:
    [HarmonyPatch]
    internal static class AuraRunHookListenersPatch
    {
        // If childCombatState != null, the combat-hook subscription already yields the auras.
        // If it's null (in or out of combat), nothing else will, so add them here.
        [HarmonyPatch(typeof(RunState), nameof(RunState.IterateHookListeners))]
        [HarmonyPostfix]
        private static void Postfix(RunState __instance, ICombatState? childCombatState, ref IEnumerable<AbstractModel> __result)
        {
            if (childCombatState != null) return;
            __result = Append(__result, __instance);
        }

        private static IEnumerable<AbstractModel> Append(IEnumerable<AbstractModel> models, RunState runState)
        {
            foreach (var model in models) yield return model;
            foreach (var aura in runState.Auras.ToList()) yield return aura;
        }
    }
    
    
    
    #endregion Subscribe Auras to Hooks
}

// Very specific issue: (putting this at the bottom because it's a lot of text)
/*
If RunState iterates the models, it will also iterate the combatState IF IT EXISTS
So during combat, if the model is subscribed for CombatStateHooks this works fine, and they get all hooks.
But if a hook is fired in the run, outside of combat (e.g. AfterActEntered) it will not fire if the model is
only subscribing to the combat hooks.
Inverse, if I subscribe to RunStateHooks. I get all the out of combat hooks, but none of the in combat hooks.
Solution: Subscribe to both, and filter in RunStateHooks if a CombatState exists. If it does, return an empty collection.

Tests using two Test Singletons without the extra exclusion logic shows the following:
[BossAuras] After Act Entered - Run Hooks
[BossAuras] Before Combat Start - Run Hooks
[BossAuras] Before Combat Start - Combat Hooks
[BossAuras] Before Attack - Combat Hooks

Tests with the extra exclusion logic shows:
[BossAuras] After Act Entered - Run Hooks
[BossAuras] Before Combat Start - Combat Hooks
[BossAuras] Before Attack - Combat Hooks

Unfortunately this was not a full fix (see AfterRoomEntered hook.) :slimepensive:
Instead we postfix RunState and only return an empty list if a CombatState has been passed.
It's correct now but damn, why did I have to patch anything..
*/