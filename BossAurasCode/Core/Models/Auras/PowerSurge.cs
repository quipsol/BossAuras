using BossAuras.Core.HoverTips;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models.Auras;


public class PowerSurge : BossAuraModel
{
    private const string EXTRA_STEAM = "ExtraSteam";
    private const string INCREASE = "Increase";
    public override List<EncounterModel> Bosses => [ModelDb.Encounter<WaterfallGiantBoss>()];
    public override BossAuraEnd AuraEnd => BossAuraEnd.AfterBoss;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new (EXTRA_STEAM, 0), new(INCREASE, 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MonsterHoverTipFactory.FromMonster<WaterfallGiant>(), HoverTipFactory.FromPower<SteamEruptionPower>()];

    [SavedProperty]
    public int ExtraSteam
    {
        get;
        private set
        {
            AssertMutable();
            field = value;
            UpdateSiphonCount();
        }
    }
    
    private void UpdateSiphonCount()
    {
        DynamicVars[EXTRA_STEAM].BaseValue = ExtraSteam;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom combatRoom) return;
        if (combatRoom.Encounter is WaterfallGiantBoss)
        {
            var waterfallGiants = combatRoom.CombatState.Enemies.Where(e => e.Monster is WaterfallGiant).ToList();
            FlashCreatures(waterfallGiants);
            await PowerCmd.Apply<SteamEruptionPower>(new ThrowingPlayerChoiceContext(), waterfallGiants, DynamicVars[EXTRA_STEAM].BaseValue, null, null);
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type is CardType.Power)
        {
            // Flash on creature
            ExtraSteam += DynamicVars[INCREASE].IntValue;
        }
        return Task.CompletedTask;
    }
}
