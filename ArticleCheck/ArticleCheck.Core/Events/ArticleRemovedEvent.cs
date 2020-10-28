using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Events
{
    public class ArticleRemovedEvent : INotification
    {
        public Article Article { get; set; }

        public ArticleRemovedEvent(Article article)
        {
            Article = article;
        }
    }
}