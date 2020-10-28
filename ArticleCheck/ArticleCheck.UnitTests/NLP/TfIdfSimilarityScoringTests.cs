using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArticleCheck.Core.NLP;
using ArticleCheck.Core.NLP.Interfaces;
using ArticleCheck.Core.NLP.Models;
using ArticleCheck.Core.NLP.Stemming;
using ArticleCheck.Core.NLP.TfIdf;
using ArticleCheck.UnitTests.Constants;
using ArticleCheck.UnitTests.Implementations;
using NUnit.Framework;

namespace ArticleCheck.UnitTests.NLP
{
    public class TfIdfSimilarityScoringTests
    {
        [Test]
        [Category(UnitTestsConstants.Category)]
        [TestCase("dogs likes people")]
        public async Task FindDuplicates(string text)
        {
            // Arrange
            var stemmer = new PorterStemmer();
            var textPreprocessor = new TextPreprocessor(stemmer);
            var document = new Document
            {
                Id = 3,
                Tokens = textPreprocessor.Tokenize(text)
            };
            var tfIdfSimilarityScoring = await BuildService(textPreprocessor);

            // Act
            var scores = await tfIdfSimilarityScoring.GetSimilarityScoresAsync(document);

            // Assert
            Assert.IsTrue(scores.Any(s => s.Score > 0.5d));
        }


        private async Task<TfIdfSimilarityScoring> BuildService(ITextPreprocessor textPreprocessor)
        {
            var documents = new List<Document>
                {
                    new Document
                    {
                        Id = 1,
                        Content = "dog can bite your cat"
                    },
                    new Document
                    {
                        Id = 2,
                        Content = "dogs likes people more than other animals"
                    },
                }
                .Select(x =>
                {
                    x.Tokens = textPreprocessor.Tokenize(x.Content);
                    return x;
                })
                .ToList();

            var service = new TfIdfSimilarityScoring(textPreprocessor, new InMemoryKeyValueStorage());

            await service.InitAsync(documents);

            return service;
        }
    }
}