using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class MissingChildModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianCNIC { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LastSeenLocation { get; set; } = string.Empty;
    public string LastSeenDistrict { get; set; } = "Islamabad";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Status { get; set; } = "Active";
    public string ReportedBy { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; } = DateTime.Now;
    public string? UpdateNotes { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? AssignedOfficer { get; set; }
    public string Priority { get; set; } = "High";
}
