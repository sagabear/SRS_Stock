namespace SRS_Stock.Core.ViewModels;

public sealed class AboutViewModel
{
    public AboutViewModel(string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        Version = version;
    }

    public string Version { get; }
}
