using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class customer_entity_updated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StripeCustomer_AspNetUsers_EmailId",
                table: "StripeCustomer");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "StripeCustomer",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_StripeCustomer_EmailId",
                table: "StripeCustomer",
                newName: "IX_StripeCustomer_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StripeCustomer_AspNetUsers_UserId",
                table: "StripeCustomer",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StripeCustomer_AspNetUsers_UserId",
                table: "StripeCustomer");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "StripeCustomer",
                newName: "EmailId");

            migrationBuilder.RenameIndex(
                name: "IX_StripeCustomer_UserId",
                table: "StripeCustomer",
                newName: "IX_StripeCustomer_EmailId");

            migrationBuilder.AddForeignKey(
                name: "FK_StripeCustomer_AspNetUsers_EmailId",
                table: "StripeCustomer",
                column: "EmailId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
