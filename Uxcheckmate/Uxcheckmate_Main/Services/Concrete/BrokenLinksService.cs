using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class BrokenLinksService : IBrokenLinksService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<BrokenLinksService> _logger; 
        private readonly UxCheckmateDbContext _dbContext;
    
    public BrokenLinksService(HttpClient httpClient, ILogger<BrokenLinksService> logger, UxCheckmateDbContext dbContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> BrokenLinkAnalysis(string url, Dictionary<string, object> scrapedData)
        {
            // Change relative links to absolute urls
            var links = scrapedData.ContainsKey("links") ? scrapedData["links"] as List<string> : new List<string>();

            // Convert relative links to absolute URLs
            links = links.Select(link => MakeAbsoluteUrl(url, link)).ToList();
            // Check for broken links
            var brokenLinks = await CheckBrokenLinksAsync(links);
            // Get http status codes for all links

            // Any 404 status codes add links to string message 
        }

        private string MakeAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
                Uri.TryCreate(baseUri, relativeUrl, out Uri absoluteUri))
            {
                return absoluteUri.ToString();
            }
            return relativeUrl; // Return as is if conversion fails
        }

        private async Task<string> CheckBrokenLinksAsync (List<string>? links)
        {
            var brokenLinks = new List<string>();

            foreach (var link in links)
            {
                var response = await _httpClient.GetAsync(link);
                if (!response.IsSuccessStatusCode)
                {
                    brokenLinks.Add($"{link} (Status: {response.StatusCode})");
                }     
            }
            return brokenLinks;
        }
    }
}