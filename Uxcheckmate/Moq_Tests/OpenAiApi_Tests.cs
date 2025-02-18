using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Uxcheckmate_Main.Models; 
using Uxcheckmate_Main.Services;

namespace Moq_Tests.OpenAiApi_Tests
{
    [TestFixture]
    public class OpenAiApi_Tests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<ILogger<OpenAiService>> _loggerMock;
        private UxCheckmateDbContext _dbContext;
        private OpenAiService _openAiService;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _loggerMock = new Mock<ILogger<OpenAiService>>();
            
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _dbContext = new UxCheckmateDbContext(options);
            _openAiService = new OpenAiService(_httpClient, _loggerMock.Object, _dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose HttpClient and dbcontext after each test
            _httpClient.Dispose();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AnalyzeAndSaveUxReports_ReturnsListOfReports()
        {
            // Arrange
            var mockResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = 
                                "### Fonts\n- Too many fonts used.\n" +
                                "### Text Structure\n- Large text blocks found."
                        }
                    }
                }
            };

            var responseJson = JsonSerializer.Serialize(mockResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var reports = await _openAiService.AnalyzeAndSaveUxReports("https://example.com");

            // Assert
            Assert.That(reports, Is.Not.Null);
            Assert.That(reports.Count, Is.EqualTo(2), "Expected 2 reports generated.");
            Assert.That(
                reports[0].Recommendations,
                Does.Contain("Section: Fonts").And.Contain("- Too many fonts used."),
                "First report does not contain expected Fonts content."
            );
            Assert.That(
                reports[1].Recommendations,
                Does.Contain("Section: Text Structure").And.Contain("- Large text blocks found."),
                "Second report does not contain expected Text Structure content."
            );
        }
    }
}