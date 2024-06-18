using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KNXBoostDesktop
{
    public partial class ConsoleWindow
    {
        public ConsoleWindow()
        {
            InitializeComponent();
            
            Height = SystemParameters.PrimaryScreenHeight * 0.4;
            Width = SystemParameters.PrimaryScreenWidth * 0.5;
            
            Uri iconUri = new Uri("./ConsoleWindowIcon.ico", UriKind.RelativeOrAbsolute);
            Icon = BitmapFrame.Create(iconUri);
            
            Console.SetOut(new TextBoxWriter(ConsoleTextBox));
        }

        public class TextBoxWriter : TextWriter
        {
            private readonly TextBox _textBox;

            public TextBoxWriter(TextBox textBox)
            {
                _textBox = textBox;
            }

            public override void Write(char value)
            {
                _textBox.Dispatcher.Invoke(() => _textBox.AppendText(value.ToString()));
            }

            public override void Write(string value)
            {
                _textBox.Dispatcher.Invoke(() => _textBox.AppendText(value));
            }

            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}