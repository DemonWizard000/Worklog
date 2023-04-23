using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Channels;
using WorkLog.Data;
using WorkLog.Models;
using WorkLog.Utils;

namespace WorkLog.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class CommentAPIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly WorkLogContext _context;

        public CommentAPIController(UserManager<AppUser> userManager, WorkLogContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("addComment")]
        [HttpPost]
        public async Task<Comment> AddComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                comment.From_Email = user?.Email;
                comment.From_Name = user?.UserName;
                _context.Comments.Add(comment);

                await _context.SaveChangesAsync();

                //Add unread comments for each users
                var groupUsers = await _context.GroupUsers.Where(x => x.GroupId == comment.GroupId && x.UserEmail != comment.From_Email).ToListAsync();
                groupUsers.ForEach(groupUser =>
                {
                    _context.UnreadComments.Add(new UnreadComment()
                    {
                        CommentId = comment.Id,
                        UserEmail = groupUser.UserEmail
                    });
                });
                await _context.SaveChangesAsync();

                return comment;
            }
            return null;
        }

        [Route("readComment")]
        [HttpPost]
        public async Task<bool> ReadComment(long groupId, string logDate, string to_email)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                DateTime log_date = DateTime.Parse(logDate);
                var query = from unreadComments in _context.UnreadComments
                            join _comment in _context.Comments on unreadComments.CommentId equals _comment.Id
                            where unreadComments.UserEmail == user.Email && _comment.GroupId == groupId && _comment.To_Email == to_email && _comment.Log_Date >= new DateTimeUtils().startTimeOfDate(log_date) && _comment.Log_Date <= new DateTimeUtils().endTimeOfDate(log_date)
                            select new { unreadComments };
                var results = query.ToList();
                results.ForEach(result =>
                {
                    _context.UnreadComments.Remove(result.unreadComments);
                });

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public List<CommentsDTO> GetCommentsDTOs(string to_email, string log_date,long groupId, long parent = -1)
        {
            DateTime LogDate = DateTime.Parse(log_date);
            var results = _context.Comments.Where(x => x.To_Email == to_email && x.Log_Date >= new DateTimeUtils().startTimeOfDate(LogDate) && x.Log_Date <= new DateTimeUtils().endTimeOfDate(LogDate) && x.Parent_Comment_Id == parent && x.GroupId == groupId).ToList();
            List<CommentsDTO> comments = new List<CommentsDTO>();
            foreach (var result in results)
            {
                CommentsDTO commentsDTO = new CommentsDTO();
                commentsDTO.comment = result;
                commentsDTO.subComments = GetCommentsDTOs(to_email, log_date, groupId, result.Id);
                comments.Add(commentsDTO);
            }
            return comments;
        }

        [Route("getComments")]
        [HttpPost]
        public async Task<List<CommentsDTO>> GetComments(string to_email, string log_date, long groupId)
        {
            if (ModelState.IsValid)
            {
                return GetCommentsDTOs(to_email, log_date, groupId);
            }
            return new List<CommentsDTO>();
        }
    }
}