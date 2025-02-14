using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class UxIssue
    {
        public string Category { get; set; } 
        public string Message { get; set; }
        public string Selector { get; set; } 
    }

    public class UxResult
    {
        public List<UxIssue> Issues { get; set; } = new List<UxIssue>();
    }
}