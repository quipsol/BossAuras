using BossAuras.Core.Helper;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace BossAuras.Core.Nodes.Auras;

public partial class NUnstableCoreUi : Control
{
    private const string SCENE_PATH = "res://BossAuras/scenes/auras/unstable_core_ui.tscn";
    private Node2D _intentPos = null!;


    public static NUnstableCoreUi Create()
    {
        var nUnstableCoreUi = PreloadManager.Cache.GetScene(SCENE_PATH).Instantiate<NUnstableCoreUi>();
        return nUnstableCoreUi;
    }
    
    public override void _Ready()
    {
        _intentPos = GetNode<Node2D>("%IntentPosition");
        GlobalPosition = NCombatRoom.Instance is not null ? 
                    GlobalPositionHelper.ScreenPosition(NCombatRoom.Instance, new Vector2(0.5f, 0.3f)) :
                    GlobalPositionHelper.GetCenter;
    }

    public void SetIntent(NIntent intent)
    {
        foreach (var child in _intentPos.GetChildren())
            child.QueueFreeSafely();
        _intentPos.AddChildSafely(intent);
    }
}