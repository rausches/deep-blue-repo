using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Moq.Protected;

[TestFixture]
public class CaptchaServiceTests
{
    private Mock<IConfiguration> _mockConfig;
    private Mock<IHttpClientFactory> _mockFactory;
    [SetUp]
    public void Setup()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["Captcha:SecretKey"]).Returns("dummy-secret");
        _mockFactory = new Mock<IHttpClientFactory>();
    }
    [Test]
    public async Task TokenReturnsTrueOnSuccess()
    {
        var json = JsonSerializer.Serialize(new { success = true });
        var client = CreateMockHttpClient(json);
        _mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        var service = new CaptchaService(_mockConfig.Object, _mockFactory.Object);
        var result = await service.VerifyTokenAsync("valid_token");
        Assert.That(result, Is.True);
    }
    [Test]
    public async Task TokenReturnsFalseOnFailure()
    {
        var json = JsonSerializer.Serialize(new { success = false });
        var client = CreateMockHttpClient(json);
        _mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        var service = new CaptchaService(_mockConfig.Object, _mockFactory.Object);
        var result = await service.VerifyTokenAsync("bad_token");
        Assert.That(result, Is.False);
    }
    [Test]
    public async Task TokenReturnsFalseOnNetworkFailure()
    {
        var client = CreateMockHttpClient("{}", HttpStatusCode.BadRequest);
        _mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        var service = new CaptchaService(_mockConfig.Object, _mockFactory.Object);
        var result = await service.VerifyTokenAsync("network_error");
        Assert.That(result, Is.False);
    }
    private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });
        return new HttpClient(handler.Object);
    }
}