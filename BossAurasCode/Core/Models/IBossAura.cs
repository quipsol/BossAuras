using BossAuras.Core.Models.Singletons;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.Models;

/// <summary>
/// Turn an <see cref="AuraModel"/> into a Boss Aura. Logic handled in <see cref="BossAuraSingleton"/>
/// </summary>
public interface IBossAura
{
    /// <summary>
    /// List of Bosses this Boss-Aura can come from.
    /// </summary>
    public List<EncounterModel> Bosses { get; }
    /// <summary>
    /// End of Lifecycle point.
    /// </summary>
    public BossAuraEnd AuraEnd { get; }
    /// <summary>
    /// Set with `= null!`. Will be overwritten later! <br/>
    /// Used to store which Boss this is connected to, for end of lifecycle logic.
    /// </summary>
    public EncounterModel Boss { get; set; }
}