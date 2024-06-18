using System.IO;
using System.Windows;

namespace KNXBoostDesktop
{
    public partial class App
    {
        public static StreamWriter writer = new StreamWriter($"./logs-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"); // Permet l'écriture du fichier de logging
        
        public static ProjectFileManager? Fm { get; private set; }
        
        public DisplayElements? DisplayElements { get; private set; }

        public Formatter? Formatter;

        
        
        protected override void OnStartup(StartupEventArgs e)
        {
            ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] STARTING KNX BOOST DESKTOP APP...");
            base.OnStartup(e);

            
            ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Opening main window");
            DisplayElements = new DisplayElements();
            DisplayElements.ShowMainWindow();
            
            
            ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Opening console window");
            DisplayElements.ShowConsoleWindow();
            
            
            ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Opening project file manager");
            Fm = new ProjectFileManager();

            
            //ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Opening string formatters");
            //formatter = new FormatterNormalize();
            
            
            ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] KNX BOOST DESKTOP APP STARTED !");
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            ConsoleAndLogWriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] CLOSING KNX BOOST DESKTOP APP...");
            base.OnExit(e);
            
            
            ConsoleAndLogWrite($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] KNX BOOST DESKTOP APP CLOSED !");
            writer.Close();
        }

        public static void ConsoleAndLogWrite(string msg)
        {
            Console.Write(msg);
            writer.Write(msg);
            writer.Flush(); // On vide le buffer du streamwriter au cas ou il resterait des caractères
        }

        public static void ConsoleAndLogWriteLine(string msg)
        {
            Console.WriteLine(msg);
            writer.WriteLine(msg);
            writer.Flush(); // On vide le buffer du streamwriter au cas ou il resterait des caractères
        }
    }
}
