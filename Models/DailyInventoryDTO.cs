namespace WorkLog.Models
{
    public class DailyInventoryDTO
    {
        public List<WorkLogAnswerDTO> Answers { get; set; } = new List<WorkLogAnswerDTO>();
        public List<string> Questions { get; set; } = new List<string>();
        public string AllAnswers { get; set; } = "";
    }
}
