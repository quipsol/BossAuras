using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.Auras;


public class Mutagen : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<TestSubjectBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.EndOfAct;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
                new PowerVar<PersonalHivePower>(1),
                //new PowerVar<StrengthPower>(1),
                new PowerVar<SlowPower>(1),
                new PowerVar<HardenedShellPower>(20),
                new PowerVar<VitalSparkPower>(1),
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
                HoverTipFactory.FromPower<PersonalHivePower>(),
                //HoverTipFactory.FromPower<StrengthPower>(),
                HoverTipFactory.FromPower<SlowPower>(),
                HoverTipFactory.FromPower<HardenedShellPower>(),
                HoverTipFactory.FromPower<VitalSparkPower>(),
    ];
    
    // TODO:
    // Vital Sparks: It collides with Queens Subservient if both are active (Ascension 10)
    // First all Skills are afflicted by Vital Sparks, and then, the first two non-skills in hand get Queens debuff.
    // Not sure if I should leave it or think about a solution. I tend to just leave it.
    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom combatRoom) return;
        if (combatRoom.RoomType is RoomType.Boss) return;
        foreach (var enemy in combatRoom.Enemies)
        {
            if (Rng.NextBool()) continue; // 50% is too difficult.
            await (Rng.NextInt(0, 4) switch
            {
                        0 => ApplyPower<PersonalHivePower>(enemy),
                        1 => ApplyPower<HardenedShellPower>(enemy),
                        2 => ApplyPower<VitalSparkPower>(enemy),
                        3 => ApplyPower<SlowPower>(enemy),
                        _ => throw new ArgumentOutOfRangeException(),
            });
        }
        
        // await PowerCmd.Apply<ChainsOfBindingPower>(new ThrowingPlayerChoiceContext(), 
        //             combatRoom.CombatState.PlayerCreatures,
        //             DynamicVars[nameof(ChainsOfBindingPower)].BaseValue,
        //             null, null);
    }

    private async Task ApplyPower<TPower>(Creature target, PlayerChoiceContext? choiceContext = null) where TPower : PowerModel
    {
        choiceContext ??= new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<TPower>(choiceContext, target, DynamicVars[typeof(TPower).Name].BaseValue, null, null);
    }
}
