using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class FIRModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string CaseId { get; set; } = string.Empty;

    [Required, Display(Name = "Citizen Name")]
    public string CitizenName { get; set; } = string.Empty;

    [Required, Display(Name = "CNIC")]
    public string CitizenCNIC { get; set; } = string.Empty;

    [Required, Phone, Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, Display(Name = "Crime Type")]
    public string CrimeType { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string District { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public DateTime DateFiled { get; set; } = DateTime.Now;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? WeatherAtScene { get; set; }
    public string? SentimentScore { get; set; }
    public string? SentimentLabel { get; set; }
}
