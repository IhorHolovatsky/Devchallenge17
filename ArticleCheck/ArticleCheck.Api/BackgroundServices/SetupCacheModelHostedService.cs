using System;
using System.Threading;
using System.Threading.Tasks;
using ArticleCheck.Core.Queries;
using ArticleCheck.Core.Services.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArticleCheck.Api.BackgroundServices
{
    public class SetupCacheModelHostedService : IHostedService
    {
        private readonly ILogger<SetupCacheModelHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _environment;
        public SetupCacheModelHostedService(ILogger<SetupCacheModelHostedService> logger,
                                            IHostEnvironment environment,
                                            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _environment = environment;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // mssql server is starting really slowly.. need to wait it
            if (_environment.EnvironmentName.Equals("Container"))
            {
                await Task.Delay(5000, cancellationToken);
            }

            _logger.LogInformation($"Starting {nameof(SetupCacheModelHostedService)}");

            using (var services = _serviceProvider.CreateScope())
            {
                var bus = services.ServiceProvider.GetService<IMediator>();
                var articleSimilarityService = services.ServiceProvider.GetService<IArticleSimilarityService>();

                var articles = await bus.Send(new GetArticlesQuery(), cancellationToken);

                _logger.LogInformation("Found {articles} articles", articles.Count);

                await articleSimilarityService.InitAsync(articles);
            }

            _logger.LogInformation($"{nameof(SetupCacheModelHostedService)} Finished");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping {nameof(SetupCacheModelHostedService)}");

            return Task.CompletedTask;
        }
    }
}