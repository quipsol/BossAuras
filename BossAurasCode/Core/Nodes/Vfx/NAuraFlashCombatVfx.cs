using System.Diagnostics;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using BossAuras.Core.Nodes.Vfx;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using TaskHelper = MegaCrit.Sts2.Core.Helpers.TaskHelper;

namespace BossAuras.Core.Nodes.Vfx;

public partial class NAuraFlashCombatVfx : Node2D
{
    private const string SCENE_PATH = "res://BossAuras/scenes/vfx/aura_flash_combat_vfx.tscn";
    
    private Sprite2D _sprite = null!;
    private AuraModel _aura = null!;
    private Tween? _spriteTween;
    public static IEnumerable<string> AssetPaths => [SCENE_PATH];
    
    
    public static NAuraFlashCombatVfx? Create(AuraModel aura)
    {
        if (TestMode.IsOn) return null;
        var vfx = (NAuraFlashCombatVfx)PreloadManager.Cache.GetScene(SCENE_PATH).Instantiate();
        vfx._aura = aura;
        return vfx;
    }
    
    
    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        // How to find the center of display?
        // The games "Turn X" counter etc. are just full rects.
        GlobalPosition = NCombatRoom.Instance is not null ? 
                    GlobalPositionHelper.ScreenPosition(NCombatRoom.Instance, new Vector2(0.5f, 0.3f)) :
                    GlobalPositionHelper.GetCenter;
        TaskHelper.RunSafely(StartVfx());
    }

    public override void _ExitTree()
    {
        _spriteTween?.Kill();
    }

    private async Task StartVfx()
    {
        _sprite.Texture = _aura.Icon;
        _sprite.Modulate = Colors.White;
        _spriteTween = CreateTween();
        _spriteTween.SetParallel();
        _spriteTween.TweenProperty(_sprite, "scale", Vector2.One * 0.4f, 0.8);
        _spriteTween.TweenProperty(_sprite, "modulate:a", 1, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        _spriteTween.SetParallel(parallel: false);
        _spriteTween.TweenProperty(_sprite, "scale", Vector2.One * 0.45f, 0.8);
        _spriteTween.TweenProperty(_sprite, "modulate:a", 0, 0.25).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
        await _spriteTween.AwaitFinished(this);
        this.QueueFreeSafely();
    }

 
    
}