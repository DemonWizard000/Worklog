using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class GroupUserDTO: Group
    {
        [Required]
        public int State { get; set; } = 0;

        [Required]
        public long InvitationId { get; set; }

        [Required]
        public string ChannelName { get; set; } = "";
    }
}
