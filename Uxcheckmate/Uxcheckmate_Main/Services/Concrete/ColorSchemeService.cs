using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Uxcheckmate_Main.Services
{
    public class ColorSchemeService : IColorSchemeService
    {
        private readonly WebScraperService _webScraperService;

        public ColorSchemeService(WebScraperService webScraperService)
        {
            _webScraperService = webScraperService;
        }
        public bool AreColorsSimilar((int R, int G, int B) color1, (int R, int G, int B) color2, int threshold = 50)
        {
            //Grabbing differences
            int diffR = color1.R - color2.R;
            int diffG = color1.G - color2.G;
            int diffB = color1.B - color2.B;
            //Grabbing overall difference
            double distance = Math.Sqrt(diffR * diffR + diffG * diffG + diffB * diffB);
            return distance < threshold;
        }
        public static (int R, int G, int B) HexToRgb(string hex)
        {
            // Removing the hashtag
            if (hex.StartsWith("#")){
                hex = hex.Substring(1);
            }
            // Taking hex and converting to RGB
            if (hex.Length == 6){
                int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                return (r, g, b);
            }else{
                throw new ArgumentException("Invalid hex color format. Use #RRGGBB or RRGGBB.");
            }
        }
        public async Task<Dictionary<string, object>> AnalyzeWebsiteColorsAsync(string url)
        {
            var scrapedData = await _webScraperService.ScrapeAsync(url);
            var colorUsage = (Dictionary<string, int>)scrapedData["colors_used"];

            var colorAnalysis = colorUsage
                .Select(c => new { Hex = c.Key, RGB = HexToRgb(c.Key), Count = c.Value })
                .OrderByDescending(c => c.Count)
                .ToList();

            return new Dictionary<string, object>
            {
                { "dominant_colors", colorAnalysis },
                { "tag_counts", scrapedData["tag_counts"] }
            };
        }
        public Dictionary<string, object> ExtractHtmlElements(string htmlContent, List<string> externalCss)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var tagColors = new Dictionary<string, string>();  // Tag colors [black default]
            var classColors = new Dictionary<string, string>(); // Class Colors
            var tagCharacterCount = new Dictionary<string, int>(); // Tag Count
            var classCharacterCount = new Dictionary<string, int>(); // Class count
            // Hex/Color setup checks
            var colorRegex = new Regex(@"(color|background(?:-color)?|border-color):\s*(#[0-9a-fA-F]{3,6}|rgba?\([^)]+\));", RegexOptions.IgnoreCase);
            var styleBlocks = doc.DocumentNode.SelectNodes("//style") ?? new HtmlNodeCollection(null);
            foreach (var styleNode in styleBlocks){
                var styleText = styleNode.InnerText;
                // Checking for color match
                var matches = Regex.Matches(styleText, @"\.([\w-]+)\s*\{[^}]*color:\s*(#[0-9a-fA-F]{3,6});", RegexOptions.IgnoreCase);
                foreach (Match match in matches){
                    string className = match.Groups[1].Value.Trim();
                    string color = match.Groups[2].Value.Trim();
                    if (!classColors.ContainsKey(className)){
                        classColors[className] = color;
                    }
                }
            }
            foreach (var tag in new[] { "h1", "h2", "h3", "h4", "h5", "h6", "p", "a", "span", "div" }){
                var elements = doc.DocumentNode.SelectNodes($"//{tag}") ?? new HtmlNodeCollection(null);
                foreach (var element in elements){
                    string assignedColor = "#000000"; // Black default
                    bool countedUnderClass = false; // Bool for need of class count or tag count
                    // Checking for class counts
                    var classAttr = element.GetAttributeValue("class", "").Split(' ');
                    foreach (var cls in classAttr){
                        if (classColors.ContainsKey(cls)){
                            assignedColor = classColors[cls];
                            // Setup for class count
                            if (!classCharacterCount.ContainsKey(cls)){
                                classCharacterCount[cls] = 0;
                            }
                            classCharacterCount[cls] += element.InnerText.Trim().Length;
                            countedUnderClass = true;
                            break; // Was setup break
                        }
                    }
                    // Inline check
                    if (!countedUnderClass){
                        string inlineStyle = element.GetAttributeValue("style", "");
                        Match colorMatch = colorRegex.Match(inlineStyle);
                        if (colorMatch.Success){
                            assignedColor = colorMatch.Groups[2].Value;
                        }
                        // Tag check
                        if (!tagCharacterCount.ContainsKey(tag)){
                            tagCharacterCount[tag] = 0;
                        }
                        tagCharacterCount[tag] += element.InnerText.Trim().Length;
                    }
                    // Store color if it hasn't been added
                    if (!tagColors.ContainsKey(tag)){
                        tagColors[tag] = assignedColor;
                    }
                }
            }
            return new Dictionary<string, object>
            {
                { "tag_colors", tagColors },
                { "class_colors", classColors },
                { "character_count_per_tag", tagCharacterCount },
                { "character_count_per_class", classCharacterCount }
            };
        }
    }
}
