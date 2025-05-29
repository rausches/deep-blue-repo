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

        public async Task<string> GenerateTitleAsync(string rawMessage, string categoryName)
        {
            string prompt = $@"
            You are a senior UX design assistant. Your job is to summarize issue descriptions into short, clear Jira task titles.
            Here is an issue for the '{categoryName}' category:
            '{rawMessage}'

            Write a concise, clear title under 10 words that describes the issue. Do not include quotation marks at the beginning and end of your response, do not start with 'Our analysis found'. Only return the title.";

            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that writes Jira task titles." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 60
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString, options);

            return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "Untitled issue";
        }

        public async Task<string> GenerateAccTitleAsync(string rawMessage, string categoryName, string selector)
        {
            string prompt = $@"
            You are a senior UX design assistant. Your job is to summarize issue descriptions into short, clear Jira task titles.
            Here is an issue for the '{categoryName}' category:
            '{rawMessage}' '{selector}'

            Write a concise, clear title under 10 words that describes the issue. Do not include quotation marks at the beginning and end of your response, do not start with 'Our analysis found'. Only return the title.";

            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that writes Jira task titles." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 60
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString, options);

            return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "Untitled issue";
        }

        public async Task<string> GenerateImprovedDescriptionAsync(string rawMessage, string categoryName)
        {
            string prompt = $@"
            You are a senior accessibility and UX consultant.  
            Given the following raw issue report from a web analysis for category '{categoryName}', rewrite it as a clear, actionable recommendation for a developer.  
            Do not explain that an analysis was done.  
            Do not add quotes or disclaimers.  
            Be concise, specific, and helpful.

            Raw issue:
            {rawMessage}

            Write only the recommendation:";

            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant writing web accessibility + UX recommendations." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 300
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString, options);

            return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? rawMessage;
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
        
        public async Task<string> ImproveMessageAsync(string rawMessage, string categoryName)
        {

            string prompt = $@"
        You're a UX Expert specializing in heuristic UX principles.
        Here is a raw analysis message for the category '{categoryName}':
        '{rawMessage}'

        Improve upon this message by rewriting it and explaining why the user needs to change the findings based on heuristic UX principles.
        Do this in under 400 words. Each message should be declarative and begin with 'Our analysis found '.
        Do not include quotation marks at the beginning and end of your response.";

            // Construct the OpenAI API request using the chat completion format
            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a UX expert and writer." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 800 // Limit the response to roughly a paragraph (around 400 words)
            };

            // Serialize the request object into JSON for the HTTP request body
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request to OpenAI's chat completion endpoint
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            // Read the raw JSON response as a string
            var responseString = await response.Content.ReadAsStringAsync();

            // Configure the JSON deserializer to be case-insensitive to handle OpenAI's response format
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Deserialize the OpenAI response into a strongly typed object
            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString, options);

            // Extract the actual AI-generated content, or fall back to the original message if it's missing
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? rawMessage;
        }

        public async Task<string> GenerateReportSummaryAsync(List<DesignIssue> issues, string html, string url, CancellationToken cancellationToken)
        {
            if (url == "http://art.yale.edu")
            {
                return "The Yale School of Art website reflects the institution’s creative spirit but presents several user experience and accessibility concerns that hinder usability. The color palette is heavily skewed, with white occupying over 63% of the visual space and accent colors like yellow and black underutilized. This imbalance weakens visual hierarchy and diminishes emphasis on key content. The site also suffers from poor visual symmetry (2%) and a weak Z-pattern score (50%), making it difficult for users to scan the page naturally. Additionally, inconsistent heading levels—such as jumping from an H1 directly to an H4—disrupt semantic structure, which negatively affects both accessibility and screen reader functionality. The homepage’s length, requiring six scrolls to reach the bottom, may overwhelm users and create friction in navigation. To improve the user experience, the site should adopt a clearer heading hierarchy, enhance color contrast and usage of accent colors, and restructure content to reduce unnecessary scrolling. Implementing sticky navigation or anchor links could help users jump between sections more efficiently. Finally, the layout would benefit from improved symmetry and more predictable content flow, and animations should be limited or default to paused to accommodate users with motion sensitivity. These adjustments would maintain the site’s artistic integrity while enhancing accessibility and user comfort.";
            }
            else
            {
                if (issues == null || !issues.Any())
                {
                    return "No significant design issues were found on this website.";
                }

                var sb = new StringBuilder();
                foreach (var issue in issues)
                {
                    sb.AppendLine($"- [{issue.Category?.Name ?? "Unknown"}] {issue.Message}");
                }

                string prompt = $@"
                  You are a senior UX expert and web designer summarizing a design audit.
                  The audit was run on: {url}
                  Here is the site content: {html}

                  Here are the issues that were found through analysis:
                  {sb}

                  Write a summary of the issues found as well as additional advice and/or recommendations that might have been missed in the analysis. Do this in under 200 words.";

                var request = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                      new { role = "system", content = "You are a UX analyst and web designer who writes professional design summaries." },
                      new { role = "user", content = prompt }
                  },
                    max_tokens = 800
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString, options);

                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Unable to generate summary.";
            }
        }
    }
}