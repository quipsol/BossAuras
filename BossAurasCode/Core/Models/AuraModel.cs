using System.Runtime.InteropServices;
using System.Text;
using BossAuras.Constants;
using BossAuras.Core.Models.Auras;
using BossAuras.Core.Saves;
using BossAuras.Core.Saves.Runs;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models;

// TODO:
// Flash(): Something that signals the player "hey, the aura just did something" to prevent confusion
public abstract class AuraModel : AbstractModel
{
    /// Name of LocTable
    internal const string AURAS = "auras";
    
    private DynamicVarSet? _dynamicVars;
    
 
    public virtual LocString Title => new (AURAS, $"{Entry.MOD_ID_PREFIX}-{Id.Entry}.title");
    private LocString Description => new (AURAS, $"{Entry.MOD_ID_PREFIX}-{Id.Entry}.description");
    public LocString DynamicDescription
    {
        get
        {
            var description = Description;
            DynamicVars.AddTo(description);
            // TODO: This causes issues because of Pools. (How do Relics get it? Oh right, they do have a pool)
            //var prefix = EnergyIconHelper.GetPrefix(this);
            //description.Add(LocKey.ENERGY_PREFIX, prefix);
            description.Add(LocKey.SINGLE_STAR_ICON, ResPath.SINGLE_STAR_ICON_IMG_PATH);
            //foreach (var variable in description.Variables)
            //    if (variable.Value is EnergyVar energyVar) energyVar.ColorPrefix = prefix;
            return description;
        }
    }
    protected LocString SelectionScreenPrompt // In case an aura wants to force a selection at any point (good example idea for the Act 2 Boss)
    {
        get
        {
            var locString = new LocString(AURAS, Id.Entry + ".selectionScreenPrompt");
            DynamicVars.AddTo(locString);
            return locString;
        }
    }

    protected virtual string IconBaseName => Id.Entry.ToLowerInvariant();
    protected virtual string IconPath => $"res://{Entry.MOD_ID}/images/auras/big/{IconBaseName}.png";
    protected virtual string SmallIconPath => $"res://{Entry.MOD_ID}/images/auras/small/{IconBaseName}.png";
    protected virtual string BetaIconPath => $"res://{Entry.MOD_ID}/images/auras/big/beta/{IconBaseName}.png";
    private static string MissingIconPath => $"res://{Entry.MOD_ID}/images/auras/missing_aura.png";

    public Texture2D Icon => ResourceLoader.Load<Texture2D>(ResolvedIconPath);
    public Texture2D? SmallIcon => ResourceLoader.Exists(SmallIconPath) ? ResourceLoader.Load<Texture2D>(SmallIconPath) : null;

    private string ResolvedIconPath
    {
        get
        {
            if (field != null) return field;
            if (ResourceLoader.Exists(IconPath)) field = IconPath;
            else if (ResourceLoader.Exists(BetaIconPath)) field = BetaIconPath;
            else field = MissingIconPath;
            return field;
        }
    }
    
    
    
    /// A reference to the active RunState
    protected IRunState RunState
    {
        get
        {
            AssertMutable();
            return field;
        }
        set
        {
            AssertMutable();
            if (field != null) throw new InvalidOperationException("Can not reassign the RunState. If this is a new run this object should have been cleared and recreated."); 
            field = value;
        }
    } = null!;
    
    
    
    public DynamicVarSet DynamicVars => _dynamicVars ??= InitDynamicVars(CanonicalVars, this);

    private static DynamicVarSet InitDynamicVars(IEnumerable<DynamicVar> canonicalVars, AuraModel model)
    {
        var dynamicVarSet = new DynamicVarSet(canonicalVars);
        dynamicVarSet.InitializeWithOwner(model);
        return dynamicVarSet;
    }
    
    protected virtual IEnumerable<DynamicVar> CanonicalVars => [];
    
    protected virtual IEnumerable<IHoverTip> ExtraHoverTips => [];
    
    public IEnumerable<IHoverTip> HoverTips
    {
        get
        {
            var list = new List<IHoverTip>();
            list.AddRange(ExtraHoverTips);
            return list;
        }
    }
    
    
    public AuraModel CanonicalInstance
    {
        get => IsMutable ? field : this;
        private set
        {
            AssertMutable();
            field = value;
        }
    } = null!; // Game Models also only set this in AfterCloned.
    
    
    public override bool ShouldReceiveCombatHooks => true;
    
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _dynamicVars = DynamicVars.Clone(this);
    }
    
    
    
    public AuraModel ToMutable()
    {
        AssertCanonical();
        return (AuraModel)MutableClone();
    }
    
    /// <summary>
    /// Turn an AuraModel into a SerializableAura.
    /// </summary>
    /// <returns></returns>
    public SerializableAura ToSerializable()
    {
        AssertMutable();
        return new SerializableAura
        { 
                    Id = Id,
                    Props = SavedProperties.From(this),
        };
    }
    
    /// <summary>
    /// Create an AuraModel from a SerializableAura.
    /// </summary>
    public static AuraModel FromSerializable(SerializableAura save)
    {
        var auraModel = AuraSaveUtil.AuraOrDeprecated(save.Id!).ToMutable();
        save.Props?.Fill(auraModel);
       
        auraModel.AfterDeserialized();
        if (auraModel is not DeprecatedAura)
        {
            // any relevant logic here
            // e.g. An Enchantment in CardModel
        }
        return auraModel;
    }


    protected override void AfterCloned()
    {
        base.AfterCloned();
        CanonicalInstance = ModelDb.GetById<AuraModel>(Id);
    }


    #region Model specific Hooks
    
    /// Extra logic that should be run after deserializing.
    protected virtual void AfterDeserialized() { }
    /// Called right after aura is added to run.
    public virtual Task AfterApplied() => Task.CompletedTask;
    /// Called right before aura is removed from run.
    public virtual Task BeforeRemoval() => Task.CompletedTask;
    
    #endregion Model specific Hooks
}