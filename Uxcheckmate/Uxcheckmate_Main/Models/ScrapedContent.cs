using System;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class ScrapedContent
    {
        public string Url { get; set; } 
        public string Html { get; set; }
        public string InlineCss { get; set; }            
        public string InlineJs { get; set; } 
        public List<string> InlineCssList { get; set; } 
        public List<string> InlineJsList { get; set; }
        public List<string> ExternalCssLinks { get; set; }
        public List<string> ExternalJsLinks { get; set; }
        public List<string> ExternalCssContents { get; set; }
        public List<string> ExternalJsContents { get; set; }
        public double ScrollHeight { get; set; }
        public double ViewportHeight { get; set; }
        public double ScrollWidth { get; set; }
        public double ViewportWidth { get; set; }
        public string ViewportLabel { get; set; }
    }
}
