using BossAuras.Core.Helper;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.HoverTips;

public class MonsterHoverTip : IHoverTip
{
    public AbstractModel Model;

    public string Id => Model.Id.ToString();
    public bool IsSmart => false;
    public bool IsDebuff => false;
    public bool IsInstanced => false;
    public AbstractModel CanonicalModel => ModelDb.GetById<AbstractModel>(Model.Id);

    public MonsterHoverTip(MonsterModel monster)
    {
        Model = monster;
    }
    public MonsterHoverTip(CharacterModel character)
    {
        Model = character;
    }
}