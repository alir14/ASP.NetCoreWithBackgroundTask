using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPICoreWithBgTask.BackgroundTasks
{
    public interface IBackGroundTaskQueue
    {
        void QueueBackGroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
    public class BackGroundTaskQueue : IBackGroundTaskQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems
            = new ConcurrentQueue<Func<CancellationToken, Task>>();

        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackGroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);

            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }

    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;

        public IBackGroundTaskQueue TaskQueue;

        public QueuedHostedService(ILoggerFactory loggerFactory,
            IBackGroundTaskQueue taskQueue)
        {
            _logger = loggerFactory.CreateLogger<QueuedHostedService>();

            TaskQueue = taskQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued hosted Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error occured executing {nameof(workItem)}");
                }
            }

            _logger.LogInformation("Queued Hosted Service Is stopping");
        }
    }
}
