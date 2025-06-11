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
        private readonly IPlaywrightApiService _playwrightApiService;

        public ScreenshotService(ILogger<ScreenshotService> logger,
            IPlaywrightApiService playwrightApiService)
        {
            _logger = logger;
            _playwrightApiService = playwrightApiService;
        }

        // PageScreenshotOptions is a class in Playwright that allows you to specify options for taking a screenshot of a page.
        // The options include the full page, the quality of the image, the type of image, and the path to save the image.
        public async Task<string> CaptureScreenshot(PageScreenshotOptions options, string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("URL cannot be empty.");
                return string.Empty;
            }

            try
            {
                _logger.LogInformation("Capturing standard viewport screenshot of {Url}", url);
                var result = await _playwrightApiService.AnalyzeWebsiteAsync(url, fullPage: false);

                if (string.IsNullOrEmpty(result?.ScreenshotBase64))
                {
                    _logger.LogWarning("ScreenshotBase64 was null or empty for {Url}", url);
                    return string.Empty;
                }

                return result.ScreenshotBase64;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture viewport screenshot of {Url}", url);
                return string.Empty;
            }
        }

        public async Task<byte[]> CaptureFullPageScreenshot(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("URL cannot be empty.");
                return Array.Empty<byte>();
            }

            try
            {
                _logger.LogInformation("Capturing full-page screenshot of {Url}", url);
                var result = await _playwrightApiService.AnalyzeWebsiteAsync(url, fullPage: true);

                if (result?.ScreenshotBase64 == null)
                {
                    _logger.LogWarning("ScreenshotBase64 is null or empty for URL: {Url}", url);
                    return Array.Empty<byte>();
                }

                // Decode the base64 string to byte[]
                return Convert.FromBase64String(result.ScreenshotBase64);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture full-page screenshot.");
                return Array.Empty<byte>();
            }
        }

    }
}