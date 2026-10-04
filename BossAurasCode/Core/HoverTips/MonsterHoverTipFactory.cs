using BossAuras.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.HoverTips;

public static class MonsterHoverTipFactory
{
    public static IHoverTip Create<T>() where T : MonsterModel
    {
        return Create(ModelDb.Monster<T>());
    }
    public static IHoverTip Create(MonsterModel monster)
    {
        return new MonsterHoverTip(monster);
    }
}