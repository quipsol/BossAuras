using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.Auras;


public class Subservient : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<QueenBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ChainsOfBindingPower>(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ChainsOfBindingPower>(), ..HoverTipFactory.FromAffliction<Bound>()];

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom combatRoom) return;
        await PowerCmd.Apply<ChainsOfBindingPower>(new ThrowingPlayerChoiceContext(), 
                    combatRoom.CombatState.PlayerCreatures,
                    DynamicVars[nameof(ChainsOfBindingPower)].BaseValue,
                    null, null);
    }
}
