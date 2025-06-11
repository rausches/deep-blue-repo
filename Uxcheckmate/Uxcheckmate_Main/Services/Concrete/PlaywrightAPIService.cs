using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class PlaywrightApiService : IPlaywrightApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _playwrightServiceUrl;

        public PlaywrightApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Add this to appsettings.json → "PlaywrightServiceUrl": "http://localhost:5000"
            _playwrightServiceUrl = configuration["PlaywrightServiceUrl"];
        }

        public async Task<PlaywrightAnalysisResult?> AnalyzeWebsiteAsync(string url, bool fullPage = false)
        {
            var requestBody = new
            {
                url = url,
                fullPage = fullPage 
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_playwrightServiceUrl}/analyze", content);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PlaywrightAnalysisResult>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }

    public class PlaywrightAnalysisResult
    {
        public string? Url { get; set; }
        public string? ScreenshotBase64 { get; set; }
        public string? Html { get; set; }
        public string? TextContent { get; set; }

        public List<string> Fonts { get; set; } = new();

        public AxeResults? AxeResults { get; set; }

        public bool HasFavicon { get; set; }
        public string? FaviconUrl { get; set; }

        public List<string> ExternalCssContents { get; set; } = new();
        public List<string> ExternalJsContents { get; set; } = new();
        public List<string> InlineCssList { get; set; } = new();
        public List<string> InlineJsList { get; set; } = new();
        public List<string> ExternalCssLinks { get; set; } = new();
        public List<string> ExternalJsLinks { get; set; } = new();
        public List<string> Links { get; set; } = new();

        public int ScrollHeight { get; set; }
        public int ScrollWidth { get; set; }
        public int ViewportHeight { get; set; }
        public int ViewportWidth { get; set; }
        public string? ViewportLabel { get; set; }
        public List<HtmlElement> LayoutElements { get; set; } = new();

}


    public class AxeResults
    {
        public List<AxeViolation>? Violations { get; set; }
    }

    public class AxeViolation
    {
        public string? Id { get; set; }
        public string? Impact { get; set; }
        public string? Help { get; set; }
        public string? Description { get; set; }
        public List<AxeNode>? Nodes { get; set; }
    }

    public class AxeNode
    {
        public List<string>? Target { get; set; }
        public string? Html { get; set; }
        public string? FailureSummary { get; set; }
    }
}
