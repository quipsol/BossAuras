using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models;

public abstract class BossAuraModel : AuraModel, IBossAura
{
    public abstract List<EncounterModel> Bosses { get; }
    public virtual BossAuraEnd AuraEnd => BossAuraEnd.EndOfAct;
    public EncounterModel Boss { get; set; } = null!;
    
    /// Used for saving the Boss reference.
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public ModelId? BossAuraBossId
    {
        get => Boss?.Id;
        set => Boss = value == null ? null! : ModelDb.GetByIdOrNull<EncounterModel>(value)!;
    }
}