using BossAuras.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossAuras.Core.Models.BossAuras;
// TODO Rename
public class SoulFysh : AuraModel, IBossAura
{
    public List<EncounterModel> Bosses => [ModelDb.Encounter<SoulFyshBoss>()];
    public BossAuraEnd AuraEnd => BossAuraEnd.BeforeBoss;
    public EncounterModel Boss { get; set; } = null!;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom combatRoom)
        {
            foreach (var player in combatRoom.CombatState.Players)
            {
                var cardModel = combatRoom.CombatState.CreateCard<MinorBeckon>(player);
                // generate in center of screen, then move to Discard
                await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Play, player);
                await CardPileCmd.Add(cardModel, PileType.Discard); 
            }
        }
    }
}