using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Commands
{
    public class CreateArticleCommand : IRequest<Article>
    {
        public Article Article { get; set; }

        public CreateArticleCommand(Article article)
        {
            Article = article;
        }
    }
}