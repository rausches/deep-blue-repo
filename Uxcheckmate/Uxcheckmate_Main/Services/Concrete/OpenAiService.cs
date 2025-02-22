using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public partial class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<OpenAiService> _logger; 
        private readonly UxCheckmateDbContext _dbContext;

        public OpenAiService(HttpClient httpClient, ILogger<OpenAiService> logger, UxCheckmateDbContext dbContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;
        }
 /*       public async Task<List<DesignIssue>> AnalyzeWebsite(string url)
        {
            WebScraperService scraper = new WebScraperService(_httpClient);

            // Scrape the webpage content asynchronously
            Dictionary<string, object> scrapedData = await scraper.ScrapeAsync(url);
            string pageContent = FormatScrapedData(scrapedData);

            // Retrieve all UX Categories=
            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            var scanResults = new List<DesignIssue>();

            // Create a new report entry
            var report = new Report
            {
                Url = url,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync(); // Save to generate ReportId

            foreach (var category in designCategories.Take(2))
            {
                string message = "";

                // Determine scan method per category
                if (category.ScanMethod == "OpenAI")
                {
                    message = await AnalyzeWithOpenAI(url, category.Name, category.Description, pageContent);
                }
                else if (category.ScanMethod == "Custom")
                {
                    message = await RunCustomAnalysisAsync(url, category.Name, scrapedData);
                }
                else
                {
                    // Default to OpenAI
                    message = await AnalyzeWithOpenAI(url, category.Name, category.Description, pageContent);
                }

                // If an issue is detected, create a new DesignIssue entry
                if (!string.IsNullOrEmpty(message))
                {
                    var designIssue = new DesignIssue
                    {
                        CategoryId = category.Id,
                        ReportId = report.Id,
                        Message = message,
                        Severity = DetermineSeverity(message)
                    };

                    // Save the detected issue to the database
                    _dbContext.DesignIssues.Add(designIssue);
                    scanResults.Add(designIssue);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Return all design issues for this scan
            return scanResults;
        }*/


        public async Task<string> AnalyzeWithOpenAI(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData)
        {
            string pageContent = FormatScrapedData(scrapedData);
            string prompt = $@"
            Analyze the webpage {url} for UX issues related to {categoryName}.
            Category Description: {categoryDescription}.
            If no issues are found, respond with 'No significant issues found.'
            
            Webpage Data:
            {pageContent}";

            var request = new
            {
                model = "gpt-4", 
                messages = new[]
                {
                    new { role = "system", content = "You are a UX expert analyzing websites." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 100 // Limit response length
            };

            // Serialize request data to JSON format
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Send the request to OpenAI's API
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            // Deserialize the response from OpenAI
            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
            
            // Extract the AI-generated content or return "No response" if empty
            string aiText = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response";

            // If OpenAI finds no issues, return an empty string; otherwise, return the result
            return aiText.Contains("No significant issues found") ? "" : aiText;
        }

    /*    private async Task<string> RunCustomAnalysisAsync(string url, string categoryName, Dictionary<string, object> scrapedData)
        {
            if (categoryName == "Broken Links")
            {
                var links = new List<string>();

                if (scrapedData.ContainsKey("links") && scrapedData["links"] != null)
                {
                    links = scrapedData["links"] as List<string> ?? new List<string>();
                }

                if (links.Any())
                {
                    links = links.Select(link =>
                        Uri.TryCreate(new Uri(url), link, out Uri absoluteUri) ? absoluteUri.ToString() : link
                    ).ToList();
                }

                var brokenLinks = new List<string>();

                foreach (var link in links)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(link);
                        if (!response.IsSuccessStatusCode)
                        {
                            brokenLinks.Add($"{link} (Status: {response.StatusCode})");
                        }
                    }
                    catch
                    {
                        brokenLinks.Add($"{link} (Failed to connect)");
                    }
                }

                return brokenLinks.ToString();
            }

            return ""; // No issues found
        }*/

  /*      public async Task<List<DesignIssue>> AnalyzeAndSaveDesignIssues(string url)
        {
            WebScraperService scraper = new WebScraperService(_httpClient);

            // Scrape the webpage content asynchronously
            Dictionary<string, object> scrapedData = await scraper.ScrapeAsync(url);
            string pageContent = FormatScrapedData(scrapedData);

            // Get all Design Categories from the database
            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            var designIssues = new List<DesignIssue>();

            // Create a new report entry
            var report = new Report
            {
                Url = url,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync(); // Save to generate ReportId

            foreach (var category in designCategories.Take(2))
            {
                string prompt = $@"
                Analyze the UX of the following webpage and identify any issues related to {category.Name}. 
                If no issues are found, respond with 'No significant issues found.'.
                {category.Description}

                Webpage Data:
                {pageContent}";

                var request = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a UX expert analyzing websites for accessibility, usability, and design flaws." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 100
                };

                var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
                var responseString = await response.Content.ReadAsStringAsync();

                // Deserialize the response
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
                string aiText = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response";

                if (!aiText.Contains("No significant issues found", StringComparison.OrdinalIgnoreCase))
                {
                    // If an issue is found, save it
                    var designIssue = new DesignIssue
                    {
                        CategoryId = category.Id,
                        ReportId = report.Id,
                        Message = aiText,
                        Severity = DetermineSeverity(aiText)
                    };

                    _dbContext.DesignIssues.Add(designIssue);
                    designIssues.Add(designIssue);
                }
            }

            await _dbContext.SaveChangesAsync();
            return designIssues;
        }*/

        private int DetermineSeverity(string aiText)
        {
            if (aiText.Contains("critical", StringComparison.OrdinalIgnoreCase)) return 3; // High Severity
            if (aiText.Contains("should", StringComparison.OrdinalIgnoreCase)) return 2; // Medium Severity
            return 1; // Low Severity
        }

        private string FormatScrapedData(Dictionary<string, object> scrapedData)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in scrapedData)
            {
                sb.AppendLine($"### {entry.Key}");
                sb.AppendLine(entry.Value.ToString());
            }
            return sb.ToString();
        }
    }
}
