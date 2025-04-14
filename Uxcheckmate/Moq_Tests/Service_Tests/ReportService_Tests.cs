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

namespace Service_Tests
{
    [TestFixture]
    public class ReportServiceTests
    {
        private ReportService _reportService;
        private Mock<IOpenAiService> _openAiServiceMock;
        private Mock<IColorSchemeService> _colorSchemeServiceMock;
        private UxCheckmateDbContext _context;
        private Mock<IWebScraperService> _webScraperServiceMock;
        private Mock<IScreenshotService> _screenshotServiceMock;
        private Mock<IPlaywrightScraperService> _playwrightScraperServiceMock;
        private Mock<IBrokenLinksService> _brokenLinksServiceMock;
        private Mock<IHeadingHierarchyService> _headingHierarchyServiceMock;
        private Mock<IDynamicSizingService> _dynamicSizingServiceMock;
        private Mock<IPopUpsService> _popUpsServiceMock;
        private Mock<IAnimationService> _animationServiceMock;
        private Mock<IAudioService> _audioServiceMock;
        private Mock<IScrollService> _scrollServiceMock;

        [SetUp]
        public void Setup()
        {
            // Use unique in-memory database per test
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new UxCheckmateDbContext(options);

            // Seed design categories
            _context.DesignCategories.AddRange(
                new DesignCategory { Id = 1, Name = "Color Scheme", ScanMethod = "Custom" },
                new DesignCategory { Id = 2, Name = "AI Analysis", ScanMethod = "OpenAI" }
            );
            _context.SaveChanges();

            // Mock loggers
            var logger = Mock.Of<ILogger<ReportService>>();

            // Create all service mocks
            _openAiServiceMock = new Mock<IOpenAiService>();
            _colorSchemeServiceMock = new Mock<IColorSchemeService>();
            _screenshotServiceMock = new Mock<IScreenshotService>();
            _webScraperServiceMock = new Mock<IWebScraperService>();
            _playwrightScraperServiceMock = new Mock<IPlaywrightScraperService>();
            _brokenLinksServiceMock = new Mock<IBrokenLinksService>();
            _headingHierarchyServiceMock = new Mock<IHeadingHierarchyService>();
            _dynamicSizingServiceMock = new Mock<IDynamicSizingService>();
            _popUpsServiceMock = new Mock<IPopUpsService>();
            _animationServiceMock = new Mock<IAnimationService>();
            _audioServiceMock = new Mock<IAudioService>();
            _scrollServiceMock = new Mock<IScrollService>();

            // Setup default web scraper response
            _webScraperServiceMock
                .Setup(s => s.ScrapeAsync(It.IsAny<string>()))
                .ReturnsAsync(new Dictionary<string, object>
                {
                    { "htmlContent", "<html><body><h1>Title</h1><p>Content</p></body></html>" }
                });

            // Setup default playwright scraper response
            _playwrightScraperServiceMock
                .Setup(s => s.ScrapeAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedContent
                {
                    ExternalCssContents = new List<string>(),
                    InlineCss = new List<string>(),
                    ExternalJsContents = new List<string>(),
                    InlineJs = new List<string>(),
                    ScrollHeight = 3000,
                    ViewportHeight = 1000
                });

            // Initialize ReportService with all mocks
            _reportService = new ReportService(
                new HttpClient(),
                logger,
                _context,
                _openAiServiceMock.Object,
                _brokenLinksServiceMock.Object,
                _headingHierarchyServiceMock.Object,
                _colorSchemeServiceMock.Object,
                _dynamicSizingServiceMock.Object,
                _screenshotServiceMock.Object,
                _webScraperServiceMock.Object,
                _playwrightScraperServiceMock.Object,
                _popUpsServiceMock.Object,
                _animationServiceMock.Object,
                _audioServiceMock.Object,
                _scrollServiceMock.Object
            );
        }

        [Test]
        public async Task GenerateReportAsync_Returns_Empty_If_No_Issues_Found()
        {
            // Arrange
            var report = new Report { Url = "https://example.com" };

            _screenshotServiceMock
                .Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            _colorSchemeServiceMock
                .Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .ReturnsAsync(""); // No issues

            // Act
            var result = await _reportService.GenerateReportAsync(report);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GenerateReportAsync_Returns_Issue_If_Issue_Found()
        {
            // Arrange
            var report = new Report { Url = "https://example.com" };

            _screenshotServiceMock
                .Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            _colorSchemeServiceMock
                .Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .ReturnsAsync("Issue Found");

            // Act
            var result = await _reportService.GenerateReportAsync(report);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Message, Is.EqualTo("Issue Found"));
        }

        [Test]
        public async Task GenerateReportAsync_Calls_OpenAI_When_ScanMethod_Is_OpenAI()
        {
            // Arrange
            var report = new Report { Url = "https://example.com" };

            _openAiServiceMock
                .Setup(s => s.AnalyzeWithOpenAI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync("AI-generated issue");

            // Act
            await _reportService.GenerateReportAsync(report);

            // Assert
            _openAiServiceMock.Verify(
                s => s.AnalyzeWithOpenAI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task GenerateReportAsync_Skips_Category_With_Empty_ScanMethod()
        {
            // Arrange
            _context.DesignCategories.Add(new DesignCategory
            {
                Id = 99,
                Name = "EmptyScan",
                ScanMethod = "" // Deliberately invalid
            });
            _context.SaveChanges();

            var report = new Report { Url = "https://example.com" };

            // Act
            var result = await _reportService.GenerateReportAsync(report);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.All(r => r.CategoryId != 99));
        }

        [Test] 
        public async Task RunCustomAnalysisAsync_Returns_String_IfIssuesFound()
        {
            _screenshotServiceMock.Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());
            
            // Update this mock to include the screenshot task parameter
            _colorSchemeServiceMock.Setup(service => service.AnalyzeWebsiteColorsAsync(
                It.IsAny<Dictionary<string, object>>(), 
                It.IsAny<Task<byte[]>>()))
                .ReturnsAsync("Issue found");

            var result = await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>()); // Run custom analysis

            Assert.That(result,Is.Not.Null); // Assert that the result is not null
            Assert.That(result, Is.Not.Empty); // Assert that the result is not empty
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_Null_If_NoIssuesFound()
        {

            _screenshotServiceMock.Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());
            
            _colorSchemeServiceMock.Setup(s => s.AnalyzeWebsiteColorsAsync(
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Task<byte[]>>()))
                .ReturnsAsync(""); // Mock the color analysis to return no issues

            var result = await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>()); // Run custom analysis

            Assert.That(string.IsNullOrEmpty(result), Is.True); // Assert that the result is null or empty
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Calls_Correct_Category_Service()
        {
            _screenshotServiceMock.Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());
            await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "desc", new Dictionary<string, object>()); // Run custom analysis

            // Verify that the color scheme service was called once
            _colorSchemeServiceMock.Verify(service => service.AnalyzeWebsiteColorsAsync(
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Task<byte[]>>()), 
                Times.Once);
        }
        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

    }
}


