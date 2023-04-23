using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    [Authorize]
    public class ChannelController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly WorkLogContext _context;

        public ChannelController(UserManager<AppUser> userManager, WorkLogContext answerContext)
        {
            _userManager = userManager;
            _context = answerContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<List<string>> GetDefaultQuestions(long tenantId)
        {
            Tenant tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return new List<string>();
            else
                return tenant.DefaultQuestions.Split(",#").ToList();
        }


        public IActionResult JoinChannel(int ChannelId)
        {
            HttpContext.Session.SetInt32("ChannelId", ChannelId);
            return Redirect("/Worklog/Answer");
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int ChannelId = -1)
        {
            

            Channel channel = await _context.Channels.FindAsync((long)ChannelId);

            if (ChannelId == -1)
                channel = new Channel()
                {
                    Id = -1,
                    Name = "Default"
                };

            ViewData["Channel"] = channel;

            /*List<ChannelUsers> channelUsers = _context.ChannelUsers.Where(x => x.ChannelId == ChannelId).ToList();
            ViewData["ChannelUsers"] = channelUsers;*/

            //Get groups that I joined
            var user = await _userManager.GetUserAsync(User);

            var query = from groupUser in _context.GroupUsers
                        where groupUser.UserEmail == user.Email && groupUser.State == 1
                        join _group in _context.Groups on groupUser.GroupId equals _group.Id
                        where _group.ChannelId == ChannelId
                        select new { groupUser, _group };

            var results = query.ToList();
            List < Group > groups = new List<Group>();
            foreach (var result in results)
                groups.Add(result._group);

            ViewData["Groups"] = groups;

            return View("Manage");
        }

        public async Task<GroupUserDTO> GetGroupInvitations(string userEmail = "")
        {
            var query = from groupUser in _context.GroupUsers
                        where groupUser.UserEmail == userEmail && groupUser.State == 0
                        join _group in _context.Groups on groupUser.GroupId equals _group.Id
                        select new { _group, groupUser.State, groupUser.Id };

            var result = await query.FirstOrDefaultAsync();
            if (result == null)
                return null;

            List<GroupUserDTO> groupUserDTOs = new List<GroupUserDTO>();

            string ChannelName = "Default";
            if (result._group.ChannelId != -1)
            {
                Channel channel = await _context.Channels.FindAsync(result._group.ChannelId);
                if (channel != null)
                    ChannelName = channel.Name;
            }

            return new GroupUserDTO()
            {
                Name = result._group.Name,
                Description = result._group.Description,
                Manager_email = result._group.Manager_email,
                Id = result._group.Id,
                ChannelName = ChannelName,
                State = result.State,
                InvitationId = result.Id
            };
        }

        public async Task<ChannelUserDTO> GetChannelInviations(string userEmail = "")
        {
            var query = from channelUser in _context.ChannelUsers
                        where channelUser.UserEmail == userEmail && channelUser.State == 0
                        join _channel in _context.Channels on channelUser.ChannelId equals _channel.Id
                        select new { _channel, channelUser };

            var result = await query.FirstOrDefaultAsync();
            
            if (result == null)
                return null;

            return new ChannelUserDTO()
            {
                Name = result._channel.Name,
                Id = result._channel.Id,
                Description = result._channel.Description,
                InvitationId = result.channelUser.Id,
                Manager_email = result._channel.Manager_email,
                Questions = result._channel.Questions
            };
        }

        [HttpGet]
        public async Task<IActionResult> Invitations()
        {

            var user = await _userManager.GetUserAsync(User);
            ChannelUserDTO channelUserDTO = await GetChannelInviations(user.Email);
            ViewData["Channel"] = null;
            ViewData["Group"] = null;
            if (channelUserDTO != null)
                ViewData["Channel"] = channelUserDTO;
            else
            {
                GroupUserDTO groupUserDTO = await GetGroupInvitations(user.Email);
                if (groupUserDTO != null)
                    ViewData["Group"] = groupUserDTO;
            }
            
            return View("Invitations");
        }
    }
}
