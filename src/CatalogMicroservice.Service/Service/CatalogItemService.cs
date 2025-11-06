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

    public async Task<IEnumerable<CatalogItem>> GetItemsAsync(int? pageIndex, int pageSize, int? brandId, int? typeId, CancellationToken token = default)
    {
        if (pageIndex is not > 0)
            pageIndex = -1;
        if (pageSize <= 0)
            pageSize = 10;

        IEnumerable<CatalogItem> page = pageIndex switch
        {
            -1 or null => await _itemRepository.GetAllItemsAsync(token),
            int pageNo => await _itemRepository.GetItemPageAsync(pageNo,
                pageSize,
                brandId,
                typeId,
                token
            )
        };

        page = page.Select(item =>
            new CatalogItem
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                PictureUri = item.PictureUri?.Replace("http://catalogbaseurltobereplaced", ""),
                Price = item.Price,
                CatalogBrandId = item.CatalogBrandId,
                CatalogTypeId = item.CatalogTypeId
            }
        );

        return page;
    }

    public async Task<int> ItemCountAsync(int? brandId, int? typeId, CancellationToken token = default)
    {
        int count = await _itemRepository.ItemCountAsync(brandId, typeId, token);
        return count;
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
