using System.Globalization;
using System.IO.Compression;
using System.Xml;
using DashboardOrders.Data;
using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Services;

public sealed class ItalianTerritoryImporter(DashboardOrdersDbContext dbContext)
{
    private const string IstatSheetNamePrefix = "CODICI al ";
    private const string PostalCodePlaceholderMunicipalityCode = "999999";

    public async Task<ItalianTerritoryImportResult> ImportAsync(
        ItalianTerritoryImportOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var istatRows = ReadIstatRows(options.IstatMunicipalitiesXlsxPath);
        ValidateIstatRows(istatRows, options.ExpectedRegionCount, options.ExpectedMunicipalityCount);

        var regions = CreateRegions(istatRows);
        var provinces = CreateProvinces(istatRows);
        var municipalities = CreateMunicipalities(istatRows);
        var postalCodes = ReadPostalCodes(options.PostalCodesCsvPath, istatRows, out var skippedPostalCodes);

        await ExecuteReplacementAsync(regions, provinces, municipalities, postalCodes, cancellationToken);

        return new ItalianTerritoryImportResult(
            regions.Count,
            provinces.Count,
            municipalities.Count,
            postalCodes.Count,
            skippedPostalCodes);
    }

    private async Task ExecuteReplacementAsync(
        IReadOnlyCollection<ItalianRegionEntity> regions,
        IReadOnlyCollection<ItalianProvinceEntity> provinces,
        IReadOnlyCollection<ItalianMunicipalityEntity> municipalities,
        IReadOnlyCollection<ItalianPostalCodeEntity> postalCodes,
        CancellationToken cancellationToken)
    {
        if (string.Equals(dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            await ReplaceDataAsync(regions, provinces, municipalities, postalCodes, cancellationToken);
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await EnsureSqlServerTablesAsync(cancellationToken);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await ReplaceDataAsync(regions, provinces, municipalities, postalCodes, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private async Task EnsureSqlServerTablesAsync(CancellationToken cancellationToken)
    {
        if (!string.Equals(dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.SqlServer", StringComparison.Ordinal))
        {
            return;
        }

        await ExecuteSqlScriptIfExistsAsync("scripts/2026-04-27-add-italian-postal-codes.sql", cancellationToken);
        await ExecuteSqlScriptIfExistsAsync("scripts/2026-04-28-add-italian-administrative-territories.sql", cancellationToken);
    }

    private async Task ExecuteSqlScriptIfExistsAsync(string scriptPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(scriptPath))
        {
            throw new ItalianTerritoryImportException($"Script SQL richiesto non trovato: {scriptPath}");
        }

        var sql = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        foreach (var batch in SplitSqlBatches(sql))
        {
            if (!string.IsNullOrWhiteSpace(batch))
            {
                await dbContext.Database.ExecuteSqlRawAsync(batch, cancellationToken);
            }
        }
    }

    private static IEnumerable<string> SplitSqlBatches(string sql)
    {
        using var reader = new StringReader(sql);
        var batch = new StringWriter(CultureInfo.InvariantCulture);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
            {
                yield return batch.ToString();
                batch.GetStringBuilder().Clear();
                continue;
            }

            batch.WriteLine(line);
        }

        yield return batch.ToString();
    }

    private async Task ReplaceDataAsync(
        IReadOnlyCollection<ItalianRegionEntity> regions,
        IReadOnlyCollection<ItalianProvinceEntity> provinces,
        IReadOnlyCollection<ItalianMunicipalityEntity> municipalities,
        IReadOnlyCollection<ItalianPostalCodeEntity> postalCodes,
        CancellationToken cancellationToken)
    {
        dbContext.ItalianPostalCodes.RemoveRange(dbContext.ItalianPostalCodes);
        dbContext.ItalianMunicipalities.RemoveRange(dbContext.ItalianMunicipalities);
        dbContext.ItalianProvinces.RemoveRange(dbContext.ItalianProvinces);
        dbContext.ItalianRegions.RemoveRange(dbContext.ItalianRegions);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ItalianRegions.AddRange(regions);
        dbContext.ItalianProvinces.AddRange(provinces);
        dbContext.ItalianMunicipalities.AddRange(municipalities);
        dbContext.ItalianPostalCodes.AddRange(postalCodes);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<IstatMunicipalityRow> ReadIstatRows(string xlsxPath)
    {
        if (string.IsNullOrWhiteSpace(xlsxPath) || !File.Exists(xlsxPath))
        {
            throw new ItalianTerritoryImportException($"File ISTAT non trovato: {xlsxPath}");
        }

        using var archive = ZipFile.OpenRead(xlsxPath);
        var workbook = ReadXmlEntry(archive, "xl/workbook.xml");
        var relationshipMap = ReadWorkbookRelationships(archive);
        var sheetRelationshipId = FindSheetRelationshipId(workbook);
        if (!relationshipMap.TryGetValue(sheetRelationshipId, out var sheetTarget))
        {
            throw new ItalianTerritoryImportException($"Foglio ISTAT principale non trovato: {sheetRelationshipId}");
        }

        var sharedStrings = ReadSharedStrings(archive);
        var sheetPath = "xl/" + sheetTarget.Replace('\\', '/').TrimStart('/');
        var sheet = ReadXmlEntry(archive, sheetPath);
        var rows = ReadWorksheetRows(sheet, sharedStrings);
        if (rows.Count < 2)
        {
            throw new ItalianTerritoryImportException("Il file ISTAT non contiene righe dati.");
        }

        var headers = rows[0];
        var indexes = CreateIstatColumnIndexes(headers);

        return rows
            .Skip(1)
            .Where(row => row.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
            .Select(row => new IstatMunicipalityRow(
                GetValue(row, indexes.RegionCode),
                GetValue(row, indexes.ProvinceCode),
                GetValue(row, indexes.MunicipalityCode),
                GetValue(row, indexes.MunicipalityName),
                GetValue(row, indexes.RegionName),
                GetValue(row, indexes.ProvinceName),
                GetValue(row, indexes.ProvinceAbbreviation),
                GetValue(row, indexes.IsProvinceCapital),
                GetValue(row, indexes.CadastralCode),
                GetValue(row, indexes.Nuts1Code),
                GetValue(row, indexes.Nuts2Code),
                GetValue(row, indexes.Nuts3Code)))
            .ToList();
    }

    private static Dictionary<string, string> ReadWorkbookRelationships(ZipArchive archive)
    {
        var relationships = ReadXmlEntry(archive, "xl/_rels/workbook.xml.rels");
        var manager = CreateNamespaceManager(relationships);
        manager.AddNamespace("r", "http://schemas.openxmlformats.org/package/2006/relationships");

        return relationships
            .SelectNodes("//r:Relationship", manager)!
            .Cast<XmlElement>()
            .ToDictionary(
                relationship => relationship.GetAttribute("Id"),
                relationship => relationship.GetAttribute("Target"),
                StringComparer.OrdinalIgnoreCase);
    }

    private static string FindSheetRelationshipId(XmlDocument workbook)
    {
        var manager = CreateNamespaceManager(workbook);
        manager.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var sheet = workbook
            .SelectNodes("//x:sheet", manager)!
            .Cast<XmlElement>()
            .FirstOrDefault(element => element.GetAttribute("name").StartsWith(IstatSheetNamePrefix, StringComparison.OrdinalIgnoreCase));

        return sheet?.GetAttribute("r:id")
            ?? throw new ItalianTerritoryImportException("Foglio ISTAT con codici comunali non trovato.");
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        var xml = ReadXmlEntry(entry);
        var manager = CreateNamespaceManager(xml);

        return xml
            .SelectNodes("//x:si", manager)!
            .Cast<XmlElement>()
            .Select(element => element.InnerText)
            .ToList();
    }

    private static List<Dictionary<int, string>> ReadWorksheetRows(XmlDocument sheet, IReadOnlyList<string> sharedStrings)
    {
        var manager = CreateNamespaceManager(sheet);
        var rows = new List<Dictionary<int, string>>();

        foreach (XmlElement row in sheet.SelectNodes("//x:sheetData/x:row", manager)!)
        {
            var values = new Dictionary<int, string>();
            foreach (XmlElement cell in row.SelectNodes("x:c", manager)!)
            {
                var columnIndex = GetColumnIndex(cell.GetAttribute("r"));
                values[columnIndex] = GetCellValue(cell, sharedStrings, manager);
            }

            rows.Add(values);
        }

        return rows;
    }

    private static string GetCellValue(XmlElement cell, IReadOnlyList<string> sharedStrings, XmlNamespaceManager manager)
    {
        var type = cell.GetAttribute("t");
        if (string.Equals(type, "inlineStr", StringComparison.Ordinal))
        {
            return cell.SelectSingleNode("x:is", manager)?.InnerText.Trim() ?? string.Empty;
        }

        var rawValue = cell.SelectSingleNode("x:v", manager)?.InnerText.Trim() ?? string.Empty;
        return string.Equals(type, "s", StringComparison.Ordinal) && int.TryParse(rawValue, out var sharedStringIndex)
            ? sharedStrings[sharedStringIndex].Trim()
            : rawValue;
    }

    private static IstatColumnIndexes CreateIstatColumnIndexes(Dictionary<int, string> headers)
    {
        var normalizedHeaders = headers.ToDictionary(
            pair => NormalizeHeader(pair.Value),
            pair => pair.Key,
            StringComparer.OrdinalIgnoreCase);

        return new IstatColumnIndexes(
            GetHeaderIndex(normalizedHeaders, "Codice Regione"),
            GetHeaderIndex(normalizedHeaders, "Codice dell'Unita territoriale sovracomunale"),
            GetHeaderIndex(normalizedHeaders, "Codice Comune formato alfanumerico"),
            GetHeaderIndex(normalizedHeaders, "Denominazione in italiano"),
            GetHeaderIndex(normalizedHeaders, "Denominazione Regione"),
            GetHeaderIndex(normalizedHeaders, "Denominazione dell'Unita territoriale sovracomunale"),
            GetHeaderIndex(normalizedHeaders, "Sigla automobilistica"),
            GetHeaderIndex(normalizedHeaders, "Flag Comune capoluogo di Provincia/Citta metropolitana/libero consorzio"),
            GetHeaderIndex(normalizedHeaders, "Codice Catastale del Comune"),
            GetHeaderIndex(normalizedHeaders, "Codice NUTS1 2024"),
            GetHeaderIndex(normalizedHeaders, "Codice NUTS2 2024"),
            GetHeaderIndex(normalizedHeaders, "Codice NUTS3 2024"));
    }

    private static int GetHeaderIndex(IReadOnlyDictionary<string, int> headers, string header)
    {
        var normalizedHeader = NormalizeHeader(header);
        if (headers.TryGetValue(normalizedHeader, out var index))
        {
            return index;
        }

        var matchingHeader = headers.FirstOrDefault(pair => pair.Key.StartsWith(normalizedHeader, StringComparison.OrdinalIgnoreCase));
        return matchingHeader.Value > 0
            ? matchingHeader.Value
            : throw new ItalianTerritoryImportException($"Colonna ISTAT obbligatoria mancante: {header}");
    }

    private static string NormalizeHeader(string value)
    {
        return value
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Replace('à', 'a')
            .Replace('è', 'e')
            .Replace('é', 'e')
            .Replace('ì', 'i')
            .Replace('ò', 'o')
            .Replace('ù', 'u')
            .Replace("  ", " ")
            .Trim();
    }

    private static void ValidateIstatRows(IReadOnlyCollection<IstatMunicipalityRow> rows, int expectedRegionCount, int expectedMunicipalityCount)
    {
        var missingRequired = rows.Where(row =>
                string.IsNullOrWhiteSpace(row.RegionCode) ||
                string.IsNullOrWhiteSpace(row.ProvinceCode) ||
                string.IsNullOrWhiteSpace(row.MunicipalityCode) ||
                string.IsNullOrWhiteSpace(row.MunicipalityName) ||
                string.IsNullOrWhiteSpace(row.RegionName) ||
                string.IsNullOrWhiteSpace(row.ProvinceName))
            .ToList();
        if (missingRequired.Count > 0)
        {
            throw new ItalianTerritoryImportException($"Il dataset ISTAT contiene {missingRequired.Count} righe con dati obbligatori mancanti.");
        }

        var municipalityCount = rows.Count;
        if (municipalityCount != expectedMunicipalityCount)
        {
            throw new ItalianTerritoryImportException($"Numero comuni ISTAT non valido: attesi {expectedMunicipalityCount}, trovati {municipalityCount}.");
        }

        var regionCount = rows.Select(row => row.RegionCode).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (regionCount != expectedRegionCount)
        {
            throw new ItalianTerritoryImportException($"Numero regioni ISTAT non valido: attese {expectedRegionCount}, trovate {regionCount}.");
        }

        var duplicateMunicipalityCodes = rows
            .GroupBy(row => row.MunicipalityCode, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateMunicipalityCodes.Count > 0)
        {
            throw new ItalianTerritoryImportException($"Codici comune ISTAT duplicati: {string.Join(", ", duplicateMunicipalityCodes.Take(10))}.");
        }
    }

    private static List<ItalianRegionEntity> CreateRegions(IEnumerable<IstatMunicipalityRow> rows)
    {
        return rows
            .GroupBy(row => row.RegionCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.RegionCode, StringComparer.OrdinalIgnoreCase)
            .Select(row => new ItalianRegionEntity
            {
                Code = row.RegionCode,
                Name = row.RegionName,
                Nuts1Code = ToNullableText(row.Nuts1Code),
                Nuts2Code = ToNullableText(row.Nuts2Code)
            })
            .ToList();
    }

    private static List<ItalianProvinceEntity> CreateProvinces(IEnumerable<IstatMunicipalityRow> rows)
    {
        return rows
            .GroupBy(row => row.ProvinceCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.ProvinceCode, StringComparer.OrdinalIgnoreCase)
            .Select(row => new ItalianProvinceEntity
            {
                Code = row.ProvinceCode,
                RegionCode = row.RegionCode,
                Name = row.ProvinceName,
                Abbreviation = ToNullableText(row.ProvinceAbbreviation),
                Nuts3Code = ToNullableText(row.Nuts3Code)
            })
            .ToList();
    }

    private static List<ItalianMunicipalityEntity> CreateMunicipalities(IEnumerable<IstatMunicipalityRow> rows)
    {
        return rows
            .OrderBy(row => row.MunicipalityCode, StringComparer.OrdinalIgnoreCase)
            .Select(row => new ItalianMunicipalityEntity
            {
                Code = row.MunicipalityCode,
                ProvinceCode = row.ProvinceCode,
                RegionCode = row.RegionCode,
                Name = row.MunicipalityName,
                CadastralCode = ToNullableText(row.CadastralCode),
                IsProvinceCapital = row.IsProvinceCapital == "1"
            })
            .ToList();
    }

    private static List<ItalianPostalCodeEntity> ReadPostalCodes(
        string? csvPath,
        IReadOnlyCollection<IstatMunicipalityRow> istatRows,
        out int skippedPostalCodes)
    {
        skippedPostalCodes = 0;
        if (string.IsNullOrWhiteSpace(csvPath))
        {
            return [];
        }

        if (!File.Exists(csvPath))
        {
            throw new ItalianTerritoryImportException($"File CAP non trovato: {csvPath}");
        }

        var municipalitiesByCode = istatRows.ToDictionary(row => row.MunicipalityCode, StringComparer.OrdinalIgnoreCase);
        var result = new List<ItalianPostalCodeEntity>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadLines(csvPath).ToList();
        if (lines.Count == 0)
        {
            throw new ItalianTerritoryImportException("Il file CAP e vuoto.");
        }

        var headers = SplitSemicolonCsvLine(lines[0]);
        var codeIndex = Array.FindIndex(headers, header => string.Equals(header, "codice_istat", StringComparison.OrdinalIgnoreCase));
        var capIndex = Array.FindIndex(headers, header => string.Equals(header, "cap", StringComparison.OrdinalIgnoreCase));
        if (codeIndex < 0 || capIndex < 0)
        {
            throw new ItalianTerritoryImportException("Il file CAP deve contenere le colonne codice_istat e cap.");
        }

        foreach (var line in lines.Skip(1))
        {
            var values = SplitSemicolonCsvLine(line);
            var municipalityCode = GetCsvValue(values, codeIndex);
            var postalCode = GetCsvValue(values, capIndex);
            if (string.IsNullOrWhiteSpace(municipalityCode) || string.IsNullOrWhiteSpace(postalCode))
            {
                skippedPostalCodes++;
                continue;
            }

            if (municipalityCode == PostalCodePlaceholderMunicipalityCode || !municipalitiesByCode.TryGetValue(municipalityCode, out var municipality))
            {
                skippedPostalCodes++;
                continue;
            }

            var key = $"{municipality.ProvinceName}|{municipality.MunicipalityName}|{postalCode}";
            if (!seen.Add(key))
            {
                skippedPostalCodes++;
                continue;
            }

            result.Add(new ItalianPostalCodeEntity
            {
                ProvinceName = municipality.ProvinceName,
                ProvinceCode = municipality.ProvinceAbbreviation,
                CityName = municipality.MunicipalityName,
                PostalCode = postalCode
            });
        }

        return result
            .OrderBy(postalCode => postalCode.ProvinceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(postalCode => postalCode.CityName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(postalCode => postalCode.PostalCode, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string[] SplitSemicolonCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringWriter(CultureInfo.InvariantCulture);
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Write('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (character == ';' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.GetStringBuilder().Clear();
                continue;
            }

            current.Write(character);
        }

        values.Add(current.ToString().Trim());
        return values.ToArray();
    }

    private static string GetCsvValue(IReadOnlyList<string> values, int index)
    {
        return index >= 0 && index < values.Count ? values[index].Trim() : string.Empty;
    }

    private static XmlDocument ReadXmlEntry(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath)
            ?? throw new ItalianTerritoryImportException($"Voce XLSX mancante: {entryPath}");

        return ReadXmlEntry(entry);
    }

    private static XmlDocument ReadXmlEntry(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        var document = new XmlDocument { PreserveWhitespace = false };
        document.Load(stream);
        return document;
    }

    private static XmlNamespaceManager CreateNamespaceManager(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        return manager;
    }

    private static int GetColumnIndex(string cellReference)
    {
        var result = 0;
        foreach (var character in cellReference.TakeWhile(char.IsLetter))
        {
            result = result * 26 + char.ToUpperInvariant(character) - 'A' + 1;
        }

        return result;
    }

    private static string GetValue(IReadOnlyDictionary<int, string> row, int index)
    {
        return row.TryGetValue(index, out var value) ? value.Trim() : string.Empty;
    }

    private static string? ToNullableText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record IstatMunicipalityRow(
        string RegionCode,
        string ProvinceCode,
        string MunicipalityCode,
        string MunicipalityName,
        string RegionName,
        string ProvinceName,
        string ProvinceAbbreviation,
        string IsProvinceCapital,
        string CadastralCode,
        string Nuts1Code,
        string Nuts2Code,
        string Nuts3Code);

    private sealed record IstatColumnIndexes(
        int RegionCode,
        int ProvinceCode,
        int MunicipalityCode,
        int MunicipalityName,
        int RegionName,
        int ProvinceName,
        int ProvinceAbbreviation,
        int IsProvinceCapital,
        int CadastralCode,
        int Nuts1Code,
        int Nuts2Code,
        int Nuts3Code);
}
