using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class MissingChildModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; set; }
    [Required(ErrorMessage = "Child name is required.")]
    public string ChildName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    [Required(ErrorMessage = "Guardian name is required.")]
    public string GuardianName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Guardian CNIC is required.")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be exactly 13 digits, no dashes or spaces.")]
    public string GuardianCNIC { get; set; } = string.Empty;
    [Required(ErrorMessage = "Guardian phone number is required.")]
    [RegularExpression(@"^(0\d{10}|92\d{10})$", ErrorMessage = "Phone must be 11 digits starting with 0, or 12 digits starting with 92, no dashes or spaces.")]
    public string GuardianPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "Last seen location is required.")]
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
    public string? PhotoPath { get; set; }
    
    // Approval Workflow properties
    public string? PendingStatus { get; set; }
    public int? RequestedByOfficerId { get; set; }
    public DateTime? RequestedAt { get; set; }
}
