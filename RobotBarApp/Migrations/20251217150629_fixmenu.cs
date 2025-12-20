using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class fixmenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Menus_MenuId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_MenuId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MenuId",
                table: "Events",
                column: "MenuId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Menus_MenuId",
                table: "Events",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Menus_MenuId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_MenuId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MenuId",
                table: "Events",
                column: "MenuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Menus_MenuId",
                table: "Events",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
