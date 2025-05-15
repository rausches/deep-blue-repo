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
    }
}