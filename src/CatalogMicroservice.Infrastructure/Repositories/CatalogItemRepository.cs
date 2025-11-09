using System.Data;
using CatalogMicroservice.Common.Models;
using CatalogMicroservice.Infrastructure.Helpers;
using CatalogMicroservice.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatalogMicroservice.Infrastructure.Repositories;

internal class CatalogItemRepository: ICatalogItemRepository
{
    internal const string CONNECTION_STRING_KEY = "catalog-repository";
    internal const string DATABASE_NAME_KEY = "catalog-repository-db-name";

    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly ILogger<CatalogItemRepository> _logger;

    public CatalogItemRepository(IServiceProvider serviceProvider, ILogger<CatalogItemRepository> logger)
    {
        string? connectionString = serviceProvider.GetKeyedService<string>(CONNECTION_STRING_KEY);
        string? databaseName = serviceProvider.GetKeyedService<string>(DATABASE_NAME_KEY);

        if (connectionString is null)
            throw new InvalidOperationException("Could not create CatalogItemRepository due to missing connection string");
        if (databaseName is null)
            throw new InvalidOperationException("Could not create CatalogItemRepository due to missing database name");

        _connectionString = connectionString;
        _databaseName = databaseName;

        _logger = logger;
    }

    public async Task EnsureDbExistsAsync(CancellationToken cancellationToken = default)
    {
        string ensureTable = $@"
IF (DB_ID('{_databaseName}') IS NOT NULL)
BEGIN
    PRINT 'Database ""{_databaseName}"" exists';
END
ELSE BEGIN
    CREATE DATABASE [{_databaseName}];
END;
";

        string ensureData = $@"
DECLARE @insertBrands INT = 0;
DECLARE @insertTypes INT = 0;
DECLARE @insertCatalog INT = 0;

USE [{_databaseName}];

IF (OBJECT_ID('CatalogBrands') IS NOT NULL)
BEGIN
    PRINT 'Table ""CatalogBrands"" exists';
END
ELSE BEGIN
    CREATE TABLE [CatalogBrands] (
        Id INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
        Brand NVARCHAR(100) NOT NULL
    );
    SET @insertBrands = 1;
END;

IF (OBJECT_ID('CatalogTypes') IS NOT NULL)
BEGIN
    PRINT 'Table ""CatalogTypes"" exists';
END
ELSE BEGIN
    CREATE TABLE CatalogTypes (
        Id INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
        [Type] NVARCHAR(100) NOT NULL
    );
    SET @insertTypes = 1;
END;

IF (OBJECT_ID('Catalog') IS NOT NULL)
BEGIN
    PRINT 'Table ""Catalog"" exists';
END
ELSE BEGIN
    CREATE TABLE [Catalog] (
        Id INT PRIMARY KEY NOT NULL,
        [Name] VARCHAR(50) NOT NULL,
        [Description] VARCHAR(MAX),
        Price DECIMAL(18, 2) NOT NULL,
        PictureUri VARCHAR(MAX) NOT NULL,
        CatalogTypeId INT NOT NULL REFERENCES CatalogTypes(Id),
        CatalogBrandId INT NOT NULL REFERENCES CatalogBrands(Id)
    );
    SET @insertCatalog = 1;
END;

IF @insertBrands = 1
BEGIN
    PRINT 'Inserting Brands';
    INSERT INTO [CatalogBrands]([Brand])
    VALUES  ('Azure'),
            ('.NET'),
            ('Visual Studio'),
            ('SQL Server'),
            ('Other');
END;

IF @insertTypes = 1
BEGIN
    PRINT 'Inserting types';
    INSERT INTO [CatalogTypes]([Type])
    VALUES  ('Mug'),
            ('T-Shirt'),
            ('Sheet'),
            ('USB Memory Stick');
END;

IF @insertCatalog = 1
BEGIN
    PRINT 'Inserting Types';
    INSERT INTO [Catalog]([Name], [Description], [Price], [PictureUri], [CatalogTypeId], [CatalogBrandId])
    VALUEs  ('.NET Bot Black Sweatshirt','.NET Bot Black Sweatshirt',19.50,'http://catalogbaseurltobereplaced/images/products/1.png',2,2),
            ('.NET Black & White Mug','.NET Black & White Mug',8.50,'http://catalogbaseurltobereplaced/images/products/2.png',1,2),
            ('Prism White T-Shirt','Prism White T-Shirt',12.00,'http://catalogbaseurltobereplaced/images/products/3.png',2,5),
            ('.NET Foundation Sweatshirt','.NET Foundation Sweatshirt',12.00,'http://catalogbaseurltobereplaced/images/products/4.png',2,2),
            ('Roslyn Red Sheet','Roslyn Red Sheet',8.50,'http://catalogbaseurltobereplaced/images/products/5.png',3,5),
            ('.NET Blue Sweatshirt','.NET Blue Sweatshirt',12.00,'http://catalogbaseurltobereplaced/images/products/6.png',2,2),
            ('Roslyn Red T-Shirt','Roslyn Red T-Shirt',12.00,'http://catalogbaseurltobereplaced/images/products/7.png',2,5),
            ('Kudu Purple Sweatshirt','Kudu Purple Sweatshirt',8.50,'http://catalogbaseurltobereplaced/images/products/8.png',2,5),
            ('Cup<T> White Mug','Cup<T> White Mug',12.00,'http://catalogbaseurltobereplaced/images/products/9.png',1,5),
            ('.NET Foundation Sheet','.NET Foundation Sheet',12.00,'http://catalogbaseurltobereplaced/images/products/10.png',3,2),
            ('Cup<T> Sheet','Cup<T> Sheet',8.50,'http://catalogbaseurltobereplaced/images/products/11.png',3,2),
            ('Prism White TShirt','Prism White TShirt',12.00,'http://catalogbaseurltobereplaced/images/products/12.png',2,5);
END;
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand setupTable = connection.CreateCommand();
            setupTable.CommandText = ensureTable;
            await setupTable.ExecuteNonQueryAsync(cancellationToken);

            using SqlCommand setupData = connection.CreateCommand();
            setupData.CommandText = ensureData;
            await setupData.ExecuteNonQueryAsync(cancellationToken);

            return;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could complete database setup due to internal error"
            );

