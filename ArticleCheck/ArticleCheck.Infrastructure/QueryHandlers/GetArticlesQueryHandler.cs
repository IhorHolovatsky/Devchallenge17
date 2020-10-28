using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArticleCheck.Core.Queries;
using ArticleCheck.Domain.Models;
using Dapper;
using MediatR;

namespace ArticleCheck.Infrastructure.QueryHandlers
{
    public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, List<Article>>
    {
        private readonly IDatabaseContext _dbContext;

        public GetArticlesQueryHandler(IDatabaseContext databaseContext)
        {
            _dbContext = databaseContext;
        }

        public async Task<List<Article>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
        {
            var query = BuildQuery(request);

            return (await _dbContext.WithConnectionAsync(connection =>
                    connection.QueryAsync<Article>(query.RawSql, query.Parameters)))
                .ToList();
        }


        private SqlBuilder.Template BuildQuery(GetArticlesQuery request)
        {
            var builder = new SqlBuilder();
            var template = builder.AddTemplate(@"SELECT * FROM Articles /**where**/");

            if (request.Ids != null)
            {
                builder.Where("Id in @Ids", new { request.Ids });
            }

            return template;
        }
    }
}