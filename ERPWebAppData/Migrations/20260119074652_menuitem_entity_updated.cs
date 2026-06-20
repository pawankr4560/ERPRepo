using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Data.Migrations
{
    public partial class menuitem_entity_updated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_MenuItem_MenuItemId",
                table: "MenuItem");

            migrationBuilder.RenameColumn(
                name: "Route",
                table: "MenuItem",
                newName: "F_route");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MenuItem",
                newName: "F_menu_Index");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MenuItem",
                newName: "T_Title");

            migrationBuilder.RenameColumn(
                name: "MenuItemId",
                table: "MenuItem",
                newName: "F_Parent_menu_index");

            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "MenuItem",
                newName: "F_Icon_class");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItem_MenuItemId",
                table: "MenuItem",
                newName: "IX_MenuItem_F_Parent_menu_index");

            migrationBuilder.AddColumn<bool>(
                name: "F_Active",
                table: "MenuItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "F_order_number",
                table: "MenuItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_MenuItem_F_Parent_menu_index",
                table: "MenuItem",
                column: "F_Parent_menu_index",
                principalTable: "MenuItem",
                principalColumn: "F_menu_Index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_MenuItem_F_Parent_menu_index",
                table: "MenuItem");

            migrationBuilder.DropColumn(
                name: "F_Active",
                table: "MenuItem");

            migrationBuilder.DropColumn(
                name: "F_order_number",
                table: "MenuItem");

            migrationBuilder.RenameColumn(
                name: "F_route",
                table: "MenuItem",
                newName: "Route");

            migrationBuilder.RenameColumn(
                name: "F_menu_Index",
                table: "MenuItem",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "T_Title",
                table: "MenuItem",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "F_Parent_menu_index",
                table: "MenuItem",
                newName: "MenuItemId");

            migrationBuilder.RenameColumn(
                name: "F_Icon_class",
                table: "MenuItem",
                newName: "Icon");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItem_F_Parent_menu_index",
                table: "MenuItem",
                newName: "IX_MenuItem_MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_MenuItem_MenuItemId",
                table: "MenuItem",
                column: "MenuItemId",
                principalTable: "MenuItem",
                principalColumn: "Id");
        }
    }
}
