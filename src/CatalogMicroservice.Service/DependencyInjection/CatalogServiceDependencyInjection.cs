using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Service.Service;
using InventoryMicroservice.Caller.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogMicroservice.Service.DependencyInjection;

public static class CatalogServiceDepedencyInjection
{
  public static IServiceCollection AddCatalogServices(this IServiceCollection @this)
  {
    @this.AddScoped<ICatalogItemService, CatalogItemService>();
    @this.AddScoped<ICatalogBrandService, CatalogBrandService>();
    @this.AddScoped<ICatalogTypeService, CatalogTypeService>();

    return @this;
  }

  public static IServiceCollection AddFakeInventoryMicroserivceConnection(this IServiceCollection @this)
  {
    @this.AddScoped<IInventoryMicroserviceCaller, FakeInventoryMicroserviceCaller>();

    return @this;
  }
}
