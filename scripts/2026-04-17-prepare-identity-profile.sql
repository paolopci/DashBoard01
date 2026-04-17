SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH('dbo.AspNetUsers', 'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD FirstName nvarchar(100) NOT NULL
            CONSTRAINT DF_AspNetUsers_FirstName DEFAULT N'';
END;
ELSE IF
(
    SELECT CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'AspNetUsers'
      AND COLUMN_NAME = 'FirstName'
) < 100
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ALTER COLUMN FirstName nvarchar(100) NOT NULL;
END;

IF COL_LENGTH('dbo.AspNetUsers', 'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD LastName nvarchar(100) NOT NULL
            CONSTRAINT DF_AspNetUsers_LastName DEFAULT N'';
END;
ELSE IF
(
    SELECT CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'AspNetUsers'
      AND COLUMN_NAME = 'LastName'
) < 100
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ALTER COLUMN LastName nvarchar(100) NOT NULL;
END;

IF COL_LENGTH('dbo.AspNetUsers', 'DateOfBirth') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD DateOfBirth datetime2 NOT NULL
            CONSTRAINT DF_AspNetUsers_DateOfBirth DEFAULT CONVERT(datetime2, '1900-01-01');
END;

IF COL_LENGTH('dbo.AspNetUsers', 'City') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD City nvarchar(100) NOT NULL
            CONSTRAINT DF_AspNetUsers_City DEFAULT N'';
END;

IF COL_LENGTH('dbo.AspNetUsers', 'Country') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD Country nvarchar(100) NOT NULL
            CONSTRAINT DF_AspNetUsers_Country DEFAULT N'';
END;

IF COL_LENGTH('dbo.AspNetUsers', 'FiscalCode') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD FiscalCode nvarchar(16) NOT NULL
            CONSTRAINT DF_AspNetUsers_FiscalCode DEFAULT N'';
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_AspNetUsers_FiscalCode'
      AND object_id = OBJECT_ID(N'dbo.AspNetUsers')
)
BEGIN
    EXEC(N'
        CREATE UNIQUE INDEX IX_AspNetUsers_FiscalCode
            ON dbo.AspNetUsers(FiscalCode)
            WHERE FiscalCode <> N'''';
    ');
END;

COMMIT TRANSACTION;
