using SRS_Stock.Core.Theming;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeThemeApplicator : IThemeApplicator
{
    public List<ThemePreference> Applied { get; } = [];

    public void Apply(ThemePreference preference) => Applied.Add(preference);
}
