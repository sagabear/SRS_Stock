using SRS_Stock.Core.Threading;

namespace SRS_Stock.Threading;

public sealed class MainThreadDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (MainThread.IsMainThread)
        {
            action();
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(action);
    }
}
