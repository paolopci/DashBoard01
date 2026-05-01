SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('OrderCheckoutDetails', 'StripeCheckoutSessionId') IS NULL
BEGIN
    ALTER TABLE OrderCheckoutDetails
    ADD StripeCheckoutSessionId nvarchar(120) NULL;
END;
GO

IF COL_LENGTH('OrderCheckoutDetails', 'StripePaymentIntentId') IS NULL
BEGIN
    ALTER TABLE OrderCheckoutDetails
    ADD StripePaymentIntentId nvarchar(120) NULL;
END;
GO

IF COL_LENGTH('OrderCheckoutDetails', 'StripePaymentStatus') IS NULL
BEGIN
    ALTER TABLE OrderCheckoutDetails
    ADD StripePaymentStatus nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_OrderCheckoutDetails_StripeCheckoutSessionId'
      AND object_id = OBJECT_ID('OrderCheckoutDetails')
)
BEGIN
    CREATE UNIQUE INDEX IX_OrderCheckoutDetails_StripeCheckoutSessionId
    ON OrderCheckoutDetails (StripeCheckoutSessionId)
    WHERE StripeCheckoutSessionId IS NOT NULL;
END;
