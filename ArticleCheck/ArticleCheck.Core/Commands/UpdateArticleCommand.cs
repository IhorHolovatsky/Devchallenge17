using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Commands
{
    public class UpdateArticleCommand : IRequest<Article>
    {
        public Article Article { get; set; }
    }
}