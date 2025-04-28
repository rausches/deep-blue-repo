using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Uxcheckmate_Main.Models;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Services
{
    public class QueueService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue; 
        private readonly ILogger<QueueService> _logger; 
        private readonly IServiceScopeFactory _scopeFactory; 


        public QueueService(IBackgroundTaskQueue taskQueue, ILogger<QueueService> logger, IServiceScopeFactory scopeFactory)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Task Worker started.");

            // Continuously run while the service is not canceled
            while (!stoppingToken.IsCancellationRequested)
            {
                // Dequeue the next work item (blocks until available)
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    // Execute the work item
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing work item.");
                }
            }
        }

        // Schedules deletion of a report after a delay, but only if it belongs to an anonymous user
        public async Task DeleteReportIfAnonymousAsync(int reportId, TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                // Wait for the specified delay before checking/deleting
                await Task.Delay(delay, cancellationToken);

                // Create a new scoped service provider to get a DbContext instance
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();

                // Find the report by ID
                var report = await context.Reports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

                if (report != null)
                {
                    // If the report has no associated user, it's considered anonymous
                    if (report.UserID == null) 
                    {
                        // Delete the anonymous report
                        context.Reports.Remove(report);
                        await context.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Deleted anonymous report ID {ReportId} after delay.", reportId);
                    }
                    else
                    {
                        // If a user is associated, don't delete
                        _logger.LogInformation("Skipped deletion for signed-in user report ID {ReportId}.", reportId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete report ID {ReportId}.", reportId);
            }
        }
    }
}
