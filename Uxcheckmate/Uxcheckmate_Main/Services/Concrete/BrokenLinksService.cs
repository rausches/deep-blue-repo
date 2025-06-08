using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
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

            // Set a short timeout to prevent hanging
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
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
            links = links
                .Select(link => MakeAbsoluteUrl(url, link))
                .Distinct()
                .ToList();

            // Check for broken links.
            var brokenLinks = await CheckBrokenLinksAsync(links);

            if (brokenLinks.Any())
            {
                var message = $"Found {brokenLinks.Count()} broken links. Links: {string.Join(", ", brokenLinks)}";
                _logger.LogInformation("Broken link analysis completed. {BrokenLinkCount} broken links found.", brokenLinks.Count);
                return message;
            }
            else
            {
                _logger.LogInformation("Broken link analysis completed. No issues found.");
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
            // Log a warning if conversion fails
            _logger.LogWarning("Failed to convert relative URL '{RelativeUrl}' using base URL '{BaseUrl}'", relativeUrl, baseUrl);
            return relativeUrl; // Return as is if conversion fails.
        }

        public async Task<List<string>> CheckBrokenLinksAsync(List<string> links)
        {
            var brokenLinks = new ConcurrentBag<string>();
            var cancelledLinks = new ConcurrentBag<string>();

            // Process links concurrently with a limit of 20 parallel tasks
            await Parallel.ForEachAsync(links, new ParallelOptions { MaxDegreeOfParallelism = 20 }, async (link, ct) =>
            {
                // Skip links that are not valid HTTP/HTTPS URLs
                if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    _logger.LogDebug("Skipping unsupported URL: {Link}", link);
                    return;
                }

                try
                {
                    HttpResponseMessage response;

                    // Attempt a HEAD request first 
                    var headRequest = new HttpRequestMessage(HttpMethod.Head, link);
                    response = await _httpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, ct);

                    // Fallback
                    if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        _logger.LogDebug("HEAD not allowed for {Link}, falling back to GET", link);
                        response = await _httpClient.GetAsync(link, ct);
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogInformation(
                            "Link {Link} returned status {StatusCode}. Marking as broken.",
                            link, response.StatusCode
                        );
                        brokenLinks.Add($"{link} (Status: {response.StatusCode})");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Link check for {Link} was cancelled.", link);
                    cancelledLinks.Add(link);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Exception occurred while checking link: {Link}", link);
                    brokenLinks.Add($"{link} (Exception: {ex.Message})");
                }
            });

            _logger.LogInformation("Skipped {Count} links due to cancellation.", cancelledLinks.Count);
            return brokenLinks.ToList();
        }
    }
}
