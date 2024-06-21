using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public ObservableCollection<TreeNode> OriginalNodes { get; set; }
    public ObservableCollection<TreeNode> ModifiedNodes { get; set; }
    
    
    
    
    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainWindow()
    {
        InitializeComponent();

        Title = App.AppName;

        Uri iconUri = new Uri("./icon.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);

        DataContext = this;

        OriginalNodes = new ObservableCollection<TreeNode>();
        ModifiedNodes = new ObservableCollection<TreeNode>();

        LoadXmlData(@".\Adresses_de_groupes_villa.xml", OriginalNodes);
        LoadXmlData(@".\\Adresses_de_groupes_villa.xml", ModifiedNodes);
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
            
            App.Fm.ExtractProjectFiles(openFileDialog.FileName);
            
            App.Fm.FindZeroXml();
            MyNameCorrector.CorrectName();
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