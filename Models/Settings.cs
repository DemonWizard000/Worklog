using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class Settings
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = "";

        [Required]
        public int Min_Freq { get; set; } = 0;

        [Required]
        public int WeightFactor { get; set; } = 20;
    }
}
