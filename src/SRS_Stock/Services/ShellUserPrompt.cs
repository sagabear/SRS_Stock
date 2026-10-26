using SRS_Stock.Core.Interaction;

namespace SRS_Stock.Services;

public sealed class ShellUserPrompt : IUserPrompt
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return Task.FromResult(false);

        if (MainThread.IsMainThread)
            return page.DisplayAlertAsync(title, message, accept, cancel);

        return MainThread.InvokeOnMainThreadAsync(() => page.DisplayAlertAsync(title, message, accept, cancel));
    }
}
