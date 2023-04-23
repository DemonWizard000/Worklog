using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class GroupAPIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly WorkLogContext _context;

        public GroupAPIController(UserManager<AppUser> userManager, WorkLogContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("addOrUpdateGroup")]
        [HttpPost]
        public async Task<Group> AddOrUpdateGroup(Group group)
        {
            if (ModelState.IsValid)
            {
                long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
                bool isUpdate = group.Id == 0 ? false : true;
                var user = await _userManager.GetUserAsync(User);

                group.Manager_email = user?.Email;
                group.TenantId = tenantId;

                //this means update
                if (isUpdate)
                    _context.Groups.Update(group);

                //this means create
                else
                    _context.Groups.Add(group);

                await _context.SaveChangesAsync();

                if (!isUpdate)
                    _context.GroupUsers.Add(new GroupUser()
                    {
                        GroupId = group.Id,
                        UserEmail = user?.Email,
                        State = 1
                    });

                await _context.SaveChangesAsync();

                return group;
            }
            return null;
        }

        [Route("removeGroup")]
        [HttpPost]
        public async Task<bool> RemoveGroup(int Id)
        {
            if (ModelState.IsValid)
            {
                Group group = await _context.Groups.FindAsync((long)Id);

                if(group != null)
                {
                    //Remove all group users
                    List<GroupUser> groupUsers = await _context.GroupUsers.Where(x => x.UserEmail == group.Manager_email).ToListAsync();
                    groupUsers.ForEach(groupUser => _context.GroupUsers.Remove(groupUser));

                    _context.Groups.Remove(group);

                }
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        [Route("inviteUser")]
        [HttpPost]
        public async Task<int> InviteUser(GroupUser groupUser)
        {
            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
            if (ModelState.IsValid)
            {
                groupUser.UserEmail = tenantId.ToString() + "-" + groupUser.UserEmail;
                var user = await _userManager.GetUserAsync(User);
                /*
                    You can't invite yourself. 
                */
                if (user?.Email == groupUser.UserEmail)
                    return -2;

                /*
                    User Email does not exist.
                */
                user = await _userManager.Users.Where(x => x.Email == groupUser.UserEmail && x.TenantId == tenantId).FirstOrDefaultAsync();
                if (user == null)
                    return -4;

                /*
                    GroupId does not exist. 
                */
                Group invitingGroup = await _context.Groups.FindAsync(groupUser.GroupId);
                if (invitingGroup == null)
                    return -1;

                /*
                    User is not member of channel yet. 
                */
                if (invitingGroup.ChannelId != -1)
                {
                    int channelUserCount = await _context.ChannelUsers.Where(x => x.ChannelId == invitingGroup.ChannelId && x.UserEmail == user.Email && x.State == 1).CountAsync();
                    if (channelUserCount == 0)
                        return -5;
                }

                /*
                    check if already invited to that group 
                */
                int count = await _context.GroupUsers.Where(x => x.UserEmail == groupUser.UserEmail && x.GroupId == groupUser.GroupId).CountAsync();
                if (count > 0)
                    return 0;


                _context.GroupUsers.Add(groupUser);
                await _context.SaveChangesAsync();
                return 1;
            }
            return -3;
        }

        [Route("cancelMember")]
        [HttpPost]
        public async Task<bool> CancelMember(long Id)
        {
            if (ModelState.IsValid)
            {
                GroupUser dbUser = await _context.GroupUsers.Where(x => x.Id == Id).FirstOrDefaultAsync();

                if (dbUser == null) return false;
                _context.GroupUsers.Remove(dbUser);
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
            GroupUser groupUser = await _context.GroupUsers.FindAsync(invitationId);

            if (groupUser == null) return false;

            groupUser.State = 1;
            _context.GroupUsers.Update(groupUser);
            await _context.SaveChangesAsync();
            return true;
        }

        [Route("declineInvite")]
        [HttpPost]
        public async Task<bool> DeclineInvite(long invitationId)
        {
            GroupUser groupUser = await _context.GroupUsers.FindAsync(invitationId);

            if (groupUser == null) return false;

            _context.GroupUsers.Remove(groupUser);
            await _context.SaveChangesAsync();
            return true;
        }

        /*
            Get all users of specific channel 
        */
        [Route("getUsers")]
        [HttpPost]
        public List<GroupUser> GetGroupUsers(long groupId)
        {
            return _context.GroupUsers.Where(e => e.GroupId == groupId).ToList<GroupUser>();
        }
    }
}
