using Godot;

namespace BossAuras.Core.Helper;

public static class GlobalPositionHelper
{
    /// <summary>
    /// Get the screen position through a Control based on a percentage value from the upper left to lower right corner.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public static Vector2 ScreenPosition(Control reference, Vector2 percentage)
    {
        var rect = reference.GetViewport().GetVisibleRect();
        var viewportPoint = rect.Position + rect.Size * percentage; 
        // get the position based on percentage from upper left to bottom right. 0.5/0.5 would be the center
        return reference.GetCanvasTransform().AffineInverse() * viewportPoint;
    }

    /// Center of the visible area in viewport coordinates (1920x1080-based logical units).
    public static Vector2 GetCenter => ((SceneTree)Engine.GetMainLoop()).Root.GetViewport().GetVisibleRect().GetCenter();
    
    /// Center of the view converted into the given node's global (canvas) coordinates.
    public static Vector2 GlobalViewCenterFor(CanvasItem node)
    {
        var center = node.GetViewport().GetVisibleRect().GetCenter();
        return node.GetCanvasTransform().AffineInverse() * center;
    }
    
    /// Positions a Control so that its center sits at the center of the view.
    public static void CenterOnView(Control control)
        => control.GlobalPosition = GlobalViewCenterFor(control) - control.Size * 0.5f;
    
}