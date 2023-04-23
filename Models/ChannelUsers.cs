using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class ChannelUsers
    {
        public long Id { get; set; }

        [Required]
        public long ChannelId { get; set; }


        [Required]
        [EmailAddress]
        public string? UserEmail { get; set; }

        /*
            0: not yet accepted
            1: accepted
            2: declined
        */
        [Required]
        public int State { get; set; } = 0;
    }
}
