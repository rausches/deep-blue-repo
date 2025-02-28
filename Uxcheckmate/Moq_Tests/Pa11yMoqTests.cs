using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Moq_Tests
{
    [TestFixture]
    public class Pa11yMoqTests
    {
        private WebScraperService _scraperService;
        private ILogger<AccessibilityController> _logger;
        // Having NUnit running issues
        #pragma warning disable NUnit1032
        private AccessibilityController _controller;
        #pragma warning restore NUnit1032

        [SetUp]
        public void Setup()
        {
            // Use Moq to create a fake HttpMessageHandler that returns a fixed response.
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html><body><h1>Test</h1></body></html>")
                })
                .Verifiable();

            // Create fake http
            var fakeHttpClient = new HttpClient(handlerMock.Object);
            // Setup webscraper with fake http
            _scraperService = new WebScraperService(fakeHttpClient);
            // Use a null logger.
            _logger = NullLogger<AccessibilityController>.Instance;
            // Setting up controller
            _controller = new AccessibilityController(_scraperService, _logger);
        }
       [TearDown]
        public void TearDown()
        {
            // Opening up resources
            _controller = null;
        }
        // Checking if handles blank URL
        [Test]
        public async Task CheckAccessibility_ReturnsProperOutput_UsingExampleCom()
        {
            // Create path to pa11y JS code
            var pa11yJsPath = Path.Combine(Directory.GetCurrentDirectory(), "runPa11y.js");
            string output = "console.log(JSON.stringify({ issues: [ { code: \"H57.2\", message: \"The html element should have a lang attribute\", selector: \"html\" } ] })); process.exit(0);";
            File.WriteAllText(pa11yJsPath, output);     
            // Using example.com which should have one error in the way it is listed aboe
            var targetUrl = "https://example.com";
            var result = await _controller.CheckAccessibility(targetUrl);
            // Delete path
            File.Delete(pa11yJsPath);
            // Checking object it okay
            Assert.That(result, Is.InstanceOf<OkObjectResult>(), "Expected an OkObjectResult.");
            var okResult = result as OkObjectResult;
            // Grabbing pa11y results
            var pa11yResult = okResult!.Value as Pa11yResult;
            Assert.That(pa11yResult, Is.Not.Null, "The result should be of type Pa11yResult.");
            // Check if it has that one issue
            Assert.That(pa11yResult!.Issues.Count, Is.EqualTo(1), "Expected 1 issue from example.com");
            // Used this for debugging, keeping in case example.com changes
            // In which case I will run pa11y from cmd
            // And check if issues are being grabbed properly again
            /**
            foreach (var issue in pa11yResult.Issues)
            {
                Console.WriteLine("Issue:");
                Console.WriteLine($"  Code: {issue.Code}");
                Console.WriteLine($"  Message: {issue.Message}");
                Console.WriteLine($"  Selector: {issue.Selector}");
                Console.WriteLine();
            }
            */
        }
    }
}