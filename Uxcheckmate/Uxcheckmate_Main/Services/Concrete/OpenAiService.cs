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

        public async Task<List<DesignIssue>> AnalyzeAndSaveDesignIssues(string url)
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

            // Send the request to OpenAI's API.
            _logger.LogInformation("Sending request to OpenAI API for URL: {Url}", url);
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            _logger.LogInformation("Received response from OpenAI API with status code: {StatusCode}", response.StatusCode);

            // Read and log the response content.
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response string from OpenAI API: {ResponseString}", responseString);

            // Configure the serializer to be case-insensitive.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the response.
            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseString, options);

            // Extract the AI-generated text or fallback if empty.
            string aiText = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response";
            _logger.LogInformation("Extracted AI response: {AiText}", aiText);

            // Return an empty string if no issues are found.
            if (aiText.Contains("No significant issues found"))
            {
                _logger.LogInformation("OpenAI analysis indicates no significant issues for URL: {Url}", url);
                return "";
            }
            else
            {
                _logger.LogInformation("OpenAI analysis detected issues for URL: {Url}", url);
                return aiText;
            }
        }

        private string FormatScrapedData(Dictionary<string, object> scrapedData)
        {
            StringBuilder sb = new StringBuilder();

            // Format headings count if available.
            if (scrapedData.TryGetValue("headings", out var headings))
            {
                sb.AppendLine($"Headings Count: {headings}");
            }

            // Format images count if available.
            if (scrapedData.TryGetValue("images", out var images))
            {
                sb.AppendLine($"Images Count: {images}");
            }

            // Format links count if available.
            if (scrapedData.TryGetValue("links", out var links))
            {
                sb.AppendLine($"Links Count: {links}");
            }

            // Format fonts list if available.
            if (scrapedData.TryGetValue("fonts", out var fontsObj) && fontsObj is IEnumerable<string> fonts)
            {
                string fontsList = string.Join(", ", fonts);
                sb.AppendLine($"Fonts Used: {fontsList}");
            }

            // Append text content if available.
            if (scrapedData.TryGetValue("text_content", out var textContent))
            {
                sb.AppendLine(textContent.ToString());
            }

            string formattedData = sb.ToString();
            return formattedData;
        }

    }
}
