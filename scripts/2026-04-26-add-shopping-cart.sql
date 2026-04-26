IF OBJECT_ID(N'[dbo].[Carts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Carts]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Carts] PRIMARY KEY,
        [CustomerEmail] nvarchar(256) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_Carts_CustomerEmail'
      AND [object_id] = OBJECT_ID(N'[dbo].[Carts]'))
BEGIN
    CREATE UNIQUE INDEX [IX_Carts_CustomerEmail] ON [dbo].[Carts] ([CustomerEmail]);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_Carts_ExpiresAt'
      AND [object_id] = OBJECT_ID(N'[dbo].[Carts]'))
BEGIN
    CREATE INDEX [IX_Carts_ExpiresAt] ON [dbo].[Carts] ([ExpiresAt]);
END;

IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItems]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CartItems] PRIMARY KEY,
        [CartId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [dbo].[Carts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_CartItems_CartId'
      AND [object_id] = OBJECT_ID(N'[dbo].[CartItems]'))
BEGIN
    CREATE INDEX [IX_CartItems_CartId] ON [dbo].[CartItems] ([CartId]);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_CartItems_CartId_ProductId'
      AND [object_id] = OBJECT_ID(N'[dbo].[CartItems]'))
BEGIN
    CREATE UNIQUE INDEX [IX_CartItems_CartId_ProductId] ON [dbo].[CartItems] ([CartId], [ProductId]);
END;
