namespace ArticleCheck.Core.Configuration
{
    public class ArticleCheckConfiguration
    {
        public double MatchingThreshold { get; set; }

        public ConnectionStrings ConnectionStrings { get; set; }

        public ArticleCheckConfiguration()
        {
            ConnectionStrings = new ConnectionStrings();
        }
    }
}