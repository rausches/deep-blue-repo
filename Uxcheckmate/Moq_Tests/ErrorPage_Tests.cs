using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework; 
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services; 

namespace Moq_Tests.ErrorPage_Tests
{
    public class ErrorPage_Tests
    {
        private Mock<ILogger<HomeController>> _mockLogger;
        private Mock<IOpenAiService> _mockOpenAiService;

        private Mock<UxCheckmateDbContext> _mockDbContext;

        private Mock<IPa11yService> _mockPa11yService;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockOpenAiService = new Mock<IOpenAiService>();
            _mockPa11yService = new Mock<IPa11yService>();
            _mockDbContext = new Mock<UxCheckmateDbContext>();

        }

        [Test]
        public void Error404_ReturnsErrorPageView()
        {
            // Arrange
           var controller = new HomeController(_mockLogger.Object, _mockDbContext.Object, _mockOpenAiService.Object, _mockPa11yService.Object);

            // Act
            var result = controller.ErrorPage() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("ErrorPage"));
        }
    }
}
