using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using BossAuras.Core.Commands;
using BossAuras.Core.Helper;
using BossAuras.Core.HoverTips;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models.Auras;

public class Kinship : BossAuraModel, IQuestProvider
{
    private const string COMBATS = "Combats";
    private const string HP_PERCENTAGE = "HpPercentage";
    
    public string MarkerPath => "res://BossAuras/images/map_markers/kinship.png";
    
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<TheKinBoss>()];
    
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new(COMBATS, 7), new (HP_PERCENTAGE, 20)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MonsterHoverTipFactory.FromMonster<KinFollower>()];

    [SavedProperty]
    private int[] KinshipCoordCols { get; set; } = [];
    [SavedProperty]
    private int[] KinshipCoordRows { get; set; } = [];
    [SavedProperty]
    private bool KinshipCoordsSet { get; set; }

    private static List<MapCoord> MarkedCoordsToMapCoords(List<Vector2I> input)
    {
        return input.Select(mapCoord => new MapCoord(mapCoord.Y, mapCoord.X)).ToList();
    }


    public List<MapCoord>? GetMarkedCoords() 
        => !KinshipCoordsSet ? null : KinshipCoordCols.Select((element, index) => new MapCoord { col = element, row = KinshipCoordRows[index] }).ToList();

    
    public override Task AfterApplied() => TaskHelper.Done(AddMarkedRooms(RunState.Map));

    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        Entry.Logger.Info("TheKinEffect - ModifyGeneratedMapLate");
        return AddMarkedRooms(map);
    }
    
    private ActMap AddMarkedRooms(ActMap map)
    {
        var markedCoords = GetMarkedCoords();
        
        if (markedCoords is not null && markedCoords.TrueForAll(c => map.HasPoint(c) && map.GetPoint(c)!.PointType is MapPointType.Monster or MapPointType.Elite))
        {
            foreach (var mapCoord in markedCoords)
            {
                var point = map.GetPoint(mapCoord);
                if (point == null) throw new InvalidOperationException($"Loaded a kinship map with coordinate {mapCoord}, but the generated map does not contain that coordinate!");
                point.AddQuest(this);
            }
        }
        else
        {
            // Rng rng = new Rng(base.Owner, base.Id, 0uL); // Fur coat uses this. We dont have an Owner(Player).
            var rng = RunState.Rng.UpFront;
            var mapPoints = map.GetAllMapPoints()
                        .Where(p => p.PointType is MapPointType.Monster or MapPointType.Elite && p.coord.row > 3)
                        .ToList().UnstableShuffle(rng).Take(DynamicVars[COMBATS].IntValue).ToList();
            
            KinshipCoordCols = new int[mapPoints.Count];
            KinshipCoordRows = new int[mapPoints.Count];
            for (var num = 0; num < mapPoints.Count; num++)
            {
                KinshipCoordCols[num] = mapPoints[num].coord.col;
                KinshipCoordRows[num] = mapPoints[num].coord.row;
            }
            KinshipCoordsSet = true;
            
            foreach (var mapPoint in mapPoints)
                mapPoint.AddQuest(this);
        }

        return map;
    }


    /*
    Cant use BeforeCombatStart because at that point the combat is already rendered and you see the creature appear
    in the middle and then reorder into the scene properly.
    
    However, CreatureCmd has a safety-check if we are in combat. By the time this runs that is still false, even though everything
    needed to add a creature is already in place.
    
    Because of these two issue, we use an earlier hook and an unsafe version of CreatureCmd.
     */
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        Entry.Logger.Info("TheKinEffect - AfterRoomEntered");
        if (room is not CombatRoom combatRoom) return;
        if (RunState.CurrentMapPoint is null) return;

        var mapCoords = GetMarkedCoords();
        if(mapCoords is null || !mapCoords.Contains(RunState.CurrentMapPoint.coord)) return;
        
        var combatState = combatRoom.CombatState;
        
        // add enemy
        var target = await UnsafeCreatureCmd.Add<KinFollower>(combatState);
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), target, 1m, null, null);
        target.SetCurrentHpInternal(target.MaxHp * 0.2m); // replace fix .2 with DynamicVar
        SpawnHelper.RepositionEnemies(combatState);
        FlashCreatures(target);
    }
    

}