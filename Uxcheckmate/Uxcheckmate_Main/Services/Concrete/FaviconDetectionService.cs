using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task<(bool hasFavicon, string faviconUrl)> DetectFaviconAsync(string url)
        {
            _logger.LogInformation("Starting favicon detection for URL: {Url}", url);

            try
            {
                var htmlContent = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var faviconUrl = ExtractFaviconUrl(doc, url);
                bool hasFavicon = !string.IsNullOrEmpty(faviconUrl);

                if (hasFavicon)
                {
                    _logger.LogInformation("Favicon found: {FaviconUrl}", faviconUrl);
                }
                else
                {
                    _logger.LogWarning("No favicon found for URL: {Url}", url);
                }

                return (hasFavicon, faviconUrl);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching HTML content for {Url}", url);
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during favicon detection for {Url}", url);
                return (false, string.Empty);
            }
        }

        private string ExtractFaviconUrl(HtmlDocument doc, string baseUrl)
        {
            var faviconNode = doc.DocumentNode.SelectSingleNode("//link[contains(@rel, 'icon')]");

            if (faviconNode != null)
            {
                var href = faviconNode.GetAttributeValue("href", "").Trim();
                
                if (!string.IsNullOrEmpty(href))
                {
                    return MakeAbsoluteUrl(baseUrl, href);
                }
            }

            return string.Empty;
        }

        private string MakeAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
                Uri.TryCreate(baseUri, relativeUrl, out Uri absoluteUri))
            {
                return absoluteUri.ToString();
            }

            _logger.LogWarning("Failed to convert relative URL '{RelativeUrl}' using base URL '{BaseUrl}'", relativeUrl, baseUrl);
            return relativeUrl;
        }
    }
}
