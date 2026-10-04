using BossAuras.Core.HoverTips;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace BossAuras.Core.Nodes.HoverTips;

public partial class NHoverTipMonsterContainer : Control
{
    private IEnumerable<Control> Tips => GetChildren().OfType<Control>();

    public void Add(MonsterHoverTip monsterTip)
    {
        var monsterHoverTip = PreloadManager.Cache.GetScene("res://BossAuras/scenes/ui/monster_hover_tip.tscn").Instantiate<NMonsterHoverTip>(PackedScene.GenEditState.Disabled);
        this.AddChildSafely(monsterHoverTip);
        monsterHoverTip.SetDisplayMonster(monsterTip.Monster);
    }
    
    
    /// <summary>
    /// Lays out monsters vertically, then horizontally, then sets the position and the size of the container according
    /// to the passed global start position and alignment.
    /// </summary>
    /// <param name="globalStartLocation">Where to start positioning nodes.</param>
    /// <param name="alignment">Which side of the global start location the monsters should be placed on.</param>
    /// <returns></returns>
    public void LayoutResizeAndReposition(Vector2 globalStartLocation, HoverTipAlignment alignment)
    {
        var size = NGame.Instance!.GetViewportRect().Size;
        var size2 = Vector2.Zero;
        var zero = Vector2.Zero;
        var b = 0f;
        foreach (var tip in Tips)
        {
            tip.Position = zero;
            size2 = new Vector2(Mathf.Max(zero.X + tip.Size.X, size2.X), Mathf.Max(zero.Y + tip.Size.Y, size2.Y));
            zero += Vector2.Down * (tip.Size.Y + 4f);
            b = Mathf.Max(tip.Size.X, b);
        }

        GlobalPosition = alignment switch
        {
                    HoverTipAlignment.Right => globalStartLocation,
                    HoverTipAlignment.Left => globalStartLocation + Vector2.Left * size2.X,
                    _ => GlobalPosition
        };
        Size = size2;
    }
}