using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Utils;
using BossAuras.Constants;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Encounters.Mocks;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Nodes;

// TODO:
// The game often exposes a static Instance of UI containers.
// We could do the same, since realistically, there will only ever be one NMapAuraContainer.
// Simply use _Create and _ExitTree to set/remove itself from the Instance.

public partial class NMapAuraContainer : Control
{
    // Map is persistent throughout the run! Once loaded it stays. So this should realistically only ever have one entry.
    private static readonly AddedNode<NMapScreen, NMapAuraContainer> MapAuraContainerNodes = new(mapScreen =>
    {
        var nMapBossAuras = Create();
        nMapBossAuras._myMapScreen = mapScreen;
        mapScreen.AddChildSafely(nMapBossAuras);
        mapScreen.MoveChild(nMapBossAuras, mapScreen.GetNode("%MapLegend").GetIndex());
        nMapBossAuras.Position = new  Vector2(0.0f, 258.0f); // 1518
        return nMapBossAuras;
    });

    /// Get the <see cref="NMapAuraContainer"/> instance attached to the <see cref="NMapScreen"/>.
    internal static NMapAuraContainer GetFromMapScreen(NMapScreen mapScreen) => MapAuraContainerNodes.Get(mapScreen);
    
    /// The  <see cref="NMapScreen"/> I am attached to.
    private NMapScreen _myMapScreen = null!;
    
    public static NMapAuraContainer Create()
    {
        var nMapBossAuras = PreloadManager.Cache.GetScene("res://BossAuras/scenes/map_aura_container.tscn").Instantiate<NMapAuraContainer>();
        return nMapBossAuras;
    }

    [Export] public float MaxAuraListHeight = 400f;
    
    private CanvasItem _background = null!;
    private MegaLabel _title = null!;
    private VBoxContainer _aurasContainer = null!;
    private ScrollContainer _auraScroll = null!;
    private readonly List<NMapAura> _nMapAuras = []; // not sure if we need it
    
    public override void _Ready()
    {
        Entry.Logger.Info("NMapAuras._Ready()");
        _background = GetNode<CanvasItem>("%Bg");
        _title = GetNode<MegaLabel>("%Title");
        _auraScroll = GetNode<ScrollContainer>("%AuraScroll");
        _aurasContainer = GetNode<VBoxContainer>("%Auras");
        _aurasContainer.MinimumSizeChanged += UpdateScrollHeight;
        Reload();
    }

    public override void _ExitTree()
    {
        _aurasContainer.MinimumSizeChanged -= UpdateScrollHeight;
    }

    private void UpdateScrollHeight()
    {
        var contentHeight = _aurasContainer.GetCombinedMinimumSize().Y;
        _auraScroll.CustomMinimumSize = new Vector2(0, Mathf.Min(contentHeight, MaxAuraListHeight));
        Callable.From(ResetSize).CallDeferred();
    }
    
    /// <summary>
    /// Clears and rebuilds the entire container. This is not meant to be called when individual auras are added or removed!
    /// </summary>
    public void Reload()
    {
        Entry.Logger.Info("Reloading NMapAuraContainer");
        _nMapAuras.Clear();
        foreach (var child in _aurasContainer.GetChildren())
            child.QueueFreeSafely();
        
        foreach (var auraModel in GameHelper.GetRunState().Auras)
            AddAuraInternal(auraModel);
        Entry.Logger.Info($"Done Reloading NMapAuraContainer. Total of {_nMapAuras.Count} Auras!");
        UpdateVisibility();
    }

    public void AddAura(AuraModel aura)
    {
        AddAuraInternal(aura);
        UpdateVisibility();
    }

    private void AddAuraInternal(AuraModel aura)
    {
        Entry.Logger.Info($"Aura: {aura.Id.Entry}");
        var roomIconPath = ImageHelper.GetRoomIconPath(MapPointType.Boss, RoomType.Boss, ModelDb.GetId<CeremonialBeastBoss>());
        var nMapAura = NMapAura.Create(aura.Title, aura.DynamicDescription, roomIconPath!, aura);
        _nMapAuras.Add(nMapAura);
        _aurasContainer.AddChildSafely(nMapAura);
    }

    public void RemoveAura(AuraModel aura)
    {
        var nMapAura = _nMapAuras.FirstOrDefault(a => a.Model == aura);
        if (nMapAura is null) return;
        _nMapAuras.Remove(nMapAura);
        nMapAura.QueueFreeSafely(); 
        //Callable.From(ResetSize).CallDeferred();
    }
    
    private void UpdateVisibility() => Visible = _nMapAuras.Count != 0;
    
    // TODO: Create a playable card that Purges and creates a new Aura, as a test on how to handle auras appearing at any point (and handle duplicates)
    // TODO: Also add new console command "aura AURA_ID"
    
    
    
    #region Patches
    
    [HarmonyPatch]
    private static class Tweening
    {
        static Tweening()
        {
            if (TweenField is null) throw new NullReferenceException("TweenField is null");
        }

        private static readonly FieldInfo TweenField = AccessTools.Field(typeof(NMapScreen), "_tween");
        
        
        [HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.Open))]
        [HarmonyPostfix]
        private static void TweenBossAuraUiIntoView(NMapScreen __instance)
        {
            var tween = (Tween)TweenField.GetValue(__instance)!;
            var nBossAuras = MapAuraContainerNodes.Get(__instance);
            var bossAurasX = __instance.Size.X * 0.0f + 10f;
            tween.TweenProperty(nBossAuras, "modulate", Colors.White, 0.25).SetDelay(0.1);
            tween.TweenProperty(nBossAuras, "position:x", bossAurasX, 0.25)
                        .From(bossAurasX - 120f).SetEase(Tween.EaseType.Out);
        }
        
        
        [HarmonyPatch(typeof(NMapScreen), "AnimClose", MethodType.Async)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var addTweenMethod = typeof(Tweening).Method(nameof(AddTween))
                                ?? throw new InvalidOperationException("IterateHook not found");
            var matcher = new CodeMatcher(instructions)
                        .MatchStartForward([
                                    new CodeMatch(OpCodes.Ldloc_1),
                                    new CodeMatch(OpCodes.Ldfld),
                                    new CodeMatch(OpCodes.Ldloc_1),
                                    new CodeMatch(OpCodes.Ldfld),
                        ]);


            matcher.Advance();
            var ldfld_tween = matcher.Instruction;
            matcher.Advance(-1);
            
            matcher.Insert([
                                    new CodeInstruction(OpCodes.Ldloc_1),
                                    ldfld_tween,
                                    new CodeInstruction(OpCodes.Ldloc_1),
                                    new CodeInstruction(OpCodes.Call, addTweenMethod),
                        ]);
            return matcher.InstructionEnumeration();
        }
        
        private static void AddTween(Tween tween, NMapScreen __instance)
        {
            var nBossAuras = MapAuraContainerNodes.Get(__instance);
            var bossAurasX = __instance.Size.X * 0.0f + 10f;
            tween.TweenProperty(nBossAuras, "modulate:a", 0f, 0.15);
            tween.TweenProperty(nBossAuras, "position:x", bossAurasX - 120f, 0.25)
                        .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        }
    }
    
    
    #endregion Patches
}