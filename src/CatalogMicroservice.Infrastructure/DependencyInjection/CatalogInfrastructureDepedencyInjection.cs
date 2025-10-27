using CatalogMicroservice.Infrastructure.Interfaces;
using CatalogMicroservice.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogMicroservice.Infrastructure.DependencyInjection;

public static class CatalogInfrastructureDependencyInjection
{
    public static IServiceCollection AddCatalogBrandRepository(this IServiceCollection @this, string connectionString, string databaseName)
    {
        @this.AddKeyedSingleton<string>(CatalogBrandRepository.CONNECTION_STRING_KEY, connectionString);
        @this.AddKeyedSingleton<string>(CatalogBrandRepository.DATABASE_NAME_KEY, databaseName);
        @this.AddScoped<ICatalogBrandRepository, CatalogBrandRepository>();

        return @this;
    }

    public static IServiceCollection AddCatalogTypeRepository(this IServiceCollection @this, string connectionString, string databaseName)
    {
        @this.AddKeyedSingleton<string>(CatalogTypeRepository.CONNECTION_STRING_KEY, connectionString);
        @this.AddKeyedSingleton<string>(CatalogTypeRepository.DATABASE_NAME_KEY, databaseName);
        @this.AddScoped<ICatalogTypeRepository, CatalogTypeRepository>();

        return @this;
    }

    public static IServiceCollection AddCatalogItemRepository(this IServiceCollection @this, string connectionString, string databaseName)
    {
        @this.AddKeyedSingleton<string>(CatalogItemRepository.CONNECTION_STRING_KEY, connectionString);
        @this.AddKeyedSingleton<string>(CatalogItemRepository.DATABASE_NAME_KEY, databaseName);
        @this.AddScoped<ICatalogItemRepository, CatalogItemRepository>();
        @this.AddScoped<CatalogItemRepository>();

        return @this;
    }

    public static IServiceScope SetupCatalogDatabase(this IServiceScope @this)
    {
        IServiceProvider provider = @this.ServiceProvider;
        CatalogItemRepository? repository = provider.GetService<CatalogItemRepository>();

        if (repository is null)
            return @this;

        Task ensureDbTask = repository.EnsureDbExistsAsync();
        ensureDbTask.Wait();

        return @this;
    }
}
