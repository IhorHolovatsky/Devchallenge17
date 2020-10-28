using System.Threading;
using System.Threading.Tasks;
using ArticleCheck.Core.Queries;
using ArticleCheck.Domain.Models;
using Dapper;
using MediatR;

namespace ArticleCheck.Infrastructure.QueryHandlers
{
    public class GetArticleByIdQueryHandler : IRequestHandler<GetArticleByIdQuery, Article>
    {
        private readonly IDatabaseContext _dbContext;

        public GetArticleByIdQueryHandler(IDatabaseContext databaseContext)
        {
            _dbContext = databaseContext;
        }

        public Task<Article> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
        {
            var query = BuildQuery(request);

            return _dbContext.WithConnectionAsync(connection => connection.QueryFirstOrDefaultAsync<Article>(query.RawSql, query.Parameters));
        }

        private SqlBuilder.Template BuildQuery(GetArticleByIdQuery request)
        {
            var builder = new SqlBuilder();
            var template = builder.AddTemplate(@"SELECT TOP 1 * FROM Articles /**where**/");

            builder.Where("Id = @Id", new { request.Id });

            return template;
        }
    }
}