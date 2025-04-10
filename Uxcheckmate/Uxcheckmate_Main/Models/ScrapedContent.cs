using System;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class ScrapedContent
    {
        public string html { get; set; }
        public string cssFile { get; set; }
        public string jsFile { get; set; }
        public List<string> cssFiles { get; set; } = new List<string>();
        public List<string> jsFiles { get; set; } = new List<string>();
        public Dictionary<string, string> cssSelectors { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> htmlElements { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> elementCounts { get; set; } = new Dictionary<string, int>();

        // method to extract inline css and js into a separate file and append to lists below
        // list of css files 
        // list of js files
        // method to extract css from css files and map [selector: rules]
        // method to extract html from html file and map [element: content]
        // method to map html elements to count
    }
}