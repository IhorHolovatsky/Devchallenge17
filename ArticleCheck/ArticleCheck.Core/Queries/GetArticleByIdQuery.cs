using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Queries
{
    public class GetArticleByIdQuery : IRequest<Article>
    {
        public int Id { get; set; }

        public GetArticleByIdQuery(int id)
        {
            Id = id;
        }
    }
}