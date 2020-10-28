using ArticleCheck.Domain.Models;
using MediatR;

namespace ArticleCheck.Core.Events
{
    public class ArticleUpdatedEvent : INotification
    {
        public Article ArticleOld { get; set; }
        public Article ArticleNew { get; set; }
    }
}