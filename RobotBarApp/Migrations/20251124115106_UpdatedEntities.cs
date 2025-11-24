using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkContents_Ingredients_IngredientId",
                table: "DrinkContents");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkScript_Drinks_DrinkId",
                table: "DrinkScript");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientScript_Ingredients_IngredientId",
                table: "IngredientScript");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IngredientScript",
                table: "IngredientScript");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DrinkScript",
                table: "DrinkScript");

            migrationBuilder.DropColumn(
                name: "Script",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "UseCount",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "PositionNumber",
                table: "IngredientScript");

            migrationBuilder.RenameTable(
                name: "IngredientScript",
                newName: "IngredientScripts");

            migrationBuilder.RenameTable(
                name: "DrinkScript",
                newName: "DrinkScripts");

            migrationBuilder.RenameColumn(
                name: "UseCount",
                table: "Ingredients",
                newName: "PositionNumber");

            migrationBuilder.RenameIndex(
                name: "IX_IngredientScript_IngredientId",
                table: "IngredientScripts",
                newName: "IX_IngredientScripts_IngredientId");

            migrationBuilder.RenameIndex(
                name: "IX_DrinkScript_DrinkId",
                table: "DrinkScripts",
                newName: "IX_DrinkScripts_DrinkId");

            migrationBuilder.AlterColumn<string>(
                name: "Dose",
                table: "Ingredients",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<Guid>(
                name: "DrinkId1",
                table: "DrinkContents",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IngredientId1",
                table: "DrinkContents",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_IngredientScripts",
                table: "IngredientScripts",
                column: "ScriptId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrinkScripts",
                table: "DrinkScripts",
                column: "ScriptId");

            migrationBuilder.CreateTable(
                name: "DrinkUseCounts",
                columns: table => new
                {
                    UseCountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DrinkId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkUseCounts", x => x.UseCountId);
                    table.ForeignKey(
                        name: "FK_DrinkUseCounts_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngredientUseCounts",
                columns: table => new
                {
                    UseCountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientUseCounts", x => x.UseCountId);
                    table.ForeignKey(
                        name: "FK_IngredientUseCounts_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrinkContents_DrinkId1",
                table: "DrinkContents",
                column: "DrinkId1");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkContents_IngredientId1",
                table: "DrinkContents",
                column: "IngredientId1");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkUseCounts_DrinkId",
                table: "DrinkUseCounts",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientUseCounts_IngredientId",
                table: "IngredientUseCounts",
                column: "IngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkContents_Drinks_DrinkId1",
                table: "DrinkContents",
                column: "DrinkId1",
                principalTable: "Drinks",
                principalColumn: "DrinkId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkContents_Ingredients_IngredientId",
                table: "DrinkContents",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkContents_Ingredients_IngredientId1",
                table: "DrinkContents",
                column: "IngredientId1",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkScripts_Drinks_DrinkId",
                table: "DrinkScripts",
                column: "DrinkId",
                principalTable: "Drinks",
                principalColumn: "DrinkId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientScripts_Ingredients_IngredientId",
                table: "IngredientScripts",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkContents_Drinks_DrinkId1",
                table: "DrinkContents");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkContents_Ingredients_IngredientId",
                table: "DrinkContents");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkContents_Ingredients_IngredientId1",
                table: "DrinkContents");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkScripts_Drinks_DrinkId",
                table: "DrinkScripts");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientScripts_Ingredients_IngredientId",
                table: "IngredientScripts");

            migrationBuilder.DropTable(
                name: "DrinkUseCounts");

            migrationBuilder.DropTable(
                name: "IngredientUseCounts");

            migrationBuilder.DropIndex(
                name: "IX_DrinkContents_DrinkId1",
                table: "DrinkContents");

            migrationBuilder.DropIndex(
                name: "IX_DrinkContents_IngredientId1",
                table: "DrinkContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IngredientScripts",
                table: "IngredientScripts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DrinkScripts",
                table: "DrinkScripts");

            migrationBuilder.DropColumn(
                name: "DrinkId1",
                table: "DrinkContents");

            migrationBuilder.DropColumn(
                name: "IngredientId1",
                table: "DrinkContents");

            migrationBuilder.RenameTable(
                name: "IngredientScripts",
                newName: "IngredientScript");

            migrationBuilder.RenameTable(
                name: "DrinkScripts",
                newName: "DrinkScript");

            migrationBuilder.RenameColumn(
                name: "PositionNumber",
                table: "Ingredients",
                newName: "UseCount");

            migrationBuilder.RenameIndex(
                name: "IX_IngredientScripts_IngredientId",
                table: "IngredientScript",
                newName: "IX_IngredientScript_IngredientId");

            migrationBuilder.RenameIndex(
                name: "IX_DrinkScripts_DrinkId",
                table: "DrinkScript",
                newName: "IX_DrinkScript_DrinkId");

            migrationBuilder.AlterColumn<double>(
                name: "Dose",
                table: "Ingredients",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Script",
                table: "Drinks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UseCount",
                table: "Drinks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionNumber",
                table: "IngredientScript",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IngredientScript",
                table: "IngredientScript",
                column: "ScriptId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DrinkScript",
                table: "DrinkScript",
                column: "ScriptId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkContents_Ingredients_IngredientId",
                table: "DrinkContents",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkScript_Drinks_DrinkId",
                table: "DrinkScript",
                column: "DrinkId",
                principalTable: "Drinks",
                principalColumn: "DrinkId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientScript_Ingredients_IngredientId",
                table: "IngredientScript",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "IngredientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
