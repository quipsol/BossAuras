using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.HoverTips;

public class MonsterHoverTip : IHoverTip
{
    public MonsterModel Monster;

    public string Id => Monster.Id.ToString();
    public bool IsSmart => false;
    public bool IsDebuff => false;
    public bool IsInstanced => false;
    public AbstractModel? CanonicalModel => Monster.CanonicalInstance;

    public MonsterHoverTip(MonsterModel monster)
    {
        Monster = monster;
    }
}