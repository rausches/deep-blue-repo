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

            List<string> links = scrapedData.ContainsKey("links") && scrapedData["links"] != null 
                                    ? scrapedData["links"] as List<string> 
                                    : new List<string>();

            _logger.LogDebug("Found {LinkCount} links in scraped data.", links.Count);

            // Convert to absolute URLs and remove duplicates
            links = links
                .Select(link => MakeAbsoluteUrl(url, link))
                .Distinct()
                .ToList();

            var sw = Stopwatch.StartNew();
            var brokenLinks = await CheckBrokenLinksAsync(links);
            sw.Stop();
            _logger.LogInformation("Broken link analysis took {Time}ms", sw.ElapsedMilliseconds);

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

            _logger.LogWarning("Failed to convert relative URL '{RelativeUrl}' using base URL '{BaseUrl}'", relativeUrl, baseUrl);
            return relativeUrl;
        }

        public async Task<List<string>> CheckBrokenLinksAsync(List<string> links)
        {
            var brokenLinks = new ConcurrentBag<string>();

            await Parallel.ForEachAsync(links, new ParallelOptions { MaxDegreeOfParallelism = 20 }, async (link, ct) =>
            {
                if (!Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    _logger.LogDebug("Skipping unsupported URL: {Link}", link);
                    return;
                }

                try
                {
                    HttpResponseMessage response;

                    // Try HEAD first for performance
                    var headRequest = new HttpRequestMessage(HttpMethod.Head, link);
                    response = await _httpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, ct);

                    if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        _logger.LogDebug("HEAD not allowed for {Link}, falling back to GET", link);
                        response = await _httpClient.GetAsync(link, ct);
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        brokenLinks.Add($"{link} (Status: {response.StatusCode})");
                        _logger.LogInformation("Link {Link} returned status {StatusCode}. Marking as broken.", link, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    brokenLinks.Add($"{link} (Exception: {ex.Message})");
                    _logger.LogInformation(ex, "Exception occurred while checking link: {Link}", link);
                }
            });

            return brokenLinks.ToList();
        }
    }
}
