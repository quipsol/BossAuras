using System.Reflection;
using BaseLib.Utils;
using BossAuras.Core.HoverTips;
using BossAuras.Core.Nodes.HoverTips;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;

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

    private const float TIP_GAP = 5f;
    
    [HarmonyPatch(typeof(NHoverTipSet), nameof(NHoverTipSet.SetAlignment))]
    [HarmonyPostfix]
    private static void AfterSetAlignment(NHoverTipSet __instance, Control node, HoverTipAlignment alignment)
    {
        // None = the caller positions things itself afterwards (card holders, relics)
        if (alignment == HoverTipAlignment.None) return;
        PlaceMonsterTips(__instance, node);
    }

    [HarmonyPatch(typeof(NHoverTipSet), nameof(NHoverTipSet.SetAlignmentForCardHolder))]
    [HarmonyPostfix]
    private static void AfterSetAlignmentForCardHolder(NHoverTipSet __instance, NCardHolder holder)
    {
        PlaceMonsterTips(__instance, holder.Hitbox);
    }
    
    [HarmonyPatch(typeof(NHoverTipSet), nameof(NHoverTipSet.SetAlignmentForRelic))]
    [HarmonyPostfix]
    private static void AfterSetAlignmentForRelic(NHoverTipSet __instance, NRelic relic)
    {
        PlaceMonsterTips(__instance, relic.Icon);
    }
    
    
    /// Runs after vanilla has finished positioning so we can append easily. Might (will) break if other mods added HoverTip Types.
    private static void PlaceMonsterTips(NHoverTipSet set, Control anchor)
    {
    var monsters = set.GetNodeOrNull<NHoverTipMonsterContainer>(MonsterHoverTipContainerStr.ToString());
    if (monsters is null) return;

    var text = TextContainer(set);
    var cards = CardContainer(set);
    var viewport = new Rect2(Vector2.Zero, NGame.Instance!.GetViewportRect().Size);

    // Layour resizing without positioning.
    monsters.LayoutResizeAndReposition(monsters.GlobalPosition, HoverTipAlignment.None);
    var size = monsters.Size;

    // Everything vanilla placed that we must not cover
    var anchorRect = anchor.GetGlobalRect();
    var textRect = text.GetGlobalRect();
    var obstacles = new List<Rect2> { anchorRect };
    if (textRect.HasArea()) obstacles.Add(textRect);
    if (cards.GetChildCount() > 0) obstacles.Add(cards.GetGlobalRect());

    // Column = the text column. Align to its edge facing away from the anchor
    var columnIsLeft = textRect.GetCenter().X < anchorRect.GetCenter().X;
    var colX = columnIsLeft ? textRect.End.X - size.X : textRect.Position.X;
    
    
    var stackBottom = textRect.End.Y;
    foreach (var r in obstacles)
        if (r.Position.X < colX + size.X && colX < r.End.X)
            stackBottom = Mathf.Max(stackBottom, r.End.Y);

    Vector2[] candidates = [
                new(colX, stackBottom + TIP_GAP),                            // below the stack in our column
                new(colX, textRect.Position.Y - TIP_GAP - size.Y),           // above the text column
                new(columnIsLeft ? textRect.Position.X - TIP_GAP - size.X    // beside the column, away from anchor
                            : textRect.End.X + TIP_GAP, textRect.Position.Y)
    ];

    foreach (var pos in candidates)
    {
        var rect = new Rect2(pos, size);
        if (!viewport.Encloses(rect) || obstacles.Any(o => o.Intersects(rect))) continue;
        monsters.GlobalPosition = pos;
        return;
    }

    // Nothing fits cleanly → first candidate, pushed on-screen
    monsters.GlobalPosition = candidates[0];
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