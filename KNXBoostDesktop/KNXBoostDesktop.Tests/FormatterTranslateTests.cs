using NUnit.Framework;
using System.Threading.Tasks;
using Polly.Caching;

namespace KNXBoostDesktop.Tests
{
    [TestFixture]
    public class FormatterTranslateTests
    {
        [Test]
        public void Format_TranslatesTextFromFrenchToEnglish()
        {
            // Arrange
            string inputText = "Bonjour le monde"; // Texte à traduire
            var formatter = new FormatterTranslate();

            // Act
            string translatedText = formatter.Format(inputText);

            // Assert
            Assert.IsNotNull(translatedText);
            Assert.IsNotEmpty(translatedText);

            Assert.AreNotEqual(inputText, translatedText); // Vérifie que la traduction est différente du texte d'entrée
            
            Console.WriteLine(inputText);
            Console.WriteLine(translatedText);
        }

        [Test]
        public async Task GetTranslatedStringAsync_ReturnsNonEmptyString()
        {
            // Arrange
            string inputText = "Bonjour le monde"; // Texte à traduire

            // Act
            string translatedText = await FormatterTranslate.GetTranslatedStringAsync(inputText);

            // Assert
            Assert.NotNull(translatedText);
            Assert.IsNotEmpty(translatedText);
            
            Console.WriteLine(inputText);
            Console.WriteLine(translatedText);
        }
        
        [Test]
        public void Format_TranslatesTextWithNumbers()
        {
            // Arrange
            string inputText = "Il y a 123 pommes qui sont rouges."; // Texte à traduire avec des nombres
            var formatter = new FormatterTranslate();

            // Act
            string translatedText = formatter.Format(inputText);

            // Assert
            Assert.NotNull(translatedText);
            Assert.IsNotEmpty(translatedText);
            Assert.AreNotEqual(inputText, translatedText); // Vérifie que la traduction est différente du texte d'entrée
            
            Console.WriteLine(inputText);
            Console.WriteLine(translatedText);
        }

        [Test]
        public void Format_TranslatesTextWithEmojisAndSpecialSymbols()
        {
            // Arrange
            string inputText = "😊 Merci beaucoup © 2023"; // Texte à traduire avec emojis et symboles spéciaux
            var formatter = new FormatterTranslate();

            // Act
            string translatedText = formatter.Format(inputText);

            // Assert
            Assert.NotNull(translatedText);
            Assert.IsNotEmpty(translatedText);
            Assert.AreNotEqual(inputText, translatedText); // Vérifie que la traduction est différente du texte d'entrée
            
            Console.WriteLine(inputText);
            Console.WriteLine(translatedText);
        }

        [Test]
        public void Format_TranslatesIdiomaticAndComplexPhrases()
        {
            // Arrange
            string inputText = "C'est la fin du monde."; // Phrase idiomatique à traduire
            var formatter = new FormatterTranslate();

            // Act
            string translatedText = formatter.Format(inputText);

            // Assert
            Assert.NotNull(translatedText);
            Assert.IsNotEmpty(translatedText);
            Assert.AreNotEqual(inputText, translatedText); // Vérifie que la traduction est différente du texte d'entrée
            
            Console.WriteLine(inputText);
            Console.WriteLine(translatedText);
        }
    }
}