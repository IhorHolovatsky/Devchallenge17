using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ArticleCheck.Infrastructure
{
    public interface IDatabaseContext
    {
        Task<T> WithConnectionAsync<T>(Func<SqlConnection, Task<T>> getData);
    }
}