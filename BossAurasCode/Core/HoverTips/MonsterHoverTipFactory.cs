using BossAuras.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.HoverTips;

public static class MonsterHoverTipFactory
{
    public static IHoverTip FromMonster<T>() where T : MonsterModel
    {
        return FromMonster(ModelDb.Monster<T>());
    }
    public static IHoverTip FromMonster(MonsterModel monster)
    {
        return new MonsterHoverTip(monster);
    }
    public static IHoverTip FromCharacter<T>() where T : CharacterModel
    {
        return FromCharacter(ModelDb.Character<T>());
    }
    public static IHoverTip FromCharacter(CharacterModel character)
    {
        return new MonsterHoverTip(character);
    }
}