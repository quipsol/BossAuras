using BossAuras.Core.Handler;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.Extensions;

public static class ActModelExtensions
{
    extension(ActModel actModel)
    {
        /// <summary>
        /// List of all Boss encounters for the act in this run
        /// </summary>
        public IEnumerable<EncounterModel> AllActiveBossEncounter
        {
            get
            { 
                yield return actModel.BossEncounter;
                if (actModel.SecondBossEncounter is not null)
                    yield return actModel.SecondBossEncounter; 
            }
        }
    }
}