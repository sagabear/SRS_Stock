using SRS_Stock.Core.Models;
using SRS_Stock.Core.Services;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeProductRepository : IProductRepository
{
    public List<Product> Products { get; } = [];

    public List<string?> Searches { get; } = [];

    public List<Product> Saved { get; } = [];

    public List<int> LoadedIds { get; } = [];

    public Exception? GetException { get; set; }

    public Exception? SaveException { get; set; }

    public Exception? GetByIdException { get; set; }

    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<Product>> GetAsync(string? search, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Searches.Add(search);
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
}
