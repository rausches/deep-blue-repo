using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class ReportCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReportCleanupService> _logger;

        public ReportCleanupService(IServiceScopeFactory scopeFactory, ILogger<ReportCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop continuously until the application is shutting down or the service is cancelled
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new DI scope to safely use scoped services like DbContext
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();

                    // Define the cutoff time: any anonymous report older than this will be deleted
                    var cutoff = DateTime.UtcNow.AddMinutes(-30);

                    // Query for reports with no associated UserID and a CreatedAt value older than the cutoff
                    var expired = await dbContext.Reports
                        .Where(r => r.UserID == null && r.CreatedAt < cutoff)
                        .ToListAsync(stoppingToken);

                    // If expired reports were found, delete them
                    if (expired.Any())
                    {
                        dbContext.Reports.RemoveRange(expired);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Deleted {Count} expired anonymous reports.", expired.Count);
                    }
                }
                catch (Exception ex)
                {
                    // Log any error that occurs during cleanup
                    _logger.LogError(ex, "Error occurred during anonymous report cleanup.");
                }

                // Wait for 10 minutes before checking again
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        // test method
        public async Task CleanupExpiredReportsAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();
            var cutoff = DateTime.UtcNow.AddMinutes(-30);
            var expired = await dbContext.Reports
                .Where(r => r.UserID == null && r.CreatedAt < cutoff)
                .ToListAsync(stoppingToken);

            if (expired.Any())
            {
                dbContext.Reports.RemoveRange(expired);
                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Deleted {Count} expired anonymous reports.", expired.Count);
            }
        }
    }
}