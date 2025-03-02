using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Tests
{
    [TestFixture]
    public class ColorSchemeServiceTests
    {
        private ColorSchemeService _colorService;
        private Mock<WebScraperService> _mockScraperService;

        [SetUp]
        public void Setup()
        {
            _mockScraperService = new Mock<WebScraperService>(new HttpClient());
            _colorService = new ColorSchemeService(_mockScraperService.Object);
        }
        [Test]
        public async Task ReturnColorInfo()
        {
            // Creating dictionary of information
            var mockScrapedData = new Dictionary<string, object>
            {
                { "colors_used", new Dictionary<string, int> { { "#FF5733", 10 }, { "#000000", 5 } } },
                { "tag_counts", new Dictionary<string, int> { { "h1", 2 }, { "p", 5 } } }
            };
            _mockScraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>())).ReturnsAsync(mockScrapedData);
            var result = await _colorService.AnalyzeWebsiteColorsAsync("https://moqs.com");
            // Making sure it has everything
            Assert.That(result, Is.Not.Null);
            Assert.That(result["dominant_colors"], Is.Not.Null);
            Assert.That(result["tag_counts"], Is.Not.Null);
            var colors = (List<dynamic>)result["dominant_colors"];
            Assert.AreEqual(2, colors.Count, "Should detect 2 colors.");
            Assert.AreEqual("#FF5733", colors[0].Hex, "Most used color should be #FF5733.");
        }
        [Test]
        public async Task NoColors()
        {
            // Creating blank information
            var mockScrapedData = new Dictionary<string, object>
            {
                { "colors_used", new Dictionary<string, int>() },
                { "tag_counts", new Dictionary<string, int>() }
            };
            _mockScraperService.Setup(s => s.ScrapeAsync(It.IsAny<string>())).ReturnsAsync(mockScrapedData);
            var result = await _colorService.AnalyzeWebsiteColorsAsync("https://moqs.com");
            var colors = (List<dynamic>)result["dominant_colors"];
            Assert.That(colors, Is.Empty, "Should return an empty list if no colors are found.");
        }
    }
}
