namespace BossAuras.Core.Helper;

/// <summary>
/// Helpers to expression bodies returning a <c>Task</c> object.
/// </summary>
public static class TaskHelper
{
    /// Captures <c>this</c> in lambdas. Prefer using <see cref="Done"/>
    public static Task Wrap(Action action)
    {
        action();
        return Task.CompletedTask;
    }
    
    /// returns <c>Task.CompletedTask</c> and ignores the parameter.
    public static Task Done<T>(T _) => Task.CompletedTask;
}