using System.Data;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Helpers;
using CatalogMicroservice.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatalogMicroservice.Infrastructure.Repositories;

internal class CatalogTypeRepository: ICatalogTypeRepository
{
    internal const string CONNECTION_STRING_KEY = "catalog-type";
    internal const string DATABASE_NAME_KEY = "catalog-type-db-name";

    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly ILogger _logger;

    public CatalogTypeRepository(IServiceProvider serviceProvider, ILogger<CatalogBrandRepository> logger)
    {
        string? connectionString = serviceProvider.GetKeyedService<string>(CONNECTION_STRING_KEY);
        string? databaseName = serviceProvider.GetKeyedService<string>(DATABASE_NAME_KEY);

        if (connectionString is null)
            throw new InvalidOperationException("Could not create CatalogTypeRepository due to missing connection string");
        if (databaseName is null)
            throw new InvalidOperationException("Could not create CatalogTypeRepository due to missing database name");

        _connectionString = connectionString;
        _databaseName = databaseName;

        _logger = logger;
    }

    public async Task<CatalogType?> GetCatalogTypeAsync(int typeId, CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

SELECT T.Id, T.[Type] from [CatalogTypes] T
WHERE T.Id = @{nameof(typeId)};
";
        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;

            command.AddParameterValue($"@{nameof(typeId)}", SqlDbType.Int, typeId);

            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            CatalogType? type = null;

            if (await reader.ReadAsync(cancellationToken))
            {
                int i = 0;
                type = new CatalogType
                {
                    Id = reader.GetInt32(i++),
                    Type = reader.GetString(i++)
                };
            }

            return type;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch catalog type with id = {TypeId} due to internal error",
                typeId
            );

            throw e;
        }
    }

    public async Task<IEnumerable<CatalogType>> GetCatalogTypesAsync(CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

SELECT T.Id, T.[Type] from [CatalogTypes];
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;
            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            List<CatalogType> result = [];
            while (await reader.ReadAsync(cancellationToken))
            {
                int i = 0;
                CatalogType type = new CatalogType
                {
                    Id = reader.GetInt32(i++),
                    Type = reader.GetString(i++)
                };
                result.Add(type);
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch catalog types due to internal error"
            );

            throw e;
        }
    }
}
