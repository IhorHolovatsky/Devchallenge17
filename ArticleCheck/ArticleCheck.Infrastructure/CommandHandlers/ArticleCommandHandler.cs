using System;
using System.Threading;
using System.Threading.Tasks;
using ArticleCheck.Core.Commands;
using ArticleCheck.Domain.Models;
using Dapper;
using MediatR;

namespace ArticleCheck.Infrastructure.CommandHandlers
{
    public class ArticleCommandHandler : IRequestHandler<CreateArticleCommand, Article>,
        IRequestHandler<UpdateArticleCommand, Article>,
        IRequestHandler<RemoveArticleCommand, int>
    {
        private readonly IDatabaseContext _dbContext;

        public ArticleCommandHandler(IDatabaseContext databaseContext)
        {
            _dbContext = databaseContext;
        }

        public Task<Article> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
        {
            request.Article.CreatedTs = DateTime.UtcNow;

            return _dbContext.WithConnectionAsync(connection => connection.QueryFirstAsync<Article>(InsertSql, request.Article));
        }

        public Task<Article> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
        {
            request.Article.UpdatedTs = DateTime.UtcNow;

            return _dbContext.WithConnectionAsync(connection => connection.QueryFirstAsync<Article>(UpdateSql, request.Article));
        }

        public Task<int> Handle(RemoveArticleCommand request, CancellationToken cancellationToken)
        {
            return _dbContext.WithConnectionAsync(connection => connection.ExecuteAsync(RemoveSql, request.Article));
        }


        private const string InsertSql = @"INSERT INTO [dbo].[articles]
                                  ([Content]
                                  ,[Tokens]
                                  ,[CreatedTs])
                            OUTPUT INSERTED.*
                            VALUES
                                  (@Content
                                  ,@Tokens
                                  ,@CreatedTs)";

        private const string UpdateSql = @"UPDATE [dbo].[articles]
                            SET Content=@Content
                                ,Tokens=@Tokens
                                ,UpdatedTs=@UpdatedTs
                            OUTPUT INSERTED.*
                            WHERE Id = @Id";

        private const string RemoveSql = @"DELETE FROM [dbo].[articles] WHERE Id = @Id";
    }
}