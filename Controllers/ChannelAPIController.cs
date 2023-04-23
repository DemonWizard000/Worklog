using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    /*
        Process Channel Related API 
    */
    [Authorize]
    [Route("api/[controller]")]
    public class ChannelAPIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly WorkLogContext _context;

        public ChannelAPIController(UserManager<AppUser> userManager, WorkLogContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        /*
            Add new Channel
        */
        [Route("addOrUpdateChannel")]
        [HttpPost]
        public async Task<Channel> AddOrUpdateChannel(Channel channel)
        {
            if (ModelState.IsValid)
            {
                long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
                var user = await _userManager.GetUserAsync(User);
                bool isUpdate = channel.Id == 0 ? false : true;

                channel.Manager_email = user?.Email;
                channel.TenantId = tenantId;

                if (isUpdate)
                    _context.Channels.Update(channel);
                else
                    _context.Channels.Add(channel);    

                await _context.SaveChangesAsync();

                if(!isUpdate)
                    _context.ChannelUsers.Add(new ChannelUsers()
                    {
                        ChannelId = channel.Id,
                        UserEmail = user?.Email,
                        State = 1
                    });

                await _context.SaveChangesAsync();

                return channel;
            }
            return channel;
        }

        [Route("removeChannel")]
        [HttpPost]
        public async Task<bool> RemoveChannel(long ChannelId)
        {
            if (ModelState.IsValid)
            {
                Channel channel = await _context.Channels.FindAsync(ChannelId);
                if (channel != null)
                {

                    _context.Channels.Remove(channel);

                    //clear Channel users
                    List<ChannelUsers> users = await _context.ChannelUsers.Where(x => x.ChannelId== ChannelId).ToListAsync();
                    users.ForEach(user => _context.ChannelUsers.Remove(user));

                    //clear groups and group users.
                    List<Group> groups = await _context.Groups.Where(x => x.ChannelId == channel.Id).ToListAsync();
                    groups.ForEach(async group =>
                    {
                        _context.Groups.Remove(group);
                        List<GroupUser> groupUsers = await _context.GroupUsers.Where(x => x.GroupId == group.Id).ToListAsync();
                        groupUsers.ForEach(groupUser => _context.GroupUsers.Remove(groupUser));
                    });
                    await _context.SaveChangesAsync();

                    return true;
                }
            }
            return false;
        }

        /*
            Invite user to channel. 
        */
        [Route("inviteUser")]
        [HttpPost]
        /* 
        response
        -4: userEmail does not exist.
        -3: Model is not valid
        -2: Can't invite yourself.
        -1: channelId does not exist
        0: already inviteds
        1: success
        */
        public async Task<int> InviteUser(ChannelUsers channelUsers)
        {
            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
            if (ModelState.IsValid)
            {
                channelUsers.UserEmail = tenantId.ToString() + "-" + channelUsers.UserEmail;
                var user = await _userManager.GetUserAsync(User);
                /*
                    You can't invite yourself. 
                */
                if (user?.Email == channelUsers.UserEmail)
                    return -2;

                /*
                    User Email does not exist.
                */
                user = await _userManager.Users.Where(x => x.Email == channelUsers.UserEmail && x.TenantId == tenantId).FirstOrDefaultAsync();
                if (user == null)
                    return -4;

                /*
                    ChannelId does not exist. 
                */
                Channel invitingChannel = await _context.Channels.FindAsync(channelUsers.ChannelId);
                if (invitingChannel == null)
                    return -1;

                /*
                    check if already invited to that channel 
                */
                int count = await _context.ChannelUsers.Where(x => x.UserEmail == channelUsers.UserEmail && x.ChannelId == channelUsers.ChannelId).CountAsync();
                if (count > 0)
                    return 0;

                _context.ChannelUsers.Add(channelUsers);
                await _context.SaveChangesAsync();
                return 1;
            }
            return -3;
        }

        [Route("cancelMember")]
        [HttpPost]
        public async Task<bool> CancelMember(ChannelUsers channelUser)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (channelUser.UserEmail == user.Email)
                    return false;

                ChannelUsers dbUser = await _context.ChannelUsers.Where(x => x.UserEmail == channelUser.UserEmail && x.ChannelId == channelUser.ChannelId).FirstOrDefaultAsync();

                if(dbUser == null) return false;
                _context.ChannelUsers.Remove(dbUser);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /*
            Accept invitation as user 
        */
        [Route("acceptInvite")]
        [HttpPost]
        public async Task<bool> AcceptInvite(long invitationId)
        {
            ChannelUsers channelUser = await _context.ChannelUsers.FindAsync(invitationId);
            
            if (channelUser == null) return false;
            
            channelUser.State = 1;
            _context.ChannelUsers.Update(channelUser);
            await _context.SaveChangesAsync();
            return true;
        }

        [Route("declineInvite")]
        [HttpPost]
        public async Task<bool> DeclineInvite(long invitationId)
        {
            ChannelUsers channelUser = await _context.ChannelUsers.FindAsync(invitationId);

            if (channelUser == null) return false;

            _context.ChannelUsers.Remove(channelUser);
            await _context.SaveChangesAsync();
            return true;
        }

        /*
            Join channel(just set session attribute ChannelId) 
        */
        [Route("joinChannel")]
        [HttpPost]
        public void JoinChannel(long channelId)
        {
            /*
                set ChannelId session attribute
            */
            HttpContext.Session.SetInt32("ChannelId", (int)channelId);
        }

        /*
            Get all users of specific channel 
        */
        [Route("getUsers")]
        [HttpPost]
        public List<ChannelUsers> GetChannelUsers(long channelId)
        {
            return _context.ChannelUsers.Where(e => e.ChannelId == channelId).ToList<ChannelUsers>();
        }
    }
}
