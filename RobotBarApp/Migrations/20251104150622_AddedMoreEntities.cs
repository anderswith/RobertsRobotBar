using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedMoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionNumber",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Script",
                table: "Ingredients");

            migrationBuilder.CreateTable(
                name: "DrinkScript",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    DrinkId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkScript", x => x.ScriptId);
                    table.ForeignKey(
                        name: "FK_DrinkScript_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngredientScript",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PositionNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientScript", x => x.ScriptId);
                    table.ForeignKey(
                        name: "FK_IngredientScript_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrinkScript_DrinkId",
                table: "DrinkScript",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientScript_IngredientId",
                table: "IngredientScript",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrinkScript");

            migrationBuilder.DropTable(
                name: "IngredientScript");

            migrationBuilder.AddColumn<int>(
                name: "PositionNumber",
                table: "Ingredients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Script",
                table: "Ingredients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
