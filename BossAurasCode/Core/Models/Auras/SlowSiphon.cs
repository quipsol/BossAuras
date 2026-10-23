using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace BossAuras.Core.Models.Auras;

public class SlowSiphon : BossAuraModel
{
    private const string TURN = "Turn";
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<LagavulinMatriarchBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new (TURN, 5), new PowerVar<StrengthPower>(1), new PowerVar<DexterityPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>()];
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (combatState.RoundNumber != DynamicVars[TURN].IntValue) return;
        var targets = combatState.Creatures.Where(c => c is { IsPet: false, IsAlive: true }).ToList();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), targets, -DynamicVars[nameof(StrengthPower)].BaseValue, null, null);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -DynamicVars[nameof(DexterityPower)].BaseValue, null, null);
        
    }
    
}