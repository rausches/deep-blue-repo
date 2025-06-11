using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Uxcheckmate_Main.Controllers;

namespace Controller_Tests
{
    [TestFixture]
    public class JiraAuthController_Tests
    {
        private Mock<IConfiguration> _configMock;
        private JiraAuthController _controller;

        [SetUp]
        public void Setup()
        {
            _configMock = new Mock<IConfiguration>();

            // Set up fake config values
            _configMock.Setup(c => c["Jira:ClientId"]).Returns("fake-client-id");
            _configMock.Setup(c => c["Jira:RedirectUri"]).Returns("https://example.com/callback");

            _controller = new JiraAuthController(_configMock.Object);

            // Mock HttpContext for session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession(); // custom mock session
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Authorize_ReturnsRedirectResult_WithCorrectUrl()
        {
            // Arrange
            int reportId = 123;

            // Act
            var result = _controller.Authorize(reportId) as RedirectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Url, Does.Contain("https://auth.atlassian.com/authorize"));
            Assert.That(result.Url, Does.Contain("client_id=fake-client-id"));
            Assert.That(result.Url, Does.Contain("redirect_uri=https://example.com/callback"));
            Assert.That(result.Url, Does.Contain($"state={reportId}"));
        }

        [Test]
        public async Task Callback_ReturnsBadRequest_WhenCodeMissing()
        {
            // Arrange
            string code = null;
            string state = "123";

            // Act
            var result = await _controller.Callback(code, state);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest.Value, Is.EqualTo("Authorization code missing."));
        }

        [Test]
        public async Task Callback_ReturnsBadRequest_WhenStateIsInvalid()
        {
            // Arrange
            string code = "valid-code";
            string state = "notanumber";

            // Act
            var result = await _controller.Callback(code, state);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest.Value, Is.EqualTo("Missing or invalid state (reportId)."));
        }
    }

    // mock ISession implementation for testing
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id => "TestSessionId";
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }
}
