using System.Collections.Generic;

namespace ArticleCheck.Core.NLP.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public List<string> Tokens { get; set; }
    }
}