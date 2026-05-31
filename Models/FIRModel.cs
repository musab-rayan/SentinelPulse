using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class FIRModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string CaseId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Citizen name is required."), Display(Name = "Citizen Name")]
    public string CitizenName { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNIC is required."), Display(Name = "CNIC")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be exactly 13 digits, no dashes or spaces.")]
    public string CitizenCNIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required."), Display(Name = "Phone Number")]
    [RegularExpression(@"^(0\d{10}|92\d{10})$", ErrorMessage = "Phone must be 11 digits starting with 0, or 12 digits starting with 92, no dashes or spaces.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Crime type is required."), Display(Name = "Crime Type")]
    public string CrimeType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
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
