namespace ArticleCheck.Infrastructure.Constants
{
    public static class RedisConstants
    {
        /// <summary>
        /// Microsoft's IDistributedCache uses it as a prefix for every key
        /// </summary>
        public const string InstanceName = "Articles:";
    }
}