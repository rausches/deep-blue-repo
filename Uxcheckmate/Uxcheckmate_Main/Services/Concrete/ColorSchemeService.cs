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
        public static bool AreColorsSimilar((int R, int G, int B) color1, (int R, int G, int B) color2, int threshold = 50)
        {
            //Grabbing differences
            int diffR = color1.R - color2.R;
            int diffG = color1.G - color2.G;
            int diffB = color1.B - color2.B;
            //Grabbing overall difference
            double distance = Math.Sqrt(diffR * diffR + diffG * diffG + diffB * diffB);
            return distance <= threshold;
        }
        public static (int R, int G, int B) HexToRgb(string hex)
        {
            // Removing the hashtag
            if (hex.StartsWith("#")){
                hex = hex.Substring(1);
            }
            // Taking hex and converting to RGB
            if (hex.Length == 6){
               try{
                    int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                    int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                    int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                    return (r, g, b);
                }catch (FormatException){
                    throw new ArgumentException("Invalid hex color format. Use #RRGGBB or RRGGBB.");
                }
            }else{
                throw new ArgumentException("Invalid hex color format. Use #RRGGBB or RRGGBB.");
            }
        }
        public async Task<string> AnalyzeWebsiteColorsAsync(string url)
        {
            var scrapedData = await _webScraperService.ScrapeAsync(url);
            string htmlContent = (string)scrapedData["html_content"];
            var externalCss = (List<string>)scrapedData["external_css"];
            var extractedElements = ExtractHtmlElements(htmlContent, externalCss);
            // Legibility Info Grab
            var legibilityIssues = CheckLegibility(extractedElements);
            // 60-30-10 Issues Grab
            var colorPixelUsage = EstimateColorPixelUsage(extractedElements);
            var colorProportions = CalculateColorProportions(colorPixelUsage);
            var colorBalanceIssues = CheckColorBalanceIssues(colorProportions);
            // Combining
            var allIssues = legibilityIssues.Concat(colorBalanceIssues).ToList();
            // Returning issues
            return allIssues.Any() ? string.Join("\n", allIssues) : string.Empty;
        }
        public Dictionary<string, object> ExtractHtmlElements(string htmlContent, List<string> externalCss)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var tagColors = new Dictionary<string, string>();  // Tag colors [black default]
            var classColors = new Dictionary<string, string>(); // Class Colors
            var tagCharacterCount = new Dictionary<string, int>(); // Tag Characters Count
            var classCharacterCount = new Dictionary<string, int>(); // Class Characters count
            var tagFontSizes = new Dictionary<string, int>();  // Tag Sizes
            var classFontSizes = new Dictionary<string, int>(); // Class Sizes
            string backgroundColor = "#FFFFFF";
            var defaultFontSizes = new Dictionary<string, int>
            {
                { "h1", 32 }, { "h2", 24 }, { "h3", 18 }, { "h4", 16 }, { "h5", 14 }, { "h6", 12 }, { "p", 16 }
            };
            // Hex/Color setup checks
            var colorRegex = new Regex(@"(color|background(?:-color)?|border-color):\s*(#[0-9a-fA-F]{3,6}|rgba?\([^)]+\));", RegexOptions.IgnoreCase);
            var fontSizeRegex = new Regex(@"font-size:\s*(\d+)px;", RegexOptions.IgnoreCase);
            var backgroundColorRegex = new Regex(@"body\s*\{[^}]*background-color:\s*(#[0-9a-fA-F]{3,6});", RegexOptions.IgnoreCase);
            var styleBlocks = doc.DocumentNode.SelectNodes("//style") ?? new HtmlNodeCollection(null);
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null){
                string bodyStyle = bodyNode.GetAttributeValue("style", "");
                Match bgColorMatch = colorRegex.Match(bodyStyle);
                if (bgColorMatch.Success){
                    backgroundColor = bgColorMatch.Groups[2].Value;
                }
            }
            foreach (var styleNode in styleBlocks){
                var styleText = styleNode.InnerText;
                Match bgMatch = backgroundColorRegex.Match(styleText);
                if (bgMatch.Success){
                    backgroundColor = bgMatch.Groups[1].Value;
                }
                // Checking for color match
                var matches = Regex.Matches(styleText, @"\.([\w-]+)\s*\{[^}]*color:\s*(#[0-9a-fA-F]{3,6});", RegexOptions.IgnoreCase);
                foreach (Match match in matches){
                    string className = match.Groups[1].Value.Trim();
                    string color = match.Groups[2].Value.Trim();
                    if (!classColors.ContainsKey(className)){
                        classColors[className] = color;
                    }
                }
                var fontMatches = Regex.Matches(styleText, @"\.([\w-]+)\s*\{[^}]*font-size:\s*(\d+)px;", RegexOptions.IgnoreCase);
                foreach (Match match in fontMatches){
                    string className = match.Groups[1].Value.Trim();
                    int fontSize = int.Parse(match.Groups[2].Value);
                    if (!classFontSizes.ContainsKey(className)) classFontSizes[className] = fontSize;
                }
            }
            foreach (var tag in defaultFontSizes.Keys){
                var elements = doc.DocumentNode.SelectNodes($"//{tag}") ?? new HtmlNodeCollection(null);
                foreach (var element in elements){
                    string assignedColor = "#000000"; // Default to black
                    int fontSize = defaultFontSizes[tag]; // Default font size
                    bool countedUnderClass = false;
                    var classAttr = element.GetAttributeValue("class", "").Split(' ');
                    foreach (var cls in classAttr){
                        if (classColors.ContainsKey(cls)){
                            assignedColor = classColors[cls];
                        }
                        if (classFontSizes.ContainsKey(cls)){
                            fontSize = classFontSizes[cls];
                        }
                    }
                    // Extract inline styles (color and font-size)
                    string inlineStyle = element.GetAttributeValue("style", "");
                    Match colorMatch = colorRegex.Match(inlineStyle);
                    if (colorMatch.Success){
                        assignedColor = colorMatch.Groups[2].Value;
                    }
                    Match fontSizeMatch = fontSizeRegex.Match(inlineStyle);
                    if (fontSizeMatch.Success){
                        fontSize = int.Parse(fontSizeMatch.Groups[1].Value);
                    }
                    // Counting characters
                    string textContent = element.InnerText.Trim();
                    if (!string.IsNullOrEmpty(textContent)){
                        if (classAttr.Any(cls => classColors.ContainsKey(cls) || classFontSizes.ContainsKey(cls))){
                            // Assign text to the class instead of the tag
                            foreach (var cls in classAttr){
                                if (!classCharacterCount.ContainsKey(cls)) classCharacterCount[cls] = 0;
                                classCharacterCount[cls] += textContent.Length;
                            }
                        }
                        else{
                            // Count text under the tag
                            if (!tagCharacterCount.ContainsKey(tag)) tagCharacterCount[tag] = 0;
                            tagCharacterCount[tag] += textContent.Length;
                        }
                    }
                    if (!tagFontSizes.ContainsKey(tag)) tagFontSizes[tag] = fontSize;
                    if (!tagColors.ContainsKey(tag)) tagColors[tag] = assignedColor;
                }
            }
            return new Dictionary<string, object>
            {
                { "tag_colors", tagColors },
                { "class_colors", classColors },
                { "character_count_per_tag", tagCharacterCount },
                { "character_count_per_class", classCharacterCount },
                { "tag_font_sizes", tagFontSizes ?? new Dictionary<string, int>() },
                { "class_font_sizes", classFontSizes ?? new Dictionary<string, int>() },
                { "background_color", backgroundColor }
            };
        }
        // Pixel formula: characterCount*(fontSize*0.5)*fontSize [Estimating how many pixels characters take up to be half probably an overestimate since spaces]
        public Dictionary<string, int> EstimateColorPixelUsage(Dictionary<string, object> extractedData)
        {
            var estimatedPixelUsage = new Dictionary<string, int>();
            var tagCharacterCount = (Dictionary<string, int>)extractedData["character_count_per_tag"];
            var classCharacterCount = (Dictionary<string, int>)extractedData["character_count_per_class"];
            var tagFontSizes = (Dictionary<string, int>)extractedData["tag_font_sizes"];
            var classFontSizes = (Dictionary<string, int>)extractedData["class_font_sizes"];
            var tagColors = (Dictionary<string, string>)extractedData["tag_colors"];
            var classColors = (Dictionary<string, string>)extractedData["class_colors"];
            string backgroundColor = extractedData.ContainsKey("background_color") ? (string)extractedData["background_color"] : "#FFFFFF";
            var defaultFontSizes = new Dictionary<string, int>
            {
                { "h1", 32 }, { "h2", 24 }, { "h3", 18 }, { "h4", 16 }, { "h5", 13 }, { "h6", 11 }, { "p", 16 }
            };
            int totalPixels = 2_073_600; // Assumming 1080p
            int totalUsedPixels = 0;
            // Tag color pixels
            foreach (var tag in tagCharacterCount.Keys){
                int fontSize = tagFontSizes.ContainsKey(tag) ? tagFontSizes[tag] : (defaultFontSizes.ContainsKey(tag) ? defaultFontSizes[tag] : 16);
                if (tagColors.TryGetValue(tag, out string color)){
                    int charCount = tagCharacterCount[tag];
                    int pixelArea = (int)(charCount * (fontSize * 0.5) * fontSize);
                    if (!estimatedPixelUsage.ContainsKey(color)){
                        estimatedPixelUsage[color] = 0;
                    }
                    estimatedPixelUsage[color] += pixelArea;
                    totalUsedPixels += pixelArea;
                }
            }
            // Class color pixels
            foreach (var className in classCharacterCount.Keys){
                int fontSize = classFontSizes.ContainsKey(className) ? classFontSizes[className] : 16;
                if (classColors.TryGetValue(className, out string color)){
                    int charCount = classCharacterCount[className];
                    int pixelArea = (int)(charCount * (fontSize * 0.5) * fontSize);
                    if (!estimatedPixelUsage.ContainsKey(color)){
                        estimatedPixelUsage[color] = 0;
                    }
                    estimatedPixelUsage[color] += pixelArea;
                    totalUsedPixels += pixelArea;
                }
            }
            int backgroundRemaining = Math.Max(totalPixels - totalUsedPixels, 0);
            if (!estimatedPixelUsage.ContainsKey(backgroundColor)){
                estimatedPixelUsage[backgroundColor] = 0;
            }
            estimatedPixelUsage[backgroundColor] = backgroundRemaining;
            return estimatedPixelUsage;
        }
        public Dictionary<string, double> CalculateColorProportions(Dictionary<string, int> colorPixelUsage)
        {
            int totalPixels = colorPixelUsage.Values.Sum();
            var colorProportions = new Dictionary<string, double>();
            // Empty check
            if (totalPixels == 0){
                return colorProportions;
            }
            foreach (var color in colorPixelUsage){
                double percentage = (color.Value / (double)totalPixels) * 100.0;
                colorProportions[color.Key] = Math.Round(percentage, 2);
            }
            return colorProportions;
        }
        public Dictionary<string, double> CheckColorBalance(Dictionary<string, double> colorProportions, int similarityThreshold = 15)
        {
            // Going to group similar colors
            var groupedColors = new Dictionary<string, double>();
            // Go in order from most dominant color for similar color checks
            var sortedColors = colorProportions.OrderByDescending(c => c.Value).ToList();
            foreach (var (color, percentage) in sortedColors){
                // Similar color compare
                var color1 = HexToRgb(color);
                string closestMatch = null;
                foreach (var existingColor in groupedColors.Keys.ToList()){
                    var color2 = HexToRgb(existingColor);
                    if (AreColorsSimilar(color1, color2)){
                        closestMatch = existingColor;
                        break;  // Mergin color
                    }
                }
                // Checking percentage matching
                if (closestMatch != null){
                    groupedColors[closestMatch] += percentage;
                }
                else{
                    groupedColors[color] = percentage;
                }
            }
            return groupedColors;
        }
        public bool IsColorBalanced(Dictionary<string, double> colorBalance)
        {
            if (colorBalance.Count == 0){
                return false; 
            }
            // Sorting
            var sortedColors = colorBalance.OrderByDescending(c => c.Value).ToList();
            // Check top 3
            double primary = sortedColors[0].Value; // Most used color
            double secondary = sortedColors.Count > 1 ? sortedColors[1].Value : 0;
            double accent = sortedColors.Count > 2 ? sortedColors[2].Value : 0;
            // Check remaining
            double remaining = sortedColors.Skip(3).Sum(c => c.Value);
            // 15% variance on colors
            bool primaryValid = primary >= 45 && primary <= 75;
            bool secondaryValid = secondary >= 15 && secondary <= 45;
            bool accentValid = accent >= 0 && accent <= 25;
            bool extraValid = remaining <= 15;
            return primaryValid && secondaryValid && accentValid && extraValid;
        }
        // Protanopia Red Color Issues 
        public static bool ProtanopiaIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            var protanopia1 = ((int)(color1.R * 0.2), (int)(color1.G * 0.8), (int)(color1.B * 0.8));
            var protanopia2 = ((int)(color2.R * 0.2), (int)(color2.G * 0.8), (int)(color2.B * 0.8));
            return AreColorsSimilar(protanopia1, protanopia2, 110);
        }
        // Protanomaly Slight Red Color issue
        public static bool ProtanomalyIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            var protanomaly1 = ((int)(color1.R * 0.3), color1.G, color1.B);
            var protanomaly2 = ((int)(color2.R * 0.3), color2.G, color2.B);
            return AreColorsSimilar(protanomaly1, protanomaly2, 110);
        }
        // Deuteranopia Green Color Issues
        public static bool DeuteranopiaIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            var deuteranopia1 = ((int)(color1.R * 0.9), 0, (int)(color1.B * 0.7));
            var deuteranopia2 = ((int)(color2.R * 0.9), 0, (int)(color2.B * 0.7));
            return AreColorsSimilar(deuteranopia1, deuteranopia2, 120);
        }
        // Deuteranomaly Slight Green Color issue
        public static bool DeuteranomalyIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            var deuteranomaly1 = (color1.R, (int)(color1.G * 0.2), color1.B);
            var deuteranomaly2 = (color2.R, (int)(color2.G * 0.2), color2.B);
            return AreColorsSimilar(deuteranomaly1, deuteranomaly2, 120);
        }
        // Tritanopia Blue Color Issues
        public static bool TritanopiaIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            var tritanopia1 = ((int)(color1.R * 0.7), (int)(color1.G * 0.9), 0);
            var tritanopia2 = ((int)(color2.R * 0.7), (int)(color2.G * 0.9), 0);
            return AreColorsSimilar(tritanopia1, tritanopia2, 130);
        }
        // Tritanomaly  Slight blue issue
        public static bool TritanomalyIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            var tritanomaly1 = (color1.R, color1.G, (int)(color1.B * 0.2));
            var tritanomaly2 = (color2.R, color2.G, (int)(color2.B * 0.2));
            return AreColorsSimilar(tritanomaly1, tritanomaly2, 130);
        }
        // Achromatopsia [Complete Color Blindness]
        public static bool AchromatopsiaIssue(string hex1, string hex2)
        {
            var color1 = HexToRgb(hex1);
            var color2 = HexToRgb(hex2);
            int gray1 = (int)(color1.R * 0.299 + color1.G * 0.587 + color1.B * 0.114);
            int gray2 = (int)(color2.R * 0.299 + color2.G * 0.587 + color2.B * 0.114);
            return AreColorsSimilar((gray1, gray1, gray1), (gray2, gray2, gray2), 100);
        }
        public List<string> CheckLegibility(Dictionary<string, object> extractedData)
        {
            var issues = new List<string>();
            var tagColors = (Dictionary<string, string>)extractedData["tag_colors"];
            var classColors = (Dictionary<string, string>)extractedData["class_colors"];
            var backgroundColor = (string)extractedData["background_color"];
            foreach (var tag in tagColors){
                string textColorHex = tag.Value;
                string bgColorHex = backgroundColor;
                // Use background if has one
                if (classColors.TryGetValue(tag.Key, out string classBgColor) && !string.IsNullOrEmpty(classBgColor)){
                    bgColorHex = classBgColor;
                }
                var textColor = HexToRgb(textColorHex);
                var bgColor = HexToRgb(bgColorHex);
                // Add to issues if 
                if (AreColorsSimilar(textColor, bgColor)){
                    issues.Add($"Low contrast in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (ProtanopiaIssue(textColorHex, bgColorHex)){
                    issues.Add($"Protanopia issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (ProtanomalyIssue(textColorHex, bgColorHex)){
                    issues.Add($"Protanomaly issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (DeuteranopiaIssue(textColorHex, bgColorHex)){
                    issues.Add($"Deuteranopia issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (DeuteranomalyIssue(textColorHex, bgColorHex)){
                    issues.Add($"Deuteranomaly issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (TritanopiaIssue(textColorHex, bgColorHex)){
                    issues.Add($"Tritanopia issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (TritanomalyIssue(textColorHex, bgColorHex)){
                    issues.Add($"Tritanomaly issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
                if (AchromatopsiaIssue(textColorHex, bgColorHex)){
                    issues.Add($"Achromatopsia issue in <{tag.Key}>: {textColorHex} on {bgColorHex}");
                }
            }
            return issues;
        }
        public List<string> CheckColorBalanceIssues(Dictionary<string, double> colorProportions)
        {
            var issues = new List<string>();
            var colorBalance = CheckColorBalance(colorProportions);
            if (!IsColorBalanced(colorBalance)){
                issues.Add("Color balance issue: The proportions of primary, secondary, and accent colors are not within recommended ranges.");
                foreach (var color in colorBalance.OrderByDescending(c => c.Value)){
                    issues.Add($"Color {color.Key}: {color.Value}%");
                }
            }
            return issues;
        }
    }
}
