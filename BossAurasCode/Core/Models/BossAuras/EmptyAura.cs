using BaseLib.Abstracts;
using BossAuras.Core.Commands;
using BossAuras.Core.Extensions;
using BossAuras.Core.Handler;
using BossAuras.Core.Helper;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models;

namespace BossAuras.Core.Models.BossAuras;

public class EmptyAura : AuraModel
{
    // public class EmptyAuraSingleton() : CustomSingletonModel(HookType.Run)
    // {
    //     public override async Task AfterActEntered()
    //     {
    //         Entry.Logger.Info("Inside AfterActEntered");
    //         if (GameHelper.GetRunState().CurrentActIndex == 0)
    //         {
    //             Entry.Logger.Info("Adding EmptyAura to game");
    //             var emptyAura = AuraDb.Get<EmptyAura>().ToMutable();
    //             await AuraCmd.AddAura(emptyAura);
    //             var kinshipAura = AuraDb.Get<Kinship>().ToMutable();
    //             await AuraCmd.AddAura(kinshipAura);
    //
    //             GameHelper.GetNMapScreen()?.AuraContainer.Reload();
    //         }
    //     }
    // }

    public override Task BeforeAttack(AttackCommand command)
    {
        Entry.Logger.Info("Before Attack - Empty Aura");
        return Task.CompletedTask;
    }
}