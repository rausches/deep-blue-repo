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

        private ReportCleanupService CreateServiceWithReports(List<Report> reports)
        {
            var reportsDbSet = GetQueryableMockDbSet(reports);

            _dbContextMock.Setup(db => db.Reports).Returns(reportsDbSet.Object);
            _providerMock.Setup(p => p.GetService(typeof(UxCheckmateDbContext)))
                        .Returns(_dbContextMock.Object);
            _scopeMock.Setup(s => s.ServiceProvider).Returns(_providerMock.Object);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

            return new ReportCleanupService(_scopeFactoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task Deletes_Expired_Anonymous_Reports()
        {
            // Arrange: create one expired anonymous report and one non-expired user report
            var expiredReport = new Report { CreatedAt = DateTime.UtcNow.AddMinutes(-40), UserID = null };
            var activeReport = new Report { CreatedAt = DateTime.UtcNow, UserID = "user" };
            var reports = new List<Report> { expiredReport, activeReport };

            var service = CreateServiceWithReports(reports);

            // Use a short timeout to cancel after one iteration of the loop
            var cts = new CancellationTokenSource(100);

            // Act
            await service.StartAsync(cts.Token);
            
            // Allow background loop to run at least once
            await Task.Delay(200); 

            // Assert: verify only the expired anonymous report is deleted
            _dbContextMock.Verify(db => db.Reports.RemoveRange(
                It.Is<List<Report>>(r => r.Contains(expiredReport) && r.Count == 1)), Times.Once);

            // Ensure SaveChangesAsync was called
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verify that the logger logged the deletion
            _loggerMock.Verify(log => log.LogInformation(
                It.IsAny<string>(), 1), Times.Once);
        }

        [Test]
        public async Task Skips_If_No_Expired_Reports()
        {
            // Arrange: only a recent anonymous report (not expired)
            var reports = new List<Report> {
                new Report { CreatedAt = DateTime.UtcNow, UserID = null }
            };

            var service = CreateServiceWithReports(reports);
            var cts = new CancellationTokenSource(100);

            // Act
            await service.StartAsync(cts.Token);
            await Task.Delay(200);

            // Assert: no deletion or save should occur
            _dbContextMock.Verify(db => db.Reports.RemoveRange(It.IsAny<IEnumerable<Report>>()), Times.Never);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // Helper method to create a mock DbSet<T> from a list
        private static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();

            // Set up LINQ support for the fake DbSet
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Set up async enumeration for EF Core's ToListAsync
            dbSet.Setup(d => d.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sourceList);

            return dbSet;
        }
    }
}