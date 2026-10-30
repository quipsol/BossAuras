using System.Diagnostics;
using BossAuras.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models;

public abstract class BossAuraModel : AuraModel, IBossAura
{
    /// <inheritdoc/>
    public abstract List<EncounterModel> Bosses { get; }
    /// <inheritdoc/>
    public virtual BossAuraEnd AuraEnd => BossAuraEnd.EndOfAct;
    /// <inheritdoc/>
    public EncounterModel Boss { get; set; } = null!;
    
    /// Used for saving the Boss reference.
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public ModelId? BossAuraBossId
    {
        get => Boss?.Id;
        set => Boss = value == null ? null! : ModelDb.GetByIdOrNull<EncounterModel>(value)!;
    }

    /// <inheritdoc/> <br/> <br/>
    /// Find an active Boss matching the Bosses list. If none is found, use the first active Boss.
    public override void DebugOnlySetup()
    {
        Boss = RunState.Act.AllActiveBossEncounter.FirstOrDefault(b => Bosses.Contains(b.CanonicalInstance)) ?? RunState.Act.BossEncounter;
    }
}