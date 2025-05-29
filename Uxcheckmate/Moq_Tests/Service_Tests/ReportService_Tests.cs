using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Threading;

namespace Service_Tests
{
    [TestFixture]
    public class ReportServiceTests
    {
        private ReportService _reportService;
        private Mock<IOpenAiService> _openAiServiceMock;
        private Mock<IColorSchemeService> _colorSchemeServiceMock;
        private UxCheckmateDbContext _context;
        private Mock<IScreenshotService> _screenshotServiceMock;
        private Mock<IPlaywrightScraperService> _playwrightScraperServiceMock;
        private Mock<IBrokenLinksService> _brokenLinksServiceMock;
        private Mock<IHeadingHierarchyService> _headingHierarchyServiceMock;
        private Mock<IMobileResponsivenessService> _mobileResponsivenessServiceMock;
        private Mock<IPopUpsService> _popUpsServiceMock;
        private Mock<IAnimationService> _animationServiceMock;
        private Mock<IAudioService> _audioServiceMock;
        private Mock<IScrollService> _scrollServiceMock;
        private Mock<IFPatternService> _fPatternServiceMock;
        private Mock<IZPatternService> _zPatternServiceMock;
        private Mock<ISymmetryService> _symmetryServiceMock;
        private ScrapedContent _mockScrapedContent;
        private Mock<IServiceScopeFactory> _scopeFactoryMock;
        private Mock<IMemoryCache> _cacheMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new UxCheckmateDbContext(options);

            _context.DesignCategories.AddRange(
                new DesignCategory { Id = 1, Name = "Color Scheme", ScanMethod = "Custom" },
                new DesignCategory { Id = 2, Name = "AI Analysis", ScanMethod = "OpenAI" }
            );
            _context.SaveChanges();

            _mockScrapedContent = new ScrapedContent
            {
                Url = "https://example.com",
                HtmlContent = "<html></html>",
                ViewportHeight = 1000,
                ViewportWidth = 1200,
            };

            var logger = Mock.Of<ILogger<ReportService>>();

            _openAiServiceMock = new Mock<IOpenAiService>();
            _colorSchemeServiceMock = new Mock<IColorSchemeService>();
            _screenshotServiceMock = new Mock<IScreenshotService>();
            _playwrightScraperServiceMock = new Mock<IPlaywrightScraperService>();
            _brokenLinksServiceMock = new Mock<IBrokenLinksService>();
            _headingHierarchyServiceMock = new Mock<IHeadingHierarchyService>();
            _mobileResponsivenessServiceMock = new Mock<IMobileResponsivenessService>();
            _popUpsServiceMock = new Mock<IPopUpsService>();
            _animationServiceMock = new Mock<IAnimationService>();
            _audioServiceMock = new Mock<IAudioService>();
            _scrollServiceMock = new Mock<IScrollService>();
            _fPatternServiceMock = new Mock<IFPatternService>();
            _zPatternServiceMock = new Mock<IZPatternService>();
            _symmetryServiceMock = new Mock<ISymmetryService>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _cacheMock = new Mock<IMemoryCache>();

            _playwrightScraperServiceMock
                .Setup(s => s.ScrapeEverythingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockScrapedContent);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();

            serviceProviderMock
                .Setup(x => x.GetService(typeof(UxCheckmateDbContext)))
                .Returns(_context);

            serviceScopeMock
                .Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            _scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);

