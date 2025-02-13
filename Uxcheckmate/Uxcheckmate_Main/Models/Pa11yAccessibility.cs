using System.Collections.Generic;

namespace Uxcheckmate_Main.Models;

public class Pa11yIssue{
        public string Code { get; set; }
        public string Message { get; set; }
        public string Selector { get; set; }
}

public class Pa11yResult{
        public List<Pa11yIssue> Issues { get; set; }
}