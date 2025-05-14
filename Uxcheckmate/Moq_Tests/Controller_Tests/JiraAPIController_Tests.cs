using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Controller_Tests
{
    [TestFixture]
    public class JiraAPIController_Tests
    {
        private UxCheckmateDbContext _dbContext;
        private Mock<IJiraService> _jiraServiceMock;
        private JiraAPIController _controller;

        [SetUp]
        public void Setup()
        {
            // Create in-memory EF Core database for isolated tests
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Add a test report to the in-memory database
            var report = new Report
            {
                Id = 1,
                Url = "http://example.com",
                DesignIssues = new List<DesignIssue>(),
                AccessibilityIssues = new List<AccessibilityIssue>()
            };

            _dbContext = new UxCheckmateDbContext(options);
            _dbContext.Reports.Add(report);
            _dbContext.SaveChanges();

            // Create mock JiraService dependency
            _jiraServiceMock = new Mock<IJiraService>();

            // Create controller with test database and mock service
            _controller = new JiraAPIController(_dbContext, _jiraServiceMock.Object);

            // Add fake session to HttpContext for controller
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task ExportReportToJira_ReturnsOk_WhenSuccessful()
        {
            // Arrange: simulate valid connected Jira session
            _controller.HttpContext.Session.SetString("JiraAccessToken", "token");
            _controller.HttpContext.Session.SetString("JiraCloudId", "cloud");

            // Act: call controller method
            var result = await _controller.ExportReportToJira(1, "PROJECT");

            // Assert: verify OkObjectResult returned with expected text
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value.ToString(), Does.Contain("Report exported to Jira"));
        }

        [Test]
        public async Task ExportReportToJira_ReturnsBadRequest_WhenNotConnected()
        {
            // Act: call controller method without session data
            var result = await _controller.ExportReportToJira(1, "PROJECT");

            // Assert: verify BadRequestObjectResult returned with expected text
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest, Is.Not.Null);
            Assert.That(badRequest!.Value.ToString(), Does.Contain("connect Jira"));
        }
    }

    // Fake in-memory implementation of ISession for unit testing
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public IEnumerable<string> Keys => _sessionStorage.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        // ISession methods implemented as simple Dictionary storage
        public void Clear() => _sessionStorage.Clear();
        public void Remove(string key) => _sessionStorage.Remove(key);
        public void Set(string key, byte[] value) => _sessionStorage[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);

        // ISession interface
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
