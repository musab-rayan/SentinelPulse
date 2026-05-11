using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class CaseModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string CaseId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AssignedOfficer { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open / Under Investigation / Pending Approval / Closed
    public string Priority { get; set; } = "Medium"; // High / Medium / Low
    public DateTime DateOpened { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? InvestigationNotes { get; set; }
    public string? ClosureReason { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
