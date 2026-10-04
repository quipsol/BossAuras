using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.BossAuras;
// TODO Rename
public class Vantom : AuraModel, IBossAura
{
    public List<EncounterModel> Bosses => [ModelDb.Encounter<VantomBoss>()];
    public BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;
    public EncounterModel Boss { get; set; } = null!;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SlipperyPower>(1)];

    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom combatRoom)
        {
            foreach (var creature in combatRoom.CombatState.Creatures.Where(c => !c.IsPet))
            {
                await PowerCmd.Apply<SlipperyPower>(new ThrowingPlayerChoiceContext(), creature, DynamicVars[nameof(SlipperyPower)].BaseValue, null, null);
            }
        }
    }
    
}