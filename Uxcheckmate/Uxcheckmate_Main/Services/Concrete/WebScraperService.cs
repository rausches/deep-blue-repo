using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Uxcheckmate_Main.Services
{
    public class WebScraperService : IWebScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebScraperService> _logger;
       /* private ILogger<ColorSchemeService> logger;*/

        public WebScraperService(HttpClient httpClient, ILogger<WebScraperService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

       /* public WebScraperService(ILogger<ColorSchemeService> logger)
        {
            this.logger = logger;
        }*/

        public async Task<string> FetchHtmlAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching HTML content: {Message}", ex.Message);
                throw new Exception($"Error fetching HTML content: {ex.Message}");
            }
        }

        public Dictionary<string, object> ExtractHtmlElements(string htmlContent, string url)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var headings = doc.DocumentNode.SelectNodes("//h1 | //h2 | //h3 | //h4 | //h5 | //h6") ?? new HtmlNodeCollection(null);
            var paragraphs = doc.DocumentNode.SelectNodes("//p") ?? new HtmlNodeCollection(null);
            var images = doc.DocumentNode.SelectNodes("//img") ?? new HtmlNodeCollection(null);
            var linkNodes = doc.DocumentNode.SelectNodes("//a");

            var links = linkNodes != null 
                ? linkNodes
                    .Select(node => node.GetAttributeValue("href", ""))
                    .Where(href => !string.IsNullOrEmpty(href))
                    .ToList()
                : new List<string>();

            var fontStyles = doc.DocumentNode.SelectNodes("//style | //*[@style]") ?? new HtmlNodeCollection(null);
            var fontsUsed = new HashSet<string>();

            foreach (var node in fontStyles)
            {
                string style = node.InnerText + node.GetAttributeValue("style", "");
                var matches = System.Text.RegularExpressions.Regex.Matches(style, @"font-family:\s*([^;]+)");
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var fontName = match.Groups[1].Value.Split(',')[0].Trim().ToLower(); // Normalize font name
                    fontsUsed.Add(fontName);
                }
            }

            string faviconUrl = ExtractFaviconUrl(doc, url);
            bool hasFavicon = !string.IsNullOrEmpty(faviconUrl);

            _logger.LogInformation("Favicon detection complete. Has favicon: {HasFavicon}, Favicon URL: {FaviconUrl}", hasFavicon, faviconUrl);

            return new Dictionary<string, object>
            {
                { "htmlContent", htmlContent },
                { "headings", headings.Count },
                { "paragraphs", paragraphs.Count },
                { "images", images.Count },
                { "links", links },
                { "text_content", string.Join("\n", paragraphs.Select(p => p.InnerText.Trim()).Where(text => !string.IsNullOrEmpty(text))) },
                { "fonts", fontsUsed.ToList() },
                { "hasFavicon", hasFavicon },
                { "faviconUrl", faviconUrl }
            };
        }

        private string ExtractFaviconUrl(HtmlDocument doc, string baseUrl)
        {
            string faviconUrl = string.Empty;
            var faviconNode = doc.DocumentNode.SelectSingleNode("//link[contains(@rel, 'icon')]");

            if (faviconNode != null)
            {
                var href = faviconNode.GetAttributeValue("href", "").Trim();
                _logger.LogDebug("Favicon node found. Extracted href: {Href}", href);

                if (!string.IsNullOrEmpty(href))
                {
                    if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            faviconUrl = new Uri(new Uri(baseUrl), href).ToString();
                            _logger.LogDebug("Resolved absolute favicon URL: {AbsoluteUrl}", faviconUrl);
                        }
                        catch (UriFormatException ex)
                        {
                            _logger.LogWarning("Invalid URI when resolving favicon. Base: {BaseUrl}, Href: {Href}, Error: {Error}", baseUrl, href, ex.Message);
                        }
                    }
                    else
                    {
                        faviconUrl = href;
                    }
                }
            }

            return faviconUrl;
        }

        public async Task<Dictionary<string, object>> ScrapeAsync(string url)
        {
            var htmlContent = await FetchHtmlAsync(url);
            return ExtractHtmlElements(htmlContent, url);
        }
    }
}
