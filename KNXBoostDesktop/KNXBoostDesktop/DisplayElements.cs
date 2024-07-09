namespace KNXBoostDesktop
{
    public class DisplayElements
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        public MainWindow MainWindow { get; } = new();
        public ConsoleWindow ConsoleWindow { get; } = new();
        public SettingsWindow? SettingsWindow { get; } = new();
        public GroupAddressRenameWindow GroupAddressRenameWindow { get; } = new();

        public LoadingWindow? LoadingWindow;
        
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
            SettingsWindow?.Show();
        }

        public void ShowLoadingWindow()
        {
            LoadingWindow?.Show();
        }

        // Fonction pour ouvrir la fenêtre de renommage des adresses de groupe
        public bool? ShowGroupAddressRenameWindow(string address)
        {
            GroupAddressRenameWindow.SetAddress(address);
            GroupAddressRenameWindow.ShowDialog();
            return GroupAddressRenameWindow.DialogResult;
        }
    }
}
