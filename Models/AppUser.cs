using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class AppUser: IdentityUser
    {
        [Required]
        public long TenantId { get; set; }

        [Required]
        public int HitCount { get; set; } = 0;
    }
}
