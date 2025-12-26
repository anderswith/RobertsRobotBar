using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class IngredientColorAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Ingredients");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Ingredients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Ingredients");

            migrationBuilder.AddColumn<double>(
                name: "Size",
                table: "Ingredients",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
