using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelPulse.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MissingChildren",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cases",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_MissingChildren_Status_ReportedDate",
                table: "MissingChildren",
                columns: new[] { "Status", "ReportedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_Status_DateOpened",
                table: "Cases",
                columns: new[] { "Status", "DateOpened" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MissingChildren_Status_ReportedDate",
                table: "MissingChildren");

            migrationBuilder.DropIndex(
                name: "IX_Cases_Status_DateOpened",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MissingChildren",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
