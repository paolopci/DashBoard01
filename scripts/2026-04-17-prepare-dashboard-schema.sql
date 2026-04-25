SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Code nvarchar(20) NOT NULL,
        Name nvarchar(100) NOT NULL,
        Description nvarchar(500) NOT NULL,
        CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Code)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Code = N'UNCATEGORIZED')
BEGIN
    INSERT INTO dbo.Categories (Code, Name, Description)
    VALUES (N'UNCATEGORIZED', N'Senza categoria', N'Categoria tecnica per dati esistenti senza mapping applicativo.');
END;

IF COL_LENGTH(N'dbo.Products', N'Code') IS NULL
BEGIN
    ALTER TABLE dbo.Products ADD Code nvarchar(30) NULL;
END;
GO

UPDATE dbo.Products
SET Code = CONCAT(N'LEGACY-PRD-', Id)
WHERE Code IS NULL OR LTRIM(RTRIM(Code)) = N'';

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Products')
        AND name = N'Code'
        AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Products ALTER COLUMN Code nvarchar(30) NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Products')
        AND name = N'IX_Products_Code'
)
BEGIN
    CREATE UNIQUE INDEX IX_Products_Code ON dbo.Products (Code);
END;

IF COL_LENGTH(N'dbo.Products', N'CategoryCode') IS NULL
BEGIN
    ALTER TABLE dbo.Products ADD CategoryCode nvarchar(20) NULL;
END;
GO

UPDATE dbo.Products
SET CategoryCode = N'UNCATEGORIZED'
WHERE CategoryCode IS NULL OR LTRIM(RTRIM(CategoryCode)) = N'';

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Products')
        AND name = N'CategoryCode'
        AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Products ALTER COLUMN CategoryCode nvarchar(20) NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Products')
        AND name = N'IX_Products_CategoryCode'
)
BEGIN
    CREATE INDEX IX_Products_CategoryCode ON dbo.Products (CategoryCode);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Products_Categories_CategoryCode'
        AND parent_object_id = OBJECT_ID(N'dbo.Products')
)
BEGIN
    ALTER TABLE dbo.Products WITH CHECK
    ADD CONSTRAINT FK_Products_Categories_CategoryCode
        FOREIGN KEY (CategoryCode) REFERENCES dbo.Categories (Code);
END;

IF COL_LENGTH(N'dbo.Customers', N'AvatarInitials') IS NULL
BEGIN
    ALTER TABLE dbo.Customers ADD AvatarInitials nvarchar(5) NULL;
END;
GO

UPDATE dbo.Customers
SET AvatarInitials =
    UPPER(
        LEFT(LTRIM(RTRIM(Name)), 1)
        + CASE
            WHEN CHARINDEX(N' ', LTRIM(RTRIM(Name))) > 0
                THEN SUBSTRING(LTRIM(RTRIM(Name)), CHARINDEX(N' ', LTRIM(RTRIM(Name))) + 1, 1)
            ELSE N''
        END)
WHERE AvatarInitials IS NULL OR LTRIM(RTRIM(AvatarInitials)) = N'';

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Customers')
        AND name = N'AvatarInitials'
        AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Customers ALTER COLUMN AvatarInitials nvarchar(5) NOT NULL;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Customers')
        AND name = N'Address'
        AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.Customers ALTER COLUMN Address nvarchar(200) NULL;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Orders')
        AND name = N'OrderNumber'
        AND max_length = -1
)
BEGIN
    ALTER TABLE dbo.Orders ALTER COLUMN OrderNumber nvarchar(30) NOT NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Orders')
        AND name = N'IX_Orders_OrderNumber'
)
BEGIN
    CREATE UNIQUE INDEX IX_Orders_OrderNumber ON dbo.Orders (OrderNumber);
END;

COMMIT TRANSACTION;
