using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDetailsLoanCustomerSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<long>(type: "bigint", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AspNetUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [UserDetails] ([FirstName], [LastName], [Mobile], [Address], [AspNetUserId])
                SELECT
                    ISNULL([FirstName], ''),
                    ISNULL([LastName], ''),
                    ISNULL([Phone], 0),
                    '',
                    [Id]
                FROM [AspNetUsers]
                WHERE ISNULL([IsDeleted], 0) = 0
                """);

            migrationBuilder.AddColumn<int>(
                name: "UserDetailsId",
                table: "Loan",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE loan
                SET [UserDetailsId] = details.[Id]
                FROM [Loan] loan
                INNER JOIN [UserDetails] details
                    ON loan.[UserId] = details.[AspNetUserId]
                """);

            migrationBuilder.Sql(
                """
                UPDATE [Loan]
                SET [UserDetailsId] = (SELECT TOP 1 [Id] FROM [UserDetails] ORDER BY [Id])
                WHERE [UserDetailsId] IS NULL
                  AND EXISTS (SELECT 1 FROM [UserDetails])
                """);

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Loan");

            migrationBuilder.RenameColumn(
                name: "UserDetailsId",
                table: "Loan",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Loan",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "UserDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "Loan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Loan");

            migrationBuilder.RenameColumn(
                name: "AspNetUserId",
                table: "Loan",
                newName: "UserId");

            migrationBuilder.DropTable(
                name: "UserDetails");
        }
    }
}
