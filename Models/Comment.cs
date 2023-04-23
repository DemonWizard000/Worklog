using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class Comment
    {
        public long Id { get; set; }

        public long GroupId { get; set; }

        [Required]
        public string To_Email { get; set; } = "a@b.com";

        [Required]
        /*
            Date of answer 
        */
        public DateTime Log_Date { get; set; } = DateTime.Now;

        [Required]
        /*
         *  Commentor Email
        */
        public string From_Email { get; set; } = "a@b.com";

        [Required]
        public string From_Name { get; set; } = "a@b.com";

        [Required]
        public string Message { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime Commented_Date { get; set; } = DateTime.Now;

        /*
            If parent comment id is -1, that means it's the first comment. 
        */
        [Required]
        public int Parent_Comment_Id { get; set; } = -1;

    }
}