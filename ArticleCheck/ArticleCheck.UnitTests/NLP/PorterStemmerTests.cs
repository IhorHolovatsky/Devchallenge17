using ArticleCheck.Core.NLP.Stemming;
using ArticleCheck.UnitTests.Constants;
using NUnit.Framework;

namespace ArticleCheck.UnitTests.NLP
{
    public class PorterStemmerTests
    {
        [Test]
        [Category(UnitTestsConstants.Category)]
        [TestCase("doing", "do")]
        [TestCase("going", "go")]
        [TestCase("test's", "test")]
        [TestCase("test's'", "test")]
        [TestCase("test'", "test")]
        [TestCase("inbreedly", "inbre")]
        [TestCase("inbreed", "inbre")]
        [TestCase("luxuriated", "luxuri")]
        [TestCase("hoping", "hope")]
        public void Stem(string word, string wordExpected)
        {
            // Arrange
            var stemmer = new PorterStemmer();

            // Act
            var wordStemmed = stemmer.Stem(word);

            // Assert
            Assert.AreEqual(wordExpected, wordStemmed);
        }
    }
}