using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPaymentServiceCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BookingPaymentFixedCharge",
                table: "LoanSetting",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BookingPaymentPercentageCharge",
                table: "LoanSetting",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingPaymentFixedCharge",
                table: "LoanSetting");

            migrationBuilder.DropColumn(
                name: "BookingPaymentPercentageCharge",
                table: "LoanSetting");
        }
    }
}
