using Microsoft.Extensions.Logging;
using SRS_Stock.Core.Models;
using SRS_Stock.Core.Services;
using SRS_Stock.Tests.Support;

namespace SRS_Stock.Tests;

public sealed class SqliteProductRepositoryTests : IAsyncLifetime
{
    private readonly ListLogger<SqliteProductRepository> _logger = new();
    private string _directory = string.Empty;
    private SqliteProductRepository? _repository;

    public Task InitializeAsync()
    {
        _directory = Path.Combine(Path.GetTempPath(), "srs-stock-tests", Guid.NewGuid().ToString("N"));
        _repository = new SqliteProductRepository(Path.Combine(_directory, "leltar.db"), _logger);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_repository is not null)
            await _repository.DisposeAsync();

        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    [Fact]
    public async Task Insert_then_list_returns_the_product_and_logs()
    {
        var repository = Repository;
        await repository.SaveAsync(new Product
        {
            Name = "Tej",
            Barcode = "599123",
            Price = 10.25m,
            Quantity = 0.5m,
        });

        var products = await repository.GetAsync(null);

        var product = Assert.Single(products);
        Assert.True(product.Id > 0);
        Assert.Equal("Tej", product.Name);
        Assert.Equal("599123", product.Barcode);
        Assert.Equal(10.25m, product.Price);
        Assert.Equal(0.5m, product.Quantity);
        Assert.True(_logger.Has(LogLevel.Information, "Inserted product"));
        Assert.True(_logger.Has(LogLevel.Information, "SQLite product table is ready"));
        Assert.True(_logger.Has(LogLevel.Information, "Loaded 1 products"));
    }

    [Fact]
    public async Task Update_overwrites_price_and_quantity()
    {
        var repository = Repository;
        var product = new Product
        {
            Name = "Kenyér",
            Barcode = "111",
            Price = 500m,
            Quantity = 2m,
        };
        await repository.SaveAsync(product);

        product.Price = 650m;
        product.Quantity = 0m;
        await repository.SaveAsync(product);

        var stored = await repository.GetByIdAsync(product.Id);
        Assert.NotNull(stored);
        Assert.Equal(650m, stored.Price);
        Assert.Equal(0m, stored.Quantity);
        Assert.True(_logger.Has(LogLevel.Information, "Updated product"));
    }

