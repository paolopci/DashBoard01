-- Reset all addresses to NULL
UPDATE dbo.Customers SET Address = NULL;
GO

-- Generate unique addresses using a pattern based on customer ID
-- Format: Via/Corso/Viale/Città + numero civico unico + città
WITH Cities AS (
    SELECT TOP 100 PERCENT
        Value AS City
    FROM STRING_SPLIT('Roma,Milano,Torino,Napoli,Firenze,Bologna,Venezia,Genova,Palermo,Bari,Catania,Verona,Messina,Padova,Trieste,Brescia,Parma,Prato,Modena,Reggio Emilia,Perugia,Livorno,Ravenna,Cagliari,Foggia,Rimini,Salerno', ',')
    WHERE RANDBIN(1) = 0
    ORDER BY NEWID()
),
Streets AS (
    SELECT TOP 100 PERCENT
        Value AS Street
    FROM STRING_SPLIT('Via Roma,Via Milano,Via Torino,Via Firenze,Via Bologna,Via Venezia,Via Genova,Via Napoli,Via Dante,Via Manzoni,Via Verdi,Via Garibaldi,Via Mazzini,Via Colosseo,Via del Corso,Via Nazionale,Viale Roma,Viale Milano,Viale Torino,Viale Firenze,Viale Bologna,Viale Venezia,Viale Genova,Viale Napoli,Viale Dante,Viale Manzoni,Viale Verdi,Viale Garibaldi,Viale Mazzini,Viale Colosseo,Piazza Roma,Piazza Milano,Piazza Torino,Piazza Firenze,Piazza Bologna,Piazza Venezia,Piazza Genova,Piazza Napoli,Piazza Dante,Piazza Manzoni,Piazza Verdi,Piazza Garibaldi,Piazza Mazzini,Piazza del Duomo,Piazza della Repubblica,Piazza Navona', ',')
    ORDER BY NEWID()
),
CustomersWithCities AS (
    SELECT
        Id,
        Name,
        (SELECT TOP 1 City FROM Cities ORDER BY NEWID()) AS City,
        (SELECT TOP 1 Street FROM Streets ORDER BY NEWID()) AS Street
    FROM dbo.Customers
)
UPDATE c
SET
    c.Address = COALESCE(s.Street, 'Via Roma 1') + ' ' + CAST(c.Id * 7 % 499 + 1 AS VARCHAR) + ', ' + COALESCE(c.City, 'Roma')
FROM dbo.Customers c
INNER JOIN CustomersWithCities t ON c.Id = t.Id
OUTER APPLY (SELECT TOP 1 Street FROM Streets ORDER BY NEWID()) s
OUTER APPLY (SELECT TOP 1 City FROM Cities ORDER BY NEWID()) c;
GO

-- Verify update
SELECT COUNT(*) AS TotalCustomers,
       COUNT(Address) AS WithAddress,
       COUNT(CASE WHEN Address IS NULL THEN 1 END) AS NullAddresses
FROM dbo.Customers;
GO

SELECT TOP 10 Address FROM dbo.Customers ORDER BY NEWID();
