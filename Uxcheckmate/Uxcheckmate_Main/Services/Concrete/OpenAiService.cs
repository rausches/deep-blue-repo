using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public partial class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<OpenAiService> _logger; 

        public OpenAiService(HttpClient httpClient, ILogger<OpenAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> AnalyzeWithOpenAI(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData)
        {
            _logger.LogInformation("Starting OpenAI analysis for URL: {Url} in category: {CategoryName}", url, categoryName);

            // Format the scraped data into a single string.
            string pageContent = FormatScrapedData(scrapedData);
            _logger.LogDebug("Formatted scraped data for URL: {Url}. Content length: {Length}", url, pageContent.Length);

            // Construct the prompt for OpenAI.
            string prompt = $@"
            Analyze the webpage {url} for UX issues related to {categoryName}.
            Category Description: {categoryDescription}.
            If no issues are found, respond with 'No significant issues found.'
            
            Webpage Data:
            {pageContent}";

            _logger.LogDebug("Constructed prompt for OpenAI: {Prompt}", prompt);

            // Create the request payload.
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

            // Serialize the request object to JSON.
            var requestJson = JsonSerializer.Serialize(request);
            _logger.LogDebug("Serialized OpenAI request: {RequestJson}", requestJson);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Send the request to OpenAI's API.
            _logger.LogInformation("Sending request to OpenAI API for URL: {Url}", url);
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            _logger.LogInformation("Received response from OpenAI API with status code: {StatusCode}", response.StatusCode);

            // Read and log the response content.
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response string from OpenAI API: {ResponseString}", responseString);

            // Deserialize the response.
            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
            
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
            _logger.LogDebug("Formatting scraped data for display in prompt.");
            StringBuilder sb = new StringBuilder();
            foreach (var entry in scrapedData)
            {
                sb.AppendLine($"### {entry.Key}");
                sb.AppendLine(entry.Value.ToString());
            }
            string formattedData = sb.ToString();
            _logger.LogDebug("Formatted scraped data length: {Length}", formattedData.Length);
            return formattedData;
        }
    }
}
