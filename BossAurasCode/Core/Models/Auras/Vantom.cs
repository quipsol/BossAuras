using BossAuras.Core.HoverTips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.Auras;
// TODO Rename
public class Vantom : BossAuraModel
{
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<VantomBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SlipperyPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SlipperyPower>(),
                MonsterHoverTipFactory.FromMonster<Architect>(),
                MonsterHoverTipFactory.FromCharacter<Silent>(),
    ];

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        Entry.Logger.Info("AfterRoomEntered Vantom");
        if (room is CombatRoom combatRoom)
        {
            foreach (var creature in combatRoom.CombatState.Creatures.Where(c => !c.IsPet))
            {
                await PowerCmd.Apply<SlipperyPower>(new ThrowingPlayerChoiceContext(), creature, DynamicVars[nameof(SlipperyPower)].BaseValue, null, null);
            }
        }
    }
}