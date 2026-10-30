using BaseLib.Utils;
using BossAuras.Core.Extensions;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using BossAuras.Core.Nodes;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Handler;

[HarmonyPatch]
public class CreatureAuraFlashHandler
{
    private class FlashContainer
    {
        private NCreature _nCreature;
        private readonly List<AuraModel> _subscribed = [];
        public FlashContainer(NCreature nCreature) => _nCreature = nCreature;

        public void Subscribe(IEnumerable<AuraModel> auras)
        {
            Unsubscribe(); // guards against double _EnterTree
            foreach (var aura in auras)
            {
                aura.FlashedCreatures += Flash;
                _subscribed.Add(aura);
            }
        }

        public void Unsubscribe()
        {
            foreach (var aura in _subscribed)
                aura.FlashedCreatures -= Flash;
            _subscribed.Clear();
        }

        private void Flash(AuraModel aura, IEnumerable<Creature> creatures)
        {
            if (!GodotObject.IsInstanceValid(_nCreature)) return;
            if (!creatures.Contains(_nCreature.Entity)) return;
            var vfx = NAuraFlashCreatureVfx.Create(aura, _nCreature);
            if (vfx != null)
                Callable.From(delegate { NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx); }).CallDeferred();
            
        }
    }

    private static readonly SpireField<NCreature, FlashContainer> Handler = new(node => new FlashContainer(node));


    [HarmonyPatch(typeof(NCreature), nameof(NCreature._EnterTree))]
    [HarmonyPostfix]
    private static void OnEnter(NCreature __instance)
        => Handler.Get(__instance)!.Subscribe(GameHelper.GetRunState().Auras);

    [HarmonyPatch(typeof(NCreature), nameof(NCreature._ExitTree))]
    [HarmonyPostfix]
    private static void OnExit(NCreature __instance)
        => Handler.Get(__instance)?.Unsubscribe();
}