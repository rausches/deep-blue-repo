using System;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class ScrapedContent
    {
        public string Url { get; set; } = "";
        public List<string> InlineCss { get; set; } = new();
        public List<string> ExternalCssLinks { get; set; } = new(); 
        public List<string> ExternalCssContents { get; set; } = new();
        public List<string> InlineJs { get; set; } = new();
        public List<string> ExternalJsLinks { get; set; } = new(); 
        public List<string> ExternalJsContents { get; set; } = new(); 
    }
}