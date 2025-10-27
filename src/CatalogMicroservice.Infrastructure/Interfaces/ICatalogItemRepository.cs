using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.Infrastructure.Interfaces;

public interface ICatalogItemRepository
{
    Task<CatalogItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CatalogItem>> GetItemPageAsync(int pageNo, int pageSize, int? brandId, int? typeId, CancellationToken cancellationToken = default);

    Task<CatalogItem> CreateItemAsync(CreateCatalogItem item, CancellationToken cancellationToken = default);
    Task UpdateItemAsync(UpdateCatalogItem item, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default);
}
