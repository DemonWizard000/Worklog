using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly WorkLogContext _workLogContext;
        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager,WorkLogContext workLogContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _workLogContext = workLogContext;
        }
        public IActionResult Index()
        {
            return View("SignIn");
        }

        public IActionResult SignIn()
        {
            return View("SignIn");
        }

        /*
            Simple SignIn API 
        */
        [HttpPost]
        /*
            Check forgery token 
        */
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInVM signIn, string ReturnUrl = "/Worklog/Welcome")
        {
            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));

            AppUser user;

            if (ModelState.IsValid)
            {
                user = await _userManager.Users.Where(x => x.Email == tenantId.ToString() + "-" + signIn.Email && x.TenantId == tenantId ).FirstOrDefaultAsync();

                if (user == null)
                {
                    ModelState.AddModelError("", "Email does not exist");
                    return View(signIn);
                }
                var result = await _signInManager.PasswordSignInAsync(user, signIn.Password, signIn.RememberMe, true);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Password is not correct");
                    return View(signIn);
                }
                HttpContext.Session.SetString("UserEmail", tenantId.ToString() + "-" + signIn.Email);
                HttpContext.Session.SetInt32("ChannelId", -1);

                user.HitCount += 1;
                await _userManager.UpdateAsync(user);

                return LocalRedirect(ReturnUrl);
            }
            return View(signIn);
        }

        public IActionResult Register()
        {
            return View("Register");
        }
        
        /*
            Simple Register API 
        */
        [HttpPost]
        /*
            Check forgery token 
        */
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM register)
        {
            if (!ModelState.IsValid) return View(register);
            
            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
            AppUser newUser = new AppUser
            {
                Email = tenantId.ToString() + "-" + register.Email,
                UserName = tenantId.ToString() + "-" + register.Username
            };
            newUser.TenantId = tenantId;

            IdentityResult result = await _userManager.CreateAsync(newUser, register.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(register);
            }

            /*
                Add default settings 
            */
            _workLogContext.Settings.Add(new Settings()
            {
                UserEmail =  newUser.Email
            });
            await _workLogContext.SaveChangesAsync();

            /*
                Set role as User as default.
            */
            result = await _userManager.AddToRoleAsync(newUser, "User");
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(register);
            }
            return RedirectToAction("SignIn");
        }
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(SignIn));
        }

        /*
            Add new Role(just for adding role to database) 
        */
        [HttpGet]
        public async Task<string> AddRole(string role)
        {
            IdentityRole newRole = new IdentityRole
            {
                Name = role
            };
            IdentityResult result = await _roleManager.CreateAsync(newRole);
            if (result.Succeeded)
                return "Success";
            else
                return "Failed";
        }

        [HttpPost]
        public async Task<bool> ChangeRole(string email, string role)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null) {
                    //var oldRoleId = user.
                    return true;
                }
            }
            return false;
        }

    }
}
