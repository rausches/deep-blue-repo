using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class BrokenLinksService : IBrokenLinksService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<BrokenLinksService> _logger; 

        public BrokenLinksService(HttpClient httpClient, ILogger<BrokenLinksService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> BrokenLinkAnalysis(string url, Dictionary<string, object> scrapedData)
        {
            _logger.LogInformation("Starting broken link analysis for URL: {Url}", url);

            // Retrieve the "links" from the scraped data; if not available, use an empty list.
            List<string> links = scrapedData.ContainsKey("links") && scrapedData["links"] != null 
                                    ? scrapedData["links"] as List<string> 
                                    : new List<string>();

            _logger.LogDebug("Found {LinkCount} links in scraped data.", links.Count);

            // Convert relative URLs to absolute URLs based on the provided base URL.
            links = links.Select(link =>
            {
                var absoluteUrl = MakeAbsoluteUrl(url, link);
                _logger.LogDebug("Converted link '{Link}' to absolute URL '{AbsoluteUrl}'", link, absoluteUrl);
                return absoluteUrl;
            }).ToList();

            // Check for broken links.
            var brokenLinks = await CheckBrokenLinksAsync(links);

            if (brokenLinks.Any())
            {
                var message = $"The following broken links were found on this page: {string.Join(", ", brokenLinks)}";
                _logger.LogInformation("Broken link analysis completed. {BrokenLinkCount} broken links found.", brokenLinks.Count);
                return message;
            }
            else
            {
                _logger.LogInformation("Broken link analysis completed. No broken links found.");
                return string.Empty;
            }
        }

        private string MakeAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
                Uri.TryCreate(baseUri, relativeUrl, out Uri absoluteUri))
            {
                return absoluteUri.ToString();
            }
            // Log a warning if conversion fails.
            _logger.LogWarning("Failed to convert relative URL '{RelativeUrl}' using base URL '{BaseUrl}'", relativeUrl, baseUrl);
            return relativeUrl; // Return as is if conversion fails.
        }

        private async Task<List<string>> CheckBrokenLinksAsync(List<string> links)
        {
            var brokenLinks = new List<string>();

            foreach (var link in links)
            {
                // Skip URLs with unsupported schemes ("mailto").
                if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    _logger.LogDebug("Skipping unsupported URL: {Link}", link);
                    continue;
                }

                try
                {
                    _logger.LogDebug("Checking URL: {Link}", link);
                    var response = await _httpClient.GetAsync(link);

                    // If the response is not successful, record the broken link.
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Broken link detected: {Link} returned status {StatusCode}", link, response.StatusCode);
                        brokenLinks.Add($"{link} (Status: {response.StatusCode})");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception and mark the link as broken.
                    _logger.LogError(ex, "Exception occurred while checking link: {Link}", link);
                    brokenLinks.Add($"{link} (Exception: {ex.Message})");
                }
            }

            return brokenLinks;
        }
    }
}
