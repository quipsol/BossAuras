using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs.History;

namespace BossAuras.Core.Commands;

/// Copy of <see cref="CreatureCmd"/> with safety checks removed.
public static class UnsafeCreatureCmd
{
    public static async Task<Creature> Add<T>(ICombatState combatState, string? slotName = null) where T : MonsterModel
    {
        Creature creature = combatState.CreateCreature(ModelDb.Monster<T>().ToMutable(), CombatSide.Enemy, slotName);
        await Add(creature);
        return creature;
    }

    public static async Task<Creature> Add(MonsterModel monster, ICombatState combatState, CombatSide side = CombatSide.Enemy, string? slotName = null)
    {
        monster.AssertMutable();
        Creature creature = combatState.CreateCreature(monster, side, slotName);
        await Add(creature);
        return creature;
    }
    
    // This could be a reverse patch but I don't know how. Maybe I will NerdSnipe PikCube at some point.
    public static async Task Add(Creature creature)
    {
        var combatState = creature.CombatState;
        if (combatState is null)
        {
            Entry.Logger.Error("Tried to add a creature in an unsafe way but the combatState was null");
            return;
        }
        combatState.AddCreature(creature);
        CombatManager.Instance.AddCreature(creature);
        NCombatRoom.Instance?.AddCreature(creature);
        await CombatManager.Instance.AfterCreatureAdded(creature);
        if (combatState.CurrentSide != CombatSide.Enemy && creature.IsMonster)
            creature.PrepareForNextTurn(combatState.Players.Select(p => p.Creature), rollNewMove: false);
        var mapPointRoomHistoryEntry = combatState.RunState.CurrentMapPointHistoryEntry?.Rooms.Last();
        if (mapPointRoomHistoryEntry != null && creature is { Monster: not null, Side: CombatSide.Enemy } && !mapPointRoomHistoryEntry.MonsterIds.Contains(creature.Monster.Id))
            mapPointRoomHistoryEntry.MonsterIds.Add(creature.Monster.Id);
        await Hook.AfterCreatureAddedToCombat(combatState, creature);
    }

}