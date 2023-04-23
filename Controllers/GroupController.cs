using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly WorkLogContext _context;

        public GroupController(UserManager<AppUser> userManager, WorkLogContext answerContext)
        {
            _userManager = userManager;
            _context = answerContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Manage(int GroupId = -1)
        {
            Group group = await _context.Groups.FindAsync((long)GroupId);
            ViewData["Group"] = group;

            List<GroupUser> groupUsers = await _context.GroupUsers.Where(x => x.GroupId == (long)GroupId).ToListAsync();
            ViewData["GroupUsers"] = groupUsers;

            return View("Manage");
        }
    }
}
