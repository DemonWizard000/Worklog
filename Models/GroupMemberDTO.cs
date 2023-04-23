namespace WorkLog.Models
{
    public class GroupMemberDTO: GroupUser
    {
        public string UserName { get; set; } = "";
        public int UnreadComments { get; set; } = 0;
    }
}
