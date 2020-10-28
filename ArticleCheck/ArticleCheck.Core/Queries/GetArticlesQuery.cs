using System.Collections.Generic;
using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Queries
{
    public class GetArticlesQuery : IRequest<List<Article>>
    {
        public List<int> Ids { get; set; }
    }
}