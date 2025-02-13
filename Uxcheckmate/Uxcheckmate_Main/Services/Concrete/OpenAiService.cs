using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<OpenAiService> _logger; 

        public OpenAiService(HttpClient httpClient, ILogger<OpenAiService> logger)
        {
            _httpClient = httpClient; 
            _logger = logger; 
        }

        public async Task<string> AnalyzeUx(string url)
        {
            // Initialize the WebScraperService to extract content from the given URL
            WebScraperService scraper = new WebScraperService(_httpClient);

            // Scrape the webpage content asynchronously
            Dictionary<string, object> scrapedData = await scraper.ScrapeAsync(url);

            // Convert the dictionary into a readable text format for AI analysis
            string pageContent = FormatScrapedData(scrapedData);

            // Create the prompt for AI analysis
            var prompt = $"Analyze the UX of the following webpage and provide recommendations for improvements based on accessibility, usability, and best design practices:\n\n{pageContent}";

            // Payload Request
            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a UX expert analyzing websites for accessibility, usability, and design flaws." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 200
            };

            // Convert the request object into a JSON payload with appropriate encoding
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Send the request to OpenAI's chat completion API endpoint
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

            // Read the API response content as a string
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        // helper method to format dictionary data into a readable string
        private string FormatScrapedData(Dictionary<string, object> scrapedData)
        {
            var sb = new StringBuilder();

            sb.AppendLine("### UX Data Extracted from Web Page ###");
            sb.AppendLine($"- Headings Count: {scrapedData["headings"]}");
            sb.AppendLine($"- Images Count: {scrapedData["images"]}");
            sb.AppendLine($"- Links Count: {scrapedData["links"]}");

            return sb.ToString();
        }
    }
}