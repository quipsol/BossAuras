using BossAuras.Core.Extensions;
using BossAuras.Core.Handler;
using BossAuras.Core.Helper;
using BossAuras.Core.Models;

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

    public static void Remove(AuraModel aura)
    {
        if (!Auras.Contains(aura)) return;
        aura.BeforeRemoval();
        Auras.Remove(aura);
    }
}