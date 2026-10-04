using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Helper;

/// <summary>
/// Helper class that wraps various undesirable actions.
/// </summary>
public class GameHelper
{
    /// <summary>
    /// Try to get the currently active IRunState object.
    /// </summary>
    /// <param name="state">the <c>IRunState</c> object</param>
    /// <returns><c>true</c> if a RunState is active</returns>
    public static bool TryGetRunState([NotNullWhen(true)]out IRunState? state)
    {
        state = RunManager.Instance.DebugOnlyGetState() ?? null;
        return state != null;
    }
    /// Returns the currently active IRunState object or null
    public static IRunState? GetNullableRunState() => RunManager.Instance.DebugOnlyGetState() ?? null;
    /// Returns the currently active IRunState object if it exists, otherwise throw a NRE
    public static IRunState GetRunState() => GetNullableRunState() ?? throw new NullReferenceException();
    
    
    
    /// <summary>
    /// Try to get the currently active ICombatState object.
    /// </summary>
    /// <param name="state">the <c>ICombatState</c> object</param>
    /// <returns><c>true</c> if a CombatState is active</returns>
    public static bool TryGetCombatState([NotNullWhen(true)]out ICombatState? state)
    {
        state = GetNullableCombatStateInternal();
        return state != null;
    }
    /// Returns the currently active ICombatState object or null
    public static ICombatState? GetNullableCombatState() => GetNullableCombatStateInternal();
    /// Returns the currently active ICombatState object if it exists, otherwise throw a NRE
    public static ICombatState GetCombatState() => GetNullableCombatState() ?? throw new NullReferenceException();


    public static NMapScreen? GetNMapScreen() => NMapScreen.Instance;
    
    
    
    
    
    
    #region Helpers
    
    
    /// CombatState doesn't have the equivalent Debug call, so we take the long way and go through the Player-Creature. <br/>
    /// Extracted into its own method to make it easier to replace if a better way comes up.
    private static ICombatState? GetNullableCombatStateInternal()
    {
        // Player creatures stay in combat, even if dead in multiplayer. Right? RIGHT??
        return RunManager.Instance.DebugOnlyGetState()?.Players[0]?.Creature?.CombatState ?? null;
    }
    
    #endregion Helpers
}