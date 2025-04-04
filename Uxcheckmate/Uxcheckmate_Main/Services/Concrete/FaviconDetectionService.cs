using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;


// MIGHT NOT NEED 
namespace Uxcheckmate_Main.Services
{
    public class FaviconDetectionService : IFaviconDetectionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FaviconDetectionService> _logger;

        public FaviconDetectionService(HttpClient httpClient, ILogger<FaviconDetectionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> DetectFaviconAsync(string url)
        {
            _logger.LogInformation("Starting favicon detection for URL: {Url}", url);

            try
            {
                var htmlContent = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                bool hasFavicon = await HasFaviconAsync(doc, url);

                if (hasFavicon)
                {
                    _logger.LogInformation("✅ Favicon found for URL: {Url}", url);
                }
                else
                {
                    _logger.LogWarning("❌ No favicon found for URL: {Url}", url);
                }

                return hasFavicon;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching HTML content for {Url}", url);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during favicon detection for {Url}", url);
                return false;
            }
        }

        private async Task<bool> HasFaviconAsync(HtmlDocument doc, string baseUrl)
        {
            // Check if a <link rel="icon"> exists in HTML
            var faviconNode = doc.DocumentNode.SelectSingleNode("//link[contains(@rel, 'icon')]");
            if (faviconNode != null)
            {
                return true; // Favicon found in HTML
            }

            // Check if /favicon.ico exists
            string defaultFaviconUrl = $"{baseUrl.TrimEnd('/')}/favicon.ico";
            return await FaviconExistsAsync(defaultFaviconUrl);
        }

        private async Task<bool> FaviconExistsAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
