using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class UnreadComment
    {
        public long Id { get; set; }

        /*
            Id of the comment 
        */
        public long CommentId { get; set; }

        /*
            The email of the user who has the unread comment.
        */
        [Required]
        public string UserEmail { get; set; } = "a@b.com";

    }
}
