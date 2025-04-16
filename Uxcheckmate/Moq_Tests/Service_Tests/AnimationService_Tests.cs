using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    [TestFixture]
    public class AnimationService_Tests
    {
        private AnimationService _animationService;
        private Mock<ILogger<AnimationService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AnimationService>>();
            _animationService = new AnimationService(_loggerMock.Object);
        }

        [Test]
        public async Task RunAnimationAnalysisAsync_ReturnsFinding_When_CssAnimationExists()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "inlineCss", new List<string> { ".box { animation: fadeIn 2s ease-in; }" } },
                { "externalCssContents", new List<string>() },
                { "inlineJs", new List<string>() },
                { "externalJsContents", new List<string>() }
            };

            // Act
            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("animation-related"));
        }

        [Test]
        public async Task RunAnimationAnalysisAsync_ReturnsFinding_When_JavascriptAnimationExists()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "inlineCss", new List<string>() },
                { "externalCssContents", new List<string>() },
                { "inlineJs", new List<string> { "requestAnimationFrame(() => {});" } },
                { "externalJsContents", new List<string>() }
            };

            // Act
            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("animation-related"));
        }

        [Test]
        public async Task RunAnimationAnalysisAsync_ReturnsEmpty_When_NoAnimationFound()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "inlineCss", new List<string> { ".box { background-color: red; }" } },
                { "externalCssContents", new List<string>() },
                { "inlineJs", new List<string> { "console.log('hello');" } },
                { "externalJsContents", new List<string>() }
            };

            // Act
            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
