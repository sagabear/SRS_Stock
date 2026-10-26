using SRS_Stock.Core.Interaction;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeUserPrompt : IUserPrompt
{
    public bool Result { get; set; } = true;

    public int Asks { get; private set; }

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        Asks++;
        return Task.FromResult(Result);
    }
}
