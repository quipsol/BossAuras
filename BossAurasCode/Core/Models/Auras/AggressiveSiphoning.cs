using BossAuras.Core.HoverTips;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models.Auras;

public class AggressiveSiphoning : BossAuraModel, IQuestProvider
{
    private const string COMBATS = "Combats";
    private const string TURN = "Turn";
    private const string SIPHON_COUNT = "SiphonCount";
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<LagavulinMatriarchBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.AfterBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new(COMBATS, 9), new (TURN, 4), new PowerVar<StrengthPower>(1), new PowerVar<DexterityPower>(1), new (SIPHON_COUNT, 0)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>(), MonsterHoverTipFactory.FromMonster<LagavulinMatriarch>()];

    private bool _isMarkedRoom;
    
    [SavedProperty]
    private int[] CoordCols { get; set; } = [];
    [SavedProperty]
    private int[] CoordRows { get; set; } = [];
    [SavedProperty]
    private bool CoordsSet { get; set; }

    [SavedProperty]
    public int SiphonCount
    {
        get;
        private set
        {
            AssertMutable();
            field = value;
            UpdateSiphonCount();
        }
    }
    
    private void UpdateSiphonCount()
    {
        DynamicVars[SIPHON_COUNT].BaseValue = SiphonCount;
        InvokeDisplayAmountChanged();
    }
    
    private static List<MapCoord> MarkedCoordsToMapCoords(List<Vector2I> input)
    {
        return input.Select(mapCoord => new MapCoord(mapCoord.Y, mapCoord.X)).ToList();
    }
    
     public List<MapCoord>? GetMarkedCoords() 
        => !CoordsSet ? null : CoordCols.Select((element, index) => new MapCoord { col = element, row = CoordRows[index] }).ToList();

    
    public override Task AfterApplied()
    {
        AddMarkedRooms(RunState.Map);
        return Task.CompletedTask;
    }

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
            
            CoordCols = new int[mapPoints.Count];
            CoordRows = new int[mapPoints.Count];
            for (var num = 0; num < mapPoints.Count; num++)
            {
                CoordCols[num] = mapPoints[num].coord.col;
                CoordRows[num] = mapPoints[num].coord.row;
            }
            CoordsSet = true;
            
            foreach (var mapPoint in mapPoints)
                mapPoint.AddQuest(this);
        }

        return map;
    }
    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        _isMarkedRoom = false;
        if (room is not CombatRoom combatRoom) return;
        if (RunState.CurrentMapPoint is null) return;
        var mapCoords = GetMarkedCoords();
        _isMarkedRoom = mapCoords?.Contains(RunState.CurrentMapPoint.coord) ?? throw new NullReferenceException("Map Coordinates were null");
        
        if (combatRoom.Encounter is LagavulinMatriarchBoss)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), combatRoom.CombatState.Enemies, DynamicVars[SIPHON_COUNT].BaseValue, null, null);
        }
        // else if(_isMarkedRoom)
        //     await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), combatRoom.CombatState.Allies.Where(a => a is { IsPet: false, IsAlive: true }), -DynamicVars[nameof(DexterityPower)].BaseValue, null, null);
        
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!_isMarkedRoom) return;
        if (side != CombatSide.Player) return;
        if (combatState.RoundNumber == DynamicVars[TURN].IntValue)
        {
            SiphonCount++;
        }
        if(combatState.RoundNumber < 4)
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), combatState.Enemies, -DynamicVars[nameof(StrengthPower)].BaseValue, null, null);
        
    }
    
    
    public string MarkerPath => "res://BossAuras/images/map_markers/missing_marker.png";
}
