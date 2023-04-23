namespace WorkLog.Models
{
    public class WorkLogSearchDTO
    {
        public string? Date { get; set; }
        public string? Answer_Concat { get; set; }  
        
        public List<int> Feeling { get; set; } = null;
    }
}
