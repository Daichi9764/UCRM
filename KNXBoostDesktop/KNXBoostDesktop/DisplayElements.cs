using System.Windows;

namespace KNXBoostDesktop
{
    public class DisplayElements
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        public MainWindow MainWindow { get; } = new();
        public ConsoleWindow ConsoleWindow { get; } = new();
        public SettingsWindow SettingsWindow { get; private set; } = new();

        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Fonction pour ouvrir la fenêtre principale
        public void ShowMainWindow()
        {
            MainWindow.Show();
        }

        
        // Fonction pour ouvrir la fenêtre de la console
        public void ShowConsoleWindow()
        {
            ConsoleWindow.Show();
        }
        
        
        // Fonction pour ouvrir la fenêtre des paramètres
        public void ShowSettingsWindow()
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.Show();
        }

        public void CloseSettingsWindow()
        {
            SettingsWindow = null;
        }
    }
}
