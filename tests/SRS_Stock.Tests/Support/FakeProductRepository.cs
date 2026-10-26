using SRS_Stock.Core.Models;
using SRS_Stock.Core.Services;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeProductRepository : IProductRepository
{
    public List<Product> Products { get; } = [];

    public List<string?> Searches { get; } = [];

    public List<bool> MissingFilters { get; } = [];

    public List<ProductSort> Sorts { get; } = [];

    public List<Product> Saved { get; } = [];

    public List<int> LoadedIds { get; } = [];

    public List<int> DeletedIds { get; } = [];

    public Exception? GetException { get; set; }

    public Exception? SaveException { get; set; }

    public Exception? GetByIdException { get; set; }

    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<Product>> GetAsync(
        string? search,
        bool missingOnly = false,
        ProductSort sort = ProductSort.Name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Searches.Add(search);
        MissingFilters.Add(missingOnly);
        Sorts.Add(sort);
        if (GetException is not null)
            throw GetException;

        return Task.FromResult<IReadOnlyList<Product>>(Products.ToList());
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LoadedIds.Add(id);
        if (GetByIdException is not null)
            throw GetByIdException;

        return Task.FromResult(Products.FirstOrDefault(product => product.Id == id));
    }

    public Task SaveAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (SaveException is not null)
            throw SaveException;

        if (product.Id == 0)
            product.Id = Saved.Count + 1;

        Saved.Add(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (SaveException is not null)
            throw SaveException;

        DeletedIds.Add(id);
        Products.RemoveAll(product => product.Id == id);
        return Task.CompletedTask;
    }
}
