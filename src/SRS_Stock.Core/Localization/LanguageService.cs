using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SRS_Stock.Core.Localization;

public sealed class LanguageService : ILanguageService
{
    private readonly ILanguagePreference _preference;
    private readonly IUiCulture _culture;
    private readonly ILogger<LanguageService> _logger;
    private readonly object _handlersGate = new();
    private readonly List<WeakReference<EventHandler>> _changedHandlers = [];

    public LanguageService(
        ILanguagePreference preference,
        IUiCulture culture,
        ILogger<LanguageService> logger)
    {
        ArgumentNullException.ThrowIfNull(preference);
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(logger);

        _preference = preference;
        _culture = culture;
        _logger = logger;

        var initial = Resolve(preference.GetCultureName())
            ?? Match(culture.Current)
            ?? AppLanguages.All[0];
        Current = initial;
        culture.Apply(CultureInfo.GetCultureInfo(initial.CultureName));
        _logger.LogInformation("UI language is {Culture}", initial.CultureName);
    }

    public IReadOnlyList<AppLanguage> Languages => AppLanguages.All;

    public AppLanguage Current { get; private set; }

    // Callers must keep the delegate alive. Only a weak reference is stored here.
    public event EventHandler? Changed
    {
        add
        {
            ArgumentNullException.ThrowIfNull(value);
            lock (_handlersGate)
                _changedHandlers.Add(new WeakReference<EventHandler>(value));
        }
        remove
        {
            if (value is null)
                return;

            lock (_handlersGate)
            {
                _changedHandlers.RemoveAll(item =>
                    !item.TryGetTarget(out var handler) || handler == value);
            }
        }
    }

    public void Apply(string cultureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);
        var language = Resolve(cultureName)
            ?? throw new ArgumentOutOfRangeException(
                nameof(cultureName),
                cultureName,
                "Unsupported UI language.");

        if (language.CultureName == Current.CultureName)
            return;

        Current = language;
        _culture.Apply(CultureInfo.GetCultureInfo(language.CultureName));
        _preference.SetCultureName(language.CultureName);
        _logger.LogInformation("UI language changed to {Culture}", language.CultureName);
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        EventHandler[] handlers;
        lock (_handlersGate)
        {
            var alive = new List<EventHandler>(_changedHandlers.Count);
            for (var index = _changedHandlers.Count - 1; index >= 0; index--)
            {
                if (_changedHandlers[index].TryGetTarget(out var handler))
                    alive.Add(handler);
                else
                    _changedHandlers.RemoveAt(index);
            }

            handlers = alive.ToArray();
        }

        foreach (var handler in handlers)
            handler(this, EventArgs.Empty);
    }

    private static AppLanguage? Resolve(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
            return null;

        var exact = AppLanguages.All.FirstOrDefault(language =>
            string.Equals(language.CultureName, cultureName, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
            return exact;

        try
        {
            var twoLetter = CultureInfo.GetCultureInfo(cultureName).TwoLetterISOLanguageName;
            return AppLanguages.All.FirstOrDefault(language =>
                string.Equals(language.CultureName, twoLetter, StringComparison.OrdinalIgnoreCase));
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    private static AppLanguage? Match(CultureInfo culture) =>
        AppLanguages.All.FirstOrDefault(language =>
            string.Equals(
                language.CultureName,
                culture.TwoLetterISOLanguageName,
                StringComparison.OrdinalIgnoreCase));
}
