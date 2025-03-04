using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.DAL.Concrete;

namespace Uxcheckmate_Tests.Services
{
    [TestFixture]
    public class AxeCoreServiceTests
    {
        private Mock<UxCheckmateDbContext> _mockDbContext;
        private Mock<ILogger<AxeCoreService>> _mockLogger;
        private AxeCoreService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<UxCheckmateDbContext>();
            _mockLogger = new Mock<ILogger<AxeCoreService>>();
            _service = new AxeCoreService(_mockLogger.Object, _mockDbContext.Object);
        }

        [Test]
        public async Task AnalyzeAndSaveAccessibilityReport_Should_NotSave_When_NoViolationsExist()
        {
            // Arrange
            var fakeReport = new Report { Id = 2, Url = "https://example.com" };

            // Simulate empty Axe-core results
            var fakeAxeResults = new AxeCoreResults { Violations = new List<AxeViolation>() };

            // Act
            var result = await _service.AnalyzeAndSaveAccessibilityReport(fakeReport);

            // Assert
            _mockDbContext.Verify(db => db.AccessibilityIssues.AddRange(It.IsAny<IEnumerable<AccessibilityIssue>>()), Times.Never);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AnalyzeAndSaveAccessibilityReport_Should_SaveIssues_When_ViolationsExist()
        {
            // Arrange
            var fakeReport = new Report { Id = 2, Url = "https://example.com" };

            // Simulate empty Axe-core results
            var fakeAxeResults = new AxeCoreResults { Violations = new List<AxeViolation>()};
            // TO DO: Create Fake results
            /*        public string Id { get; set; }
        public string Impact { get; set; }
        public string Description { get; set; }
        public string Help { get; set; }
        public List<AxeNode> Nodes { get; set; }
        public List<string> WcagTags { get; set; }*/
            // Act
            var result = await _service.AnalyzeAndSaveAccessibilityReport(fakeReport);

            // Assert
            _mockDbContext.Verify(db => db.AccessibilityIssues.AddRange(It.IsAny<IEnumerable<AccessibilityIssue>>()), Times.Never);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AnalyzeAndSaveAccessibilityReport_Should_Assign_CorrectCategory()
        {
            // Arrange
            // Act
            // Assert
        }
    }
}
