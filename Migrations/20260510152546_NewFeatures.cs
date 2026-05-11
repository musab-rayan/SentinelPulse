using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelPulse.Migrations
{
    /// <inheritdoc />
    public partial class NewFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Officers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhoto",
                table: "Officers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Officers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "FIRs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "FIRs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentimentLabel",
                table: "FIRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentimentScore",
                table: "FIRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeatherAtScene",
                table: "FIRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Cases",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Cases",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Evidence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CollectedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evidence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suspects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaceDetectionResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suspects", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evidence");

            migrationBuilder.DropTable(
                name: "Suspects");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "ProfilePhoto",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "FIRs");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "FIRs");

            migrationBuilder.DropColumn(
                name: "SentimentLabel",
                table: "FIRs");

            migrationBuilder.DropColumn(
                name: "SentimentScore",
                table: "FIRs");

            migrationBuilder.DropColumn(
                name: "WeatherAtScene",
                table: "FIRs");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Cases");
        }
    }
}
