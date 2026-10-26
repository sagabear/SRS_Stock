namespace SRS_Stock.Core.Threading;

public interface IUiDispatcher
{
    Task InvokeAsync(Action action);
}
