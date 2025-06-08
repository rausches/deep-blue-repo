using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Service_Tests
{
    [TestFixture]
    public class JiraService_Tests
    {
        private JiraService _jiraService;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private Mock<UxCheckmateDbContext> _dbContextMock;
        private Mock<IOpenAiService> _openAiServiceMock;
        private Mock<IMemoryCache> _cacheMock;

        [SetUp]
        public void Setup()
        {
            _httpHandlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_httpHandlerMock.Object);

            var options = Options.Create(new JiraSettings { ApiBaseUrl = "https://api.atlassian.com/" });

            _dbContextMock = new Mock<UxCheckmateDbContext>();
            _openAiServiceMock = new Mock<IOpenAiService>();
            _cacheMock = new Mock<IMemoryCache>();

            _jiraService = new JiraService(httpClient, options, _dbContextMock.Object, _openAiServiceMock.Object, _cacheMock.Object);
        }

        [Test]
        public async Task GetProjectsAsync_ReturnsProjects()
        {
            // Arrange
            // Setup the mock HTTP response for a successful Jira project fetch
            var jsonResponse = @"{ ""values"": [ { ""id"": ""10001"", ""key"": ""TEST"", ""name"": ""Test Project"" } ] }";

            _httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    // Target protected SendAsync method
                    "SendAsync",         

                    // Match any HttpRequestMessage
                    ItExpr.IsAny<HttpRequestMessage>(),   

                    // Match any CancellationToken
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    // Simulate success response
                    StatusCode = HttpStatusCode.OK,

                    // Return sample JSON payload
                    Content = new StringContent(jsonResponse)
                });

            // Act
            // Call the JiraService method under test
            var result = await _jiraService.GetProjectsAsync("fake_token", "fake_cloud");

            // Assert
            // Verify that the result matches expected values
            // Result list should not be null
            Assert.That(result, Is.Not.Null);      

            // Should contain exactly one project
            Assert.That(result, Has.Count.EqualTo(1));  

            // Project ID should match mock data 
            Assert.That(result[0].Id, Is.EqualTo("10001"));
        }

        [Test]
        public void GetProjectsAsync_ThrowsException_OnError()
        {
            // Arrange
            // Setup the mock HTTP response to simulate an error from Jira
            _httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    // Simulate error response 
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Error response")
                });

            // Act + Assert
            // Assert that calling GetProjectsAsync throws an Exception due to bad response
            Assert.That(async () => await _jiraService.GetProjectsAsync("fake_token", "fake_cloud"),
                        Throws.Exception.TypeOf<Exception>());
        }
    }
}