            throw e;
        }
    }

    public async Task<CatalogItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

SELECT
    C.Id,
    C.[Name],
    C.[Description],
    C.Price,
    C.PictureUri,
    C.CatalogBrandId,
    C.CatalogTypeId
FROM [Catalog] C
WHERE C.Id = @itemId;
        ";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;

            command.AddParameterValue("@itemId", SqlDbType.Int, itemId);

            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            CatalogItem? result = null;

            if (await reader.ReadAsync())
            {
                int i = 0;
                result = new CatalogItem
                {
                    Id = reader.GetInt32(i++),
                    Name = reader.GetString(i++),
                    Description = reader.GetString(i++),
                    Price = reader.GetDecimal(i++),
                    PictureUri = reader.GetString(i++),
                    CatalogBrandId = reader.GetInt32(i++),
                    CatalogTypeId = reader.GetInt32(i++)
                };
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch catalog item with id = {Id} due to internal error",
                itemId
            );

            throw e;
        }
    }

    public async Task<int> ItemCountAsync(int? brandId, int? typeId, CancellationToken cancellationToken)
    {
        string sqlString = $@"
USE [{_databaseName}]

SELECT COUNT(C.Id) From [Catalog] C
WHERE (CatalogBrandId = @{nameof(brandId)} OR @{nameof(brandId)} IS NULL)
AND (CatalogTypeId = @{nameof(typeId)} OR @{nameof(typeId)} IS NULL)
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = sqlString;

            command.AddParameterValue($"@{nameof(brandId)}", SqlDbType.Int, brandId is null ? DBNull.Value : brandId);
            command.AddParameterValue($"@{nameof(typeId)}", SqlDbType.Int, typeId is null ? DBNull.Value : typeId);

            int count = (int?)await command.ExecuteScalarAsync(cancellationToken) ?? 0;

            return count;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not get item count, due to internal error"
            );

            throw e;
        }
    }

    public async Task<IEnumerable<CatalogItem>> GetAllItemsAsync(CancellationToken cancellationToken)
    {
        string sqlString = $@"
USE [{_databaseName}];

SELECT
    C.Id,
    C.[Name],
    C.[Description],
    C.Price,
    C.PictureUri,
    C.CatalogBrandId,
    C.CatalogTypeId
FROM [Catalog] C
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;
            List<CatalogItem> items = [];
            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while(await reader.ReadAsync(cancellationToken))
            {
                int i = 0;
                CatalogItem item = new CatalogItem
                {
                    Id = reader.GetInt32(i++),
                    Name = reader.GetString(i++),
                    Description = reader.GetString(i++),
                    Price = reader.GetDecimal(i++),
                    PictureUri = reader.GetString(i++),
                    CatalogBrandId = reader.GetInt32(i++),
                    CatalogTypeId = reader.GetInt32(i++)
                };

                items.Add(item);
            }

            return items;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch items due to internal error"
            );

            throw e;
        }
    }

    public async Task<IEnumerable<CatalogItem>> GetItemPageAsync(int pageNo, int pageSize, int? brandId, int? typeId, CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

WITH [ROW] AS (
    SELECT
        C.Id,
        C.[Name],
        C.[Description],
        C.Price,
        C.PictureUri,
        C.CatalogBrandId,
        C.CatalogTypeId
    FROM [Catalog] C
    WHERE (CatalogBrandId = @{nameof(brandId)} OR @{nameof(brandId)} IS NULL)
    AND (CatalogTypeId = @{nameof(typeId)} OR @{nameof(typeId)} IS NULL)
    EXCEPT
    SELECT TOP (@{nameof(pageSize)} * (@{nameof(pageNo)} - 1))
        E.Id,
        E.[Name],
        E.[Description],
        E.Price,
        E.PictureUri,
        E.CatalogBrandId,
        E.CatalogTypeId
    FROM [Catalog] E
    WHERE (CatalogBrandId = @{nameof(brandId)} OR @{nameof(brandId)} IS NULL)
    and (CatalogTypeId = @{nameof(typeId)} OR @{nameof(typeId)} IS NULL)
)
SELECT TOP (@{nameof(pageSize)})
    R.Id,
    R.[Name],
    R.[Description],
    R.Price,
    R.PictureUri,
    R.CatalogBrandId,
    R.CatalogTypeId
FROM [ROW] R;
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;

            command.Parameters.AddWithValue($"@{nameof(pageNo)}", pageNo);
            command.Parameters.AddWithValue($"@{nameof(pageSize)}", pageSize);
            command.Parameters.AddWithValue($"@{nameof(brandId)}", brandId is null ? DBNull.Value : brandId);
            command.Parameters.AddWithValue($"@{nameof(typeId)}", typeId is null ? DBNull.Value : typeId);

            List<CatalogItem> items = [];
            SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while(await reader.ReadAsync(cancellationToken))
            {
                int i = 0;
                CatalogItem item = new CatalogItem
                {
                    Id = reader.GetInt32(i++),
                    Name = reader.GetString(i++),
                    Description = reader.GetString(i++),
                    Price = reader.GetDecimal(i++),
                    PictureUri = reader.GetString(i++),
                    CatalogBrandId = reader.GetInt32(i++),
                    CatalogTypeId = reader.GetInt32(i++)
                };

                items.Add(item);
            }

            return items;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not fetch items due to internal error"
            );

            throw e;
        }
    }

    public async Task<CatalogItem> CreateItemAsync(CreateCatalogItem item, CancellationToken cancellationToken = default)
    {
        //NOTE: this is a really bad way to do ids, but when the database does not make use for auto increment,
        //      then it stops being our problem.
        string getNextIdStmt = $@"
USE [{_databaseName}];

SELECT TOP 1 (C.Id + 1) as Id FROM [Catalog] C ORDER BY C.Id DESC
";

        string createSqlStmt = $@"
USE [{_databaseName}];

INSERT INTO [Catalog](Id, [Name], [Description], Price, PictureUri, CatalogTypeId, CatalogBrandId)
OUTPUT INSERTED.Id
     VALUES (
        @id,
        @{nameof(CreateCatalogItem.Name)},
        @{nameof(CreateCatalogItem.Description)},
        @{nameof(CreateCatalogItem.Price)},
        @{nameof(CreateCatalogItem.PictureUri)},
        @{nameof(CreateCatalogItem.CatalogTypeId)},
        @{nameof(CreateCatalogItem.CatalogBrandId)}
    );
";
        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand nextIdCommand = connection.CreateCommand();
            nextIdCommand.CommandText = getNextIdStmt;

            int nextId = (int?)await nextIdCommand.ExecuteScalarAsync(cancellationToken) ?? 1;

            using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = createSqlStmt;

            insertCommand.AddParameterValue($"@id", SqlDbType.Int, nextId);
            insertCommand.AddParameterValue($"@{nameof(CreateCatalogItem.Name)}", SqlDbType.VarChar, item.Name);
            insertCommand.AddParameterValue($"@{nameof(CreateCatalogItem.Description)}", SqlDbType.VarChar, item.Description);
            insertCommand.AddParameterValue($"@{nameof(CreateCatalogItem.Price)}", SqlDbType.Decimal, item.Price);
            insertCommand.AddParameterValue($"@{nameof(CreateCatalogItem.PictureUri)}", SqlDbType.VarChar, item.PictureUri);
            insertCommand.AddParameterValue($"@{nameof(CreateCatalogItem.CatalogTypeId)}", SqlDbType.Int, item.CatalogTypeId);
            insertCommand.AddParameterValue($"@{nameof(CreateCatalogItem.CatalogBrandId)}", SqlDbType.Int, item.CatalogBrandId);

            //NOTE: execute scalar returns the first column of the first rown, which is set to be
            //      the inserted items id.
            int insertedId = (int)await insertCommand.ExecuteScalarAsync(cancellationToken);

            CatalogItem newItem = new CatalogItem
            {
                Id = insertedId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                PictureUri = item.PictureUri,
                CatalogTypeId = item.CatalogTypeId,
                CatalogBrandId = item.CatalogBrandId
            };

            return newItem;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not insert item due to internal error"
            );

            throw e;
        }
    }

    public async Task UpdateItemAsync(UpdateCatalogItem item, CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

UPDATE [Catalog]
SET [Name] = @{nameof(CatalogItem.Name)},
    [Description] = @{nameof(CatalogItem.Description)},
    Price = @{nameof(CatalogItem.Price)},
    PictureUri = @{nameof(CatalogItem.PictureUri)},
    CatalogBrandId = @{nameof(CatalogItem.CatalogBrandId)},
    CatalogTypeId = @{nameof(CatalogItem.CatalogTypeId)}
WHERE Id = @{nameof(CatalogItem.Id)};
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;

            command.AddParameterValue($"@{nameof(UpdateCatalogItem.Id)}", SqlDbType.Int, item.Id);
            command.AddParameterValue($"@{nameof(UpdateCatalogItem.Name)}", SqlDbType.VarChar, item.Name);
            command.AddParameterValue($"@{nameof(UpdateCatalogItem.Description)}", SqlDbType.VarChar, item.Description);
            command.AddParameterValue($"@{nameof(UpdateCatalogItem.Price)}", SqlDbType.Decimal, item.Price);
            command.AddParameterValue($"@{nameof(UpdateCatalogItem.PictureUri)}", SqlDbType.VarChar, item.PictureUri);
            command.AddParameterValue($"@{nameof(UpdateCatalogItem.CatalogTypeId)}", SqlDbType.Int, item.CatalogTypeId);
            command.AddParameterValue($"@{nameof(UpdateCatalogItem.CatalogBrandId)}", SqlDbType.Int, item.CatalogBrandId);

            await command.ExecuteNonQueryAsync(cancellationToken);

            return;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to update catalog item with id = {ItemId} due to internal error",
                item.Id
            );

            throw e;
        }
    }

    public async Task DeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        string sqlString = $@"
USE [{_databaseName}];

DELETE FROM [Catalog]
WHERE Id = @{nameof(itemId)};
";

        try
        {
            await using SqlConnection connection = new SqlConnection(_connectionString);
            if (connection.State is ConnectionState.Closed)
                await connection.OpenAsync(cancellationToken);

            using SqlCommand command = connection.CreateCommand();

            command.CommandText = sqlString;
            command.AddParameterValue($"@{nameof(itemId)}", SqlDbType.Int, itemId);

            await command.ExecuteNonQueryAsync(cancellationToken);

            return;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to delete item with id = {ItemId} due to internal error",
                itemId
            );

            throw e;
        }
    }
}
