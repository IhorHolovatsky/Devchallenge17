using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArticleCheck.Core.Commands;
using ArticleCheck.Core.Events;
using ArticleCheck.Core.Models;
using ArticleCheck.Core.Queries;
using ArticleCheck.Core.Services.Interfaces;
using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Services
{
    public class ArticlesService : IArticleService
    {
        private readonly IArticleSimilarityService _articleSimilarityService;
        private readonly IMediator _bus;

        public ArticlesService(IArticleSimilarityService articleSimilarityService,
            IMediator bus)
        {
            _bus = bus;
            _articleSimilarityService = articleSimilarityService;
        }

        public async Task<ArticleDuplicatesDto> GetDuplicateGroupsAsync()
        {
            var articles = await _bus.Send(new GetArticlesQuery());

            return new ArticleDuplicatesDto
            {
                Groups = await _articleSimilarityService.GetSimilarityGroupsAsync(articles)
            };
        }

        public async Task<ArticleDto> GetByIdAsync(int id)
        {
            var article = await _bus.Send(new GetArticleByIdQuery(id));

            var articleDto = new ArticleDto
            {
                Id = article.Id,
                Content = article.Content,

                DuplicateIds = await _articleSimilarityService.FindDuplicatesAsync(article)
            };

            return articleDto;
        }


        public async Task<List<ArticleDto>> SearchAsync(ArticleSearchDto searchDto)
        {
            var articles = await _bus.Send(new GetArticlesQuery());
            var articleDtos = new List<ArticleDto>(articles.Count);

            var duplicateIds = new List<int>();

            foreach (var article in articles)
            {
                var articleDto = new ArticleDto
                {
                    Id = article.Id,
                    Content = article.Content,

                    DuplicateIds = await _articleSimilarityService.FindDuplicatesAsync(article)
                };

                if (searchDto.IsUniqueOnly && duplicateIds.Contains(articleDto.Id))
                {
                    continue;
                }

                articleDtos.Add(articleDto);
                duplicateIds.AddRange(articleDto.DuplicateIds);
            }

            return articleDtos;
        }

        public async Task<ArticleDto> CreateAsync(ArticleCreateDto articleCreateDto)
        {
            var articleToCreate = new Article
            {
                Content = articleCreateDto.Content,
                Tokens = _articleSimilarityService.Tokenize(articleCreateDto.Content)
            };

            var article = await _bus.Send(new CreateArticleCommand(articleToCreate));

            await _bus.Publish(new ArticleCreatedEvent(article));

            return new ArticleDto
            {
                Id = article.Id,
                Content = article.Content,
                CreatedTs = article.CreatedTs,
                UpdatedTs = article.UpdatedTs,

                DuplicateIds = await _articleSimilarityService.FindDuplicatesAsync(article)
            };
        }

        public async Task<ArticleDto> UpdateAsync(ArticleUpdateDto articleUpdateDto)
        {
            var article = await _bus.Send(new GetArticleByIdQuery(articleUpdateDto.Id));

            var articleToUpdate = new Article
            {
                Id = article.Id,
                Content = articleUpdateDto.Content,
                Tokens = _articleSimilarityService.Tokenize(articleUpdateDto.Content),

                CreatedTs = article.CreatedTs
            };

            var articleUpdated = await _bus.Send(new UpdateArticleCommand { Article = articleToUpdate });

            await _bus.Publish(new ArticleUpdatedEvent
            {
                ArticleOld = article,
                ArticleNew = articleUpdated
            });

            return new ArticleDto
            {
                Id = articleUpdated.Id,
                Content = articleUpdated.Content,
                CreatedTs = articleUpdated.CreatedTs,
                UpdatedTs = articleUpdated.UpdatedTs,

                DuplicateIds = await _articleSimilarityService.FindDuplicatesAsync(articleUpdated)
            };
        }

        public async Task RemoveAsync(int id)
        {
            var article = await _bus.Send(new GetArticleByIdQuery(id));

            await _bus.Send(new RemoveArticleCommand { Article = article });
            await _bus.Publish(new ArticleRemovedEvent(article));
        }


        public async Task<List<ArticleDto>> CheckDuplicatesAsync(string text)
        {
            var duplicateIds = await _articleSimilarityService.FindDuplicatesAsync(text);

            var articles = await _bus.Send(new GetArticlesQuery { Ids = duplicateIds });

            return articles.Select(x => new ArticleDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    CreatedTs = x.CreatedTs,
                    UpdatedTs = x.UpdatedTs
                })
                .ToList();
        }
    }
}