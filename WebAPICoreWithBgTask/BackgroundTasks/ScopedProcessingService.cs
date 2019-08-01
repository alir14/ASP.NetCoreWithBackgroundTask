using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPICoreWithBgTask.BackgroundTasks
{
    public interface IScopedProcessingService
    {
        void DoWork();
    }

    public class ScopedProcessingService : IScopedProcessingService
    {
        private readonly ILogger _logger;

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger)
        {
            _logger = logger;
        }

        public void DoWork()
        {
            _logger.LogInformation("Scoped Processing Service is working");
        }
    }

    public class ConsumeScopedServiceHostedService : IHostedService
    {
        private readonly ILogger _logger;

        public IServiceProvider Services { get; }

        public ConsumeScopedServiceHostedService(IServiceProvider services,
            ILogger<ConsumeScopedServiceHostedService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is starting");

            DOWork();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is stopped");

            return Task.CompletedTask;
        }

        private void DOWork()
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is working");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider
                    .GetRequiredService<IScopedProcessingService>();

                scopedProcessingService.DoWork();
            }
        }
    }
}
