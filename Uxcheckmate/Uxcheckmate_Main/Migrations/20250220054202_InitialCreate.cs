using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessibilityCategory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Accessib__3214EC274D88B479", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DesignCategory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DesignCa__3214EC27E2402227", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    URL = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Report__3214EC27DC95E762", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AccessibilityIssue",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Selector = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Accessib__3214EC2759D39646", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AccessibilityIssue_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "AccessibilityCategory",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessibilityIssue_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Report",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignIssue",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DesignIs__3214EC277C5F35C0", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DesignIssue_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "DesignCategory",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignIssue_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Report",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessibilityIssue_CategoryID",
                table: "AccessibilityIssue",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_AccessibilityIssue_ReportID",
                table: "AccessibilityIssue",
                column: "ReportID");

            migrationBuilder.CreateIndex(
                name: "IX_DesignIssue_CategoryID",
                table: "DesignIssue",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_DesignIssue_ReportID",
                table: "DesignIssue",
                column: "ReportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessibilityIssue");

            migrationBuilder.DropTable(
                name: "DesignIssue");

            migrationBuilder.DropTable(
                name: "AccessibilityCategory");

            migrationBuilder.DropTable(
                name: "DesignCategory");

            migrationBuilder.DropTable(
                name: "Report");
        }
    }
}
