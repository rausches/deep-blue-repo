using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class AddHtmlSnippetToAccessibilityIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlSnippet",
                table: "AccessibilityIssue",
                type: "varchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "FontLegibility",
                columns: new[] { "Id", "FontName" },
                values: new object[] { 11, "Comic Sans" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FontLegibility",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "HtmlSnippet",
                table: "AccessibilityIssue");
        }
    }
}
