using System;
using System.Text.Json;
using Microsoft.Playwright;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Logging;

namespace Uxcheckmate_Main.Services
{
    // ScreenshotService is responsible for taking screenshots of webpages using Playwright.
    public class ScreenshotService : IScreenshotService
    {
        private readonly ILogger<ScreenshotService> _logger;

        // Calling the PlaywrightService to get the browser context
        private readonly IPlaywrightService _playwrightService;

        public ScreenshotService(ILogger<ScreenshotService> logger, IPlaywrightService playwrightService)
        {
            _logger = logger;
            _playwrightService = playwrightService;
        }

        // PageScreenshotOptions is a class in Playwright that allows you to specify options for taking a screenshot of a page.
        // The options include the full page, the quality of the image, the type of image, and the path to save the image.
        public async Task<string> CaptureScreenshot(PageScreenshotOptions screenshotOptions, string url)
        {
            // Check if the URL is empty or null
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("URL cannot be empty.");
                return string.Empty;
            }

            try
            {
                // Get the browser context from the PlaywrightService
                // This ensures that a new browser context is created for its own session
                var context = await _playwrightService.GetBrowserContextAsync();
                
                // Create a new page in the browser context
                var page = await context.NewPageAsync();

                // Navigate to the specified URL
                await page.GotoAsync(url);

                // Take a screenshot of the page with the specified options
                var screenshotBytes = await page.ScreenshotAsync(screenshotOptions);

                // Convert the screenshot bytes to a base64 string
                // This allows the image to be displayed in the browser without saving it to disk
                // The base64 string is prefixed with the data:image/png;base64, so the browser knows it is an image
                // This uses the MIME type for PNG images image/png
                return $"data:image/png;base64,{Convert.ToBase64String(screenshotBytes)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture screenshot.");
                return string.Empty;
            }
        }

    }
}