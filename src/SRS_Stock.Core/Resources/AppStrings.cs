using System.Globalization;
using System.Resources;

namespace SRS_Stock.Core.Resources;

public static class AppStrings
{
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

    public static string AppTitle => Get(nameof(AppTitle));

    public static string SearchPlaceholder => Get(nameof(SearchPlaceholder));

    public static string Save => Get(nameof(Save));

    public static string NameLabel => Get(nameof(NameLabel));

    public static string BarcodeLabel => Get(nameof(BarcodeLabel));

    public static string PriceLabel => Get(nameof(PriceLabel));

    public static string QuantityLabel => Get(nameof(QuantityLabel));

    public static string AddProduct => Get(nameof(AddProduct));

    private static string Get(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture)
        ?? throw new InvalidOperationException($"Missing string resource '{name}'.");
}
