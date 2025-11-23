using CatalogMicroservice.Common.Models;

namespace CatalogMicroservice.Service.Interfaces;

public interface ICatalogItemService
{
    Task<IEnumerable<CatalogItem>> GetItemsAsync(int? pageIndex, int pageSize, int? brandId, int? typeId, CancellationToken token = default);
    Task<CatalogItem?> GetItemAsync(int id, CancellationToken token = default);

    Task<int> ItemCountAsync(int? brandId, int? typeId, CancellationToken token = default);

    Task CreateItemAsync(CreateCatalogItem create, CancellationToken token = default);
    Task UpdateItemAsync(UpdateCatalogItem update, CancellationToken token = default);
    Task DeleteItemAsync(int id, CancellationToken token = default);
}
