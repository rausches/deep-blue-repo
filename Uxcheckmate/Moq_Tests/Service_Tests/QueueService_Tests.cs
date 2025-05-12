using NUnit.Framework;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using Microsoft.EntityFrameworkCore;

namespace Service_Tests
{
    [TestFixture]
    public class QueueServiceTests
    {
        [Test]
        public async Task QueueWorkItem_ExecutesSuccessfully()
        {
            // Arrange
            var queue = new BackgroundTaskQueue();
            var loggerMock = new Mock<ILogger<QueueService>>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var executed = false;

            await queue.QueueBackgroundWorkItemAsync(token =>
            {
                executed = true;
                return Task.CompletedTask;
            });

            var service = new QueueService(queue, loggerMock.Object, scopeFactoryMock.Object);

            // Act
            using var cts = new CancellationTokenSource();
            var task = service.StartAsync(cts.Token);
            await Task.Delay(100);
            cts.Cancel();
            await task;

            // Assert
            Assert.That(executed, Is.True, "The queued work item should have been executed.");
        }

        [Test]
        public async Task DequeueAsync_ReturnsWorkItem()
        {
            // Arrange
            var queue = new BackgroundTaskQueue();
            var wasRun = false;

            Func<CancellationToken, Task> testTask = async token =>
            {
                await Task.Delay(10);
                wasRun = true;
            };

            await queue.QueueBackgroundWorkItemAsync(testTask);
            var dequeuedTask = await queue.DequeueAsync(CancellationToken.None);

            // Act
            await dequeuedTask(CancellationToken.None);

            // Assert
            Assert.That(wasRun, Is.True, "The dequeued task should have been run.");
        }
    }
}