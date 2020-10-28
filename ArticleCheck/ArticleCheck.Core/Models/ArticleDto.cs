using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArticleCheck.Core.Models
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTs { get; set; }
        public DateTime? UpdatedTs { get; set; }


        [JsonProperty("duplicate_article_ids")]
        public List<int> DuplicateIds { get; set; }
    }
}