using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;

        public BackgroundTaskQueue()
        {
            _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(new BoundedChannelOptions(10)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

        }

        /// Enqueues a background work item to be processed later.
        public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null) 
            // Validate input
                throw new ArgumentNullException(nameof(workItem)); 

            // Write the work item into the channel
            await _queue.Writer.WriteAsync(workItem);
        }

        /// Dequeues the next available background work item.
        /// If no work item is available, waits until one is enqueued or cancellation is requested.
        public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            // Wait for and read the next item
            var workItem = await _queue.Reader.ReadAsync(cancellationToken); 
            return workItem;
        }
    }
}