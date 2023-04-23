using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    [Authorize]
    public class GroupManagerController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly WorkLogContext _context;
        public GroupManagerController(UserManager<AppUser> userManager, WorkLogContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> List(long ChannelId)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if(ChannelId == 0)
                ChannelId = (long)HttpContext.Session.GetInt32("ChannelId");

            if (ChannelId == -1)
                ViewData["ChannelName"] = "Default";

            else
            {
                Channel t = await _context.Channels.FindAsync(ChannelId);
                if (t != null)
                    ViewData["ChannelName"] = t.Name;
                else
                    ViewData["ChannelName"] = "Default";
            }

            List<Group> Groups = _context.Groups.Where(s => s.Manager_email == user.Email && s.ChannelId == ChannelId).ToList();
            ViewData["Groups"] = Groups;
            ViewData["ChannelId"] = ChannelId;

            return View("List");
        }
    }
}
