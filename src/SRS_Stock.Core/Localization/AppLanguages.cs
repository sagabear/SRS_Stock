namespace SRS_Stock.Core.Localization;

public static class AppLanguages
{
    public static IReadOnlyList<AppLanguage> All { get; } =
    [
        new("hu", "Magyar"),
        new("en", "English"),
    ];
}
