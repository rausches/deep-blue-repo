using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class AudioService : IAudioService
    {
        private readonly ILogger<AudioService> _logger;

        public AudioService(ILogger<AudioService> logger)
        {
            _logger = logger;
        }

        // Main entry point that receives the scraped data dictionary
        public async Task<string> RunAudioAnalysisAsync(string url, Dictionary<string, object> scrapedData)
        {
            // Try to extract the full HTML content from the scraped data
            if (!scrapedData.TryGetValue("htmlContent", out var htmlObj) || htmlObj is not string htmlContent)
            {
                _logger.LogWarning("HTML content missing for audio analysis at URL: {Url}", url);
                return string.Empty;
            }

            // Try to extract external JS content, fallback to empty list
            if (!scrapedData.TryGetValue("externalJsContents", out var jsObj) || jsObj is not List<string> externalJsContents)
            {
                _logger.LogWarning("External JS contents missing for audio analysis at URL: {Url}", url);
                externalJsContents = new List<string>();
            }

            return await AnalyzeAudioAsync(htmlContent, externalJsContents);
        }

        // Scans HTML and JS for audio autoplay behavior or embedded sounds
        public async Task<string> AnalyzeAudioAsync(string htmlContent, List<string> externalJsContents)
        {
            int audioScore = 0;
            var findings = new List<string>();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Search for <audio> and <video> tags with autoplay
            var audioNodes = htmlDoc.DocumentNode
                .Descendants()
                .Where(n =>
                    (n.Name == "audio" || n.Name == "video") &&
                    n.GetAttributeValue("autoplay", null) != null)
                .ToList();

            if (audioNodes.Any())
            {
                audioScore += audioNodes.Count;
                foreach (var node in audioNodes)
                {
                    var elementHtml = node.OuterHtml.Trim();
                    findings.Add($"Autoplay Element:\n{elementHtml}");
                }
            }

            // Search JS files for common autoplay patterns
            var jsAudioKeywords = new[]
            {
                ".play(",
                "Audio(",
                "autoplay",
                "new Audio",
                "soundManager",
                "Howl(",
                "Tone.js"
            };

            foreach (var js in externalJsContents)
            {
                foreach (var keyword in jsAudioKeywords)
                {
                    if (js.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        findings.Add($"JavaScript audio trigger found: `{keyword}`");
                        audioScore++;
                    }
                }
            }

            if (audioScore == 0)
            {
                _logger.LogInformation("No audio autoplay behavior detected.");
                return string.Empty;
            }

            _logger.LogInformation("Audio autoplay behavior detected ({Score} instances).", audioScore);

            return $"Found {audioScore} audio-related behaviors.\n\n{string.Join("\n\n", findings)}. Consider disabling autoplay or prompting users before playing audio.";
        }
    }
}
