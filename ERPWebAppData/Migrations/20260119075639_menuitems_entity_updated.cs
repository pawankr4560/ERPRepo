using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class menuitems_entity_updated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_MenuItem_F_Parent_menu_index",
                table: "MenuItem");

            migrationBuilder.DropIndex(
                name: "IX_MenuItem_F_Parent_menu_index",
                table: "MenuItem");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MenuItem_F_Parent_menu_index",
                table: "MenuItem",
                column: "F_Parent_menu_index");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_MenuItem_F_Parent_menu_index",
                table: "MenuItem",
                column: "F_Parent_menu_index",
                principalTable: "MenuItem",
                principalColumn: "F_menu_Index");
        }
    }
}
