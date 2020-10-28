using Newtonsoft.Json;

namespace ArticleCheck.Core.Models
{
    public class ArticleUpdateDto
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Content { get; set; }
    }
}