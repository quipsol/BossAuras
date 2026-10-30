using BossAuras.Core.Nodes.Auras;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace BossAuras.Core.Models.Auras;


public class UnstableCore : BossAuraModel
{
    private const string TURN = "Turn";
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<WaterfallGiantBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, DamageProps.nonCardUnpowered), new (TURN, 2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    
    
    
    // This works but is an amazing amount of jank. I need to think of something better, however for now, this works so it stays.
    
    private NUnstableCoreUi? _activeUi;
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (combatState.RoundNumber != DynamicVars[TURN].IntValue) return;
        Entry.Logger.Info("Initiate Unstable Core");
        if (NCombatRoom.Instance is null) return;
        _activeUi = NUnstableCoreUi.Create();
        NCombatRoom.Instance.AddChildSafely(_activeUi);
        var nIntent = NIntent.Create(0f);
        _activeUi.SetIntent(nIntent);
        var intent = new DeathBlowIntent(() => DynamicVars.Damage.BaseValue);
        var dummy = new Creature((MonsterModel)ModelDb.Monster<BattleFriendV1>().MutableClone(), CombatSide.Enemy, null)
        {
                    CombatState = combatState,
        };
        nIntent.UpdateIntent(intent, [], dummy);
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (_activeUi is null) return;
        var targets = participants.First().CombatState?.HittableEnemies ?? [];
        await CreatureCmd.Damage(choiceContext, targets, DynamicVars.Damage, null, null, null);
        // Flash active UI??? -> yes absolutely, it looks ridiculous otherwise.
        _activeUi.QueueFreeSafely();
        _activeUi = null;

    }
}
