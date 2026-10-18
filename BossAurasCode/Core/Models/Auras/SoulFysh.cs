using BossAuras.Core.Models.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.Auras;
// TODO Rename
public class SoulFysh : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<SoulFyshBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<MinorBeckon>()];
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (combatState.RoundNumber > 1) return;
        if (side != CombatSide.Player) return;
        foreach (var player in combatState.Players)
        {
            var cardModel = combatState.CreateCard<MinorBeckon>(player);
            // generate in center of screen, then move to Discard
            // TODO: Replace with card preview
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Play, player);
            await CardPileCmd.Add(cardModel, PileType.Discard); 
        }
        
    }
}