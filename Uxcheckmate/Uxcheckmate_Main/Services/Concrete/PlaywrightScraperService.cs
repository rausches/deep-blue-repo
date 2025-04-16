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
            // Run in unique headless browser
            var browser = await _playwrightService.GetBrowserAsync(); 
            var context = await browser.NewContextAsync();   
            var page    = await context.NewPageAsync(); 

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

            var scrollHeight = await page.EvaluateAsync<double>("() => document.documentElement.scrollHeight");
            var viewportHeight = await page.EvaluateAsync<double>("() => window.innerHeight");
            var scrollWidth = await page.EvaluateAsync<double>("() => document.documentElement.scrollWidth");
            var viewportWidth = await page.EvaluateAsync<double>("() => window.innerWidth");

            await page.CloseAsync();
            await context.CloseAsync();

            // Return all the scraped asset information as a ScrapedContent object
            return new ScrapedContent
            {
                Url = url,
                InlineCss = string.Join("\n", inlineCss),
                InlineJs = string.Join("\n", inlineJs),
                InlineCssList = inlineCss.ToList(),
                InlineJsList = inlineJs.ToList(),
                ExternalCssLinks = externalCss.ToList(),
                ExternalJsLinks = externalJs.ToList(),
                ExternalCssContents = externalCssContents.ToList(),
                ExternalJsContents = externalJsContents.ToList(),
                ScrollHeight = scrollHeight,
                ViewportHeight = viewportHeight,
                ScrollWidth = scrollWidth,
                ViewportWidth = viewportWidth,
                ViewportLabel = $"{viewportWidth}x{viewportHeight}"


            };
        }

        // Asynchronously scrapes the target URL at multiple viewport sizes and returns a dictionary of ScrapedContent per size
        public async Task<Dictionary<string, ScrapedContent>> ScrapeAcrossViewportsAsync(string url, List<(int width, int height)> viewports)
        {
            // Get a reusable headless browser instance from the injected Playwright service
            var browser = await _playwrightService.GetBrowserAsync();

            // This dictionary will store the scraped results for each viewport label 
            var result = new Dictionary<string, ScrapedContent>();

            // Iterate over each specified screen size (width x height)
            foreach (var (width, height) in viewports)
            {
                // Create a new isolated browser context with the current viewport size
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize { Width = width, Height = height }
                });

                var page = await context.NewPageAsync();
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

                await page.AddScriptTagAsync(new PageAddScriptTagOptions
                {
                    Path = "wwwroot/js/playwrightScraperHelper.js"
                });

                var inlineCss = await page.EvalOnSelectorAllAsync<string[]>("style", "elements => elements.map(e => e.textContent)");
                var externalCss = await page.EvalOnSelectorAllAsync<string[]>("link[rel='stylesheet']", "elements => elements.map(e => e.href)");
                var inlineJs = await page.EvalOnSelectorAllAsync<string[]>("script:not([src])", "elements => elements.map(e => e.textContent)");
                var externalJs = await page.EvalOnSelectorAllAsync<string[]>("script[src]", "elements => elements.map(e => e.src)");

                var externalCssContents = await page.EvaluateAsync<string[]>("() => window.scrapeExternalCss()");
                var externalJsContents = await page.EvaluateAsync<string[]>("() => window.scrapeExternalJs()");

                var scrollHeight = await page.EvaluateAsync<double>("() => document.documentElement.scrollHeight");
                var viewportHeight = await page.EvaluateAsync<double>("() => window.innerHeight");
                var scrollWidth = await page.EvaluateAsync<double>("() => document.documentElement.scrollWidth");
                var viewportWidth = await page.EvaluateAsync<double>("() => window.innerWidth");

                var content = new ScrapedContent
                {
                    Url = url,
                    InlineCss = string.Join("\n", inlineCss),
                    InlineJs = string.Join("\n", inlineJs),
                    InlineCssList = inlineCss.ToList(),
                    InlineJsList = inlineJs.ToList(),
                    ExternalCssLinks = externalCss.ToList(),
                    ExternalJsLinks = externalJs.ToList(),
                    ExternalCssContents = externalCssContents.ToList(),
                    ExternalJsContents = externalJsContents.ToList(),
                    ScrollHeight = scrollHeight,
                    ViewportHeight = viewportHeight,
                    ScrollWidth = scrollWidth,
                    ViewportWidth = viewportWidth,
                    ViewportLabel = $"{width}x{height}"
                };

                result[content.ViewportLabel] = content;

                await context.CloseAsync();
            }

            return result;
        }
    }
}
