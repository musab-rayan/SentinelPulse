using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models;

public class AlertModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int? MissingChildAlertId { get; set; }
}
