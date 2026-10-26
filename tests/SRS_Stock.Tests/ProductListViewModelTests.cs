using Microsoft.Extensions.Logging;
using SRS_Stock.Core.Models;
using SRS_Stock.Core.Resources;
using SRS_Stock.Core.Threading;
using SRS_Stock.Core.ViewModels;
using SRS_Stock.Tests.Support;

namespace SRS_Stock.Tests;

public sealed class ProductListViewModelTests
{
    [Fact]
    public async Task Load_shows_products_and_clears_a_previous_error()
    {
        var repository = new FakeProductRepository
        {
            GetException = new IOException("offline"),
        };
        var logger = new ListLogger<ProductListViewModel>();
        var viewModel = Create(repository, logger: logger);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(AppStrings.ProductsLoadFailed, viewModel.ErrorMessage);
        Assert.False(viewModel.IsBusy);
        Assert.False(viewModel.IsEmpty);
        Assert.True(logger.Has(LogLevel.Error, "Failed to load products"));

        repository.GetException = null;
        repository.Products.Add(new Product { Id = 1, Name = "Tej", Barcode = "1", Price = 10m, Quantity = 2m });
        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, viewModel.ErrorMessage);
        Assert.Equal("Tej", Assert.Single(viewModel.Products).Name);
        Assert.False(viewModel.IsEmpty);
        Assert.False(viewModel.IsBusy);
        Assert.True(logger.Has(LogLevel.Information, "Showing 1 products"));
    }

    [Fact]
    public void Empty_message_follows_the_search_text()
    {
        var viewModel = Create(new FakeProductRepository(), debounce: TimeSpan.Zero);

        Assert.Equal(AppStrings.EmptyList, viewModel.EmptyMessage);

        viewModel.SearchText = "tej";

        Assert.Equal(AppStrings.NoSearchResults, viewModel.EmptyMessage);
    }

    [Fact]
    public async Task Load_of_an_empty_catalog_is_empty()
    {
        var viewModel = Create(new FakeProductRepository());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsEmpty);
        Assert.Empty(viewModel.Products);
    }

    [Fact]
    public async Task Search_passes_the_trimmed_text_to_the_repository()
    {
        var repository = new FakeProductRepository();
        var viewModel = Create(repository, debounce: TimeSpan.Zero);

        viewModel.SearchText = "  tej  ";
        await viewModel.PendingSearch;

        Assert.Equal(["tej"], repository.Searches);
    }

    [Fact]
    public async Task Blank_search_loads_every_product()
    {
        var repository = new FakeProductRepository();
        var viewModel = Create(repository, debounce: TimeSpan.Zero);

        viewModel.SearchText = "   ";
        await viewModel.PendingSearch;

        Assert.Equal([null], repository.Searches);
    }

    [Fact]
    public async Task Rapid_search_queries_only_the_latest_term()
    {
        var repository = new FakeProductRepository();
        var viewModel = Create(repository, debounce: TimeSpan.FromMilliseconds(80));

        viewModel.SearchText = "a";
        viewModel.SearchText = "alma";
        await viewModel.PendingSearch;

        Assert.Equal(["alma"], repository.Searches);
    }

    [Fact]
    public async Task Add_and_open_navigate_with_the_product_id()
    {
        var navigator = new FakeAppNavigator();
        var logger = new ListLogger<ProductListViewModel>();
        var viewModel = Create(new FakeProductRepository(), navigator, logger);
        var product = new Product { Id = 4, Name = "Kenyér" };

        await viewModel.AddProductCommand.ExecuteAsync(null);
        await viewModel.OpenProductCommand.ExecuteAsync(product);
        await viewModel.OpenProductCommand.ExecuteAsync(null);

        Assert.Equal([0, 4], navigator.EditedIds);
        Assert.True(logger.Has(LogLevel.Information, "Opening the new product form"));
        Assert.True(logger.Has(LogLevel.Information, "Opening product 4"));
    }

    [Fact]
    public async Task Navigation_failure_is_logged_and_shown()
    {
        var navigator = new FakeAppNavigator { EditException = new InvalidOperationException("shell") };
        var logger = new ListLogger<ProductListViewModel>();
        var viewModel = Create(new FakeProductRepository(), navigator, logger);

        await viewModel.AddProductCommand.ExecuteAsync(null);

        Assert.Empty(navigator.EditedIds);
        Assert.Equal(AppStrings.UnexpectedError, viewModel.ErrorMessage);
        Assert.Contains(logger.Entries, entry => entry.Exception is InvalidOperationException);
    }

    [Fact]
    public void LanguageChange_RefreshesTheEmptyMessage()
    {
        var language = new FakeLanguageService();
        var viewModel = new ProductListViewModel(
            new FakeProductRepository(),
            new FakeAppNavigator(),
            new InlineUiDispatcher(),
            new ListLogger<ProductListViewModel>(),
            TimeSpan.Zero,
            language);
        var raised = false;
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ProductListViewModel.EmptyMessage))
                raised = true;
        };

        language.Apply("en");

        Assert.True(raised);
    }

    private static ProductListViewModel Create(
        FakeProductRepository repository,
        FakeAppNavigator? navigator = null,
        ListLogger<ProductListViewModel>? logger = null,
        TimeSpan? debounce = null) =>
        new(
            repository,
            navigator ?? new FakeAppNavigator(),
            new InlineUiDispatcher(),
            logger ?? new ListLogger<ProductListViewModel>(),
            debounce ?? TimeSpan.Zero);
}
