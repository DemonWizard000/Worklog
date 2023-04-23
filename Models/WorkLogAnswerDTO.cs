namespace WorkLog.Models
{
    /*
        DTO of WorkLogAnswer for DailyInventory 
    */
    public class WorkLogAnswerDTO
    {
        public int AswerId { get; set; }
        public int QuestionId { get; set; }
        public int Feeling { get; set; }
        public List<string>? Answers { get; set; } = new List<string>();
    }
}
