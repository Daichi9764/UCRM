using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace KNXBoostDesktop;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title = App.AppName;

        Uri iconUri = new Uri("./icon.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);
    }
    
    private void ClosingMainWindow(object sender, CancelEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ImportProjectButtonClick(object sender, RoutedEventArgs e)
    {
        App.ConsoleAndLogWriteLine("Waiting for user to select KNX project file");
        
        // Créer une nouvelle instance de OpenFileDialog
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            // Définir des propriétés optionnelles
            Title = "Sélectionnez un projet KNX à importer",
            Filter = "ETS KNX Project File (*.knxproj)|*.knxproj|other file|*.*",
            FilterIndex = 1,
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        bool? result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            App.ConsoleAndLogWriteLine($"File selected: {openFileDialog.FileName}");

            if (App.Fm == null) return;
            
            App.Fm.KnxprojSourceFilePath = openFileDialog.FileName;
            App.Fm.ExtractProjectFiles();

            //findZeroXmlButton.Visibility = Visibility.Visible;
        }
        else
        {
            App.ConsoleAndLogWriteLine("User aborted the file selection operation");
        }
    }
    
    private void CloseProgramButtonClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
    
    private void OpenConsoleButtonClick(object sender, RoutedEventArgs e)
    {
        if (App.DisplayElements == null) return;
        
        App.ConsoleAndLogWriteLine("Opening console window");
        App.DisplayElements.ShowConsoleWindow();
            
        // Pour éviter qu'à la réouverture de la console on ait quelques lignes de retard, on scrolle en bas dès l'ouverture
        if (App.DisplayElements.ConsoleWindow.IsVisible)
        {
            App.DisplayElements.ConsoleWindow.ConsoleTextBox.ScrollToEnd();
        }
    }
}