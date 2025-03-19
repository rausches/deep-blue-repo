using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Uxcheckmate_Main.Controllers;

namespace Uxcheckmate_Main.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _controller;
        private HttpClient _httpClient;
        private Mock<ILogger<HomeController>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _httpClient = new HttpClient();
            _controller = new HomeController(
                _mockLogger.Object,
                _httpClient,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            (_controller as IDisposable)?.Dispose();
        }

                [Test]
                public async Task Report_ShouldReturnViewResultForValidUrl()
                {
                    // Arrange
                    string redditUrl = "https://www.reddit.com";

                    // Act
                    var result = await _controller.Report(redditUrl);

                    // Assert
                    Assert.That(result, Is.TypeOf<ViewResult>(), "Expected a ViewResult for a valid URL.");
                }

                [Test]
                public async Task Report_ShouldProcessValidUrl_WebScrapingDev()
                {
                    // Arrange
                    string webScrapingDevUrl = "https://web-scraping.dev/products";

                    // Act
                    var result = await _controller.Report(webScrapingDevUrl);

                    // Assert
                    Assert.That(result, Is.TypeOf<ViewResult>(), "Expected a ViewResult for the web-scraping.dev/products URL.");
                }
    }
}
