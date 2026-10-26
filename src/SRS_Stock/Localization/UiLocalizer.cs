using System.ComponentModel;
using SRS_Stock.Core.Localization;
using SRS_Stock.Core.Resources;

namespace SRS_Stock.Localization;

public sealed class UiLocalizer : INotifyPropertyChanged
{
    public const string ResourceKey = "UiLocalizer";

    private readonly EventHandler _onLanguageChanged;

    public UiLocalizer(ILanguageService language)
    {
        ArgumentNullException.ThrowIfNull(language);
        _onLanguageChanged = (_, _) =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        };
        language.Changed += _onLanguageChanged;
    }

    public string this[string key] => AppStrings.Get(key);

    public event PropertyChangedEventHandler? PropertyChanged;
}
