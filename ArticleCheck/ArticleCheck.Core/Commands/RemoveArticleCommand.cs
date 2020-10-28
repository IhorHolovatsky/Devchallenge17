using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Commands
{
    public class RemoveArticleCommand : IRequest<int>
    {
        public Article Article { get; set; }
    }
}