using System;

namespace ArticleCheck.Domain.Models
{
    public class Article 
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string Tokens { get; set; }
        public DateTime CreatedTs { get; set; }
        public DateTime? UpdatedTs { get; set; }
    }
}