using System;
using System.IO;
using System.Reflection;
using System.Threading;
using ArticleCheck.Api.Extensions;
using ArticleCheck.Core.Configuration;
using ArticleCheck.Core.Services;
using ArticleCheck.Infrastructure;
using ArticleCheck.Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace ArticleCheck.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionStrings = Configuration.GetSection(nameof(ConnectionStrings))
                .Get<ConnectionStrings>();

            services.AddControllers()
                .Services
                .AddOptions()
                .Configure<ArticleCheckConfiguration>(Configuration)
                .AddScoped(s => s.GetService<IOptions<ArticleCheckConfiguration>>()?.Value)
                .AddMediatR(typeof(DatabaseContext), typeof(ArticleSimilarityService))
                .AddDistributedRedisCache(redisOptions =>
                {
                    redisOptions.InstanceName = RedisConstants.InstanceName;
                    redisOptions.Configuration = connectionStrings.ArticlesRedis;
                })
                .AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(connectionStrings.ArticlesRedis))
                .AddArticleCheckServices();

            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = "Article Check Api",
                            Version = "v1",
                        });

                    // Set the comments path for the Swagger JSON and UI.
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                })
                // Should be called AFTER AddSwaggerGen
                .AddSwaggerGenNewtonsoftSupport();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.DocumentTitle = "Article Check Api";
                   c.SwaggerEndpoint("../swagger/v1/swagger.json", c.DocumentTitle);
                   c.DisplayRequestDuration();
               });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
