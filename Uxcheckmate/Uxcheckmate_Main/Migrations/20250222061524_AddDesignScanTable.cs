using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignScanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScanMethod",
                table: "DesignCategory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScanMethod",
                table: "DesignCategory");
        }
    }
}
