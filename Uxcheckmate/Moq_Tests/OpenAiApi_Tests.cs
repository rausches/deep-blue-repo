using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Text;
using System.Text.Json;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using System.Collections.Generic;

namespace Moq_Tests
{
    [TestFixture]
    public class OpenAiService_Tests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private Mock<ILogger<OpenAiService>> _mockLogger;
        private OpenAiService _openAiService;

        [SetUp]
        public void Setup()
        {
            // Setup the mocked HttpMessageHandler.
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockLogger = new Mock<ILogger<OpenAiService>>();

            _openAiService = new OpenAiService(_httpClient, _mockLogger.Object);
        }

        [Test]
        public async Task AnalyzeWithOpenAI_NoIssuesFound_ReturnsEmptyString()
        {
            // Arrange: Setup a fake response that returns "No significant issues found" from the API.
            var fakeApiResponse = new OpenAiResponse
            {
                Choices = new List<Choice>
                {
                    new Choice { Message = new Message { Content = "No significant issues found" } }
                }
            };
            var fakeResponseJson = JsonSerializer.Serialize(fakeApiResponse);

            // Setup the HttpMessageHandler to return the fake response.
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeResponseJson, Encoding.UTF8, "application/json")
                });

            // Create a sample scrapedData dictionary.
            var scrapedData = new Dictionary<string, object>
            {
                { "headings", 5 },
                { "images", 10 },
                { "links", 8 },
                { "fonts", new List<string> { "Arial", "Roboto", "Verdana" } },
                { "text_content", "This is sample text for testing purposes." }
            };

            // Act
            var result = await _openAiService.AnalyzeWithOpenAI(
                "http://example.com", 
                "Test Category", 
                "Test Description", 
                scrapedData);

            // Assert: When "No significant issues found" is returned, the service should return an empty string.
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task AnalyzeWithOpenAI_IssuesFound_ReturnsAIResponseText()
        {
            // Arrange: Setup a fake response that returns an issue message.
            var fakeApiResponse = new OpenAiResponse
            {
                Choices = new List<Choice>
                {
                    new Choice { Message = new Message { Content = "Issue detected: Button is not accessible." } }
                }
            };
            var fakeResponseJson = JsonSerializer.Serialize(fakeApiResponse);

            // Setup the HttpMessageHandler to return the fake response.
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeResponseJson, Encoding.UTF8, "application/json")
                });

            // Create a sample scrapedData dictionary.
            var scrapedData = new Dictionary<string, object>
            {
                { "headings", 3 },
                { "images", 5 },
                { "links", 2 },
                { "fonts", new List<string> { "Helvetica", "Arial" } },
                { "text_content", "Sample text content." }
            };

            // Act
            var result = await _openAiService.AnalyzeWithOpenAI(
                "http://example.com", 
                "Test Category", 
                "Test Description", 
                scrapedData);

            // Assert: The returned response should match the content from the fake API response.
            Assert.That(result, Is.EqualTo("Issue detected: Button is not accessible."));
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }
    }

    // Helper classes to match the OpenAiResponse structure in your code.
    public class OpenAiResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }
}
