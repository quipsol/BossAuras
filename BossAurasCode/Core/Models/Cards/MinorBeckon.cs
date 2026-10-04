using BaseLib.Abstracts;
using BaseLib.Utils;
using BossAuras.Core.HoverTips;
using BossAuras.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace BossAuras.Core.Models.Cards;

// TODO: May replace with a dedicated BossAurasCardModel
[Pool(typeof(StatusCardPool))]
public class MinorBeckon() : CustomCardModel(1, CardType.Status, CardRarity.Status, TargetType.None)
{
    public override string CustomPortraitPath => "res://images/atlases/card_atlas.sprites/status/beckon.tres";
    public override int MaxUpgradeLevel => 0;
    public override bool HasTurnEndInHandEffect => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(3)];

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, DamageProps.cardHpLoss, this, null);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>[MonsterHoverTipFactory.Create<KinFollower>(),
                HoverTipFactory.FromCard<MinorBeckon>(), HoverTipFactory.Static(StaticHoverTip.Block)];
}