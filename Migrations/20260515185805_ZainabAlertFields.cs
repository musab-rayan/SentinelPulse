using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelPulse.Migrations
{
    /// <inheritdoc />
    public partial class ZainabAlertFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedOfficer",
                table: "MissingChildren",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "MissingChildren",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedOfficer",
                table: "MissingChildren");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MissingChildren");
        }
    }
}
