using System.Collections.Generic;

namespace ArticleCheck.Core.NLP.Interfaces
{
    public interface ITextPreprocessor
    {
        List<string> Tokenize(string text);
        List<string> Tokenize(List<string> words);
    }
}