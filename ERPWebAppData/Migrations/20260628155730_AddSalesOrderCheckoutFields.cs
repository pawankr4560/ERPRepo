using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderCheckoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "OrderHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<float>(
                name: "ChargedAmount",
                table: "OrderHistory",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "LineTotal",
                table: "OrderHistory",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "OrderType",
                table: "OrderHistory",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "OrderHistory",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "OrderHistory",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RazorpayOrderId",
                table: "OrderHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "ServiceFeeAmount",
                table: "OrderHistory",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "OrderHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "OrderHistory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargedAmount",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "RazorpayOrderId",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "ServiceFeeAmount",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OrderHistory");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "OrderHistory",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
