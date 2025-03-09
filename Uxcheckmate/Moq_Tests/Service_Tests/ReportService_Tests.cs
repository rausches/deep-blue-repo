using NUnit.Framework;
using Moq; 
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
        private readonly ILogger<WebScraperService> webScraperLogger; 

        [SetUp]
        public void Setup()
        {
            // Configure an in-memory database for testing
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) // Unique database name for each test
                .Options;

            var context = new UxCheckmateDbContext(options);

            // Seed the in-memory database with test data
            context.DesignCategories.AddRange(
                new DesignCategory { Id = 1, Name = "Color Scheme", ScanMethod = "Custom" },
                new DesignCategory { Id = 2, Name = "AI Analysis", ScanMethod = "OpenAI" }
            );
            context.SaveChanges(); // Save changes to the database

            // Create mock loggers for ReportService and WebScraperService
            var logger = Mock.Of<ILogger<ReportService>>();
            var webScraperLogger = Mock.Of<ILogger<WebScraperService>>();

            // Initialize mock services
            _openAiServiceMock = new Mock<IOpenAiService>();
            _colorSchemeServiceMock = new Mock<IColorSchemeService>();

            // Instantiate the ReportService with mocked dependencies
            _reportService = new ReportService(
                new HttpClient(),
                logger,
                context,
                _openAiServiceMock.Object,
                Mock.Of<IBrokenLinksService>(), 
                Mock.Of<IHeadingHierarchyService>(),
                _colorSchemeServiceMock.Object, 
                Mock.Of<IDynamicSizingService>(), 
                webScraperLogger 
            );
        }

        [Test]
        public async Task GenerateReportAsync_Returns_Null_If_No_Issues_Found()
        {
            var report = new Report { Url = "https://example.com" }; // Create a report with a sample URL
            _colorSchemeServiceMock.Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<string>())).ReturnsAsync(""); // Mock the color analysis to return no issues

            var result = await _reportService.GenerateReportAsync(report); // Generate the report

            Assert.That(result, Is.Empty); // Assert that the result is empty
        }

        [Test]
        public async Task GenerateReportAsync_Returns_Issues_If_Issues_Found()
        {
            var report = new Report { Url = "https://example.com" }; // Create a report with a sample URL
            _colorSchemeServiceMock.Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<string>())).ReturnsAsync("Issue found"); // Mock the color analysis to return an issue

            var result = await _reportService.GenerateReportAsync(report); // Generate the report

            Assert.That(result, Is.Not.Null); // Assert that the result is not null
            Assert.That(result.Count, Is.EqualTo(1)); // Assert that the result contains one issue
        }

        [Test]
        public async Task GenerateReportAsync_Calls_OpenAiService_If_ScanMethod_Is_OpenAI()
        {
            var report = new Report { Url = "https://example.com" }; // Create a report with a sample URL
            _openAiServiceMock.Setup(s => s.AnalyzeWithOpenAI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync("AI-generated issue"); // Mock the OpenAI analysis to return an issue

            await _reportService.GenerateReportAsync(report); // Generate the report

            // Verify that the OpenAI service was called at least once
            _openAiServiceMock.Verify(s => s.AnalyzeWithOpenAI(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task GenerateReportAsync_Calls_CustomAnalysis_If_ScanMethod_Is_Custom()
        {
            var report = new Report { Url = "https://example.com" }; // Create a report with a sample URL
            _colorSchemeServiceMock.Setup(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<string>())).ReturnsAsync("Custom Issue"); // Mock the color analysis to return a custom issue

            await _reportService.GenerateReportAsync(report); // Generate the report

            // Verify that the color scheme service was called once
            _colorSchemeServiceMock.Verify(s => s.AnalyzeWebsiteColorsAsync(It.IsAny<string>()), Times.Once);
        }

        [Test] 
        public async Task GenerateReportAsync_Returns_Null_If_ScanMethod_Is_Empty()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var context = new UxCheckmateDbContext(options);
            context.DesignCategories.Add(new DesignCategory { Id = 99, Name = "EmptyScan", ScanMethod = "" }); // Add a design category with an empty scan method
            context.SaveChanges();

            var webScraperLoggerMock = Mock.Of<ILogger<WebScraperService>>(); // Mock logger for WebScraperService

            // Instantiate the ReportService with the new context and mocked dependencies
            var service = new ReportService(
                new HttpClient(),
                Mock.Of<ILogger<ReportService>>(),
                context,
                Mock.Of<IOpenAiService>(),
                Mock.Of<IBrokenLinksService>(),
                Mock.Of<IHeadingHierarchyService>(),
                Mock.Of<IColorSchemeService>(),
                Mock.Of<IDynamicSizingService>(),
                webScraperLoggerMock
            );

            var report = new Report { Url = "https://example.com" }; // Create a report with a sample URL

            // Act
            var result = await service.GenerateReportAsync(report); // Generate the report

            // Assert
            Assert.That(result, Is.Empty); // Assert that the result is empty
        }

        [Test] 
        public async Task RunCustomAnalysisAsync_Returns_String_IfIssuesFound()
        {
            _colorSchemeServiceMock.Setup(service => service.AnalyzeWebsiteColorsAsync(It.IsAny<string>())).ReturnsAsync("Issue found"); // Mock the color analysis to return an issue

            var result = await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>()); // Run custom analysis

            Assert.That(result,Is.Not.Null); // Assert that the result is not null
            Assert.That(result, Is.Not.Empty); // Assert that the result is not empty
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Returns_Null_If_NoIssuesFound()
        {
            _colorSchemeServiceMock.Setup(service => service.AnalyzeWebsiteColorsAsync(It.IsAny<string>())).ReturnsAsync(""); // Mock the color analysis to return no issues

            var result = await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "description", new Dictionary<string, object>()); // Run custom analysis

            Assert.That(string.IsNullOrEmpty(result), Is.True); // Assert that the result is null or empty
        }

        [Test]
        public async Task RunCustomAnalysisAsync_Calls_Correct_Category_Service()
        {
            await _reportService.RunCustomAnalysisAsync("url", "Color Scheme", "desc", new Dictionary<string, object>()); // Run custom analysis

            // Verify that the color scheme service was called once
            _colorSchemeServiceMock.Verify(service => service.AnalyzeWebsiteColorsAsync(It.IsAny<string>()), Times.Once);
        }
    }
}


