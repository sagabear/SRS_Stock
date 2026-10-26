using Microsoft.Extensions.Logging;

namespace SRS_Stock.Core.Theming;

public sealed class ThemeService : IThemeService
{
    private readonly IThemePreference _preference;
    private readonly IThemeApplicator _applicator;
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(
        IThemePreference preference,
        IThemeApplicator applicator,
        ILogger<ThemeService> logger)
    {
        ArgumentNullException.ThrowIfNull(preference);
        ArgumentNullException.ThrowIfNull(applicator);
        ArgumentNullException.ThrowIfNull(logger);

        _preference = preference;
        _applicator = applicator;
        _logger = logger;

        var initial = preference.Get() ?? ThemePreference.System;
        if (!Enum.IsDefined(initial))
            initial = ThemePreference.System;

        Current = initial;
        applicator.Apply(initial);
        _logger.LogInformation("UI theme is {Theme}", initial);
    }

    public ThemePreference Current { get; private set; }

    public event EventHandler? Changed;

    public void Apply(ThemePreference preference)
    {
        if (!Enum.IsDefined(preference))
            throw new ArgumentOutOfRangeException(nameof(preference), preference, "Unsupported theme.");

        if (preference == Current)
            return;

        Current = preference;
        _applicator.Apply(preference);
        _preference.Set(preference);
        _logger.LogInformation("UI theme changed to {Theme}", preference);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
