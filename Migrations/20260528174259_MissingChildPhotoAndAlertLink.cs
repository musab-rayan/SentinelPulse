using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelPulse.Migrations
{
    /// <inheritdoc />
    public partial class MissingChildPhotoAndAlertLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "MissingChildren",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MissingChildAlertId",
                table: "Alerts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "MissingChildren");

            migrationBuilder.DropColumn(
                name: "MissingChildAlertId",
                table: "Alerts");
        }
    }
}
