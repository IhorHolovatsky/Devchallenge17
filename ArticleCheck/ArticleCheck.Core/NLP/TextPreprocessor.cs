using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ArticleCheck.Core.NLP.Interfaces;
using Humanizer;

namespace ArticleCheck.Core.NLP
{
    public class TextPreprocessor : ITextPreprocessor
    {
        /// <summary>
        /// Punctuation are just unnecessary symbols which could appear in text.
        /// But there is concern about abbreviations which have dots.
        /// So if there is abbreviation "O.R" (o reilly) this will be converted to "or" after preprocessing
        /// </summary>
        private Regex PunctuationRegex { get; }

        /// <summary>
        /// Stop words are the most commonly occurring words which don’t give any additional value for determining text similarity
        /// Stop words should be in lower case
        /// </summary>
        private List<string> StopWords { get; }

        private readonly IStemmer _stemmer;

        public TextPreprocessor(IStemmer stemmer)
        {
            PunctuationRegex = new Regex(@"[!\""#\$%&\(\)\*\+-\.\/:;<=>\?@\[\]\^_`{\|}~\n]",
                RegexOptions.Multiline,
                TimeSpan.FromSeconds(5));

            StopWords = "i me my myself"
                .Split(' ')
                .ToList();

            _stemmer = stemmer;
        }

        public List<string> Tokenize(string text)
        {
            var words = text.Split(' ')
                                       .ToList();

            return Tokenize(words);
        }

        public List<string> Tokenize(List<string> words)
        {
            var tokens = words
                .Select(x => x.ToLowerInvariant())
                .Select(RemovePunctuation)
                .Select(RemoveApostrophe)
                .Where(RemoveStopWord)
                //.Where(RemoveSingleCharWord)
                .Aggregate(new List<string>(), ConvertNumbersToText)
                .Select(ApplyLemmatization)
                .Select(ApplyStemming)
                .ToList();

            return tokens;
        }


        #region Data preprocessing

        private string RemovePunctuation(string word)
        {
            return PunctuationRegex.Replace(word, string.Empty);
        }

        private string RemoveApostrophe(string word)
        {
            return word.Replace("'", string.Empty);
        }

        private bool RemoveStopWord(string word)
        {
            return !StopWords.Contains(word);
        }

        private bool RemoveSingleCharWord(string word)
        {
            return word.Length > 1;
        }

        private List<string> ConvertNumbersToText(List<string> words, string word)
        {
            var suffixes = new List<string> { "st", "nd", "rd", "th", "s" };
            var suffix = suffixes.FirstOrDefault(s => int.TryParse(word.Replace(s, string.Empty), out _));

            if (string.IsNullOrEmpty(suffix))
            {
                words.Add(word);

                return words;
            }

            var number = int.Parse(word.Replace(suffix, string.Empty));

            words.AddRange(number.ToWords(CultureInfo.CurrentCulture)
                .Split(' ')
                .Select(x => x.ToLowerInvariant())
                .Where(RemoveStopWord));

            return words;
        }

        /// <summary>
        /// I'm thinking that Lemmatization is not needed right now...
        /// To be honest, I don't have time implementing it
        /// </summary>
        private string ApplyLemmatization(string word)
        {
            return word;
        }

        private string ApplyStemming(string word)
        {
            return _stemmer.Stem(word);
        }

        #endregion
    }
}