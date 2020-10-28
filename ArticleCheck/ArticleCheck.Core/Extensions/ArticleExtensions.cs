using System.Linq;
using ArticleCheck.Core.NLP.Models;
using ArticleCheck.Domain.Models;

namespace ArticleCheck.Core.Extensions
{
    public static class ArticleExtensions
    {
        public static Document ToDocument(this Article article)
        {
            return new Document
            {
                Id = article.Id,
                Content = article.Content,
                Tokens = (article.Tokens ?? string.Empty )
                                .Split(' ')
                                .ToList()
            };
        }
    }
}