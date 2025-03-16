using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Main.Tests
{
    [TestFixture]
    public class PlaywrightServiceTests
    {
        private Mock<ILogger<PlaywrightService>> _loggerMock;
        private PlaywrightService _playwrightService;

        [SetUp]
        public void Setup()
        {
            // Initialize the logger mock and the PlaywrightService instance before each test
            _loggerMock = new Mock<ILogger<PlaywrightService>>();
            _playwrightService = new PlaywrightService(_loggerMock.Object);
        }

        [Test]
        public async Task GetBrowserAsync_ShouldInitializePlaywrightAndBrowser()
        {
            // Act: Call GetBrowserAsync to initialize Playwright and the browser
            var browser = await _playwrightService.GetBrowserAsync();

            // Assert: Verigy that the browser instance is not null and the log messages were called
            Assert.That(browser, Is.Not.Null);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Playwright instance created.")), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Playwright browser instance created.")), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task CloseBrowserAsync_ShouldCloseBrowserAndDisposePlaywright()
        {
            // Arrange: Initialize the browser by calling GetBrowserAsync
            await _playwrightService.GetBrowserAsync();

            // Act: Call CloseBrowserAsync to close the browser and dispose Playwright
            await _playwrightService.CloseBrowserAsync();

            // Assert: Verify that the log messages were called
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Playwright browser instance closed.")), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Playwright instance disposed.")), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task Dispose_ShouldDisposePlaywrightInstance()
        {
            // Arrange: Create a new PlaywrightService instance with a logger mock
            var mockLogger = new Mock<ILogger<PlaywrightService>>();
            var service = new PlaywrightService(mockLogger.Object);

            // Act - First initialize Playwright by calling GetBrowserAsync, 
            // which ensures _playwright is not null.
            await service.GetBrowserAsync();

            // Call Dispose to trigger the logger and disposal
            service.Dispose();

            // Assert - Verify that the log messages were called
            mockLogger.Verify(
                logger => logger.Log<It.IsAnyType>(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Playwright disposed on service shutdown.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }


        [TearDown]
        public void TearDown()
        {
            // Ensure the Playwright service is disposed after each test
            _playwrightService.Dispose();
        }
    }
}