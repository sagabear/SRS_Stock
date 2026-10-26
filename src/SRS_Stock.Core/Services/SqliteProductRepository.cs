using System.Text;
using Microsoft.Extensions.Logging;
using SQLite;
using SRS_Stock.Core.Models;

namespace SRS_Stock.Core.Services;

public sealed class SqliteProductRepository : IProductRepository, IAsyncDisposable
{
    // LIKE is case-insensitive for ASCII letters. Accented characters match as written.
    private const string SelectColumns = "SELECT Id, Name, Barcode, Price, Quantity FROM Product";

    private const string BarcodeInUseSql = """
        SELECT COUNT(*) FROM Product
        WHERE Barcode = ? AND Id != ?
        """;

    private readonly string _databasePath;
    private readonly ILogger<SqliteProductRepository> _logger;
    private readonly SQLiteAsyncConnection _connection;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    private int _disposed;

    public SqliteProductRepository(string databasePath, ILogger<SqliteProductRepository> logger)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException("A database path is required.", nameof(databasePath));

        ArgumentNullException.ThrowIfNull(logger);

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        _databasePath = databasePath;
        _logger = logger;
        _connection = new SQLiteAsyncConnection(databasePath);
        _logger.LogDebug("SQLite database path: {DatabasePath}", databasePath);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized)
                return;

            await _connection.CreateTableAsync<Product>().ConfigureAwait(false);
            _initialized = true;
            _logger.LogInformation(
                "SQLite product table is ready. Database: {DatabasePath}",
                _databasePath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(
                ex,
                "Failed to initialize the SQLite database at {DatabasePath}",
                _databasePath);
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<IReadOnlyList<Product>> GetAsync(
        string? search,
        bool missingOnly = false,
        ProductSort sort = ProductSort.Name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        try
        {
            var sql = BuildListSql(term is not null, missingOnly, sort);
            List<Product> products;
            if (term is null)
            {
                products = await _connection.QueryAsync<Product>(sql).ConfigureAwait(false);
            }
            else
            {
                _logger.LogDebug("Querying products with filter {Search}", term);
                var pattern = $"%{EscapeLike(term)}%";
                products = await _connection
                    .QueryAsync<Product>(sql, pattern, pattern)
                    .ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation(
                "Loaded {Count} products. Filtered: {Filtered}. MissingOnly: {MissingOnly}. Sort: {Sort}",
                products.Count,
                term is not null,
                missingOnly,
                sort);
            return products;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to query products");
            throw;
        }
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var product = await _connection.FindAsync<Product>(id).ConfigureAwait(false);
            if (product is null)
                _logger.LogDebug("Product {ProductId} was not found", id);
            else
                _logger.LogDebug("Loaded product {ProductId}", id);

            return product;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to load product {ProductId}", id);
            throw;
        }
    }

    public async Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            product.Barcode = product.Barcode?.Trim() ?? string.Empty;
            if (product.Barcode.Length > 0)
            {
                var used = await _connection
                    .ExecuteScalarAsync<int>(BarcodeInUseSql, product.Barcode, product.Id)
                    .ConfigureAwait(false);
                if (used > 0)
                {
                    _logger.LogWarning("Barcode {Barcode} is already used", product.Barcode);
                    throw new DuplicateBarcodeException(product.Barcode);
                }
            }

            if (product.Id == 0)
            {
                await _connection.InsertAsync(product).ConfigureAwait(false);
                _logger.LogInformation(
                    "Inserted product {ProductId} ({ProductName})",
                    product.Id,
                    product.Name);
                return;
            }

            var updated = await _connection.UpdateAsync(product).ConfigureAwait(false);
            if (updated == 0)
            {
                _logger.LogWarning(
                    "Product {ProductId} was not updated because it does not exist",
                    product.Id);
                throw new InvalidOperationException($"Product {product.Id} does not exist.");
            }

            _logger.LogInformation(
                "Updated product {ProductId} ({ProductName})",
                product.Id,
                product.Name);
        }
        catch (Exception ex) when (ex is not OperationCanceledException
            and not InvalidOperationException
            and not DuplicateBarcodeException)
        {
            _logger.LogError(ex, "Failed to save product {ProductId}", product.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var deleted = await _connection.DeleteAsync<Product>(id).ConfigureAwait(false);
            if (deleted == 0)
            {
                _logger.LogWarning("Product {ProductId} was not deleted because it does not exist", id);
                throw new InvalidOperationException($"Product {id} does not exist.");
            }

            _logger.LogInformation("Deleted product {ProductId}", id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}", id);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _connection.CloseAsync().ConfigureAwait(false);
        _initLock.Dispose();
    }

    private static string BuildListSql(bool hasSearch, bool missingOnly, ProductSort sort)
    {
        var sql = new StringBuilder(SelectColumns);
        var filters = new List<string>();
        if (missingOnly)
            filters.Add("Quantity = 0");
        if (hasSearch)
            filters.Add("(Name LIKE ? ESCAPE '\\' OR Barcode LIKE ? ESCAPE '\\')");
        if (filters.Count > 0)
            sql.Append(" WHERE ").Append(string.Join(" AND ", filters));

        sql.Append(" ORDER BY ");
        sql.Append(sort switch
        {
            ProductSort.Price => "Price, Name COLLATE NOCASE",
            ProductSort.Quantity => "Quantity, Name COLLATE NOCASE",
            ProductSort.OutOfStockFirst => "CASE WHEN Quantity = 0 THEN 0 ELSE 1 END, Name COLLATE NOCASE",
            _ => "Name COLLATE NOCASE",
        });
        return sql.ToString();
    }

    private static string EscapeLike(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
