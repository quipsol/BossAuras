using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace BossAuras.Core.Models.BossAuras;
// TODO Rename
public class LagavulinMatriarch : AuraModel, IBossAura
{
    public List<EncounterModel> Bosses => [ModelDb.Encounter<LagavulinMatriarchBoss>()];
    public BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;
    public EncounterModel Boss { get; set; } = null!;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(-1), new PowerVar<DexterityPower>(-1)];
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (combatState.RoundNumber != 5) return;
        
        foreach (var creature in combatState.Creatures.Where(c => !c.IsPet))
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, DynamicVars[nameof(StrengthPower)].BaseValue, null, null);
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), creature, DynamicVars[nameof(DexterityPower)].BaseValue, null, null);
        }
    }
    
}