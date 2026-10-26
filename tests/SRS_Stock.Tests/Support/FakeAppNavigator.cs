using SRS_Stock.Core.Navigation;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeAppNavigator : IAppNavigator
{
    public List<int> EditedIds { get; } = [];

    public int BackCount { get; private set; }

    public Exception? EditException { get; set; }

    public Exception? BackException { get; set; }

    public Task GoToProductEditAsync(int productId)
    {
        if (EditException is not null)
            throw EditException;

        EditedIds.Add(productId);
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        if (BackException is not null)
            throw BackException;

        BackCount++;
        return Task.CompletedTask;
    }
}
