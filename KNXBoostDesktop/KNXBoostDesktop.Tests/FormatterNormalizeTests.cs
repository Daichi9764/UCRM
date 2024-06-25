using NUnit.Framework;
using KNXBoostDesktop;

namespace KNXBoostDesktop.Tests
{
    [TestFixture]
    public class FormatterNormalizeTests
    {
        private FormatterNormalize _formatter;

        [SetUp]
        public void Setup()
        {
            _formatter = new FormatterNormalize();
        }

        [Test]
        public void Format_InputIsNull_ReturnsEmptyString()
        {
            var result = _formatter.Format(null);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Format_InputIsWhiteSpace_ReturnsEmptyString()
        {
            var result = _formatter.Format("   ");
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Format_InputContainsUnderscores_ReplacesWithSpaces()
        {
            var result = _formatter.Format("hello_world");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputIsLowerCase_ReturnsCapitalizedWords()
        {
            var result = _formatter.Format("hello world");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputContainsAccents_RemovesAccents()
        {
            var result = _formatter.Format("héllo wörld");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputContainsSpecialCharacters_RemovesSpecialCharacters()
        {
            var result = _formatter.Format("hello, world!");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputContainsDoubleUnderscores_ReplacesWithSingleUnderscore()
        {
            var result = _formatter.Format("hello__world");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputContainsMultipleSpaces_ReplacesWithSingleUnderscore()
        {
            var result = _formatter.Format("hello   world");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputContainsSharpS_ReplacesWithSs()
        {
            var result = _formatter.Format("straße");
            Assert.That(result, Is.EqualTo("Strasse"));
        }

        [Test]
        public void Format_InputContainsDigits_KeepsDigits()
        {
            var result = _formatter.Format("hello 123 world");
            Assert.That(result, Is.EqualTo("Hello123World"));
        }

        [Test]
        public void Format_InputIsSingleLetter_ReturnsCapitalizedLetter()
        {
            var result = _formatter.Format("a");
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void Format_InputIsSingleAccentedLetter_RemovesAccentAndReturnsCapitalizedLetter()
        {
            var result = _formatter.Format("é");
            Assert.That(result, Is.EqualTo("E"));
        }

        [Test]
        public void Format_InputIsSingleLetterAndDigit_KeepsBoth()
        {
            var result = _formatter.Format("a1");
            Assert.That(result, Is.EqualTo("A1"));
        }

        [Test]
        public void Format_InputWithNonAlphabeticCharacters_RemovesNonAlphabeticCharacters()
        {
            var result = _formatter.Format("hello$$$world");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputWithVariousPunctuation_RemovesPunctuation()
        {
            var result = _formatter.Format("hello!@#world");
            Assert.That(result, Is.EqualTo("HelloWorld"));
        }

        [Test]
        public void Format_InputIsExtremelyLong_ReturnsCorrectlyFormattedString()
        {
            string longInput = new string('a', 10000);
            string expectedOutput = new string('A', 1) + new string('a', 9999);
            var result = _formatter.Format(longInput);
            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
