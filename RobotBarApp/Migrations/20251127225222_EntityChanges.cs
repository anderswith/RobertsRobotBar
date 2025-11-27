using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class EntityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionNumber",
                table: "Ingredients");

            migrationBuilder.CreateTable(
                name: "IngredientPositions",
                columns: table => new
                {
                    IngredientPositionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientPositions", x => x.IngredientPositionId);
                    table.ForeignKey(
                        name: "FK_IngredientPositions_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientPositions_IngredientId",
                table: "IngredientPositions",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientPositions");

            migrationBuilder.AddColumn<int>(
                name: "PositionNumber",
                table: "Ingredients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
