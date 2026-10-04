using System.Reflection;
using BaseLib.Utils;
using BossAuras.Core.HoverTips;
using BossAuras.Core.Nodes.HoverTips;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace BossAuras.Patches.HoverTips;

[HarmonyPatch]
public static class MonsterHoverTipPatch
{
    private static readonly StringName MonsterHoverTipContainerStr = new StringName("monsterHoverTipContainer");
    private static readonly AddedNode<NHoverTipSet, NHoverTipMonsterContainer> HoverTipMonsterContainers = new(hoverTipSet =>
    {
        var retVal = new NHoverTipMonsterContainer();
        retVal.Name = MonsterHoverTipContainerStr;
        retVal.MouseFilter = Control.MouseFilterEnum.Ignore;
        hoverTipSet.AddChildSafely(retVal);
        return retVal;
    });

    /// Handle MonsterHoverTips here and remove them from the list passed to the method to prevent errors.
    [HarmonyPatch(typeof(NHoverTipSet), "Init")]
    [HarmonyPrefix]
    private static void HandleMonsterHoverTips(NHoverTipSet __instance, Control owner, ref IEnumerable<IHoverTip> hoverTips)
    {
        var all = hoverTips.ToList();
        var monsterTips = IHoverTip.RemoveDupes(all).OfType<MonsterHoverTip>().ToList();
        if (monsterTips.Count == 0) return;
        var container = HoverTipMonsterContainers.Get(__instance);
        foreach (var tip in monsterTips) container.Add(tip);
        hoverTips = all.Where(t => t is not MonsterHoverTip).ToList();
    }
    
    private static readonly AccessTools.FieldRef<NHoverTipSet, NHoverTipCardContainer> CardContainer =
                AccessTools.FieldRefAccess<NHoverTipSet, NHoverTipCardContainer>("_cardHoverTipContainer");
    private static readonly AccessTools.FieldRef<NHoverTipSet, VFlowContainer> TextContainer =
                AccessTools.FieldRefAccess<NHoverTipSet, VFlowContainer>("_textHoverTipContainer");

    
    [HarmonyPatch(typeof(NHoverTipSet), nameof(NHoverTipSet.SetAlignment))]
    [HarmonyPostfix]
    private static void HandleMonsterHoverTipsAlignment(NHoverTipSet __instance, Control node, HoverTipAlignment alignment)
    {
        var monsters = __instance.GetNodeOrNull<NHoverTipMonsterContainer>(MonsterHoverTipContainerStr.ToString());
        if (monsters is null || alignment == HoverTipAlignment.None) return;
        var text = TextContainer(__instance);

        // Below the text tips, same column. text.GlobalPosition is already overflow-corrected by vanilla.
        var start = text.GlobalPosition + Vector2.Down * (text.Size.Y + 5f);
        if (alignment == HoverTipAlignment.Left)
        {
            // Text column sits left of the node → line up our right edge with the text column's right edge
            start += Vector2.Right * text.Size.X;
            monsters.LayoutResizeAndReposition(start, HoverTipAlignment.Left);
        }
        else
        {
            monsters.LayoutResizeAndReposition(start, HoverTipAlignment.Right);
        }

        ClampToViewport(monsters);
    }
    
    private static void ClampToViewport(Control c)
    {
        var vp = NGame.Instance!.GetViewportRect().Size;
        var p = c.GlobalPosition;
        p.X = Mathf.Clamp(p.X, 0f, Mathf.Max(0f, vp.X - c.Size.X));
        p.Y = Mathf.Clamp(p.Y, 0f, Mathf.Max(0f, vp.Y - c.Size.Y));
        c.GlobalPosition = p;
    }
}