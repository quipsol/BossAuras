using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BaseLib.Abstracts;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using BossAuras;
using BossAuras.Core.Helper;
using BossAuras.Core.HoverTips;
using BossAuras.Core.Models.Cards;
using BossAuras.Core.Saves;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models.Auras;

public class Kinship : BossAuraModel
{
    private const string COMBATS = "Combats";
    private const string HP_PERCENTAGE = "HpPercentage";
    
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<TheKinBoss>()];
    
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new(COMBATS, 7), new (HP_PERCENTAGE, 20)];
    // TODO: Create new "Encounter Hover Tip" to display the "Kin Follower" enemy as a HoverTip for the aura!?
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MonsterHoverTipFactory.Create<KinFollower>()];

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

    
    public override Task AfterApplied()
    {
        if (GameHelper.TryGetRunState(out var state))
        { 
            AddMarkedRooms(state.Map, state);
        }

        return Task.CompletedTask;
    }

    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        Entry.Logger.Info("TheKinEffect - ModifyGeneratedMapLate");
        return AddMarkedRooms(map, runState);
    }

    
    // TODO:
    // Add our own map-markers. This reuses the same marker the rest does. Technically this is fine,
    // none of them should ever be active at the same time. But
    // 1: The marker doesnt fit this
    // 2: Better safe than sorry
    private ActMap AddMarkedRooms(ActMap map, IRunState runState) // RunState only used to get that one rng.
    {
        var markedCoords = GetMarkedCoords();
        
        if (markedCoords is not null && markedCoords.TrueForAll(c => map.HasPoint(c) && map.GetPoint(c)!.PointType is MapPointType.Monster or MapPointType.Elite))
        {
            foreach (var mapCoord in markedCoords)
            {
                var point = map.GetPoint(mapCoord);
                if (point == null)
                    throw new InvalidOperationException($"Loaded a kinship map with coordinate {mapCoord}, but the generated map does not contain that coordinate!");
                point.AddQuest(this);
            }
        }
        else
        {
            // Rng rng = new Rng(base.Owner, base.Id, 0uL); // Fur coat uses this. We dont have an Owner(Player).
            var rng = runState.Rng.UpFront;
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
    
    
    

    
    
    // public override async Task BeforeCombatStart()
    // {
    //     BossAurasInit.Logger.Info("TheKinEffect - BeforeCombatStart");
    //     var runState = GameHelper.GetNullableRunState();
    //     if (runState?.CurrentMapPoint is null) return;
    //     if (runState.CurrentRoom is not CombatRoom combatRoom) return;
    //     
    //     if (!TryGetMarkedCoords(runState, out var markedCoords)) return;
    //     var mapCoords = MarkedCoordsToMapCoords(markedCoords);
    //     if(!mapCoords.Contains(runState.CurrentMapPoint.coord)) return;
    //     
    //     var combatState = combatRoom.CombatState;
    //     
    //     // add enemy
    //     var target = await CreatureCmd.Add<KinFollower>(combatState);
    //     await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), target, 1m, null, null);
    //     target.SetCurrentHpInternal(target.MaxHp * 0.2m); // replace fix .2 with DynamicVar
    //     // placement
    //     if (combatState.Encounter!.HasScene)
    //     {
    //         var slotlessNCreatures = combatState.Enemies.Where(c => c.SlotName is null).Select(c => c.GetCreatureNode()).ToList();
    //         var slottedNCreatures = combatState.Enemies.Where(c => c.SlotName is not null).Select(c => c.GetCreatureNode()).ToList();
    //         
    //         var minX = 9999f;
    //         var maxX = 0f;
    //         foreach (var nCreature in slottedNCreatures.OfType<NCreature>())
    //         {
    //             if(nCreature.GlobalPosition.X < minX)
    //                 minX = nCreature.GlobalPosition.X;
    //             if(nCreature.GlobalPosition.X > maxX)
    //                 maxX = nCreature.GlobalPosition.X;
    //         }
    //         if (minX > maxX) return;
    //         
    //         RepositionEnemies(combatState, slotlessNCreatures, combatState.Encounter!.GetCameraScaling());
    //
    //         var slotlessMinX = slotlessNCreatures.OfType<NCreature>().Select(nCreature => nCreature.GlobalPosition.X).Prepend(9999f).Min();
    //         var moveBy = maxX - slotlessMinX + 300f;
    //         foreach (var nCreature in slotlessNCreatures.OfType<NCreature>())
    //         {
    //             nCreature.GlobalPosition += new Vector2(moveBy, 0f);
    //         }
    //         // zoom out a bit to make it fit
    //         NCombatRoom.Instance!.SceneContainer.Scale = Vector2.One * (combatState.Encounter.GetCameraScaling() - 0.1f);
    //     }
    //     else
    //     {
    //         RepositionEnemies(combatState);
    //     }
    // }

    private static readonly MethodInfo? PositionEnemiesInfo = AccessTools.Method(typeof(NCombatRoom), "PositionEnemies");
    private static void RepositionEnemies(CombatState combatState, List<NCreature?>? nCreatures = null, float? cameraScaling = null)
    {
        nCreatures ??= combatState.Enemies.Select(c => c.GetCreatureNode()).ToList();
        cameraScaling ??= combatState.Encounter!.GetCameraScaling();
        PositionEnemiesInfo?.Invoke(NCombatRoom.Instance, [nCreatures.Where(n => n is not null).ToList(), cameraScaling]);
    }



}