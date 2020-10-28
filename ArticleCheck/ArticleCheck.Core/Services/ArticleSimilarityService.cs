using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArticleCheck.Core.Configuration;
using ArticleCheck.Core.Events;
using ArticleCheck.Core.Extensions;
using ArticleCheck.Core.NLP.Interfaces;
using ArticleCheck.Core.NLP.Models;
using ArticleCheck.Core.Services.Interfaces;
using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Services
{
    public class ArticleSimilarityService : IArticleSimilarityService,
        INotificationHandler<ArticleCreatedEvent>,
        INotificationHandler<ArticleUpdatedEvent>,
        INotificationHandler<ArticleRemovedEvent>
    {
        private readonly IKeyValueStorage _cache;
        private readonly ISimilarityScoring _similarityScoring;
        private readonly ArticleCheckConfiguration _articleCheckConfiguration;

        /// <summary>
        /// Processed similarities are stored in cache in following structure
        /// {
        ///    "articles:1": [ { id: 2, score: 0.8 }, { id: 3, score: 0.4 } ]
        ///    "articles:2": [ { id: 1, score: 0.8 }, { id: 2, score: 0.2 } ]
        /// }
        /// </summary>
        private readonly Func<Article, string> _articleSimilarityCacheKey = article => $"articles:{article.Id}";

        public ArticleSimilarityService(
            IKeyValueStorage cache,
            ArticleCheckConfiguration articleCheckConfiguration,
            ISimilarityScoring similarityScoring)
        {
            _articleCheckConfiguration = articleCheckConfiguration;
            _similarityScoring = similarityScoring;
            _cache = cache;
        }

        public Task<List<int>> FindDuplicatesAsync(string text)
        {
            return FindDuplicatesAsync(new Article
            {
                Content = text,
                Tokens = Tokenize(text)
            }, false);
        }

        public Task<List<int>> FindDuplicatesAsync(Article article)
        {
            return FindDuplicatesAsync(article, true);
        }

        /// <summary>
        /// Finds duplicates of article
        /// </summary>
        /// <param name="article">article for which perform duplicate scanning</param>
        /// <param name="saveInCache">Saving results to redis cache (could be useful to set false, if article was not added to database)</param>
        /// <returns></returns>
        public async Task<List<int>> FindDuplicatesAsync(Article article, bool saveInCache)
        {
            var results = await _cache.GetAsync<List<SimilarityResult>>(_articleSimilarityCacheKey(article));

            if (results == null)
            {
                results = await _similarityScoring.GetSimilarityScoresAsync(article.ToDocument());

                if (saveInCache)
                {
                    // Persist similarity results (before MatchingThreshold check, since it could be changed in future)
                    await _cache.SetAsync(_articleSimilarityCacheKey(article), results);
                }
            }

            var duplicates = results
                .Where(r => r.Id != article.Id && r.Score >= _articleCheckConfiguration.MatchingThreshold)
                .ToList();

            return duplicates
                .Select(x => x.Id)
                .ToList();
        }

        public string Tokenize(Article article)
        {
            return Tokenize(article.Content);
        }

        public string Tokenize(string text)
        {
            var document = new Document
            {
                Content = text
            };
            var tokens = _similarityScoring.Tokenize(document);

            return string.Join(" ", tokens);
        }

        public async Task<List<int[]>> GetSimilarityGroupsAsync(List<Article> articles)
        {
            var duplicateGroups = new List<int[]>();

            foreach (var article in articles)
            {
                var duplicateIds = await FindDuplicatesAsync(article);

                // Include itself
                duplicateIds.Add(article.Id);

                // Filter out groups which includes only one article,
                // because each article is duplicate for itself, and can be considered as group of duplicated
                if (duplicateIds.Count > 1
                    && duplicateGroups.All(x => x.Length != duplicateIds.Count || duplicateIds.Except(x).Any()))
                {
                    duplicateGroups.Add(duplicateIds.ToArray());
                }
            }

            return duplicateGroups;
        }

        /// <summary>
        /// Initialize Similarity cache from articles
        /// </summary>
        public async Task InitAsync(List<Article> articles)
        {
            var documents = articles.Select(x => x.ToDocument())
                .ToList();

            // Setup similarityScoring service to initialize Computing Model
            await _similarityScoring.InitAsync(documents);

            // Pre-calculate duplicates and store it in cache
            await Task.WhenAll(articles.Select(FindDuplicatesAsync));
        }

        #region Event Handlers

        /// <summary>
        /// When new Article is created in system, we want to listen this and adjust our cache model
        /// </summary>
        public async Task Handle(ArticleCreatedEvent notification, CancellationToken cancellationToken)
        {
            var document = notification.Article.ToDocument();

            // Adding Article to our Computing Model
            await _similarityScoring.AddAsync(document);
            await AddSimilarityResults(notification.Article);
        }

        public async Task Handle(ArticleUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await Handle(new ArticleRemovedEvent(notification.ArticleOld), cancellationToken);
            await Handle(new ArticleCreatedEvent(notification.ArticleNew), cancellationToken);
        }

        /// <summary>
        /// Here is algorithm for cache adjusting when article is removed
        /// 1. Get similarityResults for {article} from cache
        /// 2. Remove similarityResults from cache for removed {article}
        /// 3. Remove similarityScore in other articles
        ///
        /// Here is better AND faster to use graph data structure,
        /// where each node is connected to all others,
        /// and connection weight is SimilarityScore (from 0 to 1),
        /// but I used simple List of Lists, because didn't have time...
        /// </summary>
        public async Task Handle(ArticleRemovedEvent notification, CancellationToken cancellationToken)
        {
            await _similarityScoring.RemoveAsync(notification.Article.ToDocument());


            var results = await _cache.GetAsync<List<SimilarityResult>>(_articleSimilarityCacheKey(notification.Article))
                                             ?? new List<SimilarityResult>();

            await _cache.RemoveAsync(_articleSimilarityCacheKey(notification.Article));

            foreach (var similarityResult in results.Where(r => r.Id != notification.Article.Id))
            {
                var cacheKey = _articleSimilarityCacheKey(new Article { Id = similarityResult.Id });
                var articleSimilarities = await _cache.GetAsync<List<SimilarityResult>>(cacheKey);

                articleSimilarities = articleSimilarities.Where(x => x.Id != notification.Article.Id)
                                                         .ToList();

                await _cache.SetAsync(cacheKey, articleSimilarities);
            }

        }

        /// <summary>
        /// Here is algorithm for cache adjusting when new article created
        /// 1. Get similarityResults for new {article}
        /// 2. Save similarityResults in cache for {article}
        /// 3. Adjust other articles with similarityScore to new article
        ///
        /// Here is better to use graph data structure, where each node is connected to all others, and connection weight is SimilarityScore (from 0 to 1),
        /// but I used simple List of Lists, because didn't have time...
        /// </summary>
        private async Task AddSimilarityResults(Article article)
        {
            var similarityResults = await _similarityScoring.GetSimilarityScoresAsync(article.ToDocument());

            await _cache.SetAsync(_articleSimilarityCacheKey(article), similarityResults);

            foreach (var similarityResult in similarityResults)
            {
                var cacheKey = _articleSimilarityCacheKey(new Article { Id = similarityResult.Id });
                var articleSimilarities = await _cache.GetAsync<List<SimilarityResult>>(cacheKey);
                var articleSimilarity = await _similarityScoring.GetSimilarityScoreAsync(similarityResult.Id, article.Id);

                articleSimilarities.Add(articleSimilarity);

                await _cache.SetAsync(cacheKey, articleSimilarities);
            }
        }

        #endregion
    }
}