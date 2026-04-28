IF OBJECT_ID(N'[dbo].[ItalianRegions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ItalianRegions]
    (
        [Code] nvarchar(2) NOT NULL CONSTRAINT [PK_ItalianRegions] PRIMARY KEY,
        [Name] nvarchar(100) NOT NULL,
        [Nuts1Code] nvarchar(5) NULL,
        [Nuts2Code] nvarchar(5) NULL
    );
END;

IF OBJECT_ID(N'[dbo].[ItalianProvinces]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ItalianProvinces]
    (
        [Code] nvarchar(3) NOT NULL CONSTRAINT [PK_ItalianProvinces] PRIMARY KEY,
        [RegionCode] nvarchar(2) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Abbreviation] nvarchar(4) NULL,
        [Nuts3Code] nvarchar(5) NULL,
        CONSTRAINT [FK_ItalianProvinces_ItalianRegions_RegionCode]
            FOREIGN KEY ([RegionCode]) REFERENCES [dbo].[ItalianRegions] ([Code])
    );
END;

IF OBJECT_ID(N'[dbo].[ItalianMunicipalities]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ItalianMunicipalities]
    (
        [Code] nvarchar(6) NOT NULL CONSTRAINT [PK_ItalianMunicipalities] PRIMARY KEY,
        [ProvinceCode] nvarchar(3) NOT NULL,
        [RegionCode] nvarchar(2) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CadastralCode] nvarchar(4) NULL,
        [IsProvinceCapital] bit NOT NULL CONSTRAINT [DF_ItalianMunicipalities_IsProvinceCapital] DEFAULT (0),
        CONSTRAINT [FK_ItalianMunicipalities_ItalianProvinces_ProvinceCode]
            FOREIGN KEY ([ProvinceCode]) REFERENCES [dbo].[ItalianProvinces] ([Code]),
        CONSTRAINT [FK_ItalianMunicipalities_ItalianRegions_RegionCode]
            FOREIGN KEY ([RegionCode]) REFERENCES [dbo].[ItalianRegions] ([Code])
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianRegions_Name'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianRegions]'))
BEGIN
    CREATE UNIQUE INDEX [IX_ItalianRegions_Name] ON [dbo].[ItalianRegions] ([Name]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianProvinces_Name'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianProvinces]'))
BEGIN
    CREATE INDEX [IX_ItalianProvinces_Name] ON [dbo].[ItalianProvinces] ([Name]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianProvinces_Abbreviation'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianProvinces]'))
BEGIN
    CREATE INDEX [IX_ItalianProvinces_Abbreviation] ON [dbo].[ItalianProvinces] ([Abbreviation]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianMunicipalities_Name'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianMunicipalities]'))
BEGIN
    CREATE INDEX [IX_ItalianMunicipalities_Name] ON [dbo].[ItalianMunicipalities] ([Name]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianMunicipalities_ProvinceCode'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianMunicipalities]'))
BEGIN
    CREATE INDEX [IX_ItalianMunicipalities_ProvinceCode] ON [dbo].[ItalianMunicipalities] ([ProvinceCode]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_ItalianMunicipalities_ProvinceCode_Name'
      AND [object_id] = OBJECT_ID(N'[dbo].[ItalianMunicipalities]'))
BEGIN
    CREATE INDEX [IX_ItalianMunicipalities_ProvinceCode_Name]
    ON [dbo].[ItalianMunicipalities] ([ProvinceCode], [Name]);
END;
