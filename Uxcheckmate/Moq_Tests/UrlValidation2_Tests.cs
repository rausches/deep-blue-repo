using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using Uxcheckmate_Main.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using Moq_Tests;

namespace UrlValidation_Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _controller;
        private HttpClient _httpClient;
        private Mock<ILogger<HomeController>> _mockLogger;
        private CaptchaService _captchaService;

        [SetUp]
        public void Setup()
        {
            var httpContext = new DefaultHttpContext();
            var dbOptions = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new UxCheckmateDbContext(dbOptions);
            _controller = TestBuilder.BuildHomeController(httpContext, dbContext);
            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            _controller.TempData = tempData;
            var captchaResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            };
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(captchaResponse);
            var mockHttpClient = new HttpClient(mockHandler.Object);
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);
            _captchaService = new CaptchaService(
                new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Captcha:SecretKey", "test-secret-key" }
                }).Build(),
                mockFactory.Object);

            // Set up any other dependencies or mocks as needed     
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            (_controller as IDisposable)?.Dispose();
        }

                [Test]
                public async Task Report_ShouldReturnViewResultForValidUrl()
                {
                    // Arrange
                    string redditUrl = "https://www.reddit.com";

                    // Act
                    var result = await _controller.Report(_captchaService, redditUrl, null, false, CancellationToken.None);

                    // Assert
                    Assert.That(result, Is.TypeOf<RedirectToActionResult>(), "Expected a ViewResult for a valid URL.");
                }

                [Test]
                public async Task Report_ShouldProcessValidUrl_WebScrapingDev()
                {
                    // Arrange
                    string url = "https://web-scraping.dev/products";
                    _controller.ControllerContext.HttpContext.Items["BypassReachability"] = true;
                    var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                    {
                        { "CaptchaToken", "test-token" }
                    });
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "testuser"),
                        new Claim(ClaimTypes.NameIdentifier, "1")
                    };
                    var identity = new ClaimsIdentity(claims, "TestAuthType");
                    var principal = new ClaimsPrincipal(identity);
                    _controller.ControllerContext.HttpContext.User = principal;

                    // Act
                    var result = await _controller.Report(_captchaService, url, null, false, CancellationToken.None);

                    // Assert
                    if (result is RedirectToActionResult redirect)
                    {
                        Assert.Fail($"Redirected to {redirect.ActionName}");
                    }

                    Assert.That(result, Is.TypeOf<ViewResult>(), "Expected a ViewResult.");
                }
    }
}
