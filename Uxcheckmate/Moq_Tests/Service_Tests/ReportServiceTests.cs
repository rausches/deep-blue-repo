/*using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Moq_Tests")]

namespace Moq_Tests.Services
{
    [TestFixture]
    public class ReportServiceTests
    {
        private ReportService _reportService;
        private Mock<ILogger<ReportService>> _loggerMock;
        private Mock<IOpenAiService> _openAiServiceMock;
        private Mock<IBrokenLinksService> _brokenLinksServiceMock;
        private Mock<IHeadingHierarchyService> _headingHierarchyServiceMock;
        private Mock<IColorSchemeService> _colorSchemeServiceMock;
        private Mock<IDynamicSizingService> _dynamicSizingServiceMock;
        private Mock<ILogger<WebScraperService>> _webScraperLoggerMock;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ReportService>>();
            _openAiServiceMock = new Mock<IOpenAiService>();
            _brokenLinksServiceMock = new Mock<IBrokenLinksService>();
            _headingHierarchyServiceMock = new Mock<IHeadingHierarchyService>();
            _colorSchemeServiceMock = new Mock<IColorSchemeService>();
            _dynamicSizingServiceMock = new Mock<IDynamicSizingService>();
            _webScraperLoggerMock = new Mock<ILogger<WebScraperService>>();
            _httpClient = new HttpClient();

            _reportService = new ReportService(
                _httpClient,
                _loggerMock.Object,
                null, // DbContext not needed for this test
                _openAiServiceMock.Object,
                _brokenLinksServiceMock.Object,
                _headingHierarchyServiceMock.Object,
                _colorSchemeServiceMock.Object,
                _dynamicSizingServiceMock.Object,
                _webScraperLoggerMock.Object
            );
        }

        [Test]
        public async Task RunCustomAnalysisAsync_FontLegibility_DetectsIllegibleFonts()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "fonts", new List<string> { "Comic Sans MS", "Arial", "Papyrus" } }
            };

            // Act
            var result = await _reportService.RunCustomAnalysisAsync("https://example.com", "Font Legibility", "", scrapedData);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result), "Font legibility analysis should return an issue message.");
            Assert.That(result, Does.Contain("Comic Sans MS"), "Comic Sans MS should be detected as an illegible font.");
            Assert.That(result, Does.Contain("Papyrus"), "Papyrus should be detected as an illegible font.");
        }

        [Test]
        public async Task RunCustomAnalysisAsync_FaviconDetection_DetectsMissingFavicon()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "hasFavicon", false }
            };

            // Act
            var result = await _reportService.RunCustomAnalysisAsync("https://example.com", "Favicon", "", scrapedData);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result), "Favicon analysis should return an issue message when no favicon is found.");
            Assert.That(result, Does.Contain("No favicon found"), "The message should indicate a missing favicon.");
        }
    }
}
*/