namespace SRS_Stock.Core.Threading;

public sealed class InlineUiDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
        return Task.CompletedTask;
    }
}
