using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace WorkLog.Models
{
    /*
        Saves individual answers to individual questions for each user. 
    */
    public class WorkLogAnswer
    {
        public long Id { get; set; }

        [Required]
        [Range(0, 6)]
        /*
            Indicates this answer belongs to which question.( 0 - 6) 
        */
        public int QuestionId { get; set; }

        /*
            Indicates answer to the questionId.
            If questionId = 0(feeling), then Answer = null;
        */
        public string? Answer { get; set; } = null;

        [Required]
        [Range(0, 5)]
        /*
            Indicates answer's id to the one questionId.
            At least one answer, At most 5 answers to each question.
        */
        public int AnswerId { get; set; } = 0;

        
        [Required]
        [EmailAddress]
        /*
            Indicates this answer belongs to who(user's email address)
        */
        public string? Email { get; set; } = "a@b.com";

        [Required]
        public int ChannelId { get; set; } = -1;

        [Range(0, 2)]
        /*
            Indicates answer to the Question 0.
            0: Sad
            1: Normal(default)
            2: Happy
            only used when questionId = 1
        */
        public int Feeling { get; set; } = 1;

        [DataType(DataType.Date)]
        /*
            Indicates answered date.
        */
        public DateTime? Date { get; set; } = DateTime.Now;
    }
}