            _reportService = new ReportService(
                new HttpClient(),
                logger,
                _context,
                _openAiServiceMock.Object,
                _brokenLinksServiceMock.Object,
                _headingHierarchyServiceMock.Object,
                _colorSchemeServiceMock.Object,
                _mobileResponsivenessServiceMock.Object,
                _screenshotServiceMock.Object,
                _playwrightScraperServiceMock.Object,
                _popUpsServiceMock.Object,
                _animationServiceMock.Object,
                _audioServiceMock.Object,
                _scrollServiceMock.Object,
                _fPatternServiceMock.Object,
                _zPatternServiceMock.Object,
                _symmetryServiceMock.Object,
                _scopeFactoryMock.Object,
                _cacheMock.Object
            );
        }

        [Test]
        public async Task GenerateReportAsync_Returns_Empty_If_No_Issues_Found()
        {
            var report = new Report { Url = "https://example.com" };

            _screenshotServiceMock
                .Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            _colorSchemeServiceMock
                .Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .ReturnsAsync("");

            var result = await _reportService.GenerateReportAsync(report, It.IsAny<CancellationToken>());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GenerateReportAsync_Returns_Issue_If_Issue_Found()
        {
            var report = new Report { Url = "https://example.com" };

            _screenshotServiceMock
                .Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            _colorSchemeServiceMock
                .Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .Callback(() => Console.WriteLine("Color analysis invoked!"))
                .ReturnsAsync("Issue Found");

            var result = await _reportService.GenerateReportAsync(report, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GenerateReportAsync_Skips_Category_With_Empty_ScanMethod()
        {
            _context.DesignCategories.Add(new DesignCategory
            {
                Id = 99,
                Name = "EmptyScan",
                ScanMethod = ""
            });
            _context.SaveChanges();

            var report = new Report { Url = "https://example.com" };

            var result = await _reportService.GenerateReportAsync(report, It.IsAny<CancellationToken>());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.All(r => r.CategoryId != 99));
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_String_IfIssuesFound()
        {
            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            _colorSchemeServiceMock.Setup(service => service.AnalyzeWebsiteColorsAsync(
                It.IsAny<Dictionary<string, object>>(),
                screenshotTask))
                .ReturnsAsync("Issue found");

            var result = await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>(), _mockScrapedContent, screenshotTask);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_Null_If_NoIssuesFound()
        {
            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            _colorSchemeServiceMock.Setup(s => s.AnalyzeWebsiteColorsAsync(
                It.IsAny<Dictionary<string, object>>(),
                screenshotTask))
                .ReturnsAsync("");

            var result = await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>(), _mockScrapedContent, screenshotTask);

            Assert.That(string.IsNullOrEmpty(result), Is.True);
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Calls_Correct_Category_Service()
        {
            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>(), _mockScrapedContent, screenshotTask);

            _colorSchemeServiceMock.Verify(service => service.AnalyzeWebsiteColorsAsync(
                It.IsAny<Dictionary<string, object>>(),
                screenshotTask), Times.Once);
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_FontIssue_When_IllegibleFontDetected()
        {
            var data = new Dictionary<string, object>
            {
                { "fonts", new List<string> { "Chiller", "wingdings" } }
            };

            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            var result = await _reportService.RunCustomAnalysisAsync("url", "Font Legibility", "desc", data, _mockScrapedContent, screenshotTask);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("illegible"));
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_Empty_For_LegibleFonts()
        {
            var data = new Dictionary<string, object>
            {
                { "fonts", new List<string> { "Arial", "Verdana" } }
            };

            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            var result = await _reportService.RunCustomAnalysisAsync("url", "Font Legibility", "desc", data, _mockScrapedContent, screenshotTask);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_FaviconWarning_If_Missing()
        {
            var data = new Dictionary<string, object>
            {
                { "hasFavicon", false }
            };

            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            var result = await _reportService.RunCustomAnalysisAsync("url", "Favicon", "desc", data, _mockScrapedContent, screenshotTask);

            Assert.That(result, Does.Contain("No favicon found"));
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_Empty_If_FaviconPresent()
        {
            var data = new Dictionary<string, object>
            {
                { "hasFavicon", true }
            };

            var screenshotTask = Task.FromResult(Array.Empty<byte>());

            var result = await _reportService.RunCustomAnalysisAsync("url", "Favicon", "desc", data, _mockScrapedContent, screenshotTask);

            Assert.That(result, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}
