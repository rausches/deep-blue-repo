using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class SeedAccessibilityCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AccessibilityCategory",
                columns: new[] { "ID", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "Color & Contrast" },
                    { 2, null, "Keyboard & Focus" },
                    { 3, null, "Page Structure & Landmarks" },
                    { 4, null, "Forms & Inputs" },
                    { 5, null, "Link & Buttons" },
                    { 6, null, "Multimedia & Animations" },
                    { 7, null, "Timeouts & Auto-Refresh" },
                    { 8, null, "Motion & Interaction" },
                    { 9, null, "ARIA & Semantic HTML" },
                    { 10, null, "Other" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AccessibilityCategory",
                keyColumn: "ID",
                keyValue: 10);
        }
    }
}
