using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SRS_Stock.Core.Localization;
using SRS_Stock.Core.Models;
using SRS_Stock.Core.Navigation;
using SRS_Stock.Core.Resources;
using SRS_Stock.Core.Services;
using SRS_Stock.Core.Threading;

namespace SRS_Stock.Core.ViewModels;

public partial class ProductEditViewModel : ObservableObject
{
    private readonly IProductRepository _repository;
    private readonly IAppNavigator _navigator;
    private readonly IUiDispatcher _uiDispatcher;
    private readonly ILogger<ProductEditViewModel> _logger;
    private readonly IFormatProvider? _format;
    private readonly EventHandler? _onLanguageChanged;
    private string? _errorKey;

    public ProductEditViewModel(
        IProductRepository repository,
        IAppNavigator navigator,
        IUiDispatcher uiDispatcher,
        ILogger<ProductEditViewModel> logger,
        IFormatProvider? formatProvider = null,
        ILanguageService? languageService = null)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(uiDispatcher);
        ArgumentNullException.ThrowIfNull(logger);

        _repository = repository;
        _navigator = navigator;
        _uiDispatcher = uiDispatcher;
        _logger = logger;
        _format = formatProvider;
        if (languageService is not null)
        {
            _onLanguageChanged = (_, _) => _ = _uiDispatcher.InvokeAsync(RefreshLocalizedText);
            languageService.Changed += _onLanguageChanged;
        }
    }

    private IFormatProvider Format => _format ?? CultureInfo.CurrentCulture;

    public int ProductId { get; private set; }

    public bool IsNew => ProductId == 0;

    public string Title => IsNew ? AppStrings.NewProductTitle : AppStrings.EditProductTitle;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _barcode = string.Empty;

    [ObservableProperty]
    private string _priceText = string.Empty;

    [ObservableProperty]
    private string _quantityText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public Task ApplyQueryAsync(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return ApplyNavigationAsync(ParseProductId(query));
    }

    public async Task ApplyNavigationAsync(int productId)
    {
        ProductId = productId;
        await DispatchAsync(() =>
        {
            OnPropertyChanged(nameof(IsNew));
            OnPropertyChanged(nameof(Title));
        }).ConfigureAwait(false);
        await LoadFormAsync().ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!TryCreateProduct(out var product))
            return;

        try
        {
            await DispatchAsync(() => IsBusy = true).ConfigureAwait(false);
            await _repository.SaveAsync(product).ConfigureAwait(false);
            _logger.LogInformation("Saved product {ProductId}", product.Id);

            try
            {
                await _navigator.GoBackAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to navigate back after saving product {ProductId}",
                    product.Id);
                await DispatchAsync(() => SetError(nameof(AppStrings.UnexpectedError))).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save product {ProductId}", product.Id);
            await DispatchAsync(() => SetError(nameof(AppStrings.SaveFailed))).ConfigureAwait(false);
        }
        finally
        {
            await DispatchAsync(() => IsBusy = false).ConfigureAwait(false);
        }
    }

    private bool CanSave() => !IsBusy;

    private async Task LoadFormAsync()
    {
        try
        {
            await DispatchAsync(() =>
            {
                IsBusy = true;
                SetError(null);
            }).ConfigureAwait(false);

            if (ProductId == 0)
            {
                await DispatchAsync(ClearFields).ConfigureAwait(false);
                _logger.LogInformation("Prepared an empty product form");
                return;
            }

            _logger.LogInformation("Loading product {ProductId}", ProductId);
            var product = await _repository.GetByIdAsync(ProductId).ConfigureAwait(false);
            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} was not found", ProductId);
                await DispatchAsync(() =>
                {
                    ClearFields();
                    SetError(nameof(AppStrings.ProductNotFound));
                }).ConfigureAwait(false);
                return;
            }

            await DispatchAsync(() =>
            {
                Name = product.Name;
                Barcode = product.Barcode;
                PriceText = product.Price.ToString("N2", Format);
                QuantityText = product.Quantity.ToString("0.##", Format);
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load product {ProductId}", ProductId);
            await DispatchAsync(() => SetError(nameof(AppStrings.ProductLoadFailed))).ConfigureAwait(false);
        }
        finally
        {
            await DispatchAsync(() => IsBusy = false).ConfigureAwait(false);
        }
    }

    private bool TryCreateProduct(out Product product)
    {
        product = new Product();
        var name = Name.Trim();
        if (name.Length == 0)
        {
            SetError(nameof(AppStrings.NameRequired));
            _logger.LogWarning("Product save rejected: name is empty");
            return false;
        }

        if (!TryParseNumber(PriceText, out var price))
        {
            SetError(nameof(AppStrings.PriceInvalid));
            _logger.LogWarning("Product save rejected: price is not a number");
            return false;
        }

        if (price < 0)
        {
            SetError(nameof(AppStrings.PriceNegative));
            _logger.LogWarning("Product save rejected: price is negative");
            return false;
        }

        if (!TryParseNumber(QuantityText, out var quantity))
        {
            SetError(nameof(AppStrings.QuantityInvalid));
            _logger.LogWarning("Product save rejected: quantity is not a number");
            return false;
        }

        if (quantity < 0)
        {
            SetError(nameof(AppStrings.QuantityNegative));
            _logger.LogWarning("Product save rejected: quantity is negative");
            return false;
        }

        product = new Product
        {
            Id = ProductId,
            Name = name,
            Barcode = Barcode.Trim(),
            Price = price,
            Quantity = quantity,
        };
        SetError(null);
        return true;
    }

    private bool TryParseNumber(string? text, out decimal value)
    {
        var trimmed = text?.Trim() ?? string.Empty;
        return decimal.TryParse(trimmed, NumberStyles.Number, Format, out value);
    }

    private void ClearFields()
    {
        Name = string.Empty;
        Barcode = string.Empty;
        PriceText = string.Empty;
        QuantityText = string.Empty;
    }

    private static int ParseProductId(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(Routes.ProductIdParameter, out var raw) || raw is null)
            return 0;

        return raw switch
        {
            int id => id,
            long id when id is >= int.MinValue and <= int.MaxValue => (int)id,
            string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ when int.TryParse(
                Convert.ToString(raw, CultureInfo.InvariantCulture),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => 0,
        };
    }

    private void RefreshLocalizedText()
    {
        OnPropertyChanged(nameof(Title));
        if (_errorKey is not null)
            ErrorMessage = AppStrings.Get(_errorKey);
    }

    private void SetError(string? key)
    {
        _errorKey = key;
        ErrorMessage = key is null ? string.Empty : AppStrings.Get(key);
    }

    private Task DispatchAsync(Action action) => _uiDispatcher.InvokeAsync(action);
}
