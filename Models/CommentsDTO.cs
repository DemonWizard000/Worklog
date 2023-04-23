namespace WorkLog.Models
{
    public class CommentsDTO
    {
        public Comment comment { get; set; }
        public List<CommentsDTO> subComments { get; set; } = new List<CommentsDTO>();
    }
}
