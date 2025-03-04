using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Uxcheckmate_Main.Services
{
    public class WebScraperService
    {
        private readonly HttpClient _httpClient;

        public WebScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> FetchHtmlAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
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

            // Extract font-family styles from <style> and inline elements
            var fontStyles = doc.DocumentNode.SelectNodes("//style | //*[@style]") ?? new HtmlNodeCollection(null);
            var fontsUsed = new HashSet<string>();

            foreach (var node in fontStyles)
            {
                string style = node.InnerText + node.GetAttributeValue("style", "");
                var matches = System.Text.RegularExpressions.Regex.Matches(style, @"font-family:\s*([^;]+)");
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    fontsUsed.Add(match.Groups[1].Value.Trim());
                }
            }

            // Extract favicon details
            string faviconUrl = ExtractFaviconUrl(doc, url);
            bool hasFavicon = !string.IsNullOrEmpty(faviconUrl);

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
            var faviconNode = doc.DocumentNode.SelectSingleNode("//link[contains(@rel, 'icon')]");

            if (faviconNode != null)
            {
                var href = faviconNode.GetAttributeValue("href", "").Trim();

                if (!string.IsNullOrEmpty(href))
                {
                    // If href is relative, ensure baseUrl is valid before constructing an absolute URL
                    if (!href.StartsWith("http"))
                    {
                        if (!string.IsNullOrEmpty(baseUrl))
                        {
                            try
                            {
                                return new Uri(new Uri(baseUrl), href).ToString();
                            }
                            catch (UriFormatException)
                            {
                                Console.WriteLine($"Invalid URI: Base '{baseUrl}' or href '{href}'");
                                return string.Empty;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Base URL is empty, cannot resolve relative favicon path.");
                            return string.Empty;
                        }
                    }
                    return href;
                }
            }

            return string.Empty;
        }

        public async Task<Dictionary<string, object>> ScrapeAsync(string url)
        {
            var htmlContent = await FetchHtmlAsync(url);
            return ExtractHtmlElements(htmlContent, url);
        }
    }
}
