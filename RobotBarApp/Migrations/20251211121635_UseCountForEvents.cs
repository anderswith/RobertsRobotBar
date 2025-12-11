using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class UseCountForEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "IngredientUseCounts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "DrinkUseCounts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_IngredientUseCounts_EventId",
                table: "IngredientUseCounts",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkUseCounts_EventId",
                table: "DrinkUseCounts",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkUseCounts_Events_EventId",
                table: "DrinkUseCounts",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IngredientUseCounts_Events_EventId",
                table: "IngredientUseCounts",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkUseCounts_Events_EventId",
                table: "DrinkUseCounts");

            migrationBuilder.DropForeignKey(
                name: "FK_IngredientUseCounts_Events_EventId",
                table: "IngredientUseCounts");

            migrationBuilder.DropIndex(
                name: "IX_IngredientUseCounts_EventId",
                table: "IngredientUseCounts");

            migrationBuilder.DropIndex(
                name: "IX_DrinkUseCounts_EventId",
                table: "DrinkUseCounts");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "IngredientUseCounts");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "DrinkUseCounts");
        }
    }
}
