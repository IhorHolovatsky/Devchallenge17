using System.Collections.Generic;
using System.Threading.Tasks;
using ArticleCheck.Core.Models;

namespace ArticleCheck.Core.Services.Interfaces
{
    public interface IArticleService
    {
        Task<ArticleDuplicatesDto> GetDuplicateGroupsAsync();
        Task<List<ArticleDto>> CheckDuplicatesAsync(string text);
        Task<ArticleDto> GetByIdAsync(int id);
        Task<List<ArticleDto>> SearchAsync(ArticleSearchDto searchDto);

        Task<ArticleDto> CreateAsync(ArticleCreateDto articleCreateDto);
        Task<ArticleDto> UpdateAsync(ArticleUpdateDto articleUpdateDto);
        Task RemoveAsync(int id);
    }
}