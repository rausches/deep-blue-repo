using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class AddFontLegibilityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FontLegibility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FontName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsLegible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FontLegibility", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FontLegibility",
                columns: new[] { "Id", "FontName" },
                values: new object[,]
                {
                    { 1, "Chiller" },
                    { 2, "Vivaldi" },
                    { 3, "Old English Text" },
                    { 4, "Jokerman" },
                    { 5, "Brush Script" },
                    { 6, "Bleeding Cowboys" },
                    { 7, "Curlz MT" },
                    { 8, "Wingdings" },
                    { 9, "Zapfino" },
                    { 10, "TrashHand" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FontLegibility");
        }
    }
}
