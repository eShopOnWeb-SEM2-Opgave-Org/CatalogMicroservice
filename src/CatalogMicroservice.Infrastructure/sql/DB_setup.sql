
DECLARE @insertBrands INT = 0;
DECLARE @insertTypes INT = 0;
DECLARE @insertCatalog INT = 0;

IF (OBJECT_ID('CatalogBrands') IS NOT NULL)
BEGIN
    PRINT 'Table "CatalogBrands" exists'
END
ELSE BEGIN
    CREATE TABLE [CatalogBrands] (
        Id INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
        Brand NVARCHAR(100) NOT NULL,
    );
    SET @insertBrands = 1;
END;

IF (OBJECT_ID('CatalogTypes') IS NOT NULL)
BEGIN
    PRINT 'Table "CatalogTypes" exists'
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
    PRINT 'Table "Catalog" exists';
END
ELSE BEGIN
    CREATE TABLE [Catalog] (
        Id INT PRIMARY KEY NOT NULL IDENTITY(1, 1),
        [Name] VARCHAR(50) NOT NULL,
        [Description] VARCHAR(MAX),
        Price DECIMAL(18, 2) NOT NULL,
        PictureUri VARCHAR(MAX) NOT NULL,
        CatalogTypeId INT NOT NULL,
        CatalogBrandId INT NOT NULL
    );
    SET @insertCatalog = 1;
END;

IF @insertBrands = 1
BEGIN
    PRINT 'Inserting Brands'
    INSERT INTO [CatalogBrands]([Brand])
    VALUES  ('Azure'),
            ('.NET'),
            ('Visual Studio'),
            ('SQL Server'),
            ('Other')
END;

IF @insertTypes = 1
BEGIN
    PRINT 'Inserting types'
    INSERT INTO [CatalogTypes]([Type])
    VALUES  ('Mug'),
            ('T-Shirt'),
            ('Sheet'),
            ('USB Memory Stick')
END;

IF @insertCatalog = 1
BEGIN
    PRINT 'Inserting Types'
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
