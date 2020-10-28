using ArticleCheck.Core.NLP;
using ArticleCheck.Core.NLP.Stemming;
using ArticleCheck.UnitTests.Constants;
using NUnit.Framework;

namespace ArticleCheck.UnitTests.NLP
{
    public class TextPreprocessorTests
    {
        [Test]
        [Category(UnitTestsConstants.Category)]
        [TestCase("I wan't to check TeXt 100 times", "want to check text one hundr time")]
        [TestCase("another example of doing this", "anoth exampl of do this")]
        public void Tokenize(string text, string textTokensExpected)
        {
            // Arrange
            var textPreprocessor = new TextPreprocessor(new PorterStemmer());

            // Act
            var tokens = textPreprocessor.Tokenize(text);

            // Assert
            Assert.AreEqual(textTokensExpected, string.Join(" ", tokens));
        }
    }
}