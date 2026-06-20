using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class dAremovedfromLoan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "F_User_Index",
                table: "Loan",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "F_Tenure",
                table: "Loan",
                newName: "Tenure");

            migrationBuilder.RenameColumn(
                name: "F_Rate",
                table: "Loan",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "F_Loan_Number",
                table: "Loan",
                newName: "LoanNumber");

            migrationBuilder.RenameColumn(
                name: "F_Loan_Amount",
                table: "Loan",
                newName: "LoanAmount");

            migrationBuilder.RenameColumn(
                name: "F_Installment",
                table: "Loan",
                newName: "EMI");

            migrationBuilder.RenameColumn(
                name: "F_Loan_Index",
                table: "Loan",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Loan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "Tenure",
                table: "Loan",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Loan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Loan",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LoanEMISchedule",
                columns: table => new
                {
                    F_Schedule_Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    F_Loan_Index = table.Column<int>(type: "int", nullable: false),
                    F_Installment_No = table.Column<int>(type: "int", nullable: false),
                    F_Due_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_EMI_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    F_Principal_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    F_Interest_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    F_Outstanding_Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    F_Is_Paid = table.Column<bool>(type: "bit", nullable: false),
                    F_Paid_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    F_Active = table.Column<bool>(type: "bit", nullable: false),
                    F_Is_Deleted = table.Column<bool>(type: "bit", nullable: false),
                    F_Created_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_Updated_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    F_User_Index_Created = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanEMISchedule", x => x.F_Schedule_Index);
                });

            migrationBuilder.CreateTable(
                name: "LoanPayment",
                columns: table => new
                {
                    F_Payment_Index = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    F_Loan_Index = table.Column<int>(type: "int", nullable: false),
                    F_Schedule_Index = table.Column<int>(type: "int", nullable: false),
                    F_Amount_Paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    F_Payment_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_Transaction_Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    F_Payment_Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    F_Payment_Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    F_Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    F_Active = table.Column<bool>(type: "bit", nullable: false),
                    F_Is_Deleted = table.Column<bool>(type: "bit", nullable: false),
                    F_Created_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    F_Updated_Date_Time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    F_User_Index_Created = table.Column<int>(type: "int", nullable: true),
                    F_User_Index_Update = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPayment", x => x.F_Payment_Index);
                    table.ForeignKey(
                        name: "FK_LoanPayment_Loan_F_Loan_Index",
                        column: x => x.F_Loan_Index,
                        principalTable: "Loan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayment_F_Loan_Index",
                table: "LoanPayment",
                column: "F_Loan_Index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanEMISchedule");

            migrationBuilder.DropTable(
                name: "LoanPayment");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Loan");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Loan",
                newName: "F_User_Index");

            migrationBuilder.RenameColumn(
                name: "Tenure",
                table: "Loan",
                newName: "F_Tenure");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "Loan",
                newName: "F_Rate");

            migrationBuilder.RenameColumn(
                name: "LoanNumber",
                table: "Loan",
                newName: "F_Loan_Number");

            migrationBuilder.RenameColumn(
                name: "LoanAmount",
                table: "Loan",
                newName: "F_Loan_Amount");

            migrationBuilder.RenameColumn(
                name: "EMI",
                table: "Loan",
                newName: "F_Installment");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Loan",
                newName: "F_Loan_Index");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Loan");

            migrationBuilder.AlterColumn<float>(
                name: "F_Tenure",
                table: "Loan",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
