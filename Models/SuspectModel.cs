using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentinelPulse.Models
{
    public class SuspectModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? CaseId { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be exactly 13 digits, no dashes or spaces.")]
        public string? CNIC { get; set; }
        public string? Description { get; set; }
        public string? PhotoPath { get; set; }
        public string? FaceDetectionResult { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
