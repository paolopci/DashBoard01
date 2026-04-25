SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.OrderStatusHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderStatusHistory
    (
        Id int IDENTITY(1, 1) NOT NULL,
        OrderId int NOT NULL,
        FromStatus int NULL,
        ToStatus int NOT NULL,
        ChangedAt datetime2 NOT NULL,
        ChangedBy nvarchar(256) NULL,
        Reason nvarchar(500) NULL,
        CorrelationId nvarchar(100) NULL,
        CONSTRAINT PK_OrderStatusHistory PRIMARY KEY CLUSTERED (Id)
    );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OrderStatusHistory')
        AND name = N'IX_OrderStatusHistory_OrderId'
)
BEGIN
    CREATE INDEX IX_OrderStatusHistory_OrderId ON dbo.OrderStatusHistory (OrderId);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OrderStatusHistory')
        AND name = N'IX_OrderStatusHistory_ChangedAt'
)
BEGIN
    CREATE INDEX IX_OrderStatusHistory_ChangedAt ON dbo.OrderStatusHistory (ChangedAt);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_OrderStatusHistory_Orders_OrderId'
        AND parent_object_id = OBJECT_ID(N'dbo.OrderStatusHistory')
)
BEGIN
    ALTER TABLE dbo.OrderStatusHistory WITH CHECK
    ADD CONSTRAINT FK_OrderStatusHistory_Orders_OrderId
        FOREIGN KEY (OrderId) REFERENCES dbo.Orders (Id)
        ON DELETE CASCADE;
END;

COMMIT TRANSACTION;
