using NUnit.Framework;
using System.Text;
using System.Windows.Controls;
using KNXBoostDesktop;

namespace KNXBoostDesktop.Tests
{
    [TestFixture]
    public class TextBoxWriterTests
    {
        private TextBox _textBox;
        private ConsoleWindow.TextBoxWriter _writer;

        [SetUp]
        public void Setup()
        {
            _textBox = new TextBox();
            _writer = new ConsoleWindow.TextBoxWriter(_textBox);
        }

        [TearDown]
        public void TearDown()
        {
            _writer.Dispose();
        }

        [Test]
        public void Write_Char_AppendsTextToTextBox()
        {
            // Arrange
            char value = 'A';

            // Act
            _writer.Write(value);

            // Assert
            Assert.AreEqual("A", _textBox.Text);
        }

        [Test]
        public void Write_String_AppendsTextToTextBox()
        {
            // Arrange
            string value = "Hello";

            // Act
            _writer.Write(value);

            // Assert
            Assert.AreEqual("Hello", _textBox.Text);
        }

        [Test]
        public void Encoding_ReturnsUtf8()
        {
            // Act
            Encoding encoding = _writer.Encoding;

            // Assert
            Assert.AreEqual(Encoding.UTF8, encoding);
        }
    }
}