using BaseLib.Abstracts;
using BaseLib.Utils;
using BossAuras.Core.HoverTips;
using BossAuras.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BossAuras.Core.Models.Relics;

[Pool(typeof(EventRelicPool))]
public class TestRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>[MonsterHoverTipFactory.Create<KinFollower>(),
                HoverTipFactory.FromCard<MinorBeckon>(), HoverTipFactory.Static(StaticHoverTip.Block),
                HoverTipFactory.FromPower<DexterityPower>(),
                HoverTipFactory.FromPower<StrengthPower>(),
                HoverTipFactory.FromPower<IntangiblePower>(),
                HoverTipFactory.FromPower<SlipperyPower>(),
    ];
}