using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    [TestFixture]
    public class PopupDetectionServiceTests
    {
        private Mock<ILogger<PopupDetectionService>> _mockLogger;
        private PopupDetectionService _service;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<PopupDetectionService>>();
            _service = new PopupDetectionService(_mockLogger.Object);
        }

        [Test]
        public async Task AnalyzePopupsAsync_MultiplePopups_ReturnsWarning()
        {
            // Arrange
            var html = @"
                <div class='popup'></div>
                <div role='dialog'></div>
            ";
            var jsList = new List<string> { "function openModal() { /* show popup */ }" };

            // Act
            var result = await _service.AnalyzePopupsAsync(html, jsList);

            // Assert
            Assert.That(result, Does.Contain("Multiple popups detected"));
        }

        [Test]
        public async Task AnalyzePopupsAsync_OnePopup_ReturnsEmpty()
        {
            // Arrange
            var html = "<div class='popup'></div>";
            var jsList = new List<string>();

            // Act
            var result = await _service.AnalyzePopupsAsync(html, jsList);

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }
    }
}
