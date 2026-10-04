using BaseLib.Utils;
using BossAuras.Constants;
using BossAuras.Core.Handler;
using BossAuras.Core.Models;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace BossAuras;

[ModInitializer(nameof(Initialize))]
public partial class Entry : Node
{
    public const string MOD_ID = "BossAuras";
    public const string MOD_ID_PREFIX = "BOSSAURAS";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(MOD_ID, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
#if !TOOLS 
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(System.Reflection.Assembly.GetExecutingAssembly());
#endif
        
        Harmony harmony = new(MOD_ID);

        harmony.PatchAll();
        AuraHandler.Register();
        AuraHandler.SubscribeForStateHooks();
        CustomLocTableManager.Register(AuraModel.AURAS);
    }
}