using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class PopUpsService : IPopUpsService
    {
        private readonly ILogger<PopUpsService> _logger;

        public PopUpsService(ILogger<PopUpsService> logger)
        {
            _logger = logger;
        }

        // Entry point method that takes scraped data and delegates to popup analysis logic
        public async Task<string> RunPopupAnalysisAsync(string url, Dictionary<string, object> scrapedData)
        {
            // Attempt to extract the HTML content from the scraped data dictionary
            if (!scrapedData.TryGetValue("htmlContent", out var htmlObj) || htmlObj is not string htmlContent)
            {
                _logger.LogWarning("HTML content missing for popup analysis at URL: {Url}", url);
                return string.Empty;
            }

            // Attempt to extract the list of external JS contents; if not found, initialize an empty list
            if (!scrapedData.TryGetValue("externalJsContents", out var jsObj) || jsObj is not List<string> externalJsContents)
            {
                _logger.LogWarning("External JS contents missing for popup analysis at URL: {Url}", url);
                externalJsContents = new List<string>();
            }

            // Delegate the analysis to a separate method
            return await AnalyzePopupsAsync(htmlContent, externalJsContents);
        }

        // Check for visual popups in both HTML and JavaScript
        public async Task<string> AnalyzePopupsAsync(string htmlContent, List<string> externalJsContents)
        {
            // Define keywords associated with popups, modals
            var popupKeywords = new[] { "popup", "modal", "overlay", "dialog", "lightbox" };
            int popupCount = 0;

            // Static HTML scan using HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Search for elements with class, id, role, or aria attributes that suggest popup behavior
            var popupNodes = htmlDoc.DocumentNode
                .Descendants() 
                .Where(n =>
                    // Combine class and id attribute values, convert to lowercase, and split by spaces
                    (n.GetAttributeValue("class", "") + " " + n.GetAttributeValue("id", ""))
                        .ToLowerInvariant().Split(' ')
                        // Check if any individual class/id string contains a popup keyword 
                        .Any(cls => popupKeywords.Any(k => cls.Contains(k))) ||
                    // Check if the node explicitly declares a dialog role
                    n.GetAttributeValue("role", "").ToLower() == "dialog" ||
                    // Or if the node has an aria-modal attribute set to true 
                    n.GetAttributeValue("aria-modal", "").ToLower() == "true"
                )
                .ToList(); 

            // Tally the number of popup-like elements found in static HTML
            popupCount += popupNodes.Count;
            _logger.LogInformation("Static popup count: {Count}", popupNodes.Count);

            // Heuristic scan of external JavaScript content
            foreach (var js in externalJsContents)
            {
                // Check if Keywords exist in JS
                if (popupKeywords.Any(k => js.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    popupCount++;
                    _logger.LogInformation("Popup-related keyword found in external JS.");
                }
            }

            // Return message if more than one popup is found
            if (popupCount > 1)
            {
                return $"Found {popupNodes.Count()} pop ups. Consider limiting your site to one or none to improve user experience.";
            }

            // If only one or no popups are found, return an empty string
            return string.Empty;
        }
    }
}
