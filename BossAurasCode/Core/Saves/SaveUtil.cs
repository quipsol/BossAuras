using BossAuras.Core.Models;
using BossAuras.Core.Models.Auras;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace BossAuras.Core.Saves;

internal static class AuraSaveUtil
{
    /// <summary>
    /// Obtains an aura by ID, or falls back to the deprecated aura if it's not found.
    /// This should only be used in save loading where it's okay to fall back to deprecated content.
    /// </summary>
    public static AuraModel AuraOrDeprecated(ModelId id)
    {
        return ModelDb.GetByIdOrNull<AuraModel>(id) ?? AuraDb.Aura<DeprecatedAura>();
    }
}