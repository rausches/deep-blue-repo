using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    [TestFixture]
    public class MobileResponsivenessService_Tests
    {
        private MobileResponsivenessService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new MobileResponsivenessService();
        }

        [Test]
        public async Task RunMobileAnalysisAsync_ReturnsReport_When_ScrollWidthExceedsViewport()
        {
            // Arrange
            var data = new Dictionary<string, object>
            {
                { "viewportLabel", "iPhone 12" },
                { "viewportWidth", 390 },
                { "scrollWidth", 500 }
            };

            // Act
            var result = await _service.RunMobileAnalysisAsync("https://example.com", data);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("Horizontal scroll detected"));
            Assert.That(result, Does.Contain("iPhone 12"));
        }

        [Test]
        public async Task RunMobileAnalysisAsync_ReturnsEmpty_When_ScrollWidthIsLessThanOrEqualToViewport()
        {
            var data = new Dictionary<string, object>
            {
                { "viewportLabel", "Pixel 6" },
                { "viewportWidth", 400 },
                { "scrollWidth", 400 }
            };

            var result = await _service.RunMobileAnalysisAsync("https://example.com", data);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task RunMobileAnalysisAsync_ReturnsEmpty_When_ViewportOrScrollWidthMissing()
        {
            var data = new Dictionary<string, object>
            {
                { "viewportLabel", "Galaxy S21" },
                { "viewportWidth", 360 }
                // scrollWidth is missing
            };

            var result = await _service.RunMobileAnalysisAsync("https://example.com", data);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task RunMobileAnalysisAsync_ReturnsEmpty_When_DataIsNotNumeric()
        {
            var data = new Dictionary<string, object>
            {
                { "viewportLabel", "Fake Device" },
                { "viewportWidth", "wide" },
                { "scrollWidth", "narrow" }
            };

            var result = await _service.RunMobileAnalysisAsync("https://example.com", data);

            Assert.That(result, Is.Empty);
        }
    }
}
