using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

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
                var response = await _httpClient.GetStringAsync(url);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching HTML content: {ex.Message}");
            }
        }

        public Dictionary<string, object> ExtractHtmlElements(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var headings = doc.DocumentNode.SelectNodes("//h2") ?? new HtmlNodeCollection(null);
            var images = doc.DocumentNode.SelectNodes("//img") ?? new HtmlNodeCollection(null);

            var result = new Dictionary<string, object>
            {
                { "headings", headings.Count },
                { "images", images.Count }
            };

            return result;
        }

        public async Task<string> ScrapeAsync(string url)
        {
            var htmlContent = await FetchHtmlAsync(url);
            var extractedData = ExtractHtmlElements(htmlContent);
            return JsonConvert.SerializeObject(extractedData, Formatting.Indented);
        }
    }
}
