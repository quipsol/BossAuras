using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Models.Auras;


public class Queen : BossAuraModel
{
    private const string POWER_AMOUNT = "PowerAmount";
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<QueenBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new (POWER_AMOUNT, 3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [
                HoverTipFactory.FromPower<WeakPower>(), 
                HoverTipFactory.FromPower<FrailPower>(), 
                HoverTipFactory.FromPower<VulnerablePower>()
    ];
    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom combatRoom) return;

        foreach (var playerCreature in combatRoom.CombatState.PlayerCreatures)
        {
            switch(RunState.Rng.Niche.NextInt(0, 3))
            {
                case 0:
                    await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), playerCreature,
                                DynamicVars[POWER_AMOUNT].BaseValue, null, null);
                    break;
                case 1:
                    await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), playerCreature,
                                DynamicVars[POWER_AMOUNT].BaseValue, null, null);
                    break;
                case 2:
                    await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), playerCreature,
                                DynamicVars[POWER_AMOUNT].BaseValue, null, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        }
    }
}
