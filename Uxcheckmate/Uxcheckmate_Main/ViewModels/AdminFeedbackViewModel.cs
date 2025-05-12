using System.Collections.Generic;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.ViewModels
{
    public class AdminFeedbackViewModel
    {
        public string? UserId { get; set; }
        public List<UserFeedback> Feedbacks { get; set; } = new();
    }
}
