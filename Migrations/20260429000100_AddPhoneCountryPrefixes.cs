using DashboardOrders.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashboardOrders.Migrations;

[DbContext(typeof(DashboardOrdersDbContext))]
[Migration("20260429000100_AddPhoneCountryPrefixes")]
public partial class AddPhoneCountryPrefixes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PhoneCountryPrefixes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Iso2 = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                Iso3 = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                CountryName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                LocalizedCountryName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                DialCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                FlagPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PhoneCountryPrefixes", prefix => prefix.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PhoneCountryPrefixes_DialCode",
            table: "PhoneCountryPrefixes",
            column: "DialCode");

        migrationBuilder.CreateIndex(
            name: "IX_PhoneCountryPrefixes_Iso2",
            table: "PhoneCountryPrefixes",
            column: "Iso2",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PhoneCountryPrefixes");
    }
}
