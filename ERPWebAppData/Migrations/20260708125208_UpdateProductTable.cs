using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products_Backup_UpdateProductTable");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubcategoryId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.Sql(@"
                IF COL_LENGTH('[Products_Backup_UpdateProductTable]', 'Code') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'Name') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'CategorieID') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'UOMIndex') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'Status') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'CreatedOn') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'IsActive') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'Price') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'IsDeleted') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'Description') IS NOT NULL
                   AND COL_LENGTH('[Products_Backup_UpdateProductTable]', 'Image') IS NOT NULL
                BEGIN
                    EXEC(N'
                        INSERT INTO [Products]
                            ([Code], [Name], [CategoryId], [SubcategoryId], [UnitId], [Status], [CreatedOn], [IsActive], [Price], [IsDeleted], [Description], [Image])
                        SELECT
                            ISNULL([Code], ''''),
                            ISNULL([Name], ''''),
                            [CategorieID],
                            0,
                            [UOMIndex],
                            [Status],
                            [CreatedOn],
                            [IsActive],
                            CAST([Price] AS decimal(18,2)),
                            [IsDeleted],
                            ISNULL([Description], ''''),
                            ISNULL([Image], '''')
                        FROM [Products_Backup_UpdateProductTable];
                    ');
                END
            ");
            migrationBuilder.CreateTable(
                name: "SubCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategory", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SubCategory");

            migrationBuilder.RenameTable(
                name: "Products_Backup_UpdateProductTable",
                newName: "Products");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");
        }
    }
}


