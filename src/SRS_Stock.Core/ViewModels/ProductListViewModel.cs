using System.Collections.ObjectModel;
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

public partial class ProductListViewModel : ObservableObject
{
    private readonly IProductRepository _repository;
    private readonly IAppNavigator _navigator;
    private readonly IUiDispatcher _uiDispatcher;
    private readonly ILogger<ProductListViewModel> _logger;
    private readonly TimeSpan _searchDebounce;
    private readonly object _loadGate = new();
    private readonly EventHandler? _onLanguageChanged;
    private CancellationTokenSource? _loadCts;
    private int _activeLoads;
    private string? _errorKey;
    private ProductSort _sort = ProductSort.Name;
    private int _itemCount;
    private int _inStockCount;
    private int _missingCount;
    private decimal _stockValue;

    public ProductListViewModel(
        IProductRepository repository,
        IAppNavigator navigator,
        IUiDispatcher uiDispatcher,
        ILogger<ProductListViewModel> logger,
        TimeSpan? searchDebounce = null,
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
        _searchDebounce = searchDebounce ?? TimeSpan.FromMilliseconds(300);
        Products = new ObservableCollection<Product>();
        RefreshSortOptions();
        if (languageService is not null)
        {
            _onLanguageChanged = (_, _) => _ = _uiDispatcher.InvokeAsync(RefreshLocalizedText);
            languageService.Changed += _onLanguageChanged;
        }
    }

    public ObservableCollection<Product> Products { get; }

    public bool IsEmpty => HasLoaded && Products.Count == 0;

    public string EmptyMessage
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
                return AppStrings.NoSearchResults;

            return MissingOnly ? AppStrings.EmptyMissing : AppStrings.EmptyList;
        }
    }

    public string SummaryText => HasLoaded
        ? string.Format(
            CultureInfo.CurrentCulture,
            AppStrings.StockSummary,
            _itemCount,
            _inStockCount,
            _missingCount,
            _stockValue,
            AppStrings.CurrencySuffix)
        : string.Empty;

    public IReadOnlyList<ProductSortOption> SortOptions { get; private set; } = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _missingOnly;

    [ObservableProperty]
    private ProductSortOption? _selectedSort;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasLoaded;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    internal Task PendingSearch { get; private set; } = Task.CompletedTask;

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(EmptyMessage));
        PendingSearch = DebouncedSearchAsync();
    }

    partial void OnHasLoadedChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

    partial void OnMissingOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(EmptyMessage));
        PendingSearch = ReloadAsync();
    }

    partial void OnSelectedSortChanged(ProductSortOption? value)
    {
        if (value is null || value.Sort == _sort)
            return;

        _sort = value.Sort;
        PendingSearch = ReloadAsync();
    }

    [RelayCommand]
    private Task LoadAsync()
    {
        var token = RestartLoad();
        return LoadCoreAsync(token);
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        try
        {
            _logger.LogInformation("Opening the new product form");
            await _navigator.GoToProductEditAsync(0).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open the new product form");
            await DispatchAsync(() => SetError(nameof(AppStrings.UnexpectedError))).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task OpenProductAsync(Product? product)
    {
        if (product is null)
            return;

        try
        {
            _logger.LogInformation("Opening product {ProductId} for edit", product.Id);
            await _navigator.GoToProductEditAsync(product.Id).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open product {ProductId}", product.Id);
            await DispatchAsync(() => SetError(nameof(AppStrings.UnexpectedError))).ConfigureAwait(false);
        }
    }

    private async Task DebouncedSearchAsync()
    {
        var token = RestartLoad();
        try
        {
            if (_searchDebounce > TimeSpan.Zero)
                await Task.Delay(_searchDebounce, token).ConfigureAwait(false);

            await LoadCoreAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Product search was cancelled");
        }
    }

    private CancellationToken RestartLoad()
    {
        lock (_loadGate)
        {
            // Cancel only. Disposing a token source while a delay still observes it can throw ObjectDisposedException.
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            return _loadCts.Token;
        }
    }

    private async Task LoadCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            var activeLoads = Interlocked.Increment(ref _activeLoads);
            if (activeLoads == 1)
                await DispatchAsync(() => IsBusy = true).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
                return;

            await DispatchAsync(() => SetError(null)).ConfigureAwait(false);
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            _logger.LogInformation("Loading products. Filtered: {Filtered}", search is not null);

            var items = await _repository
                .GetAsync(search, MissingOnly, _sort, cancellationToken)
                .ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                return;

            await DispatchAsync(() =>
            {
                Products.Clear();
                foreach (var item in items)
                    Products.Add(item);

                HasLoaded = true;
                UpdateSummary(items);
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(SummaryText));
            }).ConfigureAwait(false);

            _logger.LogInformation("Showing {Count} products", items.Count);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(ex, "Product load was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load products");
            await DispatchAsync(() => SetError(nameof(AppStrings.ProductsLoadFailed))).ConfigureAwait(false);
        }
        finally
        {
            var activeLoads = Interlocked.Decrement(ref _activeLoads);
            if (activeLoads == 0)
                await DispatchAsync(() => IsBusy = false).ConfigureAwait(false);
        }
    }

    private async Task ReloadAsync()
    {
        var token = RestartLoad();
        try
        {
            await LoadCoreAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Product load was cancelled");
        }
    }

    private void UpdateSummary(IReadOnlyList<Product> items)
    {
        _itemCount = items.Count;
        _missingCount = items.Count(product => product.Quantity == 0);
        _inStockCount = _itemCount - _missingCount;
        _stockValue = items.Sum(product => product.Price * product.Quantity);
    }

    private void RefreshSortOptions()
    {
        var sort = SelectedSort?.Sort ?? _sort;
        SortOptions =
        [
            new(ProductSort.Name, AppStrings.SortByName),
            new(ProductSort.Price, AppStrings.SortByPrice),
            new(ProductSort.Quantity, AppStrings.SortByQuantity),
            new(ProductSort.OutOfStockFirst, AppStrings.SortOutOfStockFirst),
        ];
        OnPropertyChanged(nameof(SortOptions));
        var match = SortOptions.First(option => option.Sort == sort);
        if (SelectedSort != match)
            SelectedSort = match;
    }

    private void RefreshLocalizedText()
    {
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(SummaryText));
        RefreshSortOptions();
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
