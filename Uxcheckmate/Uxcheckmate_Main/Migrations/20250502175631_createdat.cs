using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uxcheckmate_Main.Migrations
{
    /// <inheritdoc />
    public partial class createdat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Report",
                type: "datetime2",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Report");
        }
    }
}
