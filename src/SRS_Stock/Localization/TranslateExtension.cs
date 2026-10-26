using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Xaml;

namespace SRS_Stock.Localization;

[ContentProperty(nameof(Key))]
[AcceptEmptyServiceProvider]
public sealed class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new InvalidOperationException("A translation key is required.");

        return new Binding
        {
            Path = $"[{Key}]",
            Mode = BindingMode.OneWay,
            Source = ResolveLocalizer(),
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        ProvideValue(serviceProvider);

    private static UiLocalizer ResolveLocalizer()
    {
        if (Application.Current?.Resources.TryGetValue(UiLocalizer.ResourceKey, out var value) == true
            && value is UiLocalizer localizer)
            return localizer;

        return IPlatformApplication.Current?.Services.GetRequiredService<UiLocalizer>()
            ?? throw new InvalidOperationException("The UI localizer is not available.");
    }
}
