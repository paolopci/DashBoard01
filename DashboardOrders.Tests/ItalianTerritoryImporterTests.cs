using System.IO.Compression;
using System.Security;
using DashboardOrders.Data;
using DashboardOrders.Domain.Entities;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DashboardOrders.Tests;

public class ItalianTerritoryImporterTests
{
    [Fact]
    public async Task ImportAsync_QuandoDatasetValido_AlloraSostituisceDatiTerritorialiEImportaCapValidi()
    {
        // Arrange
        using var temporaryDirectory = new TemporaryDirectory();
        var xlsxPath = Path.Combine(temporaryDirectory.Path, "istat.xlsx");
        var capPath = Path.Combine(temporaryDirectory.Path, "gi_cap.csv");
        WriteIstatWorkbook(xlsxPath, CreateValidRows());
        await File.WriteAllLinesAsync(capPath,
        [
            "codice_istat;cap",
            ";",
            "041044;61121",
            "041044;61122",
            "058091;00118",
            "999999;00000"
        ]);
        using var dbContext = CreateDbContext();
        SeedDemoTerritory(dbContext);
        var sut = new ItalianTerritoryImporter(dbContext);

        // Act
        var result = await sut.ImportAsync(new ItalianTerritoryImportOptions
        {
            IstatMunicipalitiesXlsxPath = xlsxPath,
            PostalCodesCsvPath = capPath,
            ExpectedRegionCount = 2,
            ExpectedMunicipalityCount = 3
        });

        // Assert
        result.Should().Be(new ItalianTerritoryImportResult(2, 2, 3, 3, 2));
        dbContext.ItalianRegions.Select(region => region.Code).Should().BeEquivalentTo("11", "12");
        dbContext.ItalianProvinces.Select(province => province.Code).Should().BeEquivalentTo("041", "258");
        dbContext.ItalianMunicipalities.Select(municipality => municipality.Code).Should().BeEquivalentTo("041044", "041013", "058091");
        dbContext.ItalianMunicipalities.Should().NotContain(municipality => municipality.Code == "999999");
        dbContext.ItalianPostalCodes.Should().Contain(postalCode =>
            postalCode.ProvinceName == "Pesaro e Urbino" &&
            postalCode.ProvinceCode == "PU" &&
            postalCode.CityName == "Pesaro" &&
            postalCode.PostalCode == "61121");
        dbContext.ItalianPostalCodes.Should().NotContain(postalCode => postalCode.PostalCode == "00000");
        dbContext.ItalianPostalCodes.Should().NotContain(postalCode => postalCode.CityName == "Demo");
    }

    [Fact]
    public async Task ImportAsync_QuandoEseguitoDueVolte_AlloraNonDuplicaDati()
    {
        // Arrange
        using var temporaryDirectory = new TemporaryDirectory();
        var xlsxPath = Path.Combine(temporaryDirectory.Path, "istat.xlsx");
        var capPath = Path.Combine(temporaryDirectory.Path, "gi_cap.csv");
        WriteIstatWorkbook(xlsxPath, CreateValidRows());
        await File.WriteAllLinesAsync(capPath,
        [
            "codice_istat;cap",
            "041044;61121"
        ]);
        using var dbContext = CreateDbContext();
        var sut = new ItalianTerritoryImporter(dbContext);
        var options = new ItalianTerritoryImportOptions
        {
            IstatMunicipalitiesXlsxPath = xlsxPath,
            PostalCodesCsvPath = capPath,
            ExpectedRegionCount = 2,
            ExpectedMunicipalityCount = 3
        };

        // Act
        await sut.ImportAsync(options);
        await sut.ImportAsync(options);

        // Assert
        dbContext.ItalianRegions.Should().HaveCount(2);
        dbContext.ItalianProvinces.Should().HaveCount(2);
        dbContext.ItalianMunicipalities.Should().HaveCount(3);
        dbContext.ItalianPostalCodes.Should().ContainSingle();
    }

    [Fact]
    public async Task ImportAsync_QuandoDatasetNonValido_AlloraNonModificaDatiEsistenti()
    {
        // Arrange
        using var temporaryDirectory = new TemporaryDirectory();
        var xlsxPath = Path.Combine(temporaryDirectory.Path, "istat-invalid.xlsx");
        WriteIstatWorkbook(xlsxPath, CreateInvalidRowsMissingMunicipalityCode());
        using var dbContext = CreateDbContext();
        SeedDemoTerritory(dbContext);
        var sut = new ItalianTerritoryImporter(dbContext);

        // Act
        var act = async () => await sut.ImportAsync(new ItalianTerritoryImportOptions
        {
            IstatMunicipalitiesXlsxPath = xlsxPath,
            ExpectedRegionCount = 1,
            ExpectedMunicipalityCount = 1
        });

        // Assert
        await act.Should().ThrowAsync<ItalianTerritoryImportException>()
            .WithMessage("*dati obbligatori mancanti*");
        dbContext.ItalianRegions.Should().ContainSingle(region => region.Code == "99");
        dbContext.ItalianProvinces.Should().ContainSingle(province => province.Code == "999");
        dbContext.ItalianMunicipalities.Should().ContainSingle(municipality => municipality.Code == "999001");
        dbContext.ItalianPostalCodes.Should().ContainSingle(postalCode => postalCode.CityName == "Demo");
    }

