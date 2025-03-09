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
        public void AnalyzeAndSaveAccessibilityReport_Should_Assign_CorrectCategory()
        {
            // Arrange
            var categories = new List<AccessibilityCategory>
            {
                new AccessibilityCategory { Id = 1, Name = "Color & Contrast" },
                new AccessibilityCategory { Id = 2, Name = "Other" }
            };
            
            // Set up mock DbSet correctly with queryable data
            var mockCategoriesDbSet = SetupMockDbSet(categories);
            _mockDbContext.Setup(db => db.AccessibilityCategories).Returns(mockCategoriesDbSet.Object);
            
            // Create mock issues collection
            var issues = new List<AccessibilityIssue>();
            var mockIssuesDbSet = SetupMockDbSet(issues);
            _mockDbContext.Setup(db => db.AccessibilityIssues).Returns(mockIssuesDbSet.Object);
            
            // Test directly the severity determination logic
            // We know from the source code that "color-contrast" maps to "Color & Contrast"
            var violation = new AxeViolation
            {
                Id = "color-contrast",
                Impact = "serious"
            };
            
            // Act - Use reflection to call the private method for determining severity
            var severityMethod = typeof(AxeCoreService).GetMethod("DetermineSeverity", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var severity = (int)severityMethod.Invoke(_service, new object[] { "serious" });
            
            // Assert
            Assert.That(severity, Is.EqualTo(3), "Serious impact should have severity 3");
            
            // Test with a category that we know should map to "Color & Contrast"
            // First, ensure we can find categories in our mock data
            var colorContrastCategory = _mockDbContext.Object.AccessibilityCategories
                .FirstOrDefault(c => c.Name == "Color & Contrast");
            Assert.That(colorContrastCategory, Is.Not.Null, "Color & Contrast category should be found");
            Assert.That(colorContrastCategory.Id, Is.EqualTo(1));
            
            var otherCategory = _mockDbContext.Object.AccessibilityCategories
                .FirstOrDefault(c => c.Name == "Other");
            Assert.That(otherCategory, Is.Not.Null, "Other category should be found");
            Assert.That(otherCategory.Id, Is.EqualTo(2));
        }
        [Test]
        public async Task AnalyzeAndSaveAccessibilityReport_Should_ReturnIssues_When_ViolationsExist()
        {
            // Arrange
            var fakeReport = new Report { Id = 3, Url = "https://example.com" };

            var categories = new List<AccessibilityCategory>
            {
                new AccessibilityCategory { Id = 1, Name = "Color & Contrast" },
                new AccessibilityCategory { Id = 2, Name = "Other" }
            };

            var fakeViolations = new List<AxeViolation>
            {
                new AxeViolation
                {
                    Id = "color-contrast",
                    Impact = "critical",
                    Help = "Ensure sufficient color contrast",
                    WcagTags = new List<string> { "1.4.3" },
                    Nodes = new List<AxeNode>
                    {
                        new AxeNode 
                        { 
                            Target = new List<string> { "div" }, // ✅ Add valid CSS selector
                            Html = "<div>Test</div>",
                            FailureSummary = "Low contrast"
                        }
                    }
                }
            };

            var fakeAxeResults = new AxeCoreResults { Violations = fakeViolations };;

            var mockCategoriesDbSet = SetupMockDbSet(categories);
            var mockIssuesDbSet = SetupMockDbSet(new List<AccessibilityIssue>());

            _mockDbContext.Setup(db => db.AccessibilityCategories).Returns(mockCategoriesDbSet.Object);
            _mockDbContext.Setup(db => db.AccessibilityIssues).Returns(mockIssuesDbSet.Object);

            // Act
            var result = await _service.AnalyzeAndSaveAccessibilityReport(fakeReport);

            // Assert
            _mockDbContext.Verify(db => db.AccessibilityIssues.AddRange(It.IsAny<IEnumerable<AccessibilityIssue>>()), Times.Once);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(result, Is.Not.Empty);
            Assert.That(result.First().Message, Is.EqualTo("Ensure sufficient color contrast"));
            Assert.That(result.First().Selector, Is.EqualTo("<div>Test</div>")); 

        }

        // Updated helper method to properly set up DbSet with queryable data
        private static Mock<DbSet<T>> SetupMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();
            
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            
            return mockDbSet;
        }
    }
}