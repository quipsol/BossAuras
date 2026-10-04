using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.Models;

public abstract class BossAuraModel : AuraModel, IBossAura
{
    public abstract List<EncounterModel> Bosses { get; }
    public virtual BossAuraEnd AuraEnd => BossAuraEnd.EndOfAct;
    public EncounterModel Boss { get; set; } = null!;
}