using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ArticleCheck.Core.Configuration;

namespace ArticleCheck.Infrastructure
{
    public class DatabaseContext : IDatabaseContext
    {
        private readonly ConnectionStrings _connectionStrings;

        public DatabaseContext(ArticleCheckConfiguration configuration)
        {
            _connectionStrings = configuration?.ConnectionStrings;

        }


        public async Task<T> WithConnectionAsync<T>(Func<SqlConnection, Task<T>> getData)
        {
            using (var connection = new SqlConnection(_connectionStrings.ArticlesMssql))
            {
                await connection.OpenAsync();

                return await getData(connection);
            }
        }
    }
}