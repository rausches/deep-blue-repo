using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Uxcheckmate_Main.Models
{
    public class CheckHtmlHierarchy
    {
        public string? html { get; set; }
        private List<string> _headerOrder { get; set; } = new List<string>();

        public CheckHtmlHierarchy(string html)
        {
            this.html = html;
        }
        public CheckHtmlHierarchy()
        {
        }
        public static CheckHtmlHierarchy CreateFromHtml(string htmlContent)
        {
            return new CheckHtmlHierarchy(htmlContent);
        }

        // Hold Header Info
        private class HeadingInfo
        {
            public int Level { get; set; }
            public string Tag { get; set; } = "";
            public string Text { get; set; } = "";
            public int Index { get; set; }
        }
        // Grabing a list of headers in order they show up
        private List<HeadingInfo> GetHeadings()
        {
            // Setting up and redoing the listing
            var headings = new List<HeadingInfo>();
            _headerOrder.Clear();
            // If not html it would be blank
            if (string.IsNullOrEmpty(html)){
                return headings;
            }
            // Using the agility stuff
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
            if (nodes == null){
                return headings;
            }
            // Filling out the headings
            int index = 0;
            foreach (var node in nodes){
                // Make sure no typecasing issues
                string tagName = node.Name.ToLower();
                // fiding header and number
                if (tagName.StartsWith("h") && tagName.Length == 2 && char.IsDigit(tagName[1])){
                    // Turning number from string to int
                    int level = int.Parse(tagName.Substring(1));
                    headings.Add(new HeadingInfo
                    {
                        Level = level,
                        Tag = tagName,
                        Text = node.InnerText.Trim(),
                        Index = index
                    });
                    _headerOrder.Add(tagName);
                    index++;
                }
            }
            return headings;
        }
        public bool problemsFound()
        {
            var headings = GetHeadings();
            // No heading check
            if (headings.Count == 0){
                return false;
            }
            // Make sure first heading is 1
            if (headings[0].Level != 1){
                return true;
            }
            // Looking to see if there are jumps
            for (int i = 1; i < headings.Count; i++){
                // More than 1 header level away is a problem
                if (headings[i].Level > headings[i - 1].Level + 1){
                    return true;
                }
            }
            return false;
        }

        // Listing The Problems
        public List<string> ProblemSpots()
        {
            var problems = new List<string>();
            var headings = GetHeadings();
            // Return nothing if no headers
            if (headings.Count == 0){
                return problems;
            }
            // If first string isn't 1 say that it shouldn't be
            if (headings[0].Level != 1){
                problems.Add($"The first heading is {headings[0].Tag} (\"{headings[0].Text}\") but it should be h1.");
            }
            // Check each subsequent heading for improper jumps.
            for (int i = 1; i < headings.Count; i++){
                if (headings[i].Level > headings[i - 1].Level + 1){
                    // Add problem to list
                    problems.Add($"Heading jump between {headings[i - 1].Tag} (\"{headings[i - 1].Text}\") and {headings[i].Tag} (\"{headings[i].Text}\"). " + $"Expected heading level h{headings[i - 1].Level + 1} but found h{headings[i].Level}.");
                }
            }
            return problems;
        }
    }
}
