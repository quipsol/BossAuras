using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BossAuras.Patches.MapMarker;

internal static class MapMarkerHandler
{
    private static readonly SpireField<NNormalMapPoint, List<Control>> CustomMarkers = new(() => []);
    
    internal static List<Control> GetCustomMarkers(NNormalMapPoint mapPoint) => CustomMarkers.Get(mapPoint)!;

    
    internal static void AddNodeToMapPoint(NNormalMapPoint mapPoint, Control control)
    {
        GetCustomMarkers(mapPoint).Add(control);
    }
    
    internal static void ClearAllNodesFromMapPoint(NNormalMapPoint mapPoint)
    {
        var controls = GetCustomMarkers(mapPoint);
        foreach (var control in controls)
        {
            control.QueueFreeSafely();
        }
        controls.Clear();
    }
}