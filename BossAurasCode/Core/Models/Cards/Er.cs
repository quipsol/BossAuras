using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace BossAuras.Core.Models.Cards;

public class Er() : BossAurasCardModel(1, CardType.Attack, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        
    }

    protected override void OnUpgrade()
    {
    }
}
