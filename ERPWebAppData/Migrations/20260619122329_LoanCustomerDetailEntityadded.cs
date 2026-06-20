using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class LoanCustomerDetailEntityadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanCustomerDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    CustomerAadhaarNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerMobileNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPinCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuarantorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuarantorAadhaarNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuarantorMobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuarantorAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuarantorRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    F_Created_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_Updated_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanCustomerDetail", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanCustomerDetail");
        }
    }
}
