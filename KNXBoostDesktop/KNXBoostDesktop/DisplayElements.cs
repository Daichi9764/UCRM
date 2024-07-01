namespace KNXBoostDesktop
{
    public class DisplayElements
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        private MainWindow MainWindow { get; } = new();
        public ConsoleWindow ConsoleWindow { get; } = new();


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
    }
}
