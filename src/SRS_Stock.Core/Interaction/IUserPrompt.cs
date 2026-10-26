namespace SRS_Stock.Core.Interaction;

public interface IUserPrompt
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
