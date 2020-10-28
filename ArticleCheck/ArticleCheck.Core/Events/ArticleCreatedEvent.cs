using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Events
{
    public class ArticleCreatedEvent : INotification
    {
        public Article Article { get; set; }

        public ArticleCreatedEvent(Article article)
        {
            Article = article;
        }
    }
}