using BossAuras.Core.Models;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace BossAuras.Core.Nodes;


// TODO: Allow for mouse hovering and showing hovertips!?
public partial class NMapAura : Control
{
    private CanvasItem _background = null!;
    private TextureRect _auraTexture = null!;
    private TextureRect _smallAuraTexture = null!; // optional
    private MegaLabel _title = null!;
    private MegaRichTextLabel _description = null!;


    private string Title { get; set; } = null!;
    private string Description { get; set; } = null!;
    private Texture2D AuraIcon { get; set; } = null!;
    private Texture2D? SmallAuraIcon { get; set; }
    public AuraModel Model { get; private set; } = null!;


    /// <summary>
    /// Creates an empty Node.
    /// </summary>
    /// <returns></returns>
    public static NMapAura Create()
    {
        var nMapBossAura = PreloadManager.Cache.GetScene("res://BossAuras/scenes/map_aura.tscn").Instantiate<NMapAura>();
        return nMapBossAura;
    }
    public static NMapAura Create(LocString  title, LocString description, string iconPath, AuraModel aura)
    {
        var nMapBossAura = PreloadManager.Cache.GetScene("res://BossAuras/scenes/map_aura.tscn").Instantiate<NMapAura>();
        nMapBossAura.Title = aura.Title.GetFormattedText();
        nMapBossAura.Description = aura.DynamicDescription.GetFormattedText();
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
        Reload();
    }

    public void Reload()
    {
        _title.SetTextAutoSize(Title);
        _description.SetTextAutoSize(Description);
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
    
}