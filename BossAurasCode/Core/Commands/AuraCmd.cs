using System.Reflection;
using BossAuras.Core.Extensions;
using BossAuras.Core.Handler;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;
using BossAuras.Core.Models.Auras;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;

namespace BossAuras.Core.Commands;

public static class AuraCmd
{
    private static List<AuraModel> Auras => AuraHandler.GetAuras(GameHelper.GetRunState());
    
    
    public static async Task Add<TAura>() where TAura : AuraModel
    {
        await Add(AuraDb.Aura<TAura>());
    }
    public static async Task Add(AuraModel aura)
    {
        Auras.Add(aura);
        GameHelper.GetNMapScreen()?.AuraContainer.AddAura(aura);
        await aura.AfterApplied();
    }

    public static async Task Remove(AuraModel aura)
    {
        if (!Auras.Contains(aura)) return;
        await aura.BeforeRemoval();
        Auras.Remove(aura);
    }
}