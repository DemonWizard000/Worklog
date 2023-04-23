using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class ChannelUserDTO: Channel
    {
        [Required]
        public int State { get; set; } = 0;

        [Required]
        public long InvitationId { get; set; }
    }
}
