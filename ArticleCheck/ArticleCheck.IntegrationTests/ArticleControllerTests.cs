using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ArticleCheck.Api;
using ArticleCheck.Core.Models;
using ArticleCheck.IntegrationTests.Constants;
using ArticleCheck.IntegrationTests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ArticleCheck.IntegrationTests
{
    [TestFixture]
    public class ArticleControllerTests
    {
        public int ArticleId { get; set; }

        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;

        [OneTimeSetUp]
        public void GivenARequestToTheController()
        {
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                    {
                        builder.UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                    });
            _client = _factory.CreateClient();
        }

        [Test]
        [Category(IntegrationTestsConstants.Category)]
        [Order(0)]
        public async Task CreateArticle_ReturnsOk()
        {
            // Arrange
            var articleDto = new ArticleCreateDto
            {
                Content = "content"
            };
            var body = GetBody(articleDto);

            // Act
            var result = await _client.PostAsync("/articles", body);
            var article = await result.GetDataAsync<ArticleDto>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.That(article.Id, Is.Not.Null);

            ArticleId = article.Id;
        }

        [Test]
        [Category(IntegrationTestsConstants.Category)]
        [Order(1)]
        public async Task GetArticles_ReturnsOk()
        {
            // Arrange

            // Act
            var result = await _client.GetAsync("/articles");
            var articles = await result.GetDataAsync<List<ArticleDto>>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.That(articles, Is.Not.Null);
        }

        [Test]
        [Category(IntegrationTestsConstants.Category)]
        [Order(2)]
        public async Task GetArticle_ReturnsOk()
        {
            // Arrange

            // Act
            var result = await _client.GetAsync($"/articles/{ArticleId}");
            var article = await result.GetDataAsync<ArticleDto>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.That(article, Is.Not.Null);
        }

        [Test]
        [Category(IntegrationTestsConstants.Category)]
        [Order(3)]
        public async Task UpdateArticle_ReturnsOk()
        {
            // Arrange
            var articleDto = new ArticleUpdateDto
            {
                Content = "string"
            };
            var body = GetBody(articleDto);

            // Act
            var result = await _client.PutAsync($"/articles/{ArticleId}", body);
            var article = await result.GetDataAsync<ArticleDto>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.That(article.Id, Is.Not.Null);
        }

        [Test]
        [Category(IntegrationTestsConstants.Category)]
        [Order(4)]
        public async Task CheckArticle_ReturnsOk()
        {
            // Arrange
            var articleDto = new ArticleCreateDto
            {
                Content = "string"
            };
            var body = GetBody(articleDto);

            // Act
            var result = await _client.PostAsync($"/articles/check", body);
            var articles = await result.GetDataAsync<List<ArticleDto>>();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.That(articles, Is.Not.Null);
        }


        [Test]
        [Category(IntegrationTestsConstants.Category)]
        [Order(5)]
        public async Task RemoveArticle_ReturnsOk()
        {
            // Arrange

            // Act
            var result = await _client.DeleteAsync($"/articles/{ArticleId}");

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }


        private StringContent GetBody<T>(T data)
        {
            var textContent = new StringContent(JsonConvert.SerializeObject(data));
            textContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return textContent;
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}