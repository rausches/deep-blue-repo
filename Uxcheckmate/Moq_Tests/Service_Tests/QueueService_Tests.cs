/*using NUnit.Framework;
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
            // Arrange: create queue and mocks for logging and scope
            var queue = new BackgroundTaskQueue();
            var loggerMock = new Mock<ILogger<QueueService>>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();

            var executed = false;
            // Enqueue a test task that sets a flag to true when run
            await queue.QueueBackgroundWorkItemAsync(token => {
                executed = true;
                return Task.CompletedTask;
            });

            // Instantiate the service
            var service = new QueueService(queue, loggerMock.Object, scopeFactoryMock.Object);

            // Act: run the background worker briefly, then cancel
            using var cts = new CancellationTokenSource();
            var task = service.StartAsync(cts.Token);
            // allow time for the task to run
            await Task.Delay(100);
            cts.Cancel();
            await task;

            // Assert: ensure the task was executed
            Assert.IsTrue(executed);
        }

        [Test]
        public void QueueBackgroundWorkItemAsync_ThrowsIfNull()
        {
            // Arrange: create a queue instance
            var queue = new BackgroundTaskQueue();

            // Act & Assert: enqueueing a null task should throw an ArgumentNullException
            Assert.ThrowsAsync<ArgumentNullException>(() => queue.QueueBackgroundWorkItemAsync(null));
        }

        [Test]
        public async Task DeleteReportIfAnonymous_DeletesIfNoUserID()
        {
            // Arrange: create logger and in-memory database with an anonymous report
            var loggerMock = new Mock<ILogger<QueueService>>();
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(databaseName: "DeleteIfAnonymousTest")
                .Options;

            var context = new UxCheckmateDbContext(options);
            var report = new Report { Id = 1, UserID = null };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Mock service scope and provider
            var scopeMock = new Mock<IServiceScope>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(x => x.GetService(typeof(UxCheckmateDbContext))).Returns(context);
            scopeMock.Setup(x => x.ServiceProvider).Returns(providerMock.Object);

            // Mock scope factory
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            var service = new QueueService(null, loggerMock.Object, scopeFactoryMock.Object);

            // Act: attempt to delete the anonymous report
            await service.DeleteReportIfAnonymousAsync(1, TimeSpan.Zero, CancellationToken.None);

            // Assert: report should be removed from DB
            var deleted = await context.Reports.FindAsync(1);
            Assert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteReportIfAnonymous_SkipsIfUserIDExists()
        {
            // Arrange: create logger and in-memory database with a report that has a user ID
            var loggerMock = new Mock<ILogger<QueueService>>();
            var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
                .UseInMemoryDatabase(databaseName: "SkipIfNotAnonymousTest")
                .Options;

            var context = new UxCheckmateDbContext(options);
            var report = new Report { Id = 2, UserID = "user123" };
            context.Reports.Add(report);
            await context.SaveChangesAsync();

            // Mock service scope and provider
            var scopeMock = new Mock<IServiceScope>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(x => x.GetService(typeof(UxCheckmateDbContext))).Returns(context);
            scopeMock.Setup(x => x.ServiceProvider).Returns(providerMock.Object);

            // Mock scope factory
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            var service = new QueueService(null, loggerMock.Object, scopeFactoryMock.Object);

            // Act: call delete logic on a report with a UserID
            await service.DeleteReportIfAnonymousAsync(2, TimeSpan.Zero, CancellationToken.None);

            // Assert: report should still exist in the database
            var stillExists = await context.Reports.FindAsync(2);
            Assert.IsNotNull(stillExists);
        }

        [Test]
        public async Task DequeueAsync_ReturnsWorkItem()
        {
            // Arrange: create a new queue and a flag to indicate execution
            var queue = new BackgroundTaskQueue();
            bool wasRun = false;

            // Define a task that will set the flag to true
            Func<CancellationToken, Task> testTask = async (token) =>
            {
                await Task.Delay(10); // simulate short async work
                wasRun = true;
            };

            // Act: enqueue and then dequeue the task
            await queue.QueueBackgroundWorkItemAsync(testTask);
            var dequeuedTask = await queue.DequeueAsync(CancellationToken.None);

            // Run the dequeued task
            await dequeuedTask(CancellationToken.None);

            // Assert: make sure the task was executed
            Assert.IsTrue(wasRun, "The dequeued background task should have run and set the flag.");
        }
    }
}
*/