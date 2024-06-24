using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;

namespace KNXBoostDesktop;

public partial class MainWindow

{
    public ObservableCollection<TreeNode> Nodes { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        Title = App.AppName;

        Uri iconUri = new Uri("./icon.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);

        Nodes = new ObservableCollection<TreeNode>();
        DataContext = this;

        LoadXmlData(@"D:\OneDrive - INSA Toulouse\Documents\Cours\INSA\Stage4A\Projet\UCRM\KNXBoostDesktop\KNXBoostDesktop\bin\Debug\net8.0-windows\Adresses_de_groupes_villa.xml"); // Chemin du fichier original d'adresses de groupe
    }

    private void ImportProjectButtonClick(object sender, RoutedEventArgs e)
    {
        App.ConsoleAndLogWriteLine("Waiting for user to select KNX project file");

        // Créer une nouvelle instance de OpenFileDialog
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            // Définir des propriétés optionnelles
            Title = "Sélectionnez un projet KNX à importer",
            Filter = "ETS KNX Project File (.knxproj)|.knxproj|other file|.",
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

    private void FindZeroXmlButtonClick(object sender, RoutedEventArgs e)
    {
        App.Fm.FindZeroXml();
        
        //checkMarkImage.Visibility = Visibility.Visible;
        
        //findZeroXmlButton.Visibility = Visibility.Collapsed;
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

    private void LoadXmlData(string filePath)
    {
        try
        {
            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            XDocument doc = XDocument.Load(fullPath);
            XElement rootElement = doc.Root;
            if (rootElement != null)
            {
                Nodes.Clear();
                foreach (var childElement in rootElement.Elements())
                {
                    TreeNode rootNode = ParseXmlNode(childElement);
                    Nodes.Add(rootNode);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading XML file: {ex.Message}");
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