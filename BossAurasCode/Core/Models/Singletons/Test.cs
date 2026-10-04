using BaseLib.Abstracts;
using BossAuras.Core.Helper;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace BossAuras.Core.Models.Singletons;

public class TestCombat : SingletonModel
{
    public TestCombat()
    {
        //ModHelper.SubscribeForRunStateHooks($"{BossAurasInit.MOD_ID_SLUGGIFIED}-SINGLETON_TEST_COMBAT", IterateBossAurasForRunHooks);
        ModHelper.SubscribeForCombatStateHooks($"{Entry.MOD_ID_PREFIX}-SINGLETON_TEST_COMBAT", IterateBossAurasForCombatHooks);
    }

    private IEnumerable<AbstractModel> IterateBossAurasForCombatHooks(CombatState combatState) => [this];

    private IEnumerable<AbstractModel> IterateBossAurasForRunHooks(RunState runState)
        => GameHelper.GetNullableCombatState() is null ? [this] : [];

    public override bool ShouldReceiveCombatHooks => true;
    

    // Run only
    public override Task AfterActEntered()
    {
        Entry.Logger.Warn("After Act Entered - Combat Hooks");
        return Task.CompletedTask;
    }
    
    // Both
    public override Task BeforeCombatStart()
    {
        Entry.Logger.Warn("Before Combat Start - Combat Hooks");
        return Task.CompletedTask;
    }

    // Combat Only 
    public override Task BeforeAttack(AttackCommand command)
    {
        Entry.Logger.Warn("Before Attack - Combat Hooks");
        return Task.CompletedTask;
    }
}

public class TestRun : SingletonModel
{
    public TestRun()
    {
        ModHelper.SubscribeForRunStateHooks($"{Entry.MOD_ID_PREFIX}-SINGLETON_TEST_RUN", IterateBossAurasForRunHooks);
        //ModHelper.SubscribeForCombatStateHooks($"{BossAurasInit.MOD_ID_SLUGGIFIED}-SINGLETON_TEST_RUN", IterateBossAurasForCombatHooks);
    }

    private IEnumerable<AbstractModel> IterateBossAurasForCombatHooks(CombatState combatState) => [this];

    private IEnumerable<AbstractModel> IterateBossAurasForRunHooks(RunState runState)
        => GameHelper.GetNullableCombatState() is null ? [this] : [];

    public override bool ShouldReceiveCombatHooks => true;
    
    // Run only
    public override Task AfterActEntered()
    {
        Entry.Logger.Warn("After Act Entered - Run Hooks");
        return Task.CompletedTask;
    }

    // Both
    public override Task BeforeCombatStart()
    {
        Entry.Logger.Warn("Before Combat Start - Run Hooks");
        return Task.CompletedTask;
    }

    // Combat Only
    public override Task BeforeAttack(AttackCommand command)
    {
        Entry.Logger.Warn("Before Attack - Run Hooks");
        return Task.CompletedTask;
    }
}