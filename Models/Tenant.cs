using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    
    public class Tenant
    {
        public long id { get; set; }

        [Required]
        public string URL { get; set; } = "";

        [Required]
        public string PageName { get; set; } = "";

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string CustomCSSURL { get; set; } = "";

        [Required]
        public string CustomSettingURL { get; set; } = "";


        public string NavigationLabels { get; set; } = "";

        public string DefaultQuestions { get; set; } = "";

        public string Home_Page_Content { get; set; } = "";
        public string About_Page_Content { get; set; } = "";
        public string How_To_Page_Content { get; set; } = "";

        [Required]
        public bool IsReady { get; set; } = false;
    }
}
