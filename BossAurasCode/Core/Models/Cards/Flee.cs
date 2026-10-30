using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BossAuras.Core.Models;
using BossAuras.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace BossAuras.Core.Models.Cards;

[Pool(typeof(ColorlessCardPool))]
public class Flee() : BossAurasCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override HashSet<CardTag> CanonicalTags => [];

    public override int MaxUpgradeLevel => 0;
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HuntedPower>(choiceContext, Owner.Creature, DynamicVars[nameof(HuntedPower)].BaseValue, Owner.Creature, this);
        EnergyCost.AddThisCombat(1);
    }

}
