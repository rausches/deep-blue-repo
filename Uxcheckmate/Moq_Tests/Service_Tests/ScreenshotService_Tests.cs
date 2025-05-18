using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Services;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Service_Tests
{
    public class ScreenshotServiceTests
    {
        private Mock<ILogger<ScreenshotService>> _mockLogger;
        private ScreenshotService _screenshotService;
        private IPlaywrightApiService _mockPlaywrightApiService;

        // This setup is responsible for creating the mock instances of the Logger and PlaywrightService
        // and initializing the ScreenshotService with these mock instances.
        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ScreenshotService>>();
            _mockPlaywrightApiService = new MockPlaywrightApiService();
            _screenshotService = new ScreenshotService(_mockLogger.Object, _mockPlaywrightApiService);
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

            var result = await _screenshotService.CaptureScreenshot(screenshotOptions, "https://example.com");

            Assert.That(result, Is.Not.EqualTo(string.Empty));
        }

        // Test to verify that the CaptureScreenshot method returns an empty string when an exception is thrown.
        [Test]
        public async Task CaptureScreenshot_ThrowsException_ReturnsEmptyString()
        {
            var service = new ScreenshotService(
                new Mock<ILogger<ScreenshotService>>().Object,
                new FailingPlaywrightApiService()
            );

            var result = await service.CaptureScreenshot(new PageScreenshotOptions(), "https://example.com");

            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}

