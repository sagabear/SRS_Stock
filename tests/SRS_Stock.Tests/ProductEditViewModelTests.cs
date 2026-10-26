using System.Globalization;
using Microsoft.Extensions.Logging;
using SRS_Stock.Core.Models;
using SRS_Stock.Core.Resources;
using SRS_Stock.Core.Threading;
using SRS_Stock.Core.ViewModels;
using SRS_Stock.Tests.Support;

namespace SRS_Stock.Tests;

public sealed class ProductEditViewModelTests
{
    [Fact]
    public async Task Empty_name_does_not_save()
    {
        var repository = new FakeProductRepository();
        var navigator = new FakeAppNavigator();
        var logger = new ListLogger<ProductEditViewModel>();
        var viewModel = Create(repository, navigator, logger);
        viewModel.Name = "   ";
        viewModel.PriceText = "10";
        viewModel.QuantityText = "1";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Empty(repository.Saved);
        Assert.Equal(0, navigator.BackCount);
        Assert.Equal(AppStrings.NameRequired, viewModel.ErrorMessage);
        Assert.True(logger.Has(LogLevel.Warning, "name is empty"));
    }

    [Theory]
    [InlineData("abc", "1", "Az ár nem érvényes szám.")]
    [InlineData("-1", "1", "Az ár nem lehet negatív.")]
    [InlineData("10", "x", "A mennyiség nem érvényes szám.")]
    [InlineData("10", "-0.5", "A mennyiség nem lehet negatív.")]
    public async Task Invalid_numbers_do_not_save(string price, string quantity, string expectedMessage)
    {
        var repository = new FakeProductRepository();
        var navigator = new FakeAppNavigator();
        var viewModel = Create(repository, navigator);
        viewModel.Name = "Tej";
        viewModel.PriceText = price;
        viewModel.QuantityText = quantity;

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Empty(repository.Saved);
        Assert.Equal(0, navigator.BackCount);
        Assert.Equal(expectedMessage, viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Save_trims_text_accepts_zero_stock_and_goes_back()
    {
        var repository = new FakeProductRepository();
        var navigator = new FakeAppNavigator();
        var logger = new ListLogger<ProductEditViewModel>();
        var viewModel = Create(repository, navigator, logger);
        await viewModel.ApplyNavigationAsync(0);
        viewModel.Name = "  Víz  ";
        viewModel.Barcode = "  000  ";
        viewModel.PriceText = "0";
        viewModel.QuantityText = "0";

        await viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(repository.Saved);
        Assert.Equal("Víz", saved.Name);
        Assert.Equal("000", saved.Barcode);
        Assert.Equal(0m, saved.Price);
        Assert.Equal(0m, saved.Quantity);
        Assert.Equal(1, saved.Id);
        Assert.Equal(1, navigator.BackCount);
        Assert.Equal(string.Empty, viewModel.ErrorMessage);
        Assert.False(viewModel.IsBusy);
        Assert.Equal(AppStrings.NewProductTitle, viewModel.Title);
        Assert.True(logger.Has(LogLevel.Information, "Saved product"));
    }

    [Fact]
    public async Task Save_failure_stays_on_the_form()
    {
        var repository = new FakeProductRepository { SaveException = new IOException("locked") };
        var navigator = new FakeAppNavigator();
        var logger = new ListLogger<ProductEditViewModel>();
        var viewModel = Create(repository, navigator, logger);
        viewModel.Name = "Tej";
        viewModel.PriceText = "12.50";
        viewModel.QuantityText = "1";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(0, navigator.BackCount);
        Assert.Equal(AppStrings.SaveFailed, viewModel.ErrorMessage);
        Assert.False(viewModel.IsBusy);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception is IOException);
    }

    [Fact]
    public async Task Navigation_failure_after_save_is_shown()
    {
        var repository = new FakeProductRepository();
        var navigator = new FakeAppNavigator { BackException = new InvalidOperationException("shell") };
        var viewModel = Create(repository, navigator);
        viewModel.Name = "Tej";
        viewModel.PriceText = "1";
        viewModel.QuantityText = "1";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Single(repository.Saved);
        Assert.Equal(0, navigator.BackCount);
        Assert.Equal(AppStrings.UnexpectedError, viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Existing_product_is_loaded_for_edit()
    {
        var repository = new FakeProductRepository();
        repository.Products.Add(new Product
        {
            Id = 3,
            Name = "Tej",
            Barcode = "599",
            Price = 12.5m,
            Quantity = 0m,
        });
        var viewModel = Create(repository);

        await viewModel.ApplyNavigationAsync(3);

        Assert.Equal(3, viewModel.ProductId);
        Assert.False(viewModel.IsNew);
        Assert.Equal(AppStrings.EditProductTitle, viewModel.Title);
        Assert.Equal("Tej", viewModel.Name);
        Assert.Equal("599", viewModel.Barcode);
        Assert.Equal("12.50", viewModel.PriceText);
        Assert.Equal("0", viewModel.QuantityText);
        Assert.False(viewModel.IsBusy);
        Assert.Equal(string.Empty, viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Missing_product_sets_an_error()
    {
        var logger = new ListLogger<ProductEditViewModel>();
        var viewModel = Create(new FakeProductRepository(), logger: logger);

        await viewModel.ApplyNavigationAsync(99);

        Assert.Equal(AppStrings.ProductNotFound, viewModel.ErrorMessage);
        Assert.Equal(string.Empty, viewModel.Name);
        Assert.True(logger.Has(LogLevel.Warning, "was not found"));
    }

    [Fact]
    public async Task Query_string_loads_the_requested_product()
    {
        var repository = new FakeProductRepository();
        repository.Products.Add(new Product { Id = 8, Name = "Liszt", Price = 1m, Quantity = 1m });
        var viewModel = Create(repository);

        await viewModel.ApplyQueryAsync(new Dictionary<string, object> { [ "id" ] = "8" });

        Assert.Equal(8, viewModel.ProductId);
        Assert.Equal("Liszt", viewModel.Name);
    }

    [Fact]
    public async Task Missing_query_id_prepares_a_new_product()
    {
        var repository = new FakeProductRepository();
        var logger = new ListLogger<ProductEditViewModel>();
        var viewModel = Create(repository, logger: logger);

        await viewModel.ApplyQueryAsync(new Dictionary<string, object>());

        Assert.True(viewModel.IsNew);
        Assert.Equal(AppStrings.NewProductTitle, viewModel.Title);
        Assert.Empty(repository.LoadedIds);
        Assert.True(logger.Has(LogLevel.Information, "Prepared an empty product form"));
    }

    [Fact]
    public async Task Load_failure_is_logged()
    {
        var repository = new FakeProductRepository { GetByIdException = new IOException("disk") };
        var logger = new ListLogger<ProductEditViewModel>();
        var viewModel = Create(repository, logger: logger);

        await viewModel.ApplyNavigationAsync(5);

        Assert.Equal(AppStrings.ProductLoadFailed, viewModel.ErrorMessage);
        Assert.False(viewModel.IsBusy);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception is IOException);
    }

    [Fact]
    public void Save_is_disabled_while_busy()
    {
        var viewModel = Create(new FakeProductRepository());

        viewModel.IsBusy = true;

        Assert.False(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsBusy = false;

        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    private static ProductEditViewModel Create(
        FakeProductRepository repository,
        FakeAppNavigator? navigator = null,
        ListLogger<ProductEditViewModel>? logger = null) =>
        new(
            repository,
            navigator ?? new FakeAppNavigator(),
            new InlineUiDispatcher(),
            logger ?? new ListLogger<ProductEditViewModel>(),
            CultureInfo.InvariantCulture);
}
