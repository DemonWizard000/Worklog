using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    public class AdminController : Controller
    {
        private readonly WorkLogContext _context;
        public AdminController(WorkLogContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateSites()
        {
            return View("CreateSites");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ManageSites()
        {
            List<Tenant> tenantList = new List<Tenant>();
            tenantList = _context.Tenants.ToList();
            ViewData["Tenants"] = tenantList;
            return View("ManageSites");
        }

        [Authorize]
        public async Task<IActionResult> AccountSettings()
        {
            return View("Settings");
        }

        [HttpPost]
        [Authorize]
        public async Task<Settings> GetSettings(string UserEmail = null)
        {
            if (UserEmail == null)            
                UserEmail = HttpContext.Session.GetString("UserEmail");

            Settings settings = await _context.Settings.Where(x => x.UserEmail == UserEmail).FirstOrDefaultAsync();
            return settings;
        }

        [HttpPost]
        [Authorize]
        public async Task<bool> SetSettings(Settings settings)
        {
            try
            {
                _context.Settings.Update(settings);
                await _context.SaveChangesAsync();
                return true;
            } catch(Exception ex)
            {
                return false;
            }

        }
    }
}
