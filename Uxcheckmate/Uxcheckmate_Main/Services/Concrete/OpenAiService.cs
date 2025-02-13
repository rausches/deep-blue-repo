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

        public async Task<UxResult> AnalyzeUx(string url)
        {
            WebScraperService scraper = new WebScraperService(_httpClient);

            // Scrape the webpage content asynchronously
            Dictionary<string, object> scrapedData = await scraper.ScrapeAsync(url);

            // Convert scraped data into a readable format for AI analysis
            string pageContent = FormatScrapedData(scrapedData);

            var prompt = @$"Analyze the UX of the following webpage in structured sections:

            ### Fonts
            - How many unique fonts are used? Is this too many?
            - Is the typography consistent?

            ### Text Structure
            - Are there large blocks of text that need better separation?
            - Are headings used correctly for readability?

            ### Usability Issues
            - Are there too many links or images?
            - Are buttons, navigation, and layout elements easy to use?

            If no issues are found in a category, return 'No significant issues found' under that section.

            Webpage Data:
            {pageContent}";

            // Payload Request
            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a UX expert analyzing websites for accessibility, usability, and design flaws." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 500
            };

            // Convert the request object into a JSON payload
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Send the request to OpenAI's chat completion API endpoint
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

            // Read the API response content as a string
            var responseString = await response.Content.ReadAsStringAsync();

            // Extract AI response into structured sections
            var sections = ExtractSections(responseString);

            // Convert AI response into `UxResult` format
            UxResult uxResult = ConvertToUxResult(sections);

            return uxResult;
        }

        private UxResult ConvertToUxResult(Dictionary<string, string> sections)
        {
            var uxResult = new UxResult();

            foreach (var section in sections)
            {
                // Create a new UxIssue for each AI-generated category
                var issue = new UxIssue
                {
                    Category = section.Key, 
                    Message = section.Value, 
                    Selector = "" // 
                };

                uxResult.Issues.Add(issue);
            }

            return uxResult;
        }

        // Helper method to format the extracted webpage data into a readable text string
        private string FormatScrapedData(Dictionary<string, object> scrapedData)
        {
            var sb = new StringBuilder();

            sb.AppendLine("### UX Data Extracted from Web Page ###");

            // Add extracted numeric data (headings, images, and links count)
            sb.AppendLine($"- Headings Count: {scrapedData["headings"]}");
            sb.AppendLine($"- Images Count: {scrapedData["images"]}");
            sb.AppendLine($"- Links Count: {scrapedData["links"]}");

            // Extract and format font usage data
            var fonts = (List<string>)scrapedData["fonts"];
            sb.AppendLine($"- Fonts Used: {string.Join(", ", fonts)}"); // List all detected fonts
            sb.AppendLine($"- Total Unique Fonts: {fonts.Count}"); // Show the number of unique fonts used

            // Extract and format webpage text content
            string textContent = (string)scrapedData["text_content"];
            sb.AppendLine("\n### Extracted Page Text ###");

            // Limit displayed text content to 500 characters to avoid excessive output
            sb.AppendLine(textContent.Length > 500 ? textContent.Substring(0, 500) + "..." : textContent);

            // Return the formatted UX data string
            return sb.ToString();
        }
    }
}