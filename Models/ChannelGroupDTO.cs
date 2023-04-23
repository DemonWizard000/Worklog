namespace WorkLog.Models
{
    public class ChannelGroupDTO
    {
        public long Id { get; set; }
        public long ChannelId { get; set; }
        public string ChannelName { get; set; }

        public string ChannelDescription { get; set; } = string.Empty;

        public string ChannelManager { get; set; } = "";

        public List<Group> Groups { get; set; } = new List<Group>();
    }
}
