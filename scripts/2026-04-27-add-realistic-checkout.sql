IF OBJECT_ID(N'[dbo].[CheckoutSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CheckoutSessions]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CheckoutSessions] PRIMARY KEY,
        [CustomerEmail] nvarchar(256) NOT NULL,
        [CurrentStep] int NOT NULL,
        [TotalItems] int NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [ShippingFullName] nvarchar(120) NULL,
        [ShippingAddressLine] nvarchar(200) NULL,
        [ShippingCity] nvarchar(100) NULL,
        [ShippingPostalCode] nvarchar(20) NULL,
        [ShippingCountry] nvarchar(100) NULL,
        [ShippingPhone] nvarchar(30) NULL,
        [BillingSameAsShipping] bit NOT NULL CONSTRAINT [DF_CheckoutSessions_BillingSameAsShipping] DEFAULT 1,
        [BillingFullName] nvarchar(120) NULL,
        [BillingAddressLine] nvarchar(200) NULL,
        [BillingCity] nvarchar(100) NULL,
        [BillingPostalCode] nvarchar(20) NULL,
        [BillingCountry] nvarchar(100) NULL,
        [BillingVatNumber] nvarchar(40) NULL,
        [DeliveryMethod] nvarchar(30) NOT NULL CONSTRAINT [DF_CheckoutSessions_DeliveryMethod] DEFAULT N'standard',
        [PaymentMethod] nvarchar(30) NOT NULL CONSTRAINT [DF_CheckoutSessions_PaymentMethod] DEFAULT N'pending',
        [CreatedOrderId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_CheckoutSessions_CustomerEmail'
      AND [object_id] = OBJECT_ID(N'[dbo].[CheckoutSessions]'))
BEGIN
    CREATE UNIQUE INDEX [IX_CheckoutSessions_CustomerEmail] ON [dbo].[CheckoutSessions] ([CustomerEmail]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_CheckoutSessions_ExpiresAt'
      AND [object_id] = OBJECT_ID(N'[dbo].[CheckoutSessions]'))
BEGIN
    CREATE INDEX [IX_CheckoutSessions_ExpiresAt] ON [dbo].[CheckoutSessions] ([ExpiresAt]);
END;

IF OBJECT_ID(N'[dbo].[OrderCheckoutDetails]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderCheckoutDetails]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_OrderCheckoutDetails] PRIMARY KEY,
        [OrderId] int NOT NULL,
        [ShippingFullName] nvarchar(120) NOT NULL,
        [ShippingAddressLine] nvarchar(200) NOT NULL,
        [ShippingCity] nvarchar(100) NOT NULL,
        [ShippingPostalCode] nvarchar(20) NOT NULL,
        [ShippingCountry] nvarchar(100) NOT NULL,
        [ShippingPhone] nvarchar(30) NOT NULL,
        [BillingSameAsShipping] bit NOT NULL,
        [BillingFullName] nvarchar(120) NOT NULL,
        [BillingAddressLine] nvarchar(200) NOT NULL,
        [BillingCity] nvarchar(100) NOT NULL,
        [BillingPostalCode] nvarchar(20) NOT NULL,
        [BillingCountry] nvarchar(100) NOT NULL,
        [BillingVatNumber] nvarchar(40) NULL,
        [DeliveryMethod] nvarchar(30) NOT NULL,
        [PaymentMethod] nvarchar(30) NOT NULL,
        [PaymentStatus] nvarchar(30) NOT NULL,
        [TestTransactionReference] nvarchar(80) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [FK_OrderCheckoutDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_OrderCheckoutDetails_OrderId'
      AND [object_id] = OBJECT_ID(N'[dbo].[OrderCheckoutDetails]'))
BEGIN
    CREATE UNIQUE INDEX [IX_OrderCheckoutDetails_OrderId] ON [dbo].[OrderCheckoutDetails] ([OrderId]);
END;
