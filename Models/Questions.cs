using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class Questions
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; } = "";

        [Required]
        public int Variation { get; set; }

        [Required]
        public long TenantId { get; set; }
    }
}
