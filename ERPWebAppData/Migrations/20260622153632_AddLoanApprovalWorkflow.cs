using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebAppData.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "Loan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Loan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReducingInterest",
                table: "Loan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAtUtc",
                table: "Loan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedByUserId",
                table: "Loan",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "IsReducingInterest",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "RejectedAtUtc",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "Loan");
        }
    }
}
