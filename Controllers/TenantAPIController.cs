using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TenantAPIController : Controller
    {
        private List<string> defaultQuestions = new List<string>{
            "How do you feel?",
            "What are 5 things I accomplished yesterday?",
            "What are 5 things I didn't completed yesterday?",
            "What are 5 things I can do differently today?",
            "What are 5 things I want to accomplish today?",
            "What are 5 things that might get in my way today?",
            "What is my commitment today?"
        };

        private List<string> defaultNavigationLabels = new List<string>
        {
            "Home", "About", "How to", "Contact us", "Performance Insights", "Feedback"
        };

        private string defaultHomePageContent = "Work Log provides a guided reflection of your daily work habits.<br/> Over time, patterns will emerge and you will find opportunities to improve your work and work experience.";
        private string defaultAboutPageContent = "Self-reflection leads to greater life satisfaction and personal growth.<br/>Myworklog.net provides a guided path to reviewing your work day and reflecting on your thoughts and performance.<br/>This can lead to increased self-awareness and a better understanding of one\''s strengths and weaknesses.<br/>In turn, this self-reflection can lead to improved decision-making and a more fulfilling work life (Baumeister, 2018). Research has shown that guided logs and journals can be an effective tool for self-reflection and personal growth. In a study by Baumeister et al. (2018), participants who completed a guided self-reflection exercise showed improvements in self-awareness, goal clarity, and life satisfaction.";
        private string defaultHowToPageContent = "1. Follow the question prompts.<br/> 2.Spend a few moments on the Summary View when you are done.<br/> 3. After a 10 visits, you may use the Insight Options.";

        private readonly WorkLogContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TenantAPIController(WorkLogContext context, UserManager<AppUser> userMananger)
        {
            _context = context;
            _userManager= userMananger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("createOrEditSite")]
        [HttpPost]
        public async Task<int> createOrEditSite(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                Tenant oldTenant = _context.Tenants.Where(x => x.URL == tenant.URL || x.Name == tenant.Name || x.Title == tenant.Title).FirstOrDefault();
                if (oldTenant != null)
                {
                    return -1;
                }
                tenant.NavigationLabels = String.Join(",#", defaultNavigationLabels);
                tenant.DefaultQuestions = String.Join(",#", defaultQuestions);
                tenant.Home_Page_Content = defaultHomePageContent;
                tenant.About_Page_Content = defaultAboutPageContent;
                tenant.How_To_Page_Content = defaultHowToPageContent;

                _context.Add(tenant);
                await _context.SaveChangesAsync();
                return 1;
            }
            return 0;
        }

        [Route("editSite")]
        [HttpPost]
        public async Task<IActionResult> EditSites(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                Tenant originalTenant = await _context.Tenants.FindAsync(tenant.id);
                originalTenant.CustomCSSURL = tenant.CustomCSSURL;
                originalTenant.Description = tenant.Description;
                originalTenant.Title = tenant.Title;
                originalTenant.PageName = tenant.PageName;
                originalTenant.URL = tenant.URL;
                
                originalTenant.Home_Page_Content = tenant.Home_Page_Content;
                originalTenant.About_Page_Content = tenant.About_Page_Content;
                originalTenant.How_To_Page_Content = tenant.How_To_Page_Content;

                _context.Tenants.Update(originalTenant);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return NoContent();
        }

        [Route("setNavigationLabelAndQuestions")]
        [HttpPost]
        public async Task<bool> SetNavigationLabelAndQuestions(long tenantId, string navigationLabels, string questions)
        {
            Tenant tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.DefaultQuestions = questions;
                tenant.NavigationLabels = navigationLabels;
                _context.Tenants.Update(tenant);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        [Route("setReady")]
        [HttpPost]
        public async Task<bool> SetReady(long tenantId)
        {
            Tenant tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null)
            {
                tenant.IsReady= true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
