using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class AnimationService : IAnimationService
    {
        private readonly ILogger<AnimationService> _logger;
        private const int MaxRecommendedAnimations = 5;
        private const int MaxRecommendedAnimationsPerContainer = 2; 

        public AnimationService(ILogger<AnimationService> logger)
        {
            _logger = logger;
        }

        // Entry point method runs the animation analysis using the scraped content
        public async Task<string> RunAnimationAnalysisAsync(string url, Dictionary<string, object> scrapedData)
        {
            // Attempt to extract html content from scraped data
            if (!scrapedData.TryGetValue("htmlContent", out var htmlObj) || htmlObj is not string htmlContent)
            {
                _logger.LogWarning("HTML content missing for animation analysis at URL: {Url}", url);
                return "Unable to analyze animations - HTML content not available.";
            }

            // Attempt to extract inline CSS from scraped data
            if (!scrapedData.TryGetValue("inlineCssList", out var inlineCssObj) || inlineCssObj is not List<string> inlineCss)
            {
                _logger.LogWarning("Inline CSS content missing for animation analysis at URL: {Url}", url);
                inlineCss = new List<string>();
            }

            // Attempt to extract external CSS from scraped data
            if (!scrapedData.TryGetValue("externalCssContents", out var externalCssObj) || externalCssObj is not List<string> externalCss)
            {
                _logger.LogWarning("External CSS content missing for animation analysis at URL: {Url}", url);
                externalCss = new List<string>();
            }

            // Attempt to extract inline JavaScript from scraped data
            if (!scrapedData.TryGetValue("inlineJsList", out var inlineJsObj) || inlineJsObj is not List<string> inlineJs)
            {
                _logger.LogWarning("Inline JS content missing for animation analysis at URL: {Url}", url);
                inlineJs = new List<string>();
            }

            // Attempt to extract external JavaScript from scraped data
            if (!scrapedData.TryGetValue("externalJsContents", out var externalJsObj) || externalJsObj is not List<string> externalJs)
            {
                _logger.LogWarning("External JS content missing for animation analysis at URL: {Url}", url);
                externalJs = new List<string>();
            }

            return await AnalyzeAnimationsAsync(htmlContent, inlineCss, externalCss, inlineJs, externalJs);
        }

        public async Task<string> AnalyzeAnimationsAsync(string htmlContent, List<string> inlineCss, List<string> externalCss, List<string> inlineJs, List<string> externalJs)
        {
            return await Task.Run(() =>
            {
                var findings = new List<string>();
                var animationElements = new List<AnimationElement>();
                
                // Find all CSS keyframe definitions first
                var keyframeNames = FindKeyframeNames(inlineCss, externalCss);
                
                // Find elements with inline animations
                FindInlineAnimatedElements(htmlContent, keyframeNames, animationElements);
                
                // Find JS-triggered animations
                FindJsAnimatedElements(htmlContent, inlineJs, externalJs, animationElements);
                
                if (animationElements.Count == 0)
                {
                    _logger.LogInformation("No animation detected.");
                    return string.Empty;
                }

                // Analyze animation density
                AnalyzeAnimationDensity(animationElements, findings);
                
                // Check total animation count
                if (animationElements.Count > MaxRecommendedAnimations)
                {
                    findings.Add($"Found {animationElements.Count} animations on the page (recommended max is {MaxRecommendedAnimations}). Too many animations can be distracting and affect performance.");
                }

                // Group findings by element for better reporting
                findings.Add("\nAnimated elements found:");
                foreach (var element in animationElements)
                {
                    findings.Add($"- {element.ElementHtml.Trim()} (animation: {element.Type})");
                }

                _logger.LogInformation("Animation behavior detected with {Count} instances.", animationElements.Count);
                return string.Join("\n\n", findings);
            });
        }

        private List<string> FindKeyframeNames(List<string> inlineCss, List<string> externalCss)
        {
            var keyframeNames = new List<string>();
            
            // Regex pattern to match @keyframes rule names (e.g., @keyframes slidein)
            var keyframePattern = @"@keyframes\s+([^\s{]+)";

            // Combine inline and external CSS and search for keyframe patterns
            foreach (var css in inlineCss.Concat(externalCss))
            {
                var matches = Regex.Matches(css, keyframePattern, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    if (match.Success && match.Groups.Count > 1)
                    {
                        // Add the keyframe name (group 1) to the list
                        keyframeNames.Add(match.Groups[1].Value.Trim());
                    }
                }
            }

            return keyframeNames;
        }

        private void FindInlineAnimatedElements(string htmlContent, List<string> keyframeNames, List<AnimationElement> animationElements)
        {
            // Regex to find elements with a 'style' attribute that includes 'animation'
            var animationStylePattern = @"<[^>]+\sstyle\s*=\s*[""'][^""']*animation[^""']*[""'][^>]*>";

            var matches = Regex.Matches(htmlContent, animationStylePattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var elementHtml = match.Value;

                    // Try to extract the specific animation name from the style
                    var animationName = ExtractAnimationName(elementHtml);
                    
                    // If the animation name matches a known keyframe, label it as such
                    if (!string.IsNullOrEmpty(animationName) && keyframeNames.Contains(animationName))
                    {
                        animationElements.Add(new AnimationElement
                        {
                            Type = $"CSS Animation (keyframes: {animationName})",
                            ElementHtml = elementHtml
                        });
                    }
                    else
                    {
                        // Otherwise, still consider it a CSS animation
                        animationElements.Add(new AnimationElement
                        {
                            Type = "CSS Animation",
                            ElementHtml = elementHtml
                        });
                    }
                }
            }
        }

        private string ExtractAnimationName(string elementHtml)
        {
            // Regex to capture the animation name in a CSS animation shorthand property
            var animationNamePattern = @"animation\s*:\s*[^;\}]*\b(\w+)\b";
            var match = Regex.Match(elementHtml, animationNamePattern, RegexOptions.IgnoreCase);

            // Return the captured name if present
            return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;
        }

        private void FindJsAnimatedElements(string htmlContent, List<string> inlineJs, List<string> externalJs, List<AnimationElement> animationElements)
        {
            // Dictionary of JavaScript animation patterns to detect common animation methods
            var jsAnimationPatterns = new Dictionary<string, string>
            {
                { "setTimeout", @"setTimeout\s*\([^)]*document\.(?:getElementById|querySelector)\s*\(['""]([^'""]+)" },
                { "setInterval", @"setInterval\s*\([^)]*document\.(?:getElementById|querySelector)\s*\(['""]([^'""]+)" },
                { "Element.animate()", @"document\.(?:getElementById|querySelector)\s*\(['""]([^'""]+)[^)]*\)\.animate\(" },
                { "requestAnimationFrame", @"requestAnimationFrame\s*\([^)]*document\.(?:getElementById|querySelector)\s*\(['""]([^'""]+)" }
            };

            // Combine inline and external JS and search for patterns
            foreach (var js in inlineJs.Concat(externalJs))
            {
                foreach (var pattern in jsAnimationPatterns)
                {
                    var matches = Regex.Matches(js, pattern.Value, RegexOptions.IgnoreCase);

                    foreach (Match match in matches)
                    {
                        if (match.Success && match.Groups.Count > 1)
                        {
                            var elementId = match.Groups[1].Value;

                            // Attempt to locate the corresponding HTML element using its ID
                            var elementMatch = Regex.Match(htmlContent, $@"<[^>]+\sid\s*=\s*[""']{elementId}[""'][^>]*>", RegexOptions.IgnoreCase);

                            if (elementMatch.Success)
                            {
                                animationElements.Add(new AnimationElement
                                {
                                    Type = pattern.Key,
                                    ElementHtml = elementMatch.Value
                                });
                            }
                            else
                            {
                                // If the element is not found, log a placeholder instead
                                animationElements.Add(new AnimationElement
                                {
                                    Type = pattern.Key,
                                    ElementHtml = $"[Element with id='{elementId}']"
                                });
                            }
                        }
                    }
                }
            }
        }

        private void AnalyzeAnimationDensity(List<AnimationElement> animationElements, List<string> findings)
        {
            // Group animations by parent container to detect localized density
            var containerGroups = animationElements
                .GroupBy(a => GetParentContainer(a.ElementHtml)) 
                .Where(g => g.Count() > MaxRecommendedAnimationsPerContainer);

            foreach (var group in containerGroups)
            {
                // Add a warning if too many animations are in close proximity
                findings.Add($"Found {group.Count()} animations in the same area (recommended max is {MaxRecommendedAnimationsPerContainer}). " +
                    $"Multiple animations in close proximity can be overwhelming.");
            }
        }

        private string GetParentContainer(string elementHtml)
        {
            if (elementHtml.Contains("class=\"section\"")) return "Section Container";
            if (elementHtml.Contains("class=\"container\"")) return "Main Container";
            return "Page Body";
        }

        private class AnimationElement
        {
            public string Type { get; set; }
            public string ElementHtml { get; set; }
        }
    }
}