    [Fact]
    public async Task Update_missing_product_logs_and_throws()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Repository.SaveAsync(new Product { Id = 42, Name = "Hiányzó", Price = 1m, Quantity = 1m }));

        Assert.Contains("42", exception.Message, StringComparison.Ordinal);
        Assert.True(_logger.Has(LogLevel.Warning, "was not updated"));
    }

    [Fact]
    public async Task Get_returns_zero_quantity_products()
    {
        await Repository.SaveAsync(new Product
        {
            Name = "Üres",
            Barcode = "000",
            Price = 100m,
            Quantity = 0m,
        });

        var products = await Repository.GetAsync(search: null);
        Assert.Equal(0m, Assert.Single(products).Quantity);
    }

    [Fact]
    public async Task Search_matches_name_and_barcode_without_wildcards()
    {
        var repository = Repository;
        await repository.SaveAsync(new Product { Name = "Tej", Barcode = "111", Price = 1m, Quantity = 1m });
        await repository.SaveAsync(new Product { Name = "Alma", Barcode = "599000111", Price = 2m, Quantity = 1m });
        await repository.SaveAsync(new Product { Name = "100%_tej", Barcode = "222", Price = 3m, Quantity = 1m });

        var byName = await repository.GetAsync("tej");
        Assert.Equal(["100%_tej", "Tej"], byName.Select(product => product.Name));

        var byBarcode = await repository.GetAsync("000111");
        Assert.Equal("Alma", Assert.Single(byBarcode).Name);

        var literalPercent = await repository.GetAsync("%");
        Assert.Equal("100%_tej", Assert.Single(literalPercent).Name);

        var literalUnderscore = await repository.GetAsync("_");
        Assert.Equal("100%_tej", Assert.Single(literalUnderscore).Name);

        var blank = await repository.GetAsync("   ");
        Assert.Equal(3, blank.Count);
        Assert.Equal(["100%_tej", "Alma", "Tej"], blank.Select(product => product.Name));
    }

    [Fact]
    public async Task Filtered_query_stays_correct_for_a_few_hundred_rows()
    {
        var repository = Repository;
        for (var i = 0; i < 300; i++)
        {
            await repository.SaveAsync(new Product
            {
                Name = $"Termék {i:D3}",
                Barcode = $"BC{i:D4}",
                Price = i,
                Quantity = i,
            });
        }

        var match = await repository.GetAsync("BC0150");
        var product = Assert.Single(match);
        Assert.Equal("Termék 150", product.Name);
        Assert.Equal(150m, product.Quantity);

        var all = await repository.GetAsync(null);
        Assert.Equal(300, all.Count);
    }

    [Fact]
    public async Task Initialize_is_logged_once()
    {
        await Repository.InitializeAsync();
        await Repository.InitializeAsync();

        Assert.Equal(1, _logger.Entries.Count(entry =>
            entry.Level == LogLevel.Information &&
            entry.Message.Contains("table is ready", StringComparison.Ordinal)));
    }

    [Fact]
    public void Constructor_rejects_a_blank_path()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new SqliteProductRepository("  ", _logger));

        Assert.Equal("databasePath", exception.ParamName);
    }

    [Fact]
    public async Task List_can_show_only_missing_items_and_sort_by_price()
    {
        var repository = Repository;
        await repository.SaveAsync(new Product { Name = "Drága", Barcode = "1", Price = 30m, Quantity = 2m });
        await repository.SaveAsync(new Product { Name = "Olcsó", Barcode = "2", Price = 5m, Quantity = 0m });
        await repository.SaveAsync(new Product { Name = "Közepes", Barcode = "3", Price = 10m, Quantity = 1m });

        var missing = await repository.GetAsync(null, missingOnly: true);
        Assert.Equal("Olcsó", Assert.Single(missing).Name);

        var byPrice = await repository.GetAsync(null, sort: ProductSort.Price);
        Assert.Equal(["Olcsó", "Közepes", "Drága"], byPrice.Select(product => product.Name));

        var outOfStockFirst = await repository.GetAsync(null, sort: ProductSort.OutOfStockFirst);
        Assert.Equal("Olcsó", outOfStockFirst[0].Name);
    }

    [Fact]
    public async Task Save_rejects_a_barcode_that_belongs_to_another_product()
    {
        var repository = Repository;
        await repository.SaveAsync(new Product { Name = "Tej", Barcode = "111", Price = 1m, Quantity = 1m });
        await repository.SaveAsync(new Product { Name = "Víz", Barcode = "", Price = 1m, Quantity = 1m });
        await repository.SaveAsync(new Product { Name = "Lé", Barcode = "   ", Price = 1m, Quantity = 1m });

        var duplicate = new Product { Name = "Másik", Barcode = "111", Price = 2m, Quantity = 1m };
        await Assert.ThrowsAsync<DuplicateBarcodeException>(() => repository.SaveAsync(duplicate));

        var existing = Assert.Single(await repository.GetAsync(null), product => product.Name == "Tej");
        existing.Price = 9m;
        await repository.SaveAsync(existing);

        var updated = await repository.GetByIdAsync(existing.Id);
        Assert.Equal(9m, updated!.Price);
    }

    [Fact]
    public async Task Delete_removes_the_product()
    {
        var repository = Repository;
        await repository.SaveAsync(new Product { Name = "Tej", Barcode = "111", Price = 1m, Quantity = 1m });
        var id = Assert.Single(await repository.GetAsync(null)).Id;

        await repository.DeleteAsync(id);

        Assert.Empty(await repository.GetAsync(null));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.DeleteAsync(id));
    }

    private SqliteProductRepository Repository =>
        _repository ?? throw new InvalidOperationException("Repository was not created.");
}
