/*using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Tests.Controllers
{
    [TestFixture]
    public class AnalysisReportsControllerTests
    {
        private Mock<IOpenAiService> _mockOpenAiService;
        private Mock<IPa11yService> _mockPa11yService;
        private AnalysisReportsController _controller;

        [SetUp]
        public void SetUp()
        {
            // Initialize mocks
            _mockOpenAiService = new Mock<IOpenAiService>();
            _mockPa11yService = new Mock<IPa11yService>();

            // Create the controller with mocked dependencies
            _controller = new AnalysisReportsController(
                _mockOpenAiService.Object,
                _mockPa11yService.Object
            );
        }

        [Test]
        public async Task GetAnalysisReports_EmptyUrl_ReturnsBadRequest()
        {
            // Arrange: Define an empty URL input
            var emptyUrl = string.Empty;

            // Act: Call the controller's method with an empty URL
            var result = await _controller.GetAnalysisReports(emptyUrl);

            // Assert: Ensure the result is a BadRequestObjectResult (HTTP 400)
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>(), 
                "Expected a BadRequest when URL is empty.");

            // Extract the BadRequest result and verify it's not null
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "BadRequestObjectResult should not be null.");

            // Verify the error message matches the expected response
            Assert.That(badRequestResult.Value, Is.EqualTo("URL cannot be empty."),
                "The error message should indicate that the URL cannot be empty.");
        }

        [Test]
        public async Task GetAnalysisReports_NullUrl_ReturnsBadRequest()
        {
            // Arrange: Define a null URL input
            string nullUrl = null;

            // Act: Call the controller's method with a null URL
            var result = await _controller.GetAnalysisReports(nullUrl);

            // Assert: Ensure the result is a BadRequestObjectResult (HTTP 400)
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>(),
                "Expected a BadRequest when URL is null.");

            // Extract the BadRequest result and verify it's not null
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "BadRequestObjectResult should not be null.");

            // Verify the error message matches the expected response
            Assert.That(badRequestResult.Value, Is.EqualTo("URL cannot be empty."),
                "The error message should indicate that the URL cannot be empty.");
        }

        [Test]
        public async Task GetAnalysisReports_ValidUrl_ReturnsOkAndCombinedResults()
        {
            // Arrange: Define a valid URL input
            var validUrl = "http://example.com";

            // Mock UX analysis report response
            var mockUxReports = new List<Report>
            {
                new Report { ReportId = 1, Recommendations = "UX Recommendation" }
            };

            // Mock Pa11y accessibility report response
            var mockPa11yIssues = new List<Pa11yIssue>
            {
                new Pa11yIssue { Code = "WCAG2AA.Principle1.Guideline1_1.1_1_1.H37", Message = "Accessibility issue" }
            };

            // Configure the mocked OpenAI service to return the fake UX reports
            _mockOpenAiService
                .Setup(s => s.AnalyzeAndSaveUxReports(validUrl))
                .ReturnsAsync(mockUxReports);

            // Configure the mocked Pa11y service to return the fake accessibility issues
            _mockPa11yService
                .Setup(s => s.AnalyzeAndSaveAccessibilityReport(validUrl))
                .ReturnsAsync(mockPa11yIssues);

            // Act: Call the controller's method with a valid URL
            var result = await _controller.GetAnalysisReports(validUrl);

            // Assert: Ensure the result is an OkObjectResult (HTTP 200)
            Assert.That(result, Is.InstanceOf<OkObjectResult>(), 
                "Expected an OkObjectResult for a valid URL.");

            // Extract the OkObjectResult and verify it's not null
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Expected an OkObjectResult but got null.");

            // Ensure the response contains a valid object
            var responseValue = okResult.Value;
            Assert.That(responseValue, Is.Not.Null, "Expected responseValue to be non-null.");

            // Use reflection to access properties of the anonymous object returned by the controller
            var responseType = responseValue.GetType();
            var item1Property = responseType.GetProperty("Item1");
            var item2Property = responseType.GetProperty("Item2");

            // Ensure the anonymous type has the expected properties
            Assert.That(item1Property, Is.Not.Null, "Item1 property not found in response.");
            Assert.That(item2Property, Is.Not.Null, "Item2 property not found in response.");

            // Get values using reflection
            var uxReportsResult = item1Property.GetValue(responseValue);
            var pa11yReportsResult = item2Property.GetValue(responseValue);

            // Validate the returned UX reports match the expected mock data
            Assert.That(uxReportsResult, Is.EqualTo(mockUxReports), "UX reports should match the mock data.");

            // Validate the returned Pa11y issues match the expected mock data
            Assert.That(pa11yReportsResult, Is.EqualTo(mockPa11yIssues), "Pa11y issues should match the mock data.");
        }
    }
}
*/