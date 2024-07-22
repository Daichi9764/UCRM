namespace KNXBoostDesktop
{
    public class DisplayElements
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        /// <summary>
        /// Represents the main window instance of the application.
        /// </summary>
        public MainWindow MainWindow { get; } = new();
        
        /// <summary>
        /// Represents the settings window instance where application settings are configured.
        /// </summary>
        public SettingsWindow? SettingsWindow { get; } = new();
        
        /// <summary>
        /// Represents the window for renaming group addresses within the application.
        /// </summary>
        public GroupAddressRenameWindow GroupAddressRenameWindow { get; } = new();

        /// <summary>
        /// Represents the window displayed during application loading processes.
        /// </summary>
        public LoadingWindow? LoadingWindow;
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        /// <summary>
        /// Shows the main window of the application.
        /// </summary>
        public void ShowMainWindow()
        {
            MainWindow.Show();
        }
        
        
        /// <summary>
        /// Shows the settings window of the application if it is available.
        /// </summary>
        public void ShowSettingsWindow()
        {
            SettingsWindow?.Show();
        }

        /// <summary>
        /// Shows the loading window of the application if it is available.
        /// </summary>
        public void ShowLoadingWindow()
        {
            LoadingWindow?.Show();
        }

        /// <summary>
        /// Shows the group address rename window, allowing the user to edit and confirm changes to a group address.
        /// </summary>
        /// <param name="addressOriginal">The original group address.</param>
        /// <param name="addressEdited">The edited group address.</param>
        /// <returns>The dialog result of the group address rename window.</returns>
        public bool? ShowGroupAddressRenameWindow(string addressOriginal, string addressEdited, string xmlRenameFilePath)
        {
            GroupAddressRenameWindow.SetAddress(addressOriginal, addressEdited);
            GroupAddressRenameWindow.ShowDialog();
            GroupAddressRenameWindow.SetPath(xmlRenameFilePath);

            if (MainWindow != null)
            {
                GroupAddressRenameWindow.Owner = MainWindow;
            }


            return GroupAddressRenameWindow.DialogResult;
        }
    } 
    
    // masquer boutons en bas quand onglet debug
}
