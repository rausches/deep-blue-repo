using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    public class ScrollService_Tests
    {
        private ScrollService _scrollService;
        private Mock<ILogger<ScrollService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ScrollService>>();
            _scrollService = new ScrollService(_loggerMock.Object);
        }

        [Test]
        public async Task RunScrollAnalysisAsync_WhenDataIsValid_ReturnsScrollMessage()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "scrollHeight", 3000.0 },
                { "viewportHeight", 1000.0 }
            };

            // Act
            var result = await _scrollService.RunScrollAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Contain("scrolls"));
        }

        [Test]
        public async Task AnalyzeScrollDepthAsync_WhenScrollCountIsLow_ReturnsPositiveFeedback()
        {
            // Arrange
            double scrollHeight = 2000.0;
            double viewportHeight = 1000.0;

            // Act
            var result = await _scrollService.AnalyzeScrollDepthAsync(scrollHeight, viewportHeight);

            // Assert
            Assert.That(result, Does.Contain("easily viewable"));
        }

        [Test]
        public async Task AnalyzeScrollDepthAsync_WhenScrollCountIsHigh_ReturnsStreamlineSuggestion()
        {
            // Arrange
            double scrollHeight = 8000.0;
            double viewportHeight = 1000.0;

            // Act
            var result = await _scrollService.AnalyzeScrollDepthAsync(scrollHeight, viewportHeight);

            // Assert
            Assert.That(result, Does.Contain("streamline content").IgnoreCase);
        }
    }
}
