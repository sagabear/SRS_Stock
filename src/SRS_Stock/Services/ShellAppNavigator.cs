using SRS_Stock.Core.Navigation;

namespace SRS_Stock.Services;

public sealed class ShellAppNavigator : IAppNavigator
{
    public Task GoToProductEditAsync(int productId)
    {
        var route = $"{Routes.ProductEdit}?{Routes.ProductIdParameter}={productId}";
        return OnMainThreadAsync(() => Shell.Current.GoToAsync(route));
    }

    public Task GoBackAsync() => OnMainThreadAsync(() => Shell.Current.GoToAsync(".."));

    private static Task OnMainThreadAsync(Func<Task> navigate)
    {
        if (MainThread.IsMainThread)
            return navigate();

        return MainThread.InvokeOnMainThreadAsync(navigate);
    }
}
