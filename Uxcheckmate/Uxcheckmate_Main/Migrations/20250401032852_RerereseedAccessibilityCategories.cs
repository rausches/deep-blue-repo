using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class RerereseedAccessibilityCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AccessibilityCategory",
                columns: new[] { "ID", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Issues related to color contrast and visual accessibility.", "Color & Contrast" },
                    { 2, "Problems with keyboard navigation and focus management.", "Keyboard & Focus" },
                    { 3, "Issues with headings, ARIA landmarks, and document structure.", "Page Structure & Landmarks" },
                    { 4, "Issues with forms, labels, and input fields.", "Forms & Inputs" },
                    { 5, "Problems with links, buttons, and interactive elements.", "Link & Buttons" },
                    { 6, "Issues related to videos, audio, images, and animations.", "Multimedia & Animations" },
                    { 7, "Problems with session timeouts, auto-refreshing pages, and dynamic content updates.", "Timeouts & Auto-Refresh" },
                    { 8, "Issues related to animations, scrolling, and movement.", "Motion & Interaction" },
                    { 9, "Issues with incorrect or missing ARIA roles and attributes.", "ARIA & Semantic HTML" },
                    { 10, "Unknown or experimental WCAG violations", "Other" }
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
