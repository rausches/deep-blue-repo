using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Uxcheckmate_Main.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Uxcheckmate_Main.Services
{
    public class PlaywrightScraperService : IPlaywrightScraperService
    {
        private readonly IPlaywrightService _playwrightService;
        private readonly ILogger<PlaywrightScraperService> _logger;

        public PlaywrightScraperService(IPlaywrightService playwrightService, ILogger<PlaywrightScraperService> logger)
        {
            _playwrightService = playwrightService;
            _logger = logger;
        }

        public async Task<ScrapedContent> ScrapeAsync(string url)
        {
            // Create a new browser context (isolated session)
            var context = await _playwrightService.GetBrowserContextAsync();

            // Open a new page in the browser context
            var page = await context.NewPageAsync();

            // Navigate to the URL and wait until network is idle 
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Inject custom JavaScript helpers into the page context
            await page.AddScriptTagAsync(new PageAddScriptTagOptions
            {
                Path = "wwwroot/js/playwrightScraperHelper.js"
            });

            // Extract all inline <style> tag content
            var inlineCss = await page.EvalOnSelectorAllAsync<string[]>("style", "elements => elements.map(e => e.textContent)");

            // Extract all external stylesheet links
            var externalCss = await page.EvalOnSelectorAllAsync<string[]>("link[rel='stylesheet']", "elements => elements.map(e => e.href)");

            // Extract all inline <script> tag content
            var inlineJs = await page.EvalOnSelectorAllAsync<string[]>("script:not([src])", "elements => elements.map(e => e.textContent)");

            // Extract all external JavaScript links
            var externalJs = await page.EvalOnSelectorAllAsync<string[]>("script[src]", "elements => elements.map(e => e.src)");

            // Use helper functions injected via scrape-assets.js to fetch content from external CSS files
            var externalCssContents = await page.EvaluateAsync<string[]>("() => window.scrapeExternalCss()");

            // Use helper functions injected via scrape-assets.js to fetch content from external JS files
            var externalJsContents = await page.EvaluateAsync<string[]>("() => window.scrapeExternalJs()");

            // Clean up by closing the page and context
            await page.CloseAsync();
            await context.CloseAsync();

            // Return all the scraped asset information as a ScrapedContent object
            return new ScrapedContent
            {
                Url = url,
                InlineCss = inlineCss.ToList(),
                ExternalCssLinks = externalCss.ToList(),
                ExternalCssContents = externalCssContents.ToList(),
                InlineJs = inlineJs.ToList(),
                ExternalJsLinks = externalJs.ToList(),
                ExternalJsContents = externalJsContents.ToList()
            };
        }
    }
}
