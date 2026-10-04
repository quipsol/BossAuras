using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace BossAuras.Core.Nodes.HoverTips;

public partial class NMonsterHoverTip: MarginContainer
{
    private const float FIT_PADDING = 0.8f;
    
    private NinePatchRect _background = null!;
    private MegaLabel _title = null!;
    private Control _monsterContainer = null!;
    
    public override void _Ready()
    {
        Entry.Logger.Info("Entering NHoverTipMonsterContainer._Ready()");
        _background = GetNode<NinePatchRect>("%Bg");
        _title = GetNode<MegaLabel>("%Title");
        _monsterContainer = GetNode<Control>("%MonsterContainer");
    }

    public void SetDisplayMonster(MonsterModel monster)
    {
        Entry.Logger.Info("Entering NHoverTipMonsterContainer.SetDisplayCreature()");
        _title.Text = monster.Title.GetFormattedText(); 
        _monsterContainer.FreeChildren();
        var nCreature = monster.CreateVisuals();
        _monsterContainer.AddChildSafely(nCreature);
        FitToContainer(nCreature);

    }
    
    private void FitToContainer(NCreatureVisuals visuals)
    {
        var area = _monsterContainer.CustomMinimumSize;
        var bounds = visuals.Bounds;
        var toVisuals = visuals.GetGlobalTransform().AffineInverse() * bounds.GetGlobalTransform();
        var rect = new Rect2(toVisuals.Origin, bounds.Size * toVisuals.Scale);

        if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
        {
            visuals.Position = area / 2f;
            return;
        }

        var scale = Mathf.Min(area.X / rect.Size.X, area.Y / rect.Size.Y) * FIT_PADDING;
        visuals.Scale = Vector2.One * scale;
        visuals.Position = area / 2f - rect.GetCenter() * scale;
    }
}