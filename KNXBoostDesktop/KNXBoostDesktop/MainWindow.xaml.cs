using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;

namespace KNXBoostDesktop;

public partial class MainWindow

{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public ObservableCollection<TreeNode> OriginalNodes { get; }
    public ObservableCollection<TreeNode> ModifiedNodes { get; }
    
    
    
    
    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainWindow()
    {
        InitializeComponent();

        Title = $"{App.AppName} v{App.AppVersion}";

        Uri iconUri = new Uri("./icon.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);

        DataContext = this;

        OriginalNodes = new ObservableCollection<TreeNode>();
        ModifiedNodes = new ObservableCollection<TreeNode>();

        LoadXmlData(@".\Adresses_de_groupes_villa.xml", OriginalNodes);
        LoadXmlData(@".\Adresses_de_groupes_villa.xml", ModifiedNodes);
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

            // Si le file manager n'existe pas ou que l'on n'a pas réussi à extraire les fichiers du projet, on annule l'opération
            if ((App.Fm == null)||(!App.Fm.ExtractProjectFiles(openFileDialog.FileName))) return;
            
            App.Fm.FindZeroXml();
            MyNameCorrector.CorrectName();
            ExportUpdatedNameAddresses.Export();
        }
        else
        {
            App.ConsoleAndLogWriteLine("User aborted the file selection operation");
        }
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


    private void OpenGroupAddressFileButtonClick(object sender, RoutedEventArgs e)
    {
        App.ConsoleAndLogWriteLine($"Opening {App.Fm?.ProjectFolderPath}0_updated.xml externally");

        // Résoudre le chemin absolu
        string absoluteFilePath = Path.GetFullPath($"{App.Fm?.ProjectFolderPath}0_updated.xml");

        // Vérifier si le fichier existe
        if (File.Exists(absoluteFilePath))
        {
            try
            {
                // Ouvrir le fichier avec l'application par défaut
                Process.Start(new ProcessStartInfo
                {
                    FileName = absoluteFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"Failed to open file: {ex.Message}");
            }
        }
        else
        {
            App.ConsoleAndLogWriteLine($"The file {absoluteFilePath} does not exist.");
        }
    }
    
    
    private void ExportModifiedProjectButtonClick(object sender, RoutedEventArgs e)
    {
        string sourceFilePath = $"{App.Fm?.ProjectFolderPath}0_updated.xml";
        App.ConsoleAndLogWriteLine($"User is exporting {sourceFilePath}");
        
        // Vérifier si le fichier source existe
        if (!File.Exists(sourceFilePath))
        {
            App.ConsoleAndLogWriteLine($"The source file {sourceFilePath} does not exist.");
            return;
        }

        // Initialiser et configurer le SaveFileDialog
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            FileName = "0_updated.xml", // Nom de fichier par défaut
            DefaultExt = ".xml", // Extension par défaut
            Filter = "XML files (.xml)|*.xml|All files (*.*)|*.*" // Filtre des types de fichiers
        };

        // Afficher le dialogue et vérifier si l'utilisateur a sélectionné un emplacement
        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            // Chemin du fichier sélectionné par l'utilisateur
            string destinationFilePath = saveFileDialog.FileName;
            App.ConsoleAndLogWriteLine($"Destination path selected: {destinationFilePath}");

            try
            {
                // Copier le fichier source à l'emplacement sélectionné par l'utilisateur
                File.Copy(sourceFilePath, destinationFilePath, true);
                App.ConsoleAndLogWriteLine($"File saved successfully at {destinationFilePath}.");
            }
            catch (Exception ex)
            {
                // Gérer les exceptions et afficher un message d'erreur
                App.ConsoleAndLogWriteLine($"Failed to save the file: {ex.Message}");
            }
        }
    }
    

    private void ClosingMainWindow(object sender, CancelEventArgs e)
    {
        Application.Current.Shutdown();
    }
    

    private void LoadXmlData(string filePath, ObservableCollection<TreeNode> nodesCollection)
    {
        try
        {
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            XDocument doc = XDocument.Load(fullPath);
            XElement rootElement = doc.Root;
            if (rootElement != null)
            {
                nodesCollection.Clear();
                foreach (var childElement in rootElement.Elements())
                {
                    TreeNode rootNode = ParseXmlNode(childElement);
                    nodesCollection.Add(rootNode);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading XML file {filePath}: {ex.Message}");
        }
    }

    
    private TreeNode ParseXmlNode(XElement element)
    {
        var treeNode = new TreeNode(element.Attribute("Name")?.Value ?? "Unnamed");

        foreach (var childElement in element.Elements())
        {
            if (childElement.Name.LocalName == "GroupRange" || childElement.Name.LocalName == "GroupAddress")
            {
                treeNode.Children.Add(ParseXmlNode(childElement));
            }
        }

        return treeNode;
    }
    
    
    // Pour la future synchronisation des arbres
    private void AdressesDeGroupesOriginales_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
    }
    private void AdressesDeGroupesModifiées_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
    }

}

public class TreeNode
{
    public string Name { get; set; }
    public ObservableCollection<TreeNode> Children { get; set; }

    public TreeNode(string name)
    {
        Name = name;
        Children = new ObservableCollection<TreeNode>();
    }
}