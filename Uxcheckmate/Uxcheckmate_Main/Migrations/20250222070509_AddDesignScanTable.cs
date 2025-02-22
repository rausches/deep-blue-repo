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
            migrationBuilder.DropForeignKey(
                name: "FK_DesignIssue_DesignReport_DesignScanId",
                table: "DesignIssue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DesignReport",
                table: "DesignReport");

            migrationBuilder.RenameTable(
                name: "DesignReport",
                newName: "DesignScan");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DesignScan",
                table: "DesignScan",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DesignIssue_DesignScan_DesignScanId",
                table: "DesignIssue",
                column: "DesignScanId",
                principalTable: "DesignScan",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DesignIssue_DesignScan_DesignScanId",
                table: "DesignIssue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DesignScan",
                table: "DesignScan");

            migrationBuilder.RenameTable(
                name: "DesignScan",
                newName: "DesignReport");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DesignReport",
                table: "DesignReport",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DesignIssue_DesignReport_DesignScanId",
                table: "DesignIssue",
                column: "DesignScanId",
                principalTable: "DesignReport",
                principalColumn: "ID");
        }
    }
}
