using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotBarApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SopSteps");

            migrationBuilder.DropTable(
                name: "Sops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sops",
                columns: table => new
                {
                    SopId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Image = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sops", x => x.SopId);
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

            migrationBuilder.CreateIndex(
                name: "IX_SopSteps_SopId",
                table: "SopSteps",
                column: "SopId");
        }
    }
}