    private static DashboardOrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DashboardOrdersDbContext(options);
    }

    private static void SeedDemoTerritory(DashboardOrdersDbContext dbContext)
    {
        dbContext.ItalianRegions.Add(new ItalianRegionEntity { Code = "99", Name = "Demo Region" });
        dbContext.ItalianProvinces.Add(new ItalianProvinceEntity
        {
            Code = "999",
            RegionCode = "99",
            Name = "Demo Province",
            Abbreviation = "DM"
        });
        dbContext.ItalianMunicipalities.Add(new ItalianMunicipalityEntity
        {
            Code = "999001",
            RegionCode = "99",
            ProvinceCode = "999",
            Name = "Demo"
        });
        dbContext.ItalianPostalCodes.Add(new ItalianPostalCodeEntity
        {
            ProvinceName = "Demo Province",
            ProvinceCode = "DM",
            CityName = "Demo",
            PostalCode = "00001"
        });
        dbContext.SaveChanges();
    }

    private static List<string[]> CreateValidRows()
    {
        return
        [
            CreateRow("11", "041", "041044", "Pesaro", "Marche", "Pesaro e Urbino", "PU", "1", "G479", "ITI", "ITI3", "ITI31"),
            CreateRow("11", "041", "041013", "Fano", "Marche", "Pesaro e Urbino", "PU", "0", "D488", "ITI", "ITI3", "ITI31"),
            CreateRow("12", "258", "058091", "Roma", "Lazio", "Roma", "RM", "1", "H501", "ITI", "ITI4", "ITI43")
        ];
    }

    private static List<string[]> CreateInvalidRowsMissingMunicipalityCode()
    {
        return
        [
            CreateRow("11", "041", string.Empty, "Pesaro", "Marche", "Pesaro e Urbino", "PU", "1", "G479", "ITI", "ITI3", "ITI31")
        ];
    }

    private static string[] CreateRow(
        string regionCode,
        string provinceCode,
        string municipalityCode,
        string municipalityName,
        string regionName,
        string provinceName,
        string provinceAbbreviation,
        string isCapital,
        string cadastralCode,
        string nuts1,
        string nuts2,
        string nuts3)
    {
        var row = new string[27];
        row[0] = regionCode;
        row[1] = provinceCode;
        row[2] = provinceCode;
        row[3] = municipalityCode.Length >= 3 ? municipalityCode[^3..] : string.Empty;
        row[4] = municipalityCode;
        row[5] = municipalityName;
        row[6] = municipalityName;
        row[8] = regionCode == "11" ? "3" : "4";
        row[9] = regionCode == "11" ? "Centro" : "Centro";
        row[10] = regionName;
        row[11] = provinceName;
        row[12] = "Provincia";
        row[13] = isCapital;
        row[14] = provinceAbbreviation;
        row[15] = municipalityCode.TrimStart('0');
        row[20] = cadastralCode;
        row[24] = nuts1;
        row[25] = nuts2;
        row[26] = nuts3;
        return row;
    }

    private static void WriteIstatWorkbook(string path, IReadOnlyList<string[]> rows)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        WriteEntry(archive, "xl/workbook.xml",
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets>
                <sheet name="CODICI al 21_02_2026" sheetId="1" r:id="rId1"/>
              </sheets>
            </workbook>
            """);
        WriteEntry(archive, "xl/_rels/workbook.xml.rels",
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
            </Relationships>
            """);
        WriteEntry(archive, "xl/worksheets/sheet1.xml", CreateSheetXml(rows));
    }

    private static string CreateSheetXml(IReadOnlyList<string[]> rows)
    {
        var allRows = new List<string[]> { Headers };
        allRows.AddRange(rows);
        var sheetRows = allRows
            .Select((row, index) => CreateSheetRow(index + 1, row));

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <sheetData>
                {{string.Join(Environment.NewLine, sheetRows)}}
              </sheetData>
            </worksheet>
            """;
    }

    private static string CreateSheetRow(int rowNumber, IReadOnlyList<string> values)
    {
        var cells = values
            .Select((value, index) =>
            {
                var reference = $"{ToColumnName(index + 1)}{rowNumber}";
                return string.IsNullOrEmpty(value)
                    ? null
                    : $"""<c r="{reference}" t="inlineStr"><is><t>{SecurityElement.Escape(value)}</t></is></c>""";
            })
            .Where(cell => cell is not null);

        return $"<row r=\"{rowNumber}\">{string.Join(string.Empty, cells)}</row>";
    }

    private static string ToColumnName(int index)
    {
        var dividend = index;
        var columnName = string.Empty;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open());
        writer.Write(content);
    }

    private static readonly string[] Headers =
    [
        "Codice Regione",
        "Codice dell'Unità territoriale sovracomunale \n(valida a fini statistici)",
        "Codice Provincia (Storico)(1)",
        "Progressivo del Comune (2)",
        "Codice Comune formato alfanumerico",
        "Denominazione (Italiana e straniera)",
        "Denominazione in italiano",
        "Denominazione altra lingua",
        "Codice Ripartizione Geografica",
        "Ripartizione geografica",
        "Denominazione Regione",
        "Denominazione dell'Unità territoriale sovracomunale \n(valida a fini statistici)",
        "Tipologia di Unità territoriale sovracomunale ",
        "Flag Comune capoluogo di Provincia/Città metropolitana/libero consorzio",
        "Sigla automobilistica",
        "Codice Comune formato numerico",
        "Codice Comune numerico con 107 Province (dal 2017 al 2025)",
        "Codice Comune numerico con 110 Province (dal 2010 al 2016)",
        "Codice Comune numerico con 107 Province (dal 2006 al 2009)",
        "Codice Comune numerico con 103 Province (dal 1995 al 2005)",
        "Codice Catastale del Comune",
        "Codice NUTS1 2021",
        "Codice NUTS2 2021 (3) ",
        "Codice NUTS3 2021",
        "Codice NUTS1 2024",
        "Codice NUTS2 2024 (3) ",
        "Codice NUTS3 2024"
    ];

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
