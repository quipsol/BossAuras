using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace BossAuras.Core.Models.Auras;


public class UnstableCore : BossAuraModel
{
    private const string TURN = "Turn";
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<WaterfallGiantBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, DamageProps.nonCardUnpowered),new (TURN, 5)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (combatState.RoundNumber != DynamicVars[TURN].IntValue) return;
        Entry.Logger.Info("Initiate Unstable Core");
        // TODO
        // Begin showing UI elements that informs the player that the effect is currently active (potentially with its own hover tips!).
        // Two options from here:
        // 1. The new element will take care of itself for exploding -> Needs to be a Model that gets hooks
        // 2. This model holds a reference to it, and still takes care of all the logic, the UI element is only for visuals.
        
        // Option 2 seems like the better option. Either way, I would want a Cmd or Helper class for the UI element (placement, etc.)
        // Placing it at the center makes sense, though it might fight against the UI of other mods
        // or even base game features (Knowledge Demons "choose a card" must be in front of this UI!)
    }
}
