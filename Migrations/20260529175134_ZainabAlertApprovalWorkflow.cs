using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelPulse.Migrations
{
    /// <inheritdoc />
    public partial class ZainabAlertApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingStatus",
                table: "MissingChildren",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "MissingChildren",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedByOfficerId",
                table: "MissingChildren",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MissingChildAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissingChildId = table.Column<int>(type: "int", nullable: false),
                    OfficerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissingChildAssignments", x => x.Id);
                });

            migrationBuilder.Sql(@"
                -- 1. Split existing AssignedOfficer free-text names and map to Officer ID (Best-Effort)
                INSERT INTO MissingChildAssignments (MissingChildId, OfficerId)
                SELECT m.AlertId, o.Id
                FROM MissingChildren m
                CROSS APPLY string_split(m.AssignedOfficer, ',') s
                INNER JOIN Officers o ON o.Name = LTRIM(RTRIM(s.value))
                WHERE m.AssignedOfficer IS NOT NULL AND m.AssignedOfficer <> 'Unassigned'
                  AND NOT EXISTS (
                      SELECT 1 FROM MissingChildAssignments ma 
                      WHERE ma.MissingChildId = m.AlertId AND ma.OfficerId = o.Id
                  );

                -- 2. Force-assign Kamran Bashir (SP-1042) to top 2 active alerts to ensure testability
                DECLARE @KamranId INT = (SELECT Id FROM Officers WHERE BadgeNumber = 'SP-1042');
                IF @KamranId IS NOT NULL
                BEGIN
                    INSERT INTO MissingChildAssignments (MissingChildId, OfficerId)
                    SELECT TOP 2 AlertId, @KamranId
                    FROM MissingChildren
                    WHERE Status NOT IN ('Closed', 'Found Safe', 'Found Deceased')
                      AND NOT EXISTS (
                          SELECT 1 FROM MissingChildAssignments ma 
                          WHERE ma.MissingChildId = MissingChildren.AlertId AND ma.OfficerId = @KamranId
                      )
                    ORDER BY ReportedDate DESC;
                END

                -- 3. Force-assign to another known officer 1-2 alerts for blocked demo
                DECLARE @OtherOfficerId INT = (SELECT TOP 1 Id FROM Officers WHERE BadgeNumber <> 'SP-1042' AND Role = 'Officer' AND Status = 'Active');
                IF @OtherOfficerId IS NOT NULL
                BEGIN
                    INSERT INTO MissingChildAssignments (MissingChildId, OfficerId)
                    SELECT TOP 2 AlertId, @OtherOfficerId
                    FROM MissingChildren
                    WHERE Status NOT IN ('Closed', 'Found Safe', 'Found Deceased')
                      AND AlertId NOT IN (SELECT MissingChildId FROM MissingChildAssignments WHERE OfficerId = @KamranId)
                      AND NOT EXISTS (
                          SELECT 1 FROM MissingChildAssignments ma 
                          WHERE ma.MissingChildId = MissingChildren.AlertId AND ma.OfficerId = @OtherOfficerId
                      )
                    ORDER BY ReportedDate ASC;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissingChildAssignments");

            migrationBuilder.DropColumn(
                name: "PendingStatus",
                table: "MissingChildren");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "MissingChildren");

            migrationBuilder.DropColumn(
                name: "RequestedByOfficerId",
                table: "MissingChildren");
        }
    }
}
