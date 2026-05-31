using System.ComponentModel.DataAnnotations;

namespace SentinelPulse.Models;

public class MissingChildAssignment
{
    [Key]
    public int Id { get; set; }
    public int MissingChildId { get; set; }
    public int OfficerId { get; set; }
}
