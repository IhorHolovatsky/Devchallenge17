namespace ArticleCheck.Core.Models
{
    public class ArticleSearchDto
    {
        /// <summary>
        /// Return only unique articles (by default true)
        /// </summary>
        public bool IsUniqueOnly { get; set; } = true;
    }
}