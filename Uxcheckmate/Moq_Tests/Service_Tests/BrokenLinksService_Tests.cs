using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    [TestFixture]
    public class BrokenLinks_Tests
    {
        private BrokenLinksService _service;
        private Mock<HttpMessageHandler> _httpMessageHandler;
        private HttpClient _httpClient;
        private Mock<ILogger<BrokenLinksService>> _mockLogger;
        
        [SetUp]
        public void Setup()
        {
            _httpMessageHandler = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://example.com")
            };

            _mockLogger = new Mock<ILogger<BrokenLinksService>>();

            _service = new BrokenLinksService(_httpClient, _mockLogger.Object);
        }

        // Creates a test dictionary mimicking scraped HTML data.
        private Dictionary<string, object> TestScrapedData(List<string> links)
        {
            return new Dictionary<string, object>
            {
                { "headings", 3 },  
                { "paragraphs", 5 }, 
                { "images", 2 },    
                { "links", links }, 
                { "text_content", "Test text content" }, 
                { "fonts", new List<string> { "Arial", "Verdana" } } 
            };
        }

        // Tests that when only valid links are present, result is empty.
        [Test]
        public async Task ValidLinks_Return_Null()
        {
            var goodScrapedData = TestScrapedData(new List<string>
            {
                "https://example.com/page1",
                "https://example.com/page2"
            });

            // Mock HTTP responses so all links return 200 OK
            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Call the BrokenLinksService to analyze the links
            string result = await _service.BrokenLinkAnalysis("https://example.com", goodScrapedData);

            // Assert that no broken links were found, so result should be empty
            Assert.That(result, Is.Empty, "No broken links should be found.");
        }

        // Tests that when broken links are present, they are detected and returned.
        [Test]
        public async Task InvalidLinks_Return_String()
        {
            var badScrapedData = TestScrapedData(new List<string>
            {
                "https://thereisnowaythissiteexistsdfdsjfad.com",
                "https://hfdshfndsfuewfesf.org/"
            });

            // Mock HTTP responses for broken links (404 Not Found)
            _httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("broken")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound)); // 404

            // Call the BrokenLinksService to analyze the links
            string result = await _service.BrokenLinkAnalysis("https://example.com", badScrapedData);

            // Assert that broken links are detected and returned as a string
            Assert.That(result, Is.Not.Empty, "Broken links should be reported.");
            Assert.That(result, Does.Contain("https://thereisnowaythissiteexistsdfdsjfad.com"), "Broken link should be detected.");
        }

        // Tests that when no links are found on the page, the service returns null or an empty string.
        [Test]
        public async Task NoLinks_ReturnsNull()
        {
            var scrapedData = TestScrapedData(new List<string>()); // No links provided

            // Call the BrokenLinksService to analyze an empty link list
            string result = await _service.BrokenLinkAnalysis("https://example.com", scrapedData);

            // Assert that the result is null or an empty string
            Assert.That(result, Is.Null.Or.Empty, "Should return null or an empty string when no links are found.");
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }
    }
}