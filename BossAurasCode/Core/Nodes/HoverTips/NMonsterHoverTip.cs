using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Nodes.HoverTips;

public partial class NMonsterHoverTip: MarginContainer
{
    private const float FIT_PADDING = 0.8f;
    
    private NinePatchRect _background = null!;
    private MegaLabel _title = null!;
    private Control _displayContainer = null!;
    
    public override void _Ready()
    {
        Entry.Logger.Info("Entering NHoverTipMonsterContainer._Ready()");
        _background = GetNode<NinePatchRect>("%Bg");
        _title = GetNode<MegaLabel>("%Title");
        _displayContainer = GetNode<Control>("%MonsterContainer");
    }

    public void SetDisplayCharacter(CharacterModel character)
    {
        if (!character.IsMutable) character = (CharacterModel)character.MutableClone();
        Entry.Logger.Info("Entering NHoverTipMonsterContainer.SetDisplayCharacter()");
        _title.Text = character.Title.GetFormattedText();
        _displayContainer.FreeChildren();
        AnimatedCharacterVisuals(character);
    }
    
    public void SetDisplayMonster(MonsterModel monster)
    {
        if (!monster.IsMutable) monster = monster.ToMutable();
        Entry.Logger.Info("Entering NHoverTipMonsterContainer.SetDisplayMonster()");
        _title.Text = monster.Title.GetFormattedText(); 
        _displayContainer.FreeChildren();
        
        AnimatedMonsterVisuals(monster);
    }

    private void AnimatedCharacterVisuals(CharacterModel character)
    {
        var nCreatureVisuals = character.CreateVisuals();
        _displayContainer.AddChildSafely(nCreatureVisuals);
        nCreatureVisuals.SpineAnimation.AddAnimation("idle_loop", loop: true);
        FitToContainer(nCreatureVisuals);
    }
    
    private void FlatMonsterVisuals(MonsterModel monster)
    {
        var nCreatureVisuals = monster.CreateVisuals();
        _displayContainer.AddChildSafely(nCreatureVisuals);
        nCreatureVisuals.SpineBody?.GetAnimationState().SetAnimation("idle_loop", loop: true); // Try to animate it, but may fail
        FitToContainer(nCreatureVisuals);
    }

    private void AnimatedMonsterVisuals(MonsterModel monster)
    {
        monster.Rng = Rng.Chaotic;
        monster.RunRng = new RunRngSet(Rng.Chaotic);
        monster.SetUpForCombat();
        var entity = new Creature(monster, CombatSide.Enemy, null)
        {
                    CombatState = NullCombatState.Instance
        };
        var creature = NCreature.Create(entity);
        if (creature is null) return;
        _displayContainer.AddChildSafely(creature);
        creature.SetupForBestiary();
        FitToContainer(creature);
    }


    // We need two methods either way, because NCreature is a Control and NCreatureVisuals is a Node2D.
    // Both inherit from CanvasItem, but CanvasItem does not have Position and Scale, those are added by the others respectively.
    private void FitToContainer(NCreature creature)
    {
        var area = _displayContainer.CustomMinimumSize;
        var bounds = creature.Visuals.Bounds;
        var toVisuals = creature.GetGlobalTransform().AffineInverse() * bounds.GetGlobalTransform();
        var rect = new Rect2(toVisuals.Origin, bounds.Size * toVisuals.Scale);

        if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
        {
            creature.Position = area / 2f;
            return;
        }
        var scale = Mathf.Min(area.X / rect.Size.X, area.Y / rect.Size.Y) * FIT_PADDING;
        creature.Scale = Vector2.One * scale;
        creature.Position = area / 2f - rect.GetCenter() * scale;
    }
    
    private void FitToContainer(NCreatureVisuals visuals)
    {
        var area = _displayContainer.CustomMinimumSize;
        var bounds = visuals.Bounds;
        var toVisuals = visuals.GetGlobalTransform().AffineInverse() * bounds.GetGlobalTransform();
        var rect = new Rect2(toVisuals.Origin, bounds.Size * toVisuals.Scale);

        if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
        {
            visuals.Position = area / 2f;
            return;
        }

        var scale = Mathf.Min(area.X / rect.Size.X, area.Y / rect.Size.Y) * FIT_PADDING;
        visuals.Scale = Vector2.One * scale;
        visuals.Position = area / 2f - rect.GetCenter() * scale;
    }
}