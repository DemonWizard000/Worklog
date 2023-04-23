using Microsoft.AspNetCore.Identity;
using WorkLog.Models;

namespace WorkLog.Utils
{
    public class AuthUtils
    {
        private IHttpContextAccessor _httpContextAccessor;
        private UserManager<AppUser> _userManager;
        public AuthUtils(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task<AppUser?> getCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }
            return await _userManager.GetUserAsync(user);

        }
    }
}
