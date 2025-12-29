using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class ScriptEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientScripts");

            migrationBuilder.DropColumn(
                name: "Dose",
                table: "Ingredients");

            migrationBuilder.CreateTable(
                name: "DoubleScripts",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoubleScripts", x => x.ScriptId);
                    table.ForeignKey(
                        name: "FK_DoubleScripts_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SingleScripts",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleScripts", x => x.ScriptId);
                    table.ForeignKey(
                        name: "FK_SingleScripts_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoubleScripts_IngredientId",
                table: "DoubleScripts",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleScripts_IngredientId",
                table: "SingleScripts",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoubleScripts");

            migrationBuilder.DropTable(
                name: "SingleScripts");

            migrationBuilder.AddColumn<string>(
                name: "Dose",
                table: "Ingredients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "IngredientScripts",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientScripts", x => x.ScriptId);
                    table.ForeignKey(
                        name: "FK_IngredientScripts_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientScripts_IngredientId",
                table: "IngredientScripts",
                column: "IngredientId");
        }
    }
}
