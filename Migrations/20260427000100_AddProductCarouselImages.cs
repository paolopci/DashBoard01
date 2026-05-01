using System;
using DashboardOrders.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashboardOrders.Migrations;

[DbContext(typeof(DashboardOrdersDbContext))]
[Migration("20260427000100_AddProductCarouselImages")]
public partial class AddProductCarouselImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProductCarouselImages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                AltText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCarouselImages", image => image.Id);
                table.ForeignKey(
                    name: "FK_ProductCarouselImages_Products_ProductId",
                    column: image => image.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProductCarouselImages_ImageUrl",
            table: "ProductCarouselImages",
            column: "ImageUrl",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductCarouselImages_ProductId_DisplayOrder",
            table: "ProductCarouselImages",
            columns: new[] { "ProductId", "DisplayOrder" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ProductCarouselImages");
    }
}
