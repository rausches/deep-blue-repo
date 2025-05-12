using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;

namespace Service_Tests
{
    public static class DbSetMock
    {
        public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockSet.As<IAsyncEnumerable<T>>()
                   .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                   .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            return mockSet;
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public T Current => _inner.Current;
    }

    [TestFixture]
    public class ReportCleanupService_Tests
    {
        private Mock<IServiceScopeFactory> _scopeFactoryMock;
        private Mock<IServiceScope> _scopeMock;
        private Mock<IServiceProvider> _providerMock;
        private Mock<UxCheckmateDbContext> _dbContextMock;
        private Mock<ILogger<ReportCleanupService>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _providerMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<UxCheckmateDbContext>();
            _loggerMock = new Mock<ILogger<ReportCleanupService>>();
        }

        private ReportCleanupService CreateServiceWithMockDbContext(Mock<DbSet<Report>> reportsMock)
        {
            _providerMock.Setup(p => p.GetService(typeof(UxCheckmateDbContext)))
                         .Returns(_dbContextMock.Object);
            _scopeMock.Setup(s => s.ServiceProvider).Returns(_providerMock.Object);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

            _dbContextMock.Setup(db => db.Reports).Returns(reportsMock.Object);

            return new ReportCleanupService(_scopeFactoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Deletes_Expired_Anonymous_Reports()
        {
            // Arrange: use fixed now to avoid timing issues
            var now = DateTime.UtcNow;
            var expiredReport = new Report { CreatedAt = now.AddHours(-2), UserID = null };
            var activeReport = new Report { CreatedAt = now, UserID = "user" };
            var reports = new List<Report> { expiredReport, activeReport };

            var reportsDbSetMock = DbSetMock.CreateMockDbSet(reports);
            var service = CreateServiceWithMockDbContext(reportsDbSetMock);

            // Act
            var cts = new CancellationTokenSource();
            var serviceTask = service.StartAsync(cts.Token);

            await Task.Delay(500);  // let background loop run once
            cts.Cancel();
            try { await serviceTask; } catch { }

            // Assert
            _dbContextMock.Verify(db => db.Reports.RemoveRange(
                It.Is<IEnumerable<Report>>(r => r.Count() == 1 && r.Contains(expiredReport))), Times.Once);

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            _loggerMock.Verify(log => log.LogInformation(
                It.Is<string>(s => s.Contains("Deleted")),
                It.IsAny<object[]>()), Times.Once);
        }

        [Test]
        public async Task Skips_If_No_Expired_Reports()
        {
            // Arrange
            var noReports = new List<Report>();
            var reportsDbSetMock = DbSetMock.CreateMockDbSet(noReports);
            var service = CreateServiceWithMockDbContext(reportsDbSetMock);

            // Act
            var cts = new CancellationTokenSource();
            var serviceTask = service.StartAsync(cts.Token);

            await Task.Delay(500);  // let background loop run once
            cts.Cancel();
            try { await serviceTask; } catch { }

            // Assert
            _dbContextMock.Verify(db => db.Reports.RemoveRange(It.IsAny<IEnumerable<Report>>()), Times.Never);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}