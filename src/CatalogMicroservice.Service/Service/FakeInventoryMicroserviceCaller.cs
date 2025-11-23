using InventoryMicroservice.Caller.Interfaces;
using InventoryMicroservice.Common.Models;
using InventoryMicroservice.Common.Requests;

namespace CatalogMicroservice.Service.Service;

public class FakeInventoryMicroserviceCaller : IInventoryMicroserviceCaller
{
  public async Task CreateInventoryStatusAsync(CreateInventory request, Func<CancellationToken, Task> success, Func<CancellationToken, Task>? failure, CancellationToken cancellationToken)
  {
    _ = request;
    _ = failure;

    await success(cancellationToken);
  }

  public async Task DeleteInventoryStatusAsync(DeleteInventory request, Func<CancellationToken, Task> success, Func<CancellationToken, Task>? failure, CancellationToken cancellationToken)
  {
    _ = request;
    _ = failure;

    await success(cancellationToken);
  }

  public Task<InventoryStatus?> GetInventoryStatusAsync(int catalogItemId, CancellationToken cancellationToken)
  {
    return Task.FromResult<InventoryStatus?>(new InventoryStatus
    {
      CatalogItemId = catalogItemId,
      ItemId = 1,
      ItemCount = 1
    });
  }

  public Task<IEnumerable<InventoryStatus>> GetMultipleInventoryStatusAsync(IEnumerable<int> catalogItemIds, CancellationToken cancellationToken)
  {
    return Task.FromResult(catalogItemIds.Select(id => new InventoryStatus
    {
      CatalogItemId = id,
      ItemId = 1,
      ItemCount = 1
    }));
  }

  public async Task UpdateInventoryStatusAsync(UpdateInventory request, Func<CancellationToken, Task> success, Func<CancellationToken, Task>? failure, CancellationToken cancellationToken)
  {
    _ = request;
    _ = failure;

    await success(cancellationToken);
  }
}
