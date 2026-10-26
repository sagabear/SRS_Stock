using SRS_Stock.Core.Localization;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeLanguagePreference : ILanguagePreference
{
    public string? CultureName { get; set; }

    public int SetCount { get; private set; }

    public string? GetCultureName() => CultureName;

    public void SetCultureName(string cultureName)
    {
        SetCount++;
        CultureName = cultureName;
    }
}
