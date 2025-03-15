/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Main.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests : IDisposable
    {
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<HttpClient> _httpClientMock;
        private Mock<UxCheckmateDbContext> _dbContextMock;
        private Mock<IOpenAiService> _openAiServiceMock;
        private Mock<IReportService> _reportServiceMock;
        private Mock<IAxeCoreService> _axeCoreServiceMock;
        private Mock<PdfExportService> _pdfExportServiceMock;
        private HomeController _controller;
        private Mock<DbSet<Report>> _mockReportDbSet;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _httpClientMock = new Mock<HttpClient>();
            _dbContextMock = new Mock<UxCheckmateDbContext>(MockBehavior.Loose);
            _openAiServiceMock = new Mock<IOpenAiService>();
            _axeCoreServiceMock = new Mock<IAxeCoreService>();
            _reportServiceMock = new Mock<IReportService>();
            _pdfExportServiceMock = new Mock<PdfExportService>();

            // Setup mock DbContext and DbSet
            _mockReportDbSet = CreateMockDbSet<Report>(new List<Report>());
            _dbContextMock.Setup(x => x.Reports).Returns(_mockReportDbSet.Object);
            _dbContextMock.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            _controller = new HomeController(
                _loggerMock.Object,
                _httpClientMock.Object,
                _dbContextMock.Object,
                _openAiServiceMock.Object,
                _axeCoreServiceMock.Object,
                _reportServiceMock.Object,
                _pdfExportServiceMock.Object
            );

            // Setup HttpContext for controller
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task GetSortedIssues_ValidId_ReturnsSortedResults()
        {
            // Arrange
            int reportId = 1;
            var designCategories = new List<DesignCategory>
            {
                new DesignCategory { Id = 1, Name = "Layout" },
                new DesignCategory { Id = 2, Name = "Color" }
            };

            var accessibilityCategories = new List<AccessibilityCategory>
            {
                new AccessibilityCategory { Id = 1, Name = "Contrast" },
                new AccessibilityCategory { Id = 2, Name = "Alt Text" }
            };

            var designIssues = new List<DesignIssue>
            {
                new DesignIssue { Id = 1, Severity = 3, CategoryId = 1, Category = designCategories[0] },
                new DesignIssue { Id = 2, Severity = 1, CategoryId = 2, Category = designCategories[1] }
            };

            var accessibilityIssues = new List<AccessibilityIssue>
            {
                new AccessibilityIssue { Id = 1, Severity = 2, CategoryId = 1, Category = accessibilityCategories[0] },
                new AccessibilityIssue { Id = 2, Severity = 3, CategoryId = 2, Category = accessibilityCategories[1] }
            };

            var report = new Report
            {
                Id = reportId,
                Url = "https://test.com",
                Date = DateOnly.FromDateTime(DateTime.Now),
                DesignIssues = designIssues,
                AccessibilityIssues = accessibilityIssues
            };

            // Mock the reports DbSet to return our test report
            var mockReports = new List<Report> { report };
            var mockDbSet = CreateMockDbSet(mockReports);
            _dbContextMock.Setup(x => x.Reports).Returns(mockDbSet.Object);

            // Mock the Include expressions for eager loading
            _dbContextMock.Setup(x => x.Reports.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Report, object>>>()))
                .Returns(mockDbSet.Object);

            // Mock the ThenInclude expressions for nested eager loading
            mockDbSet.Setup(x => x.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Report, object>>>()))
                .Returns(mockDbSet.Object);

            // Simulate finding the report by ID
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Report, bool>>>(), default))
                .ReturnsAsync(report);

            // Mock view rendering
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewResult = new Mock<ViewEngineResult>();
            var mockView = new Mock<IView>();

            mockViewEngine.Setup(x => x.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(mockViewResult.Object);
            mockViewResult.Setup(x => x.Success).Returns(true);
            mockViewResult.Setup(x => x.View).Returns(mockView.Object);

            mockView.Setup(x => x.RenderAsync(It.IsAny<ViewContext>()))
                .Returns(Task.CompletedTask);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);

            _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;

            // Act
            var result = await _controller.GetSortedIssues(reportId, "severity-high-low");

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            var jsonResult = result as JsonResult;
            Assert.That(jsonResult, Is.Not.Null);
            Assert.That(jsonResult.Value, Is.Not.Null);

            dynamic resultValue = jsonResult.Value;
            Assert.That(resultValue.designHtml, Is.Not.Null);
            Assert.That(resultValue.designHtml.ToString(), Is.Not.Empty);
            Assert.That(resultValue.accessibilityHtml, Is.Not.Null);
            Assert.That(resultValue.accessibilityHtml.ToString(), Is.Not.Empty);
        }

        [Test]
        public async Task GetSortedIssues_InvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidReportId = 999;
            
            // Setup DbSet to return null for non-existent report
            _mockReportDbSet.Setup(x => x.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Report, object>>>()))
                .Returns(_mockReportDbSet.Object);
            _mockReportDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Report, bool>>>(), default))
                .ReturnsAsync((Report)null);

            // Act
            var result = await _controller.GetSortedIssues(invalidReportId, "category");

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public void RenderViewAsync_ValidView_ReturnsHtml()
        {
            // Arrange
            var model = new List<DesignIssue> { new DesignIssue { Id = 1, Severity = 3 } };
            var viewName = "_DesignIssuesPartial";
            
            // Mock view engine and view rendering
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewResult = new Mock<ViewEngineResult>();
            var mockView = new Mock<IView>();

            mockViewEngine.Setup(x => x.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(mockViewResult.Object);
            mockViewResult.Setup(x => x.Success).Returns(true);
            mockViewResult.Setup(x => x.View).Returns(mockView.Object);

            mockView.Setup(x => x.RenderAsync(It.IsAny<ViewContext>()))
                .Callback<ViewContext>(vc => {
                    // Write to the output stream to simulate rendering
                    var writer = vc.Writer as StringWriter;
                    writer.Write("<div>Test HTML</div>");
                })
                .Returns(Task.CompletedTask);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);

            _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;

            // Act
            var renderTask = ControllerExtensions.RenderViewAsync(_controller, viewName, model);
            Assert.That(renderTask, Is.Not.Null);
            renderTask.Wait();
            var result = renderTask.Result;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Is.EqualTo("<div>Test HTML</div>"));
        }

        [Test]
        public void RenderViewAsync_InvalidView_ThrowsException()
        {
            // Arrange
            var model = new List<DesignIssue> { new DesignIssue { Id = 1, Severity = 3 } };
            var invalidViewName = "NonExistentView";

            // Setup view engine to fail finding the view
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewResult = new Mock<ViewEngineResult>();

            mockViewEngine.Setup(x => x.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(mockViewResult.Object);
            mockViewResult.Setup(x => x.Success).Returns(false);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);

            _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => 
                await ControllerExtensions.RenderViewAsync(_controller, invalidViewName, model));
            
            Assert.That(ex.Message, Does.Contain("not found"));
        }

        // Helper method to create mock DbSet with LINQ queryable operations
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryableData = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();

            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

            return mockDbSet;
        }
    }
}
*/