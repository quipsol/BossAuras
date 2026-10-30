using BossAuras.Core.Helper;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.Auras;

// This is a good example for "what if auras could link into combat to display things" because
// then I could just give this the "WitheringPresencePower" that Aeonglass has. But powers are only for creatures so this might end up needing jank.
public class Sandglass : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<AeonglassBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBosses;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(10)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Wither>()];

    // TODO: This is a bad temp fix to get it working because I'm too lazy right now. Like, the ENTIRE thing is.
    private Dictionary<Player, int> _perPlayerCounter = new();
    // reset dictionary
    public override Task BeforeCombatStart() => TaskHelper.Done(_perPlayerCounter = new());
    public override Task AfterCombatEnd(CombatRoom room) => TaskHelper.Done(_perPlayerCounter = new());

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _perPlayerCounter[cardPlay.Player] = (_perPlayerCounter[cardPlay.Player] + 1) % DynamicVars.Cards.IntValue;
        if (_perPlayerCounter[cardPlay.Player] != 0) return;
        Flash();
        await Cmd.Wait(0.5f);
        await CardPileCmd.AddToCombatAndPreview<Wither>(cardPlay.Card.Owner.Creature, PileType.Hand, 1, null);
    }
}
