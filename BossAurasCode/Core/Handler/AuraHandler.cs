using BaseLib.Patches.Saves;
using BaseLib.Utils;
using BossAuras.Constants;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using BossAuras.Core.Saves.Runs;
using BossAuras.Util;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Handler;

/// Connects the AuraModels to the RunState. <br/>
/// Holds SpireField. Registers Save logic. Subscribes Auras to hooks.
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
        if (auras.Count == 0) return null; // null = nothing saved
        return auras.Select(aura => aura.ToSerializable()).ToList();
    }

    private static void SetAurasOnRunState(IRunState rs, List<SerializableAura>? saved)
    {
        if (saved == null) return;
        //SetAuras(rs, saved.Select(AuraModel.FromSerializable).ToList());
        // We circumvent the map reload on purpose here
        Auras.Set(rs, saved.Select(AuraModel.FromSerializable).ToList());
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
    
    private static bool _alreadySubscribed = false;
    internal static void SubscribeForStateHooks()
    {
        if (_alreadySubscribed)
        {
            Entry.Logger.Warn("Attempted to subscribe to combat hooks a second time! Request ignored");
            return;
        }
        _alreadySubscribed = true;
        // yes, we subscribe to both. See below for why.
        ModHelper.SubscribeForRunStateHooks($"{Entry.MOD_ID_PREFIX}-BOSS_AURA_ITERATOR", IterateBossAurasForRunHooks);
        ModHelper.SubscribeForCombatStateHooks($"{Entry.MOD_ID_PREFIX}-BOSS_AURA_ITERATOR", IterateBossAurasForCombatHooks);
    }
    
    // This will go through the entire "get the CombatState logic" every time a run-hook is called.
    // May be better to think of a way to store a CombatState reference (not here but in GameHelper)
    // in a way that guarantees it goes null of combat ends (storing as a weak reference)
    private static IEnumerable<AbstractModel> IterateBossAurasForRunHooks(RunState runState) 
        => GameHelper.GetNullableCombatState() is null ? runState.Auras : [];
    private static IEnumerable<AbstractModel> IterateBossAurasForCombatHooks(CombatState combatState)=> combatState.RunState.Auras;
    
    #endregion Subscribe Auras to Hooks
}

// Very specific issue: (putting this at the bottom because it's a lot)
/*
If RunState iterates the models, it will also iterate the combatState IF IT EXISTS
So during combat, if the model is subscribed for CombatStateHooks this works fine, and they get all hooks.
But if a hook is fired in the run, outside of combat (e.g. AfterActEntered) it will not fire if the model is only
subscribing to the combat hooks.
Solution: Subscribe to both, and filter in RunStateHooks if a CombatState exists. If it does, return an empty collection.

Tests using the two Test Singletons without the extra exclusion logic shows the following:
[BossAuras] After Act Entered - Run Hooks
[BossAuras] Before Combat Start - Run Hooks
[BossAuras] Before Combat Start - Combat Hooks
[BossAuras] Before Attack - Combat Hooks

Tests with the extra exclusion logic shows:
[BossAuras] After Act Entered - Run Hooks
[BossAuras] Before Combat Start - Combat Hooks
[BossAuras] Before Attack - Combat Hooks


*/