using SRS_Stock.Core.Models;

namespace SRS_Stock.Core.Services;

public interface IProductRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAsync(
        string? search,
        bool missingOnly = false,
        ProductSort sort = ProductSort.Name,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task SaveAsync(Product product, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
