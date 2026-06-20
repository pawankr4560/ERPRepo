using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class loanEntityUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loan",
                columns: table => new
                {
                    F_Loan_Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    F_User_Index = table.Column<int>(type: "int", nullable: false),
                    F_Loan_Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    F_Loan_Amount = table.Column<double>(type: "float", nullable: false),
                    F_Rate = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    F_Created_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_Updated_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_User_Index_Created = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loan", x => x.F_Loan_Index);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Loan");
        }
    }
}
