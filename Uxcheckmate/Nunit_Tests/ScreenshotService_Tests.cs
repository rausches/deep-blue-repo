using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Services;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Uxcheckmate_Main.Tests
{
    public class ScreenshotServiceTests
    {
        private Mock<ILogger<ScreenshotService>> _mockLogger;
        private Mock<IPlaywrightService> _mockPlaywrightService;
        private ScreenshotService _screenshotService;

        // This setup is responsible for creating the mock instances of the Logger and PlaywrightService
        // and initializing the ScreenshotService with these mock instances.
        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ScreenshotService>>();
            _mockPlaywrightService = new Mock<IPlaywrightService>();
            _screenshotService = new ScreenshotService(_mockLogger.Object, _mockPlaywrightService.Object);
        }

        // Test to veify that the CaptureScreenshot method returns an empty string when the URL is empty.
        [Test]
        public async Task CaptureScreenshot_WithEmptyUrl_ReturnsEmptyString()
        {
            // Arrange
            var screenshotOptions = new PageScreenshotOptions { FullPage = true };
            var result = await _screenshotService.CaptureScreenshot(screenshotOptions, string.Empty);

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
            Assert.That(_mockLogger.Invocations.Any(invocation =>
                invocation.Method.Name == "Log" &&
                invocation.Arguments.Any(a => a.ToString().Contains("URL cannot be empty"))));
        }


        // Test to verify that the CaptureScreenshot method captures a screenshot when a valid URL is provided.
        [Test]
        public async Task CaptureScreenshot_WithValidUrl_CapturesScreenshot()
        {
            var screenshotOptions = new PageScreenshotOptions { FullPage = true };
            var mockPage = new Mock<IPage>();
            var mockContext = new Mock<IBrowserContext>();

            // Setup the mock PlaywrightService to return the mock browser context.
            _mockPlaywrightService.Setup(x => x.GetBrowserContextAsync())
                .ReturnsAsync(mockContext.Object);

            // Setup the mock browser context to return the mock page.
            mockContext.Setup(x => x.NewPageAsync())
                .ReturnsAsync(mockPage.Object);

            // Setup the mock page to return a byte array when the ScreenshotAsync method is called.
            mockPage.Setup(x => x.ScreenshotAsync(It.IsAny<PageScreenshotOptions>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            var result = await _screenshotService.CaptureScreenshot(screenshotOptions, "https://example.com");

            Assert.That(result, Is.Not.EqualTo(string.Empty));
        }

        // Test to verify that the CaptureScreenshot method returns an empty string when an exception is thrown.
        [Test]
        public async Task CaptureScreenshot_ThrowsException_ReturnsEmptyString()
        {
            _mockPlaywrightService.Setup(x => x.GetBrowserContextAsync())
                .ThrowsAsync(new Exception("Failed to capture screenshot"));

            var screenshotOptions = new PageScreenshotOptions { FullPage = true };
            var result = await _screenshotService.CaptureScreenshot(screenshotOptions, "https://example.com");

            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
