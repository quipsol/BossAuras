using BossAuras.Core.Models;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace BossAuras.Core.Nodes;


public partial class NMapAura : Control
{
    private const string SCENE_PATH = "res://BossAuras/scenes/ui/map_aura_rework.tscn";
    
    private CanvasItem _background = null!;
    private TextureRect _auraTexture = null!;
    private TextureRect _smallAuraTexture = null!; // optional
    private MegaLabel _title = null!;
    private MegaRichTextLabel _description = null!;


    private string Title { get; set; } = null!;
    private LocString Description { get; set; } = null!;
    private Texture2D AuraIcon { get; set; } = null!;
    private Texture2D? SmallAuraIcon { get; set; }
    public AuraModel Model { get; private set; } = null!;


    /// Creates an empty Node.
    public static NMapAura Create()
    {
        var nMapBossAura = PreloadManager.Cache.GetScene(SCENE_PATH).Instantiate<NMapAura>();
        return nMapBossAura;
    }
    public static NMapAura Create(AuraModel aura)
    {
        var nMapBossAura = PreloadManager.Cache.GetScene(SCENE_PATH).Instantiate<NMapAura>();
        nMapBossAura.Title = aura.Title.GetFormattedText();
        nMapBossAura.Description = aura.DynamicDescription;
        nMapBossAura.AuraIcon = aura.Icon;
        nMapBossAura.SmallAuraIcon = aura.SmallIcon;
        nMapBossAura.Model = aura;
        return nMapBossAura;
    }
    
    public override void _Ready()
    {
        _background = GetNode<CanvasItem>("%Bg");
        _title = GetNode<MegaLabel>("%Title");
        _description = GetNode<MegaRichTextLabel>("%Description");
        _auraTexture = GetNode<TextureRect>("%AuraIcon");
        _smallAuraTexture = GetNode<TextureRect>("%SmallAuraIcon");
        _description.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
        Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
        Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
        Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
        SubscribeToAuraEvents();
        Reload();
    }

    public override void _ExitTree()
    {
        UnsubscribeFromAuraEvents();
    }

    private void SubscribeToAuraEvents()
    {
        Model.DisplayAmountChanged += OnDisplayAmountChanged;
        Model.Flashed += Flash;
    }
    
    private void UnsubscribeFromAuraEvents()
    {
        Model.DisplayAmountChanged -= OnDisplayAmountChanged;
        Model.Flashed -= Flash;
    }
    
    public void Reload()
    {
        _title.SetTextAutoSize(Title);
        _description.SetTextAutoSize(Description.GetFormattedText());
        _auraTexture.Texture = AuraIcon;
        _smallAuraTexture.Texture = SmallAuraIcon;
    }

    private void OnFocus()
    {
        ShowHoverTips(Model.HoverTips);
    }

    private void OnUnfocus()
    {
        HideHoverTips();
    }
    
    public void ShowHoverTips(IEnumerable<IHoverTip> hoverTips)
    {
        HideHoverTips();
        NHoverTipSet.CreateAndShow(this, hoverTips, HoverTip.GetHoverTipAlignment(this, 0.5f));
    }
    
    private void HideHoverTips() => NHoverTipSet.Remove(this);
    
    private void OnDisplayAmountChanged()
    {
        Entry.Logger.Info("OnDisplayAmountChanged fired!");
        Reload();
    }

    private void Flash()
    {
        // Something build into the MapAura node that makes a sheen go over it or something
    }
}