using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class quoteEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "ConstructionQuotes");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "ConstructionQuotes");

            migrationBuilder.RenameColumn(
                name: "UnitIndex",
                table: "ConstructionQuotes",
                newName: "UnitId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "ConstructionQuotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ConstructionQuotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ConstructionQuotes");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ConstructionQuotes");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "ConstructionQuotes",
                newName: "UnitIndex");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "ConstructionQuotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "ConstructionQuotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
