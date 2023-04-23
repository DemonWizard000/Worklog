using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class WordCloud
    {
        public long Id { get; set; }

        [Required]
        public string Word { get; set; } = string.Empty;

        [Required]
        public int Count { get; set; } = 0;

        [Required]
        public string Email { get; set; } = "a@b.com";

        [Required]
        public int ChannelId { get; set; } = -1;

        public DateTime? LogDate { get; set; } = DateTime.Now;
    }
}
