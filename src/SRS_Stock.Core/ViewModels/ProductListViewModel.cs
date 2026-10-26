using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
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
    private CancellationTokenSource? _loadCts;
    private int _activeLoads;

    public ProductListViewModel(
        IProductRepository repository,
        IAppNavigator navigator,
        IUiDispatcher uiDispatcher,
        ILogger<ProductListViewModel> logger,
        TimeSpan? searchDebounce = null)
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
    }

    public ObservableCollection<Product> Products { get; }

    public bool IsEmpty => HasLoaded && Products.Count == 0;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasLoaded;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    internal Task PendingSearch { get; private set; } = Task.CompletedTask;

    partial void OnSearchTextChanged(string value) => PendingSearch = DebouncedSearchAsync();

    partial void OnHasLoadedChanged(bool value) => OnPropertyChanged(nameof(IsEmpty));

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
            await DispatchAsync(() => ErrorMessage = AppStrings.UnexpectedError).ConfigureAwait(false);
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
            await DispatchAsync(() => ErrorMessage = AppStrings.UnexpectedError).ConfigureAwait(false);
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

            await DispatchAsync(() => ErrorMessage = string.Empty).ConfigureAwait(false);
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            _logger.LogInformation("Loading products. Filtered: {Filtered}", search is not null);

            var items = await _repository.GetAsync(search, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                return;

            await DispatchAsync(() =>
            {
                Products.Clear();
                foreach (var item in items)
                    Products.Add(item);

                HasLoaded = true;
                OnPropertyChanged(nameof(IsEmpty));
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
            await DispatchAsync(() => ErrorMessage = AppStrings.ProductsLoadFailed).ConfigureAwait(false);
        }
        finally
        {
            var activeLoads = Interlocked.Decrement(ref _activeLoads);
            if (activeLoads == 0)
                await DispatchAsync(() => IsBusy = false).ConfigureAwait(false);
        }
    }

    private Task DispatchAsync(Action action) => _uiDispatcher.InvokeAsync(action);
}
