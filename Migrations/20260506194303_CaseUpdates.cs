using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelPulse.Migrations
{
    /// <inheritdoc />
    public partial class CaseUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClosureReason",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestigationNotes",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosureReason",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "InvestigationNotes",
                table: "Cases");
        }
    }
}
