using BossAuras.Constants;
using BossAuras.Core.Models.Auras;
using BossAuras.Core.Nodes.Vfx;
using BossAuras.Core.Saves;
using BossAuras.Core.Saves.Runs;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Models;

// TODO:
// Flash(): Something that signals the player "hey, the aura just did something" to prevent confusion
// Potentially have multiple Flash options.
// Flash during open map
// Flash during combat (general)
// Flash during combat (on creature)

// TODO: Go through all auras and replace nieche rng calls with the aura specific Rng.

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
            description.Add(LocKey.ENERGY_PREFIX, LocKey.COLORLESS_ENERGY);
            description.Add(LocKey.SINGLE_STAR_ICON, ResPath.SINGLE_STAR_ICON_IMG_PATH);
            foreach (var variable in description.Variables)
                if (variable.Value is EnergyVar energyVar) energyVar.ColorPrefix = LocKey.COLORLESS_ENERGY;
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
    /// Aura specific Rng
    public Rng Rng { get; private set; } = null!;
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
    
    
    public event Action? DisplayAmountChanged;
    public event Action? Flashed;
    public event Action<AuraModel>? FlashedRoom;
    public event Action<AuraModel>? FlashedCombat;
    public event Action<AuraModel, IEnumerable<Creature>>? FlashedCreatures;


    protected void InvokeDisplayAmountChanged() => DisplayAmountChanged?.Invoke();
    
    // They should all trigger the normal Flash too, in case the map was open as it happened!?
    
    // When to flash? It doesn't make sense to always flash. Lure always adds the card into your discard pile.
    // This will always happen, and you always see the card being added.
    // Flashing here would be completely unnecessary.
    // In general, I should later unify flashing and set up some guidelines when I flash and what I flash.
    // Aggressive siphon takes from all creatures every turn. Should it flash at all? (yes)
    // Should it flash on every creature or just a generic combat room flash?
    
    // Flash() doesnt do anything yet because uhm..... why should it? I'm glad Flash is working at all, but the only two I need
    // for a first step QOL are creature and combat flash. (and honestly, just creature is enough too for now)
    
    /// Flash the map display
    public void Flash() => Flashed?.Invoke();
    /// Flash visible from inside a room
    public void FlashRoom() => FlashedRoom?.Invoke(this);

    /// Flash specifically in a combat room
    public void FlashCombat()
    {
        // This is old code. I don't think I will need this as there is only ever one combat.
        // No reason to fire an event in that case when I can just do it here directly.
        FlashedCombat?.Invoke(this);
        
        NCombatRoom.Instance?.AddChildSafely(NAuraFlashCombatVfx.Create(this));
    }
    
    /// Flash on specific creatures in a combat
    public void FlashCreatures(IEnumerable<Creature> creatures) => FlashedCreatures?.Invoke(this, creatures);
    public void FlashCreatures(Creature creature) => FlashedCreatures?.Invoke(this, [creature]);
    
    // Creatures subscribe during enterTree and unsubscribes during ExitTree
    
    
    
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
                    Rng = Rng.ToSerializable(),
                    Props = SavedProperties.From(this),
        };
    }
    
    /// <summary>
    /// Create an AuraModel from a SerializableAura.
    /// </summary>
    public static AuraModel FromSerializable(SerializableAura save)
    {
        var auraModel = AuraSaveUtil.AuraOrDeprecated(save.Id!).ToMutable();
        // TODO: if check to guard against loading previous version without rng. can be removed soon.
        if (save.Rng != null) auraModel.Rng = new Rng(save.Rng!);
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
        DisplayAmountChanged = null;
        Flashed = null;
        FlashedCombat = null;
        FlashedCreatures = null;
    }

    /// <summary>
    /// NEVER CALL THIS!
    /// Only AuraCmd.Add and save/load stuff should be calling this.
    /// </summary>
    public void AddInternal(IRunState runState)
    {
        RunState = runState;
        // Multiple of the same aura have the same seed! Fine for now because you never have the same Aura multiple times.
        Rng ??= new Rng(RunState.Rng.Seed + StringHelper.GetDeterministicHashCode(Id.Entry));
    }

    /// NEVER CALL THIS! <br/> Allows an Aura to set up fields in itself it requires to prevent crashing. Called from AuraConsoleCmd only!
    public virtual void DebugOnlySetup() { }

    #region Model specific Hooks
    
    /// Extra logic that should be run after deserializing.
    protected virtual void AfterDeserialized() { }
    /// Called right after aura is added to run.
    public virtual Task AfterApplied() => Task.CompletedTask;
    /// Called right before aura is removed from run.
    public virtual Task BeforeRemoval() => Task.CompletedTask;
    
    #endregion Model specific Hooks
}