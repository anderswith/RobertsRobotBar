using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class InitClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drinks",
                columns: table => new
                {
                    DrinkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false),
                    IsMocktail = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drinks", x => x.DrinkId);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<double>(type: "REAL", nullable: false),
                    Dose = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.IngredientId);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "Sops",
                columns: table => new
                {
                    SopId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sops", x => x.SopId);
                });

            migrationBuilder.CreateTable(
                name: "DrinkScripts",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    DrinkId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkScripts", x => x.ScriptId);
                    table.ForeignKey(
                        name: "FK_DrinkScripts_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "DrinkContents",
                columns: table => new
                {
                    DrinkContentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DrinkId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkContents", x => x.DrinkContentId);
                    table.ForeignKey(
                        name: "FK_DrinkContents_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkContents_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "IngredientScripts",
                columns: table => new
                {
                    ScriptId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UrScript = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MenuId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Image = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MenuContents",
                columns: table => new
                {
                    MenuContentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MenuId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DrinkId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuContents", x => x.MenuContentId);
                    table.ForeignKey(
                        name: "FK_MenuContents_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "DrinkId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuContents_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SopSteps",
                columns: table => new
                {
                    SopStepsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SopId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false),
                    StepCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SopSteps", x => x.SopStepsId);
                    table.ForeignKey(
                        name: "FK_SopSteps_Sops_SopId",
                        column: x => x.SopId,
                        principalTable: "Sops",
                        principalColumn: "SopId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarSetups",
                columns: table => new
                {
                    EventBarSetupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PositionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarSetups", x => x.EventBarSetupId);
                    table.ForeignKey(
                        name: "FK_BarSetups_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarSetups_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LogMsg = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_Logs_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarSetups_EventId",
                table: "BarSetups",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_BarSetups_IngredientId",
                table: "BarSetups",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkContents_DrinkId",
                table: "DrinkContents",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkContents_IngredientId",
                table: "DrinkContents",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkScripts_DrinkId",
                table: "DrinkScripts",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkUseCounts_DrinkId",
                table: "DrinkUseCounts",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MenuId",
                table: "Events",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientPositions_IngredientId",
                table: "IngredientPositions",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientScripts_IngredientId",
                table: "IngredientScripts",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientUseCounts_IngredientId",
                table: "IngredientUseCounts",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_EventId",
                table: "Logs",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuContents_DrinkId",
                table: "MenuContents",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuContents_MenuId",
                table: "MenuContents",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_SopSteps_SopId",
                table: "SopSteps",
                column: "SopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarSetups");

            migrationBuilder.DropTable(
                name: "DrinkContents");

            migrationBuilder.DropTable(
                name: "DrinkScripts");

            migrationBuilder.DropTable(
                name: "DrinkUseCounts");

            migrationBuilder.DropTable(
                name: "IngredientPositions");

            migrationBuilder.DropTable(
                name: "IngredientScripts");

            migrationBuilder.DropTable(
                name: "IngredientUseCounts");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "MenuContents");

            migrationBuilder.DropTable(
                name: "SopSteps");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Drinks");

            migrationBuilder.DropTable(
                name: "Sops");

            migrationBuilder.DropTable(
                name: "Menus");
        }
    }
}
