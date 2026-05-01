using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashboardOrders.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneCountryIso2",
                table: "AspNetUsers",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "IT");

            migrationBuilder.AddColumn<string>(
                name: "PhonePrefix",
                table: "AspNetUsers",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "+39");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneCountryIso2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhonePrefix",
                table: "AspNetUsers");
        }
    }
}
