using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models
{
    public class EvidenceModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string CaseId { get; set; } = "";

        [Required]
        public string Type { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        public string? CollectedBy { get; set; }
        public DateTime CollectedDate { get; set; } = DateTime.Now;
        public string? FilePath { get; set; }
    }
}
