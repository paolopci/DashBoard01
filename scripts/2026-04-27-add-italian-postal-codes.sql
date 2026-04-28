IF OBJECT_ID(N'[dbo].[ItalianPostalCodes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ItalianPostalCodes]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ItalianPostalCodes] PRIMARY KEY,
        [ProvinceName] nvarchar(100) NOT NULL,
        [ProvinceCode] nvarchar(4) NOT NULL,
        [CityName] nvarchar(100) NOT NULL,
        [PostalCode] nvarchar(10) NOT NULL
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianPostalCodes_ProvinceName'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianPostalCodes]'))
BEGIN
    CREATE INDEX [IX_ItalianPostalCodes_ProvinceName] ON [dbo].[ItalianPostalCodes] ([ProvinceName]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianPostalCodes_CityName'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianPostalCodes]'))
BEGIN
    CREATE INDEX [IX_ItalianPostalCodes_CityName] ON [dbo].[ItalianPostalCodes] ([CityName]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianPostalCodes_ProvinceName_CityName_PostalCode'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianPostalCodes]'))
BEGIN
    CREATE UNIQUE INDEX [IX_ItalianPostalCodes_ProvinceName_CityName_PostalCode]
    ON [dbo].[ItalianPostalCodes] ([ProvinceName], [CityName], [PostalCode]);
END;

DECLARE @ItalianPostalCodes TABLE
(
    [ProvinceName] nvarchar(100) NOT NULL,
    [ProvinceCode] nvarchar(4) NOT NULL,
    [CityName] nvarchar(100) NOT NULL,
    [PostalCode] nvarchar(10) NOT NULL
);

INSERT INTO @ItalianPostalCodes ([ProvinceName], [ProvinceCode], [CityName], [PostalCode])
VALUES
    (N'Ancona', N'AN', N'Ancona', N'60121'),
    (N'Ancona', N'AN', N'Ancona', N'60122'),
    (N'Pesaro e Urbino', N'PU', N'Fano', N'61032'),
    (N'Pesaro e Urbino', N'PU', N'Pesaro', N'61121'),
    (N'Pesaro e Urbino', N'PU', N'Pesaro', N'61122'),
    (N'Roma', N'RM', N'Roma', N'00100'),
    (N'Milano', N'MI', N'Milano', N'20100'),
    (N'Torino', N'TO', N'Torino', N'10100'),
    (N'Bologna', N'BO', N'Bologna', N'40100'),
    (N'Firenze', N'FI', N'Firenze', N'50100'),
    (N'Napoli', N'NA', N'Napoli', N'80100'),
    (N'Bari', N'BA', N'Bari', N'70100'),
    (N'Palermo', N'PA', N'Palermo', N'90100'),
    (N'Cagliari', N'CA', N'Cagliari', N'09100');

INSERT INTO [dbo].[ItalianPostalCodes] ([ProvinceName], [ProvinceCode], [CityName], [PostalCode])
SELECT source.[ProvinceName], source.[ProvinceCode], source.[CityName], source.[PostalCode]
FROM @ItalianPostalCodes AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[ItalianPostalCodes] AS existing
    WHERE existing.[ProvinceName] = source.[ProvinceName]
      AND existing.[CityName] = source.[CityName]
      AND existing.[PostalCode] = source.[PostalCode]
);
