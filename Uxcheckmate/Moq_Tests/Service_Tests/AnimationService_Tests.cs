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
            var html = @"<div style='animation: fadeIn 2s;'></div>";
            var css = new List<string> { "@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }" };

            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", html },
                { "inlineCssList", css },
                { "externalCssContents", new List<string>() },
                { "inlineJsList", new List<string>() },
                { "externalJsContents", new List<string>() }
            };

            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("CSS Animation"));
            Assert.That(result, Does.Contain("fadeIn"));
        }

        [Test]
        public async Task RunAnimationAnalysisAsync_ReturnsFinding_When_JavascriptAnimationExists()
        {
            var html = @"<div id='box'></div>";
            var js = new List<string> { "requestAnimationFrame(document.getElementById('box'));" };

            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", html },
                { "inlineCssList", new List<string>() },
                { "externalCssContents", new List<string>() },
                { "inlineJsList", js },
                { "externalJsContents", new List<string>() }
            };

            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("requestAnimationFrame"));
            Assert.That(result, Does.Contain("box"));
        }

        [Test]
        public async Task RunAnimationAnalysisAsync_ReturnsEmpty_When_NoAnimationFound()
        {
            var html = "<div class='static'></div>";

            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", html },
                { "inlineCssList", new List<string> { ".box { background-color: red; }" } },
                { "externalCssContents", new List<string>() },
                { "inlineJsList", new List<string> { "console.log('hello');" } },
                { "externalJsContents", new List<string>() }
            };

            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task RunAnimationAnalysisAsync_ReturnsWarning_When_TooManyAnimations()
        {
            var html = string.Join("\n", Enumerable.Range(1, 6).Select(i => $"<div style='animation: fadeIn 1s;' class='section'></div>"));
            var css = new List<string> { "@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }" };

            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", html },
                { "inlineCssList", css },
                { "externalCssContents", new List<string>() },
                { "inlineJsList", new List<string>() },
                { "externalJsContents", new List<string>() }
            };

            var result = await _animationService.RunAnimationAnalysisAsync("https://example.com", scrapedData);

            Assert.That(result, Does.Contain("Too many animations"));
            Assert.That(result, Does.Contain("Multiple animations"));
        }
    }
}
