using ArticleCheck.Api.BackgroundServices;
using ArticleCheck.Core.NLP;
using ArticleCheck.Core.NLP.Interfaces;
using ArticleCheck.Core.NLP.Stemming;
using ArticleCheck.Core.NLP.TfIdf;
using ArticleCheck.Core.Services;
using ArticleCheck.Core.Services.Interfaces;
using ArticleCheck.Infrastructure;
using ArticleCheck.Infrastructure.Storages;
using Microsoft.Extensions.DependencyInjection;

namespace ArticleCheck.Api.Extensions
{
    public static class ServiceBuilderExtensions
    {
        public static IServiceCollection AddArticleCheckServices(this IServiceCollection services)
        {
            services.AddScoped<IArticleService, ArticlesService>()
                    .AddScoped<IArticleSimilarityService, ArticleSimilarityService>()
                    .AddScoped<IDatabaseContext, DatabaseContext>()
                    
                    .AddNlpServices()
                    
                    .AddHostedService<SetupCacheModelHostedService>()
                    ;


            return services;
        }

        public static IServiceCollection AddNlpServices(this IServiceCollection services)
        {
            services.AddScoped<ITextPreprocessor, TextPreprocessor>();
            services.AddScoped<IStemmer, PorterStemmer>();
            services.AddScoped<ISimilarityScoring, TfIdfSimilarityScoring>();
            services.AddScoped<IKeyValueStorage, RedisStorage>();

            return services;
        }
    }
}