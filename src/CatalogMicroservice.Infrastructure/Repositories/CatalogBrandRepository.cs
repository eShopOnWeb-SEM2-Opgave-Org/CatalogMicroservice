using System.Data;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Interfaces;
using CatalogMicroservice.Infrastructure.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogMicroservice.Infrastructure.Repositories;

internal class CatalogBrandRepository: ICatalogBrandRepository
{
    internal const string CONNECTION_STRING_KEY = "catalog-brand";
    internal const string DATABASE_NAME_KEY = "catalog-brand-db-name";

    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly ILogger<CatalogBrandRepository> _logger;

    public CatalogBrandRepository(IServiceProvider serviceProvider, ILogger<CatalogBrandRepository> logger)
    {
        string? connectionString = serviceProvider.GetKeyedService<string>(CONNECTION_STRING_KEY);
        string? databaseName = serviceProvider.GetKeyedService<string>(DATABASE_NAME_KEY);

        if (connectionString is null)
            throw new InvalidOperationException("Could not create CatalogBrandRepository due to missing connection string");
        if (databaseName is null)
            throw new InvalidOperationException("Could not create CatalogBrandRepository due to missing database name");

        _connectionString = connectionString;
        _databaseName = databaseName;

        _logger = logger;
    }

    public async Task<CatalogBrand?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

SELECT B.Id, B.Brand FROM [CatalogBrands] B
WHERE B.Id = @{nameof(brandId)};
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;

            command.AddParameterValue($"@{nameof(brandId)}", SqlDbType.Int, brandId);

            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            CatalogBrand? result = null;

            if (await reader.ReadAsync(cancellationToken))
            {
                int i = 0;
                result = new CatalogBrand
                {
                    Id = reader.GetInt32(i++),
                    Name = reader.GetString(i++)
                };
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch catalog brand with id = {BrandId} due to internal error",
                brandId
            );

            throw e;
        }
    }

    public async Task<IEnumerable<CatalogBrand>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

SELECT B.Id, B.Brand FROM [CatalogBrands] B;
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;
            List<CatalogBrand> result = [];

            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                int i = 0;
                CatalogBrand brand = new CatalogBrand
                {
                    Id = reader.GetInt32(i++),
                    Name = reader.GetString(i++)
                };

                result.Add(brand);
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch catalog brands due to internal error"
            );

            throw e;
        }
    }
}

