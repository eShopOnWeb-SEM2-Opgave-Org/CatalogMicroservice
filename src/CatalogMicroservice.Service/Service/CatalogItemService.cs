using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Interfaces;

namespace CatalogMicroservice.Service;

internal class CatalogItemService : ICatalogItemService
{
    private readonly ICatalogItemRepository _itemRepository;
    private readonly ICatalogBrandService _brandService;
    private readonly ICatalogTypeService _typeService;

    public CatalogItemService(
        ICatalogItemRepository itemRepository, ICatalogBrandService brandService, ICatalogTypeService typeService)
    {
        _itemRepository = itemRepository;
        _brandService = brandService;
        _typeService = typeService;
    }

    public async Task<IEnumerable<CatalogItem>> GetItemsAsync(int pageIndex, int pageSize, int? brandId, int? typeId, CancellationToken token = default)
    {
        if (pageIndex < 1) { pageIndex = 1; }
        if (pageSize <= 0) { pageSize = 10; }

        IEnumerable<CatalogItem> page = await _itemRepository.GetItemPageAsync(pageIndex,
            pageSize,
            brandId,
            typeId,
            token
        );

        return page;
    }

    public async Task<CatalogItem?> GetItemAsync(int id, CancellationToken token = default)
    {
        CatalogItem? item =  await _itemRepository.GetItemAsync(id, token);
        return item;
    }

    public async Task<CatalogItem?> CreateItemAsync(CreateCatalogItem item, CancellationToken token = default)
    {
        CatalogBrand? brand = await _brandService.GetBrandByIdAsync(item.CatalogBrandId, token);
        CatalogType? type  = await _typeService.GetCatalogTypeAsync(item.CatalogTypeId, token);

        if (brand is null || type is null)
            return null;

        CatalogItem created = await _itemRepository.CreateItemAsync(item, token);
        return created;
    }

    public async Task<bool> UpdateItemAsync(UpdateCatalogItem updateItem, CancellationToken token = default)
    {
        CatalogItem? existing = await _itemRepository.GetItemAsync(updateItem.Id, token);
        if (existing is null)
            return false;

        CatalogBrand? brand = await _brandService.GetBrandByIdAsync(updateItem.CatalogBrandId, token);
        CatalogType? type  = await _typeService.GetCatalogTypeAsync(updateItem.CatalogTypeId, token);
        if (brand is null || type is null)
            return false;

        await _itemRepository.UpdateItemAsync(updateItem, token);
        return true;
    }

    public async Task<bool> DeleteItemAsync(int id, CancellationToken token = default)
    {
        CatalogItem? exists = await _itemRepository.GetItemAsync(id, token);
        if (exists is null)
            return false;

        await _itemRepository.DeleteItemAsync(id, token);
        return true;
    }
}
