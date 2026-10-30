using BossAuras.Core.Models;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BossAuras.Patches.MapMarker;

[HarmonyPatch]
public class QuestProviderPatch
{
    private const string SCENE_PATH = "res://BossAuras/scenes/ui/generic_map_marker.tscn";
    private const string REFRESH_MARKER_ICONS = "RefreshMarkedIconVisibility";
    private const string ANIM_HOVER = "AnimHover";
    private const string ANIM_UNHOVER = "AnimUnhover";
    private const string ANIM_PRESS_DOWN = "AnimPressDown";
    
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(NNormalMapPoint), REFRESH_MARKER_ICONS)]
    [HarmonyPrefix]
    private static bool HandleQuestProviderInterface(NNormalMapPoint __instance, Control ____iconContainer)
    {
        var questProviderCounter = 0;
        // clear all stored icons
        MapMarkerHandler.ClearAllNodesFromMapPoint(__instance);
        foreach (var abstractModel in __instance.Point.Quests)
        {
            if (abstractModel is not IQuestProvider questProvider) continue;
            questProviderCounter++;
            var path = questProvider.MarkerPath;
            var texture = ResourceLoader.Load<Texture2D>(path);
            var textureRect = PreloadManager.Cache.GetScene(SCENE_PATH).Instantiate<TextureRect>();
            textureRect.Texture = texture;
            textureRect.PivotOffset = 0.5f * textureRect.Size;
            ____iconContainer.AddChildSafely(textureRect);
            MapMarkerHandler.AddNodeToMapPoint(__instance, textureRect);
        }
        // if all quests where IQuestProvider, return
        return questProviderCounter != __instance.Point.Quests.Count;
    }
    
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(NNormalMapPoint), nameof(NNormalMapPoint.OnSelected))]
    [HarmonyPostfix]
    private static void TweenFixesOnSelected(NNormalMapPoint __instance, Tween ____tween)
    {
        var markers = MapMarkerHandler.GetCustomMarkers(__instance);
        foreach (var marker in markers)
        { 
            ____tween.TweenProperty(marker, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        }
    }
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(NNormalMapPoint), ANIM_HOVER)]
    [HarmonyPostfix]
    private static void TweenFixesAnimHover(NNormalMapPoint __instance,  Tween ____tween)
    {
        var markers = MapMarkerHandler.GetCustomMarkers(__instance);
        foreach (var marker in markers)
        { 
            ____tween.TweenProperty(marker, "scale", Vector2.One * 1.45f, 0.05);
        }
    }
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(NNormalMapPoint), ANIM_UNHOVER)]
    [HarmonyPostfix]
    private static void TweenFixesAnimUnhover(NNormalMapPoint __instance, Tween ____tween)
    {
        var markers = MapMarkerHandler.GetCustomMarkers(__instance);
        foreach (var marker in markers)
        { 
            ____tween.TweenProperty(marker, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        }
    }
    
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(NNormalMapPoint), ANIM_PRESS_DOWN)]
    [HarmonyPostfix]
    private static void TweenFixesAnimPressDown(NNormalMapPoint __instance, Tween ____tween)
    {
        var markers = MapMarkerHandler.GetCustomMarkers(__instance);
        foreach (var marker in markers)
        { 
            ____tween.TweenProperty(marker, "scale", Vector2.One * 0.9f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        }
    }
}