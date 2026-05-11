using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class OfficerModel
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BadgeNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Officer"; // Admin / Officer
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Status { get; set; } = "Active";
    public string? Phone { get; set; }
    public string? ProfilePhoto { get; set; }
}
