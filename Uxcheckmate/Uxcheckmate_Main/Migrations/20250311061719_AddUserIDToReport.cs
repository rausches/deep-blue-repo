using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIDToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Report",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Report");
        }
    }
}
