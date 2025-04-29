/*using NUnit.Framework;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using System.Collections.Generic;

namespace Uxcheckmate_Tests
{
    [TestFixture]
    public class ErrorHandlingTests
    {
        // PlaywrightScraperService - exception is thrown and logged
        [Test]
        public void ScrapeEverythingAsync_WhenExceptionThrown_LogsAndRethrows()
        {
            var playwrightMock = new Mock<IPlaywrightService>();
            var loggerMock = new Mock<ILogger<PlaywrightScraperService>>();
            var layoutMock = new Mock<ILayoutParsingService>();

            playwrightMock.Setup(p => p.GetBrowserAsync()).ThrowsAsync(new Exception("Playwright error"));

            var service = new PlaywrightScraperService(playwrightMock.Object, loggerMock.Object, layoutMock.Object);

            var ex = Assert.ThrowsAsync<Exception>(() => service.ScrapeEverythingAsync("https://example.com"));
            Assert.That(ex.Message, Does.Contain("Playwright error"));
        }

        // Report Generation - failed service returns error message and next analysis continues
        [Test]
        public async Task GenerateReportAsync_Animation_PartialFailureContinues()
        {
            // Arrange: create all necessary service mocks
            var loggerMock = new Mock<ILogger<ReportService>>();
            var scraperMock = new Mock<IPlaywrightScraperService>();
            var dbLoggerMock = new Mock<ILogger<UxCheckmateDbContext>>();
            var brokenMock = new Mock<IBrokenLinksService>();
            var headingMock = new Mock<IHeadingHierarchyService>();
            var colorMock = new Mock<IColorSchemeService>();
            var mobileMock = new Mock<IMobileResponsivenessService>();
            var screenshotMock = new Mock<IScreenshotService>();
            var popupMock = new Mock<IPopUpsService>();
            var animationMock = new Mock<IAnimationService>();
            var audioMock = new Mock<IAudioService>();
            var scrollMock = new Mock<IScrollService>();
            var fMock = new Mock<IFPatternService>();
            var zMock = new Mock<IZPatternService>();
            var symMock = new Mock<ISymmetryService>();
            var openAiMock = new Mock<IOpenAiService>();

            // Set up in-memory database context
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new UxCheckmateDbContext(options);

            // Add two categories: one will succeed, one will fail
            context.DesignCategories.AddRange(
                new DesignCategory { Id = 1, Name = "Broken Links", ScanMethod = "Custom" },
                new DesignCategory { Id = 2, Name = "Animation", ScanMethod = "Custom" }
            );
            var report = new Report { Id = 1, Url = "https://example.com" };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Set up successful scraping
            var scraped = new ScrapedContent { Url = report.Url, HtmlContent = "<html></html>" };
            scraperMock.Setup(s => s.ScrapeEverythingAsync(It.IsAny<string>())).ReturnsAsync(scraped);

            // Set up "Broken Links" service to succeed
            brokenMock.Setup(b => b.BrokenLinkAnalysis(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync("Broken link found");

            // Set up "Animation" service to throw an exception
            animationMock.Setup(c => c.RunAnimationAnalysisAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .ThrowsAsync(new Exception("Animation failure"));

            // Create ReportService with all mocks
            var service = new ReportService(
                new HttpClient(), loggerMock.Object, context, openAiMock.Object,
                brokenMock.Object, headingMock.Object, colorMock.Object, mobileMock.Object,
                screenshotMock.Object, scraperMock.Object, popupMock.Object, animationMock.Object,
                audioMock.Object, scrollMock.Object, fMock.Object, zMock.Object, symMock.Object
            );

            // Act: attempt to generate the report
            var result = await service.GenerateReportAsync(report);

            // Assert: ensure that only the successful category was added
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Message, Does.Contain("Broken link"));
        }

        [Test]
        public async Task GenerateReportAsync_Audio_PartialFailureContinues()
        {
            // Arrange: create all necessary service mocks
            var loggerMock = new Mock<ILogger<ReportService>>();
            var scraperMock = new Mock<IPlaywrightScraperService>();
            var dbLoggerMock = new Mock<ILogger<UxCheckmateDbContext>>();
            var brokenMock = new Mock<IBrokenLinksService>();
            var headingMock = new Mock<IHeadingHierarchyService>();
            var colorMock = new Mock<IColorSchemeService>();
            var mobileMock = new Mock<IMobileResponsivenessService>();
            var screenshotMock = new Mock<IScreenshotService>();
            var popupMock = new Mock<IPopUpsService>();
            var animationMock = new Mock<IAnimationService>();
            var audioMock = new Mock<IAudioService>();
            var scrollMock = new Mock<IScrollService>();
            var fMock = new Mock<IFPatternService>();
            var zMock = new Mock<IZPatternService>();
            var symMock = new Mock<ISymmetryService>();
            var openAiMock = new Mock<IOpenAiService>();

            // Set up in-memory database context
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new UxCheckmateDbContext(options);

            // Add two categories: one will succeed, one will fail
            context.DesignCategories.AddRange(
                new DesignCategory { Id = 1, Name = "Broken Links", ScanMethod = "Custom" },
                new DesignCategory { Id = 2, Name = "Animation", ScanMethod = "Custom" }
            );
            var report = new Report { Id = 1, Url = "https://example.com" };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Set up successful scraping
            var scraped = new ScrapedContent { Url = report.Url, HtmlContent = "<html></html>" };
            scraperMock.Setup(s => s.ScrapeEverythingAsync(It.IsAny<string>())).ReturnsAsync(scraped);

            // Set up "Broken Links" service to succeed
            brokenMock.Setup(b => b.BrokenLinkAnalysis(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync("Broken link found");

            // Set up "Audio" service to throw an exception
            audioMock.Setup(c => c.RunAudioAnalysisAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .ThrowsAsync(new Exception("Animation failure"));

            // Create ReportService with all mocks
            var service = new ReportService(
                new HttpClient(), loggerMock.Object, context, openAiMock.Object,
                brokenMock.Object, headingMock.Object, colorMock.Object, mobileMock.Object,
                screenshotMock.Object, scraperMock.Object, popupMock.Object, animationMock.Object,
                audioMock.Object, scrollMock.Object, fMock.Object, zMock.Object, symMock.Object
            );

            // Act: attempt to generate the report
            var result = await service.GenerateReportAsync(report);

            // Assert: ensure that only the successful category was added
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Message, Does.Contain("Broken link"));
        }

        [Test]
        public async Task GenerateReportAsync_ColorScheme_PartialFailureContinues()
        {
            // Arrange: create all necessary service mocks
            var loggerMock = new Mock<ILogger<ReportService>>();
            var scraperMock = new Mock<IPlaywrightScraperService>();
            var dbLoggerMock = new Mock<ILogger<UxCheckmateDbContext>>();
            var brokenMock = new Mock<IBrokenLinksService>();
            var headingMock = new Mock<IHeadingHierarchyService>();
            var colorMock = new Mock<IColorSchemeService>();
            var mobileMock = new Mock<IMobileResponsivenessService>();
            var screenshotMock = new Mock<IScreenshotService>();
            var popupMock = new Mock<IPopUpsService>();
            var animationMock = new Mock<IAnimationService>();
            var audioMock = new Mock<IAudioService>();
            var scrollMock = new Mock<IScrollService>();
            var fMock = new Mock<IFPatternService>();
            var zMock = new Mock<IZPatternService>();
            var symMock = new Mock<ISymmetryService>();
            var openAiMock = new Mock<IOpenAiService>();

            // Set up in-memory database context
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new UxCheckmateDbContext(options);

            // Add two categories: one will succeed, one will fail
            context.DesignCategories.AddRange(
                new DesignCategory { Id = 1, Name = "Broken Links", ScanMethod = "Custom" },
                new DesignCategory { Id = 2, Name = "Color Scheme", ScanMethod = "Custom" }
            );
            var report = new Report { Id = 1, Url = "https://example.com" };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Set up successful scraping
            var scraped = new ScrapedContent { Url = report.Url, HtmlContent = "<html></html>" };
            scraperMock.Setup(s => s.ScrapeEverythingAsync(It.IsAny<string>())).ReturnsAsync(scraped);

            // Set up "Broken Links" service to succeed
            brokenMock.Setup(b => b.BrokenLinkAnalysis(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync("Broken link found");

            // Set up "Color Scheme" service to throw an exception
            colorMock.Setup(c => c.AnalyzeWebsiteColorsAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<Task<byte[]>>()))
                .ThrowsAsync(new Exception("Color failure"));

            // Create ReportService with all mocks
            var service = new ReportService(
                new HttpClient(), loggerMock.Object, context, openAiMock.Object,
                brokenMock.Object, headingMock.Object, colorMock.Object, mobileMock.Object,
                screenshotMock.Object, scraperMock.Object, popupMock.Object, animationMock.Object,
                audioMock.Object, scrollMock.Object, fMock.Object, zMock.Object, symMock.Object
            );

            // Act: attempt to generate the report
            var result = await service.GenerateReportAsync(report);

            // Assert: ensure that only the successful category was added
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Message, Does.Contain("Broken link"));
        }

        // Helper: simulates HttpClient failure
        public class FailingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("Simulated failure");
            }
        }
    }
}
*/