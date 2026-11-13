using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;

namespace BossAuras.Core.Models.Auras;


public class VagueMap : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<KnowledgeDemonBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.EndOfAct;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
}
