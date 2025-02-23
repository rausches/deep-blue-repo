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
        private readonly UxCheckmateDbContext _dbContext;
    
    public BrokenLinksService(HttpClient httpClient, ILogger<BrokenLinksService> logger, UxCheckmateDbContext dbContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> BrokenLinkAnalysis(string url, Dictionary<string, object> scrapedData)
        {
            // Ensure that links is not null
            List<string> links = scrapedData.ContainsKey("links") && scrapedData["links"] != null 
                                    ? scrapedData["links"] as List<string> 
                                    : new List<string>();

            // Now links is guaranteed not to be null.
            links = links.Select(link => MakeAbsoluteUrl(url, link)).ToList();

            var brokenLinks = await CheckBrokenLinksAsync(links);
            if (brokenLinks.Any())
            {
                return $"The following broken links were found on this page: {string.Join(", ", brokenLinks)}";
            }
            else
            {
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
            return relativeUrl; // Return as is if conversion fails
        }

        private async Task<List<string>> CheckBrokenLinksAsync (List<string>? links)
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