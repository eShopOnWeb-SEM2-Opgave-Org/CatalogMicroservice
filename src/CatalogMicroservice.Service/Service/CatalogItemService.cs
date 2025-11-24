using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Interfaces;
using InventoryMicroservice.Caller.Interfaces;
using InventoryMicroservice.Common.Models;
using InventoryMicroservice.Common.Requests;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CatalogMicroservice.Service;

internal class CatalogItemService : ICatalogItemService
{
  private readonly ICatalogItemRepository _itemRepository;
  private readonly ICatalogBrandService _brandService;
  private readonly ICatalogTypeService _typeService;

  private readonly IInventoryMicroserviceCaller _caller;
  private readonly ILogger<CatalogItemService> _logger;

  public CatalogItemService(
      ICatalogItemRepository itemRepository, ICatalogBrandService brandService, ICatalogTypeService typeService, IInventoryMicroserviceCaller caller, ILogger<CatalogItemService> logger)
  {
    _itemRepository = itemRepository;
    _brandService = brandService;
    _typeService = typeService;
    _caller = caller;
    _logger = logger;
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

    IEnumerable<InventoryStatus> status = await _caller.GetMultipleInventoryStatusAsync(page.Select(item => item.Id), token);
    _logger.LogInformation("Found status: (" + string.Join(", ", status.Select(inventory => $"Inventory{{InventoryId = {inventory.ItemId}, CatalogItemId = {inventory.CatalogItemId}, Count = {inventory.ItemCount}}}")) + ")");

    return page;
  }

  public async Task<int> ItemCountAsync(int? brandId, int? typeId, CancellationToken token = default)
  {
    int count = await _itemRepository.ItemCountAsync(brandId, typeId, token);
    return count;
  }

  public async Task<CatalogItem?> GetItemAsync(int id, CancellationToken token = default)
  {
    CatalogItem? item = await _itemRepository.GetItemAsync(id, token);

    if (item is not null)
    {
        InventoryStatus? status = await _caller.GetInventoryStatusAsync(item.Id, token);
        _logger.LogInformation($"Found status: Inventory{{InventoryId = {status?.ItemId}, CatalogItemId = {status?.CatalogItemId}, Count = {status?.ItemCount}}}");
    }

    return item;
  }

  public async Task CreateItemAsync(CreateCatalogItem item, CancellationToken token = default)
  {
    CatalogBrand? brand = await _brandService.GetBrandByIdAsync(item.CatalogBrandId, token);
    CatalogType? type = await _typeService.GetCatalogTypeAsync(item.CatalogTypeId, token);

    if (brand is null || type is null)
      return;

    (CatalogItem newItem, DbTransaction transaction) = await _itemRepository.CreateItemAsync(item, token);

    CreateInventory create = new CreateInventory
    {
        CatalogItemId = newItem.Id,
        StartingAmount = 5
    };

    _logger.LogInformation("Sending to inventory service");
    await _caller.CreateInventoryStatusAsync(
        create,
        async cancellationToken => {
            await transaction.CommitAsync(cancellationToken);
            if (transaction.Connection is not null)
            {
                await transaction.Connection.CloseAsync();
            }
        },
        async cancellationToken => {
            _logger.LogWarning("Could not create item on inventory microservice");
            await transaction.RollbackAsync(cancellationToken);
        },
        token
    );
  }

  public async Task UpdateItemAsync(UpdateCatalogItem updateItem, CancellationToken token = default)
  {
    CatalogItem? existing = await _itemRepository.GetItemAsync(updateItem.Id, token);
    if (existing is null)
      return;

    CatalogBrand? brand = await _brandService.GetBrandByIdAsync(updateItem.CatalogBrandId, token);
    CatalogType? type = await _typeService.GetCatalogTypeAsync(updateItem.CatalogTypeId, token);
    if (brand is null || type is null)
      return;

    UpdateInventory create = new UpdateInventory
    {
        CatalogItemId = updateItem.CatalogBrandId,
        NewAmount = 10
    };

    await _caller.UpdateInventoryStatusAsync(
        create,
        async cancellationToken => {
            await _itemRepository.UpdateItemAsync(updateItem, token);
        },
        async (cancellationToken) => {
            _logger.LogWarning("Could not update item {ItemId} on inventory microservice", updateItem.Id);
            await Task.CompletedTask;
        },
        token
    );
  }

  public async Task DeleteItemAsync(int id, CancellationToken token = default)
  {
    CatalogItem? exists = await _itemRepository.GetItemAsync(id, token);
    if (exists is null)
      return;

    DeleteInventory create = new DeleteInventory
    {
        CatalogItemId = id,
    };

    await _caller.DeleteInventoryStatusAsync(
        create,
        async cancellationToken => await _itemRepository.DeleteItemAsync(id, token),
        async (cancellationToken) => {
            _logger.LogWarning("Could not delete item {ItemId} on inventory microservice", id);
            await Task.CompletedTask;
        },
        token
    );

    await _itemRepository.DeleteItemAsync(id, token);
  }
}
