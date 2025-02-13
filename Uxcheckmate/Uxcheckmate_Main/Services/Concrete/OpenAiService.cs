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

        public async Task<Dictionary<string, string>> AnalyzeUx(string url)
        {
            // Initialize the WebScraperService to extract content from the given URL
            WebScraperService scraper = new WebScraperService(_httpClient);

            // Scrape the webpage content asynchronously
            Dictionary<string, object> scrapedData = await scraper.ScrapeAsync(url);

            // Convert the dictionary into a readable text format for AI analysis
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

            // Convert the request object into a JSON payload with appropriate encoding
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Send the request to OpenAI's chat completion API endpoint
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

            // Read the API response content as a string
            var responseString = await response.Content.ReadAsStringAsync();

            // Split AI response into sections
            var sections = ExtractSections(responseString);

            return sections;
        }

        // helper method to format dictionary data into a readable string
        private string FormatScrapedData(Dictionary<string, object> scrapedData)
        {
            var sections = new Dictionary<string, string>();
            var regex = new System.Text.RegularExpressions.Regex(@"### (.*?)\n(.*?)(?=###|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
            
            var matches = regex.Matches(aiResponse);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string sectionTitle = match.Groups[1].Value.Trim();
                string sectionContent = match.Groups[2].Value.Trim();
                sections[sectionTitle] = sectionContent;
            }

            return sections;
        }
    }
}