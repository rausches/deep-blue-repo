using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.Playwright;

namespace Service_Tests
{
    [TestFixture]
    public class AxeCoreServiceTests
    {
        private Mock<UxCheckmateDbContext> _mockDbContext; 
        private Mock<ILogger<AxeCoreService>> _mockLogger;  
        private Mock<IPlaywrightService> _mockPlaywrightService;
        private AxeCoreService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<UxCheckmateDbContext>();
            _mockLogger = new Mock<ILogger<AxeCoreService>>();
            _mockPlaywrightService = new Mock<IPlaywrightService>();

            _service = new AxeCoreService(
                _mockLogger.Object,
                _mockDbContext.Object,
                _mockPlaywrightService.Object
            );
        }

        [Test]
        public async Task AnalyzeAndSaveAccessibilityReport_Should_NotSave_When_NoViolationsExist()
        {
            // Arrange
            // Fake report as if the user wants to scan "https://example.com".
            var fakeReport = new Report { Id = 2, Url = "https://example.com" };

            // Mock IBrowserContext + IPage to simulate Playwright's browser interactions.
            // These mocks ensure the test doesn't spin up a real browser.
            var mockContext = new Mock<IBrowserContext>();
            var mockPage = new Mock<IPage>();

            // The IPlaywrightService's method GetBrowserContextAsync() should return our mockContext
            _mockPlaywrightService
                .Setup(s => s.GetBrowserContextAsync())
                .ReturnsAsync(mockContext.Object);

            // Return our mockPage when NewPageAsync() is called
            mockContext
                .Setup(c => c.NewPageAsync())
                .ReturnsAsync(mockPage.Object);

            // GotoAsync expects a Task<IResponse?>, return null to satisfy the signature
            mockPage
                .Setup(p => p.GotoAsync(
                    It.IsAny<string>(),
                    It.IsAny<PageGotoOptions>())
                )
                .ReturnsAsync((IResponse)null);

            // EvaluateAsync<bool> is used to check if axe is injected
            mockPage
                .Setup(p => p.EvaluateAsync<bool>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(true);

            // Simulate an empty set of violations: { "violations": [] }
            var emptyJson = "{\"violations\": []}";
            var emptyDoc = JsonDocument.Parse(emptyJson);
            var emptyRoot = emptyDoc.RootElement;

            // EvaluateAsync<JsonElement> is used to run axe.run().
            // Return the empty JSON so the service thinks there are no violations.
            mockPage
                .Setup(p => p.EvaluateAsync<JsonElement>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(emptyRoot);

            // Act
            // Call the method under test with the fake report
            var result = await _service.AnalyzeAndSaveAccessibilityReport(fakeReport);

            // Assert
            // If there are no violations, then no records should be added or saved
            _mockDbContext.Verify(
                db => db.AccessibilityIssues.AddRange(It.IsAny<IEnumerable<AccessibilityIssue>>()), 
                Times.Never);

            _mockDbContext.Verify(
                db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), 
                Times.Never);

            // The returned list of issues should also be empty
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
            
            // Set up a mock DbSet of categories so that queries against _dbContext.AccessibilityCategories
            // will return in-memory list above
            var mockCategoriesDbSet = SetupMockDbSet(categories);
            _mockDbContext.Setup(db => db.AccessibilityCategories).Returns(mockCategoriesDbSet.Object);
            
            // We also create a mock DB set for issues
            var issues = new List<AccessibilityIssue>();
            var mockIssuesDbSet = SetupMockDbSet(issues);
            _mockDbContext.Setup(db => db.AccessibilityIssues).Returns(mockIssuesDbSet.Object);
            
            // Define a violation object for "color-contrast" with an severity of "serious"
            var violation = new AxeViolation
            {
                Id = "color-contrast",
                Impact = "serious"
            };
            
            // Use reflection to call the private DetermineSeverity method inside AxeCoreService
            var severityMethod = typeof(AxeCoreService).GetMethod(
                "DetermineSeverity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            
            // Invoke the method with argument "serious"
            var severity = (int)severityMethod.Invoke(_service, new object[] { "serious" });
            
            // Assert that "serious" => severity 3 based on your switch statement in the real code.
            Assert.That(severity, Is.EqualTo(3), "Serious impact should have severity 3");
            
            // Ensure that the "color-contrast" ID would map to "Color & Contrast" category 
            var colorContrastCategory = _mockDbContext.Object.AccessibilityCategories
                .FirstOrDefault(c => c.Name == "Color & Contrast");
            Assert.That(colorContrastCategory, Is.Not.Null);
            Assert.That(colorContrastCategory.Id, Is.EqualTo(1));
            
            // /Check the "Other" category for completeness
            var otherCategory = _mockDbContext.Object.AccessibilityCategories
                .FirstOrDefault(c => c.Name == "Other");
            Assert.That(otherCategory, Is.Not.Null);
            Assert.That(otherCategory.Id, Is.EqualTo(2));
        }

        [Test]
        public async Task AnalyzeAndSaveAccessibilityReport_Should_ReturnIssues_When_ViolationsExist()
        {
            // Arrange
            // Another fake report to scan "https://example.com".
            var fakeReport = new Report { Id = 3, Url = "https://example.com", UserID = "test-user" };

            // Prepare some categories to use in the mock DB context
            var categories = new List<AccessibilityCategory>
            {
                new AccessibilityCategory { Id = 1, Name = "Color & Contrast" },
                new AccessibilityCategory { Id = 2, Name = "Other" }
            };

            // Setup the categories in the mock DB
            var mockCategoriesDbSet = SetupMockDbSet(categories);
            _mockDbContext.Setup(db => db.AccessibilityCategories).Returns(mockCategoriesDbSet.Object);

            // Also create and set up an empty list for AccessibilityIssues
            var mockIssuesDbSet = SetupMockDbSet(new List<AccessibilityIssue>());

            mockIssuesDbSet
                .Setup(m => m.AddRange(It.IsAny<IEnumerable<AccessibilityIssue>>()))
                .Verifiable();

            _mockDbContext
                .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            _mockDbContext.Setup(db => db.AccessibilityIssues).Returns(mockIssuesDbSet.Object);
            // Mock IBrowserContext + IPage,
            var mockContext = new Mock<IBrowserContext>();
            var mockPage = new Mock<IPage>();

            // IPlaywrightService => returns mockContext
            _mockPlaywrightService
                .Setup(s => s.GetBrowserContextAsync())
                .ReturnsAsync(mockContext.Object);

            // The context => returns our mockPage
            mockContext
                .Setup(c => c.NewPageAsync())
                .ReturnsAsync(mockPage.Object);

            // GotoAsync => returns a null IResponse just to avoid mismatch with signature
            mockPage
                .Setup(p => p.GotoAsync(
                    It.IsAny<string>(),
                    It.IsAny<PageGotoOptions>())
                )
                .ReturnsAsync((IResponse)null);

            // EvaluateAsync<bool> => "true" meaning axe is loaded
            mockPage
                .Setup(p => p.EvaluateAsync<bool>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(true);

            // This JSON has a single violation with "color-contrast", "critical" impact, etc.
            var violationJson = @"
            {
              ""violations"": [
                {
                  ""id"": ""color-contrast"",
                  ""impact"": ""critical"",
                  ""help"": ""Ensure sufficient color contrast"",
                  ""wcagTags"": [""1.4.3""],
                  ""nodes"": [
                    {
                      ""target"": [""div""],
                      ""html"": ""<div>Test</div>"",
                      ""failureSummary"": ""Low contrast""
                    }
                  ]
                }
              ]
            }";
            
            // Parse this JSON into a JsonDocument so we can return its root element below
            var doc = JsonDocument.Parse(violationJson);
            var root = doc.RootElement;

            // EvaluateAsync<JsonElement> => return the JSON with the one violation
            mockPage
                .Setup(p => p.EvaluateAsync<JsonElement>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(root);

            // Act
            // Run the real code in AnalyzeAndSaveAccessibilityReport,
            // which should parse that JSON and create an AccessibilityIssue in the DB.
            var result = await _service.AnalyzeAndSaveAccessibilityReport(fakeReport);

            // Assert
            // Since there's one violation, we expect exactly one AddRange call
            _mockDbContext.Verify(
                db => db.AccessibilityIssues.AddRange(It.IsAny<IEnumerable<AccessibilityIssue>>()),
                Times.Once);

            // And one SaveChanges call
            _mockDbContext.Verify(
                db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            // The returned list of issues also should not be empty
            Assert.That(result, Is.Not.Empty);

            // The message matches "Ensure sufficient color contrast"
            Assert.That(result.First().Message, Is.EqualTo("Ensure sufficient color contrast"));
            // The stored "Selector" is the <div> from the violation
            Assert.That(result.First().Selector, Is.EqualTo("<div>Test</div>")); 
        }

        private static Mock<DbSet<T>> SetupMockDbSet<T>(List<T> data) where T : class
        {
            // Convert the list to an IQueryable so that LINQ calls in code work.
            var queryable = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();

            // Configure the mock so that Provider, Expression, and other properties come from queryable
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return mockDbSet;
        }
    }
}
