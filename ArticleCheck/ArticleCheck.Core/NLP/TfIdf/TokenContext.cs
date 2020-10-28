using System.Collections.Generic;

namespace ArticleCheck.Core.NLP.TfIdf
{
    public class TokenContext
    {
        public TokenContext()
        {
            DocumentIds = new List<int>();
        }

        public int Frequency => DocumentIds.Count;

        public List<int> DocumentIds { get; set; }

    }
}