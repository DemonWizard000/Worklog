using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class GroupUser
    {
        public long Id { get; set; }

        [Required]
        public long GroupId { get; set; }

        [Required]
        [EmailAddress]
        public string? UserEmail { get; set; }

        /*
            0: not yet accepted
            1: accepted
        */
        [Required]
        public int State { get; set; }
    }
}
