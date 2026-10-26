using BossAuras.Core.Models;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;

namespace BossAuras.Core.Nodes;

public partial class NAuraFlashCreatureVfx : Node2D
{
    private const string SCENE_PATH = "res://BossAuras/scenes/vfx/aura_flash_creature_vfx.tscn";
    
    private Sprite2D _sprite = null!;
    private AuraModel _aura = null!;
    private NCreature? _nCreature = null;
    private Tween? _spriteTween;
    public static IEnumerable<string> AssetPaths => [SCENE_PATH];
    
    
    public static NAuraFlashCreatureVfx? Create(AuraModel aura, NCreature nCreature)
    {
        if (TestMode.IsOn) return null;
        var vfx = (NAuraFlashCreatureVfx)PreloadManager.Cache.GetScene(SCENE_PATH).Instantiate();
        vfx._aura = aura;
        vfx._nCreature = nCreature;
        return vfx;
    }
    
    
    public override void _Ready()
    {
        if (_nCreature is null)
        {
            this.QueueFreeSafely();
            return;
        }
        _sprite = GetNode<Sprite2D>("Sprite2D");
        GlobalPosition = _nCreature.VfxSpawnPosition;
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
        _spriteTween.TweenProperty(_sprite, "scale", Vector2.One * 0.4f, 0.4);
        _spriteTween.TweenProperty(_sprite, "modulate:a", 1, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        _spriteTween.SetParallel(parallel: false);
        _spriteTween.TweenProperty(_sprite, "scale", Vector2.One * 0.45f, 0.4);
        _spriteTween.TweenProperty(_sprite, "modulate:a", 0, 0.25).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
        await _spriteTween.AwaitFinished(this);
        this.QueueFreeSafely();
    }

 
    
}