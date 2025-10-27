using CatalogMicroservice.Service.Interfaces;
using CatalogMicroservice.Service.Service;
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
}
