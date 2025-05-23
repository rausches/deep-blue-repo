using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    [TestFixture]
    public class ReportCleanupService_IntegrationTests
    {
        private ServiceProvider _serviceProvider;
        private UxCheckmateDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            // Create a single in-memory database name to ensure shared context
            var dbName = Guid.NewGuid().ToString();

            services.AddDbContext<UxCheckmateDbContext>(options =>
                options.UseInMemoryDatabase(dbName), ServiceLifetime.Singleton);

            services.AddLogging();
            services.AddScoped<ReportCleanupService>();

            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<UxCheckmateDbContext>();
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _serviceProvider?.Dispose();
        }

        [Test]
        public async Task Deletes_Expired_Anonymous_Reports()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var expiredReport = new Report
            {
                CreatedAt = now.AddMinutes(-31),
                UserID = null,
                Url = "http://expired.com"
            };

            var activeReport = new Report
            {
                CreatedAt = now,
                UserID = "777",
                Url = "http://active.com"
            };

            await _dbContext.Reports.AddRangeAsync(expiredReport, activeReport);
            await _dbContext.SaveChangesAsync();

            var service = new ReportCleanupService(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                _serviceProvider.GetRequiredService<ILogger<ReportCleanupService>>());

            // Act
            await service.CleanupExpiredReportsAsync(CancellationToken.None);

            // Assert
            var remainingReports = await _dbContext.Reports.ToListAsync();
            Assert.That(remainingReports, Has.Count.EqualTo(1));
            Assert.That(remainingReports[0].UserID, Is.EqualTo("777"));
        }

        [Test]
        public async Task Skips_If_No_Expired_Reports()
        {
            // Arrange
            var activeReport = new Report
            {
                CreatedAt = DateTime.UtcNow,
                UserID = "user123",
                Url = "http://active.com"
            };

            await _dbContext.Reports.AddAsync(activeReport);
            await _dbContext.SaveChangesAsync();

            var service = new ReportCleanupService(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                _serviceProvider.GetRequiredService<ILogger<ReportCleanupService>>());

            // Act
            var cts = new CancellationTokenSource();
            var executeTask = service.StartAsync(cts.Token);

            await Task.Delay(500);
            cts.Cancel();
            try { await executeTask; } catch { }

            var remainingReports = await _dbContext.Reports.ToListAsync();

            // Assert
            Assert.That(remainingReports, Has.Count.EqualTo(1));
        }
    }
}