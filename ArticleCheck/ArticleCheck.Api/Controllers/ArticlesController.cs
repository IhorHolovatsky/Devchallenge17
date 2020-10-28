using System.Collections.Generic;
using System.Threading.Tasks;
using ArticleCheck.Core.Models;
using ArticleCheck.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArticleCheck.Api.Controllers
{
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet("articles")]
        public Task<List<ArticleDto>> Get([FromQuery] ArticleSearchDto searchDto)
        {
            return _articleService.SearchAsync(searchDto);
        }


        [HttpGet("articles/{id}")]
        public Task<ArticleDto> GetById(int id)
        {
            return _articleService.GetByIdAsync(id);
        }

        /// <summary>
        /// Add article to system
        /// </summary>
        [HttpPost("articles")]
        public Task<ArticleDto> Create([FromBody] ArticleCreateDto articleCreateDto)
        {
            return _articleService.CreateAsync(articleCreateDto);
        }

        /// <summary>
        /// Finding articles which are duplicates to passed text (without adding this text to system)
        /// </summary>
        [HttpPost("articles/check")]
        public Task<List<ArticleDto>> Check([FromBody] ArticleCreateDto body)
        {
            return _articleService.CheckDuplicatesAsync(body.Content);
        }

        [HttpPut("articles/{id}")]
        public Task<ArticleDto> Update(int id, [FromBody] ArticleUpdateDto articleUpdateDto)
        {
            articleUpdateDto.Id = id;

            return _articleService.UpdateAsync(articleUpdateDto);
        }

        [HttpDelete("articles/{id}")]
        public Task Delete(int id)
        {
            return _articleService.RemoveAsync(id);
        }


        [HttpGet("duplicate_groups")]
        public Task<ArticleDuplicatesDto> GetGroups()
        {
            return _articleService.GetDuplicateGroupsAsync();
        }
    }
}
