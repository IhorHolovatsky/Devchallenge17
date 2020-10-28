using System.Collections.Generic;
using System.Threading.Tasks;
using ArticleCheck.Domain.Models;

namespace ArticleCheck.Core.Services.Interfaces
{
    public interface IArticleSimilarityService
    {
        /// <summary>
        /// Initialize NLP Model in cache from passed articles
        /// </summary>
        Task InitAsync(List<Article> articles);

        Task<List<int[]>> GetSimilarityGroupsAsync(List<Article> articles);
        
        /// <summary>
        /// Finding articles duplicates and persist it in cache
        /// </summary>
        Task<List<int>> FindDuplicatesAsync(Article article);

        /// <summary>
        /// Finding text duplicates (without persisting text in cache)
        /// </summary>
        Task<List<int>> FindDuplicatesAsync(string text);


        string Tokenize(Article article);
        string Tokenize(string text);
    }
}