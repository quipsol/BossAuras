using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Patches;


// Only exists for better testing. 2 Bosses make it easier to test some stuff. Eventually we would remove this entirely.
// If run in the editor, this will make testing easier.
#if TOOLS

[HarmonyPatch(typeof(RunManager), nameof(RunManager.GenerateRooms))]
public class DoubleActOneAndTwoBosses
{
    private static readonly PropertyInfo? StateProperty = typeof(RunManager).GetProperty("State", 
                BindingFlags.NonPublic | BindingFlags.Instance);
    
    [HarmonyPostfix]
    private static void Postfix(RunManager __instance)
    {
        if (StateProperty == null) return;
        var state = (RunState?)StateProperty.GetValue(__instance);
        if (state == null) return;
       
        var act1 = state.Acts[0];
        var secondAct1BossEncounter = state.Rng.UpFront.NextItem(act1.AllBossEncounters.Where(e => e.Id != act1.BossEncounter.Id));
        if (secondAct1BossEncounter == null) return;
        act1.SetSecondBossEncounter(secondAct1BossEncounter);
        
        var act2 = state.Acts[1];
        var secondAct2BossEncounter = state.Rng.UpFront.NextItem(act2.AllBossEncounters.Where(e => e.Id != act2.BossEncounter.Id));
        if (secondAct2BossEncounter == null) return;
        act2.SetSecondBossEncounter(secondAct2BossEncounter);
        

        
    }
}

#endif