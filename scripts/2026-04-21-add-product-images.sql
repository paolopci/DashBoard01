SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Products', N'ImageUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Products ADD ImageUrl nvarchar(200) NULL;
END;
GO

UPDATE dbo.Products
SET ImageUrl = CONCAT(
    N'https://loremflickr.com/320/240/',
    CASE
        WHEN LOWER(Name) LIKE N'%calcolatrice%' THEN N'calculator,office/all'
        WHEN LOWER(Name) LIKE N'%cuffie%' OR LOWER(Name) LIKE N'%headset%' THEN N'headphones,audio/all'
        WHEN LOWER(Name) LIKE N'%laptop%' OR LOWER(Name) LIKE N'%notebook%' THEN N'laptop,computer/all'
        WHEN LOWER(Name) LIKE N'%tastiera%' THEN N'keyboard,computer/all'
        WHEN LOWER(Name) LIKE N'%monitor%' THEN N'monitor,computer/all'
        WHEN LOWER(Name) LIKE N'%mouse%' THEN N'mouse,computer/all'
        WHEN LOWER(Name) LIKE N'%workstation%' OR LOWER(Name) LIKE N'%desktop%' THEN N'desktop,computer/all'
        WHEN LOWER(Name) LIKE N'%webcam%' THEN N'webcam,video/all'
        WHEN LOWER(Name) LIKE N'%ssd%' OR LOWER(Name) LIKE N'%hard disk%' OR LOWER(Name) LIKE N'%storage%' THEN N'harddrive,storage/all'
        WHEN LOWER(Name) LIKE N'%stampante%' OR LOWER(Name) LIKE N'%multifunzione%' THEN N'printer,office/all'
        WHEN LOWER(Name) LIKE N'%hub%' OR LOWER(Name) LIKE N'%usb%' THEN N'usb,technology/all'
        WHEN LOWER(Name) LIKE N'%scanner%' OR LOWER(Name) LIKE N'%barcode%' THEN N'scanner,office/all'
        WHEN LOWER(Name) LIKE N'%nas%' OR LOWER(Name) LIKE N'%backup%' THEN N'server,storage/all'
        WHEN LOWER(Name) LIKE N'%microfono%' THEN N'microphone,audio/all'
        WHEN LOWER(Name) LIKE N'%speaker%' THEN N'speaker,audio/all'
        WHEN LOWER(Name) LIKE N'%router%' OR LOWER(Name) LIKE N'%switch%' OR LOWER(Name) LIKE N'%firewall%' THEN N'network,router/all'
        WHEN LOWER(Name) LIKE N'%access point%' OR LOWER(Name) LIKE N'%bridge%' THEN N'wifi,network/all'
        WHEN LOWER(Name) LIKE N'%etichettatrice%' THEN N'label,printer/all'
        WHEN LOWER(Name) LIKE N'%distruggidocumenti%' THEN N'shredder,office/all'
        WHEN LOWER(Name) LIKE N'%proiettore%' THEN N'projector,office/all'
        WHEN LOWER(Name) LIKE N'%plotter%' THEN N'plotter,printer/all'
        WHEN LOWER(Name) LIKE N'%controller%' THEN N'gamepad,gaming/all'
        WHEN LOWER(Name) LIKE N'%console%' THEN N'console,gaming/all'
        ELSE N'technology,product/all'
    END,
    N'?lock=',
    Id
)
WHERE ImageUrl IS NULL OR LTRIM(RTRIM(ImageUrl)) = N'';

COMMIT TRANSACTION;
