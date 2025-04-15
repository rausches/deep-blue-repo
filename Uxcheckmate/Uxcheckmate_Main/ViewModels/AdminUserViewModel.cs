using Microsoft.AspNetCore.Identity;
using Uxcheckmate_Main.Models; // Adjust if Report is elsewhere

namespace Uxcheckmate_Main.ViewModels
{
    public class AdminUserViewModel
    {
        public IdentityUser User { get; set; }
        public List<Report> Reports { get; set; }
    }
}