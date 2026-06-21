using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanDashboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Loan",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayment_F_Is_Deleted_F_Payment_Status_F_Payment_Date",
                table: "LoanPayment",
                columns: new[] { "F_Is_Deleted", "F_Payment_Status", "F_Payment_Date" });

            migrationBuilder.CreateIndex(
                name: "IX_LoanEMISchedule_F_Is_Deleted_F_Is_Paid_F_Due_Date",
                table: "LoanEMISchedule",
                columns: new[] { "F_Is_Deleted", "F_Is_Paid", "F_Due_Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Loan_IsDeleted_Status_Active",
                table: "Loan",
                columns: new[] { "IsDeleted", "Status", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoanPayment_F_Is_Deleted_F_Payment_Status_F_Payment_Date",
                table: "LoanPayment");

            migrationBuilder.DropIndex(
                name: "IX_LoanEMISchedule_F_Is_Deleted_F_Is_Paid_F_Due_Date",
                table: "LoanEMISchedule");

            migrationBuilder.DropIndex(
                name: "IX_Loan_IsDeleted_Status_Active",
                table: "Loan");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Loan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
