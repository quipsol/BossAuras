using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace BossAuras.Core.Models;

public abstract class BossAurasCardModel(int cost, CardType cardType, CardRarity rarity, TargetType targetType) : CustomCardModel(cost, cardType, rarity, targetType)
{
    
}