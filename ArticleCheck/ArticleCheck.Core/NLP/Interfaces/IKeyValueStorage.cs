using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArticleCheck.Core.NLP.Interfaces
{
    public interface IKeyValueStorage
    {
        Task SetAsync<T>(string key, T item);

        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);

        Task RemoveAsync(string key);

        bool Contains(string key);
        Task<bool> ContainsAsync(string key);
    }
}