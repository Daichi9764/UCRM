using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KNXBoostDesktop
{
    public partial class ConsoleWindow
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Boite de texte contenant les messages console affichés dans la fenêtre
        public class TextBoxWriter(TextBox textBox) : TextWriter
        {
            public override void Write(char value)
            {
                textBox.Dispatcher.Invoke(() => textBox.AppendText(value.ToString()));
            }

            public override void Write(string? value)
            {
                textBox.Dispatcher.Invoke(() => textBox.AppendText(value));
            }

            public override Encoding Encoding => Encoding.UTF8;
        }
        
        
        
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Constructeur
        public ConsoleWindow()
        {
            InitializeComponent();
            
            Height = SystemParameters.PrimaryScreenHeight * 0.4;
            Width = SystemParameters.PrimaryScreenWidth * 0.5;
            
            Uri iconUri = new("pack://application:,,,/resources/ConsoleWindowIcon.ico", UriKind.RelativeOrAbsolute);
            Icon = BitmapFrame.Create(iconUri);
            
            Console.SetOut(new TextBoxWriter(ConsoleTextBox));
        }

        
        // Fonction modifiant le comportement de la fenêtre console lorsque l'on clique sur la croix de fermeture
        private void ClosingConsoleWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // On annule la fermeture de la fenêtre
            this.Hide(); // On la cache à la place
            App.ConsoleAndLogWriteLine("Hiding console");
        }
    }
}