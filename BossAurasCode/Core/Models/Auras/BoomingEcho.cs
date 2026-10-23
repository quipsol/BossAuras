using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace BossAuras.Core.Models.Auras;

/// Localization: "BOSSAURAS-<ID>.title" and "BOSSAURAS-<ID>.description" in BossAuras/localization/eng/auras.json.
public class BoomingEcho : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<CeremonialBeastBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ("Round", 5), new PowerVar<RingingPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Stun), HoverTipFactory.FromPower<RingingPower>()];
}
