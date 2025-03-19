using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Main.Tests.Services
{
    [TestFixture]
    public class FaviconDetectionServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<ILogger<FaviconDetectionService>> _loggerMock;
        private FaviconDetectionService _service;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _loggerMock = new Mock<ILogger<FaviconDetectionService>>();
            _service = new FaviconDetectionService(_httpClient, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task DetectFaviconAsync_ShouldReturnTrue_WhenFaviconExists()
        {
            // Arrange
            string url = "https://example.com";
            string htmlContent = "<link rel=\"icon\" href=\"favicon.ico\">";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });

            // Act
            bool result = await _service.DetectFaviconAsync(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DetectFaviconAsync_ShouldReturnFalse_WhenFaviconDoesNotExist()
        {
            // Arrange
            string url = "https://example.com";
            string htmlContent = "<html><head></head><body></body></html>";
            string faviconUrl = $"{url.TrimEnd('/')}/favicon.ico";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == url),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == faviconUrl),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            bool result = await _service.DetectFaviconAsync(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DetectFaviconAsync_ShouldReturnFalse_WhenHttpRequestExceptionIsThrown()
        {
            // Arrange
            string url = "https://example.com";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            // Act
            bool result = await _service.DetectFaviconAsync(url);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
