using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework; 
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services; 

namespace ErrorPage_Tests
{
    public class ErrorPage_Tests
    {
        private Mock<ILogger<HomeController>> _mockLogger;
        private Mock<IOpenAiService> _mockOpenAiService;
        private Mock<UxCheckmateDbContext> _mockDbContext;
        private Mock<IAxeCoreService> _mockAxeCoreService;
        private Mock<IReportService> _mockReportService;
        private Mock<HttpClient> _mockHttpClient; 
        private Mock<PdfExportService> _mockpdfExportService;
        private Mock<IViewRenderService> _mockViewRenderService;
        private Mock<IScreenshotService> _mockScreenshotService;
        private Mock<IBackgroundTaskQueue> _mockBackgroundTaskQueue;
        private Mock<IServiceScopeFactory> _mockScopeFactory;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockOpenAiService = new Mock<IOpenAiService>();
            _mockAxeCoreService = new Mock<IAxeCoreService>();
            _mockDbContext = new Mock<UxCheckmateDbContext>();
            _mockReportService = new Mock<IReportService>();
            _mockHttpClient = new Mock<HttpClient>();
            _mockpdfExportService = new Mock<PdfExportService>();
            _mockScreenshotService = new Mock<IScreenshotService>();
            _mockViewRenderService = new Mock<IViewRenderService>();
            _mockBackgroundTaskQueue = new Mock<IBackgroundTaskQueue>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
        }

        [Test]
        public void Error404_ReturnsErrorPageView()
        {
            // Arrange
           var controller = new HomeController(_mockLogger.Object, _mockHttpClient.Object, _mockDbContext.Object, _mockOpenAiService.Object, _mockAxeCoreService.Object, _mockReportService.Object, _mockpdfExportService.Object, _mockScreenshotService.Object, _mockViewRenderService.Object,   _mockBackgroundTaskQueue.Object, _mockScopeFactory.Object);

            // Act
            var result = controller.ErrorPage() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("ErrorPage"));
        }
    }
}