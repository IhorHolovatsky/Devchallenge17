using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArticleCheck.IntegrationTests.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<T> GetDataAsync<T>(this HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

    }
}