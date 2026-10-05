namespace BossAuras.Core.Models;

/// At what point should a Boss-Aura be removed!?
public enum BossAuraEnd
{
    /// Handle removal yourself.
    None,
    /// Removed before entering the first boss fight of the Act.
    BeforeBosses,
    /// Removed right before entering the boss whose aura this is. <br/>
    /// Not the same as <see cref="BeforeBosses"/> because multiple Bosses are possible.
    BeforeBoss,
    /// Removed right after exiting the boss whose aura this is. <br/>
    /// Not the same as <see cref="EndOfAct"/> because multiple Bosses are possible.
    AfterBoss,
    /// Removed when leaving the Act.
    EndOfAct,
}