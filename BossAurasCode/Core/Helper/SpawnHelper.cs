using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace BossAuras.Core.Helper;


public static class SpawnHelper
{
    private static readonly MethodInfo? PositionEnemiesInfo = AccessTools.Method(typeof(NCombatRoom), "PositionEnemies");
    

    public static void RepositionEnemies(CombatState combatState)
    {
        var slotlessNCreatures = combatState.Enemies.Where(c => c.SlotName is null).Select(c => c.GetCreatureNode()).ToList();
        var slottedNCreatures = combatState.Enemies.Where(c => c.SlotName is not null).Select(c => c.GetCreatureNode()).ToList();
        
        RepositionEnemies(combatState, slotlessNCreatures, combatState.Encounter!.GetCameraScaling());
       
        
        // if there are no slotted creatures, we can stop here since RepositionEnemies took care of it.
        if (slottedNCreatures.Count is 0) return;
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        foreach (var nCreature in slottedNCreatures.OfType<NCreature>())
        {
            var x = nCreature.GlobalPosition.X;
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
        }
        
        var slotlessMinX = slotlessNCreatures.OfType<NCreature>().Select(nCreature => nCreature.GlobalPosition.X).Prepend(999f).Min();
        var moveBy = maxX - slotlessMinX + 300f;
        foreach (var nCreature in slotlessNCreatures.OfType<NCreature>())
        {
            nCreature.GlobalPosition += new Vector2(moveBy, 0f);
        }
        // zoom out a bit to make it fit
        // TODO: This does not guarantee everything is on the screen!
        NCombatRoom.Instance!.SceneContainer.Scale = Vector2.One * (combatState.Encounter.GetCameraScaling() - 0.1f);
    }
    
    private static void RepositionEnemies(CombatState combatState, List<NCreature?>? nCreatures = null, float? cameraScaling = null)
    {
        nCreatures ??= combatState.Enemies.Select(c => c.GetCreatureNode()).ToList();
        cameraScaling ??= combatState.Encounter!.GetCameraScaling();
        PositionEnemiesInfo?.Invoke(NCombatRoom.Instance, [nCreatures.Where(n => n is not null).ToList(), cameraScaling]);
    }
}