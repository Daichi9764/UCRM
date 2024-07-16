using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KNXBoostDesktop
{
    /// <summary>
    /// Represents a console window in the application that displays console output in a TextBox control.
    /// Provides functionality to initialize the window with specific dimensions and an icon, 
    /// and to handle the window's close event by hiding the window instead of closing it.
    /// </summary>
    public partial class ConsoleWindow
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Boite de texte contenant les messages console affichés dans la fenêtre
        /// <summary>
        /// A TextWriter implementation that writes text to a TextBox control.
        /// </summary>
        public class TextBoxWriter(TextBox textBox) : TextWriter
        {
            /// <summary>
            /// Writes a single character to the TextBox.
            /// </summary>
            /// <param name="value">The character to write to the TextBox.</param>
            public override void Write(char value)
            {
                textBox.Dispatcher.Invoke(() => textBox.AppendText(value.ToString()));
            }

            /// <summary>
            /// Writes a string to the TextBox.
            /// </summary>
            /// <param name="value">The string to write to the TextBox.</param>
            public override void Write(string? value)
            {
                textBox.Dispatcher.Invoke(() => textBox.AppendText(value));
            }

            /// <summary>
            /// Gets the encoding in which the output is written.
            /// </summary>
            public override Encoding Encoding => Encoding.UTF8;
        }
        
        
        
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Constructeur
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWindow"/> class.
        /// Sets the size of the window, the icon, and redirects the console output to the TextBox.
        /// </summary>
        public ConsoleWindow()
        {
            InitializeComponent();
            
            Height = SystemParameters.PrimaryScreenHeight * 0.4;
            Width = SystemParameters.PrimaryScreenWidth * 0.5;
            
            Uri iconUri = new("pack://application:,,,/resources/ConsoleWindowIcon.ico", UriKind.RelativeOrAbsolute);
            Icon = BitmapFrame.Create(iconUri);
            
            Console.SetOut(new TextBoxWriter(ConsoleTextBox));
        }

        
        // Fonction modifiant le comportement de la fenêtre console lorsque l'on clique sur la croix de fermeture.
        /// <summary>
        /// Handles the event when the console window's close button is clicked.
        /// Prevents the window from closing and hides it instead.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ClosingConsoleWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // On annule la fermeture de la fenêtre
            this.Hide(); // On la cache à la place
            App.ConsoleAndLogWriteLine("Hiding console");
        }


        public void ApplyScaling(float scale)
        {
            ConsoleTextBox.FontSize *= scale;
        }
    }
}