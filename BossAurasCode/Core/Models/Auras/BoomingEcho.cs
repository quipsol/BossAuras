using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace BossAuras.Core.Models.Auras;

public class BoomingEcho : BossAuraModel
{
    private const string TURN = "Turn";
    
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<CeremonialBeastBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new (TURN, 5), new PowerVar<RingingPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Stun), HoverTipFactory.FromPower<RingingPower>()];
    
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (combatState.RoundNumber == DynamicVars[TURN].BaseValue)
        {
            FlashCombat();
            foreach (var creature in combatState.HittableEnemies)
                await CreatureCmd.Stun(creature);
        }
        
        if (combatState.RoundNumber == DynamicVars[TURN].BaseValue + 1)
        {
            FlashCombat();
            await PowerCmd.Apply<RingingPower>(new ThrowingPlayerChoiceContext(), combatState.Allies.Where(a => !a.IsPet), DynamicVars[nameof(RingingPower)].BaseValue, null, null);
        }
    }
}
