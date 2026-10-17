using BossAuras.Core.Handler;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Extensions;

public static class RunStateExtensions
{
    extension(IRunState runState)
    {
        public bool IsMultiplayer => runState.Players.Count > 1;
        
        //public IReadOnlyList<AuraModel> Auras => AuraHandler.GetAuras(runState);
        //public void RemoveAura(IEnumerable<AuraModel> auras) => AuraHandler.SetAuras(runState, auras.ToList());
        public List<AuraModel> Auras
        {
            get => AuraHandler.GetAuras(runState);
            set => AuraHandler.SetAuras(runState, value);
        }

        /// Returns the first AuraModel that matches the ID, or null.
        public AuraModel? GetAuraById(ModelId id) => runState.Auras.FirstOrDefault(a => a.Id == id);
    }
}