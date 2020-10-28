using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArticleCheck.Core.Models
{
    public class ArticleDuplicatesDto
    {
        [JsonProperty("duplicate_groups")]
        public List<int[]> Groups { get; set; }
    }
}