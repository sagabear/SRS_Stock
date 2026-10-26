using System.Globalization;
using System.Resources;

namespace SRS_Stock.Core.Resources;

public static class AppStrings
{
    // Default text is Hungarian in AppStrings.resx.
    // Add another language as Resources/AppStrings.{culture}.resx, for example AppStrings.en.resx.
    private static readonly ResourceManager ResourceManager = new(
        "SRS_Stock.Core.Resources.AppStrings",
        typeof(AppStrings).Assembly);

    public static string NameRequired => Get(nameof(NameRequired));

    public static string PriceInvalid => Get(nameof(PriceInvalid));

    public static string PriceNegative => Get(nameof(PriceNegative));

    public static string QuantityInvalid => Get(nameof(QuantityInvalid));

    public static string QuantityNegative => Get(nameof(QuantityNegative));

    public static string ProductsLoadFailed => Get(nameof(ProductsLoadFailed));

    public static string ProductLoadFailed => Get(nameof(ProductLoadFailed));

    public static string SaveFailed => Get(nameof(SaveFailed));

    public static string ProductNotFound => Get(nameof(ProductNotFound));

    public static string UnexpectedError => Get(nameof(UnexpectedError));

    public static string NewProductTitle => Get(nameof(NewProductTitle));

    public static string EditProductTitle => Get(nameof(EditProductTitle));

    public static string EmptyList => Get(nameof(EmptyList));

    public static string NoSearchResults => Get(nameof(NoSearchResults));

    public static string OutOfStock => Get(nameof(OutOfStock));

    public static string CurrencySuffix => Get(nameof(CurrencySuffix));

    public static string AppTitle => Get(nameof(AppTitle));

    public static string SearchPlaceholder => Get(nameof(SearchPlaceholder));

    public static string Save => Get(nameof(Save));

    public static string NameLabel => Get(nameof(NameLabel));

    public static string BarcodeLabel => Get(nameof(BarcodeLabel));

    public static string PriceLabel => Get(nameof(PriceLabel));

    public static string QuantityLabel => Get(nameof(QuantityLabel));

    public static string AddProduct => Get(nameof(AddProduct));

    public static string MenuInventory => Get(nameof(MenuInventory));

    public static string MenuSettings => Get(nameof(MenuSettings));

    public static string MenuAbout => Get(nameof(MenuAbout));

    public static string OpenMenu => Get(nameof(OpenMenu));

    public static string LanguageLabel => Get(nameof(LanguageLabel));

    public static string LanguageHint => Get(nameof(LanguageHint));

    public static string AboutSummary => Get(nameof(AboutSummary));

    public static string AboutVersion => Get(nameof(AboutVersion));

    public static string Back => Get(nameof(Back));

    public static string ThemeLabel => Get(nameof(ThemeLabel));

    public static string ThemeSystem => Get(nameof(ThemeSystem));

    public static string ThemeLight => Get(nameof(ThemeLight));

    public static string ThemeDark => Get(nameof(ThemeDark));

    public static string StockSummary => Get(nameof(StockSummary));

    public static string SortLabel => Get(nameof(SortLabel));

    public static string SortByName => Get(nameof(SortByName));

    public static string SortByPrice => Get(nameof(SortByPrice));

    public static string SortByQuantity => Get(nameof(SortByQuantity));

    public static string SortOutOfStockFirst => Get(nameof(SortOutOfStockFirst));

    public static string MissingOnly => Get(nameof(MissingOnly));

    public static string EmptyMissing => Get(nameof(EmptyMissing));

    public static string BarcodeInUse => Get(nameof(BarcodeInUse));

    public static string Delete => Get(nameof(Delete));

    public static string DeleteTitle => Get(nameof(DeleteTitle));

    public static string DeleteConfirm => Get(nameof(DeleteConfirm));

    public static string DeleteFailed => Get(nameof(DeleteFailed));

    public static string Cancel => Get(nameof(Cancel));

    public static string Get(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return ResourceManager.GetString(name, CultureInfo.CurrentUICulture)
            ?? throw new InvalidOperationException($"Missing string resource '{name}'.");
    }
}
