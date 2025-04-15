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

        public AnimationService(ILogger<AnimationService> logger)
        {
            _logger = logger;
        }

        // Entry point method runs the animation analysis using the scraped content
        public async Task<string> RunAnimationAnalysisAsync(string url, Dictionary<string, object> scrapedData)
        {
            // Attempt to extract inline CSS from scraped data
            if (!scrapedData.TryGetValue("inlineCss", out var inlineCssObj) || inlineCssObj is not List<string> inlineCss)
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
            if (!scrapedData.TryGetValue("inlineJs", out var inlineJsObj) || inlineJsObj is not List<string> inlineJs)
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

            // Run actual analysis logic
            return await AnalyzeAnimationsAsync(inlineCss, externalCss, inlineJs, externalJs);
        }

        public async Task<string> AnalyzeAnimationsAsync(List<string> inlineCss, List<string> externalCss, List<string> inlineJs, List<string> externalJs)
        {
            return await Task.Run(() =>
            {
                // counter for detected animations
                int animationScore = 0;
                var findings = new List<string>();

                // Patterns to detect CSS animations
                var cssAnimationPatterns = new[]
                {
                    // general CSS animation property
                    @"animation\s*:",     
                    // custom keyframe definitions
                    @"@keyframes\s+",
                    // CSS transition property
                    @"transition\s*:"
                };

                // Loop through all CSS (inline + external) and match patterns
                foreach (var css in inlineCss.Concat(externalCss))
                {
                    foreach (var pattern in cssAnimationPatterns)
                    {
                        if (Regex.IsMatch(css, pattern, RegexOptions.IgnoreCase))
                        {
                            findings.Add($"Found CSS animation: `{pattern}`");
                            animationScore++;
                        }
                    }
                }

                // Keywords commonly associated with JavaScript-based animations
                var jsAnimationKeywords = new[]
                {
                    "requestAnimationFrame",
                    ".animate(",
                    "setInterval",
                    "setTimeout",
                    "velocity.js",
                    "gsap",
                    "anime(",
                    "$().fadeIn",
                    "$().slideDown"
                };

                // Loop through all JS (inline + external) and check for keywords
                foreach (var js in inlineJs.Concat(externalJs))
                {
                    foreach (var keyword in jsAnimationKeywords)
                    {
                        if (js.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            findings.Add($"Found JavaScript animation: `{keyword}`");
                            animationScore++;
                        }
                    }
                }

                // If no animations found, return early
                if (animationScore == 0)
                {
                    _logger.LogInformation("No animation detected.");
                    return string.Empty;
                }

                // If Animations found, return string
                _logger.LogInformation("Animation behavior detected with {Score} instances.", animationScore);

                return $"Detected {animationScore} animation-related behaviors. Consider whether these animations enhance or hinder the user experience.\n\n {string.Join("\n- ", findings)}";
            });
        }
    }
}
