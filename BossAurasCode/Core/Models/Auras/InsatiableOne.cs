using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using BossAuras.Core.Models.Cards;
using BossAuras.Core.Models.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace BossAuras.Core.Models.Auras;


public class InsatiableOne : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<TheInsatiableBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<HuntedPower>(4), new CardsVar(3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];


    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (combatState.RoundNumber > 1) return;
        if (side != CombatSide.Player) return;
        await PowerCmd.Apply<HuntedPower>(new ThrowingPlayerChoiceContext(), combatState.Players.Select(p => p.Creature),
                    DynamicVars[nameof(HuntedPower)].BaseValue, null, null);
        
        foreach (var player in combatState.Players)
        {
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                var cardModel = combatState.CreateCard<Flee>(player);
                // generate in center of screen, then move to Draw
                // TODO: Replace with card preview
                await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Play, player);
                await CardPileCmd.Add(cardModel, PileType.Draw, CardPilePosition.Random); 
            }
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                var cardModel = combatState.CreateCard<Flee>(player);
                // generate in center of screen, then move to Discard
                // TODO: Replace with card preview
                await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Play, player);
                await CardPileCmd.Add(cardModel, PileType.Discard); 
            }
        }
        
    }
}
