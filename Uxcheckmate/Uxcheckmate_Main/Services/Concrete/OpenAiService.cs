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

        // Sends a message to the OpenAI API and retrieves the AI-generated response.
        // param: The user's input message to be sent to OpenAI.
        // returns: A string containing the AI-generated response from OpenAI.
        public async Task<string> GetChatResponse(string message)
        {
            // Create the request payload for OpenAI API
            var request = new
            {
                model = "gpt-4", // Specify the AI model to use (ensure you have access to GPT-4)
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." }, // System role defines assistant behavior
                    new { role = "user", content = message } // User's message input
                },
                max_tokens = 50 // Limit the response length to 50 tokens
            };

            // Serialize the request payload to JSON format
            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Send an HTTP POST request to OpenAI's API endpoint
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

            // Read the API response content as a string
            var responseString = await response.Content.ReadAsStringAsync();

            // Return the AI-generated response
            return responseString;
        }
    

        public async Task<string> AnalyzeUx(string url)
        {
            // Scrape the webpage
            WebScraperService scraper = new WebScraperService(_httpClient);
            string pageContent = await scraper.ScrapeAsync(url);

            var prompt = $"Analyze the UX of the following webpage and provide recommendations for improvements based on accessibility, usability, and best design practices:\n\n{pageContent}";

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

            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
    }
}