using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace KNXBoostDesktop;

public partial class MainWindow : Window

{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    //private readonly string xmlFilePath1 = App.Fm?.ProjectFolderPath + "GroupAddresses.xml"; 
    private string xmlFilePath1;
    
    private string xmlFilePath2;

    public MainViewModel ViewModel { get; set; }


    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainWindow()
    {   
        InitializeComponent();
        //LoadXmlFiles();
        
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        //ViewModel.IsProjectImported = false;

        Title = $"{App.AppName} v{App.AppVersion}";

        Uri iconUri = new ("./resources/icon.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);
        
        

        //DataContext = this;
    }
    //--------------------- Gestion des boutons -----------------------------------------------------//

    private void ImportProjectButtonClick(object sender, RoutedEventArgs e)
    {
        App.ConsoleAndLogWriteLine("Waiting for user to select KNX project file");
        
        // Créer une nouvelle instance de OpenFileDialog
        OpenFileDialog openFileDialog = new()
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
            xmlFilePath1 = $"{App.Fm?.ProjectFolderPath}/GroupAddresses.xml";
            xmlFilePath2 = App.Fm?.ProjectFolderPath + "UpdatedGroupAddresses.xml"; 
            //Define the project path
            ExportUpdatedNameAddresses.Export(App.Fm?.ZeroXmlPath,App.Fm?.ProjectFolderPath + "/GroupAddresses.xml");
            ExportUpdatedNameAddresses.Export(App.Fm?.ProjectFolderPath + "/0_updated.xml",App.Fm?.ProjectFolderPath + "/UpdatedGroupAddresses.xml");
            LoadXmlFiles();

            ViewModel.IsProjectImported = true;
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
        SaveFileDialog saveFileDialog = new()
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

    private void OpenParameters(object sender, RoutedEventArgs e)
    {
        App.DisplayElements!.ShowSettingsWindow();
    }


    //--------------------- Gestion de l'affichage à partir de fichiers -------------------------------//

    private void LoadXmlFiles()
    {
        LoadXmlFile(xmlFilePath1, treeView1);
        LoadXmlFile(xmlFilePath2, treeView2);
    }
    
    private static void LoadXmlFile(string filePath, TreeView treeView)
        {
            try
            {
                XmlDocument xmlDoc = new();
                xmlDoc.Load(filePath);

                treeView.Items.Clear();

                // Ajouter tous les nœuds récursivement
                if (xmlDoc.DocumentElement != null)
                {
                    foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                    {
                        AddNodeRecursively(node, treeView.Items, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    private static void AddNodeRecursively(XmlNode xmlNode, ItemCollection parentItems, int level)
    {
        if (xmlNode.NodeType == XmlNodeType.Element)
        {
            TreeViewItem treeNode = CreateTreeViewItemFromXmlNode(xmlNode, level);

            parentItems.Add(treeNode);

            // Parcourir récursivement les enfants
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                AddNodeRecursively(childNode, treeNode.Items, level + 1);
            }
        }
    }

    private static TreeViewItem CreateTreeViewItemFromXmlNode(XmlNode xmlNode, int level)
    {
        StackPanel stack = new StackPanel { Orientation = Orientation.Horizontal };

        // Définir l'icône en fonction du niveau
        Image icon = new Image
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 5, 0)
        };

        switch (level)
        {
            case 0:
                icon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level.png"));
                break;
            case 1:
                icon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level2.png"));
                break;
            case 2:
                icon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level3.png"));
                break;
            default:
                icon.Source = new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level3.png"));
                break;
        }

        TextBlock text = new TextBlock { Text = ((XmlElement)xmlNode).GetAttribute("Name") };

        stack.Children.Add(icon);
        stack.Children.Add(text);

        TreeViewItem treeNode = new() { Header = stack };
        return treeNode;
    }
    

    //-------------------- Gestion du scroll verticale synchronisé ------------------------------------//

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer changedScrollViewer)
        {
            // Défilement horizontal
            if (changedScrollViewer == scrollViewer1 && e.HorizontalChange != 0)
            {
                scrollViewer2.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
            }
            else if (changedScrollViewer == scrollViewer2 && e.HorizontalChange != 0)
            {
                scrollViewer1.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
            }

            // Défilement vertical
            if (changedScrollViewer == scrollViewer1 && e.VerticalChange != 0)
            {
                scrollViewer2.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
            }
            else if (changedScrollViewer == scrollViewer2 && e.VerticalChange != 0)
            {
                scrollViewer1.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
            }
        }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            // Vérifier si Ctrl est enfoncé
            bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            // Défilement horizontal
            if (isShiftPressed)
            {
                if (scrollViewer == scrollViewer1)
                {
                    scrollViewer1.ScrollToHorizontalOffset(scrollViewer1.HorizontalOffset - e.Delta);
                }
                else if (scrollViewer == scrollViewer2)
                {
                    scrollViewer2.ScrollToHorizontalOffset(scrollViewer2.HorizontalOffset - e.Delta);
                }
            }

            // Défilement vertical avec Ctrl enfoncé
            if (!isShiftPressed)
            {
                if (scrollViewer == scrollViewer1)
                {
                    scrollViewer1.ScrollToVerticalOffset(scrollViewer1.VerticalOffset - e.Delta);
                }
                else if (scrollViewer == scrollViewer2)
                {
                    scrollViewer2.ScrollToVerticalOffset(scrollViewer2.VerticalOffset - e.Delta);
                }

                e.Handled = true; // Indiquer que l'événement a été géré
            }
        }
    }
    
    
    // CHANGMENTS NATHAN
    
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        TextBox tb = sender as TextBox;
        if (tb.Text == "Chercher...")
        {
            tb.Text = "";
            tb.Foreground = new SolidColorBrush(Colors.Black);
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        TextBox tb = sender as TextBox;
        // Utiliser un Dispatcher pour s'assurer que le TextBox a réellement perdu le focus
        tb.Dispatcher.BeginInvoke(new Action(() => {
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Chercher...";
                tb.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    //-------------------- Gestion de la recherche ---------------------------------------------------//

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
     
        if (txtSearch1.Text != "Chercher...")
        {
            string normalizedSearchText = NormalizeString(txtSearch1.Text);

            // Filtrer et masquer les éléments de la TreeView basé sur le texte de recherche
            foreach (object obj in treeView1.Items)
            {
                if (obj is TreeViewItem item)
                {
                    item.Visibility = Visibility.Visible;
                    FilterTreeViewItems(item, normalizedSearchText);
                }
            }
        }
    }

    private static bool FilterTreeViewItems(TreeViewItem item, string searchText)
    {
        bool itemVisible = false; // Indicateur pour déterminer si l'élément est visible

        // Extraire le texte du TextBlock dans le Header du TreeViewItem
        string? headerText = null;
        if (item.Header is StackPanel headerStack)
        {
            var textBlock = headerStack.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBlock != null)
            {
                headerText = textBlock.Text;
            }
        }

        if (headerText == null)
        {
            item.Visibility = Visibility.Collapsed;
            return false; // Si l'entête est null, l'élément n'est pas visible
        }

        string normalizedHeader = NormalizeString(headerText);

        // Vérifier si l'élément correspond au texte de recherche
        if (normalizedHeader.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            item.Visibility = Visibility.Visible; // Rendre visible l'élément
            item.IsExpanded = true; // Développer l'élément pour montrer les enfants correspondants
            itemVisible = true; // Indiquer que l'élément est visible
        }
        else
        {
            item.Visibility = Visibility.Collapsed; // Masquer l'élément si le texte ne correspond pas
        }

        // Filtrer récursivement les enfants
        bool hasVisibleChild = false;
        foreach (object obj in item.Items)
        {
            if (obj is TreeViewItem childItem)
            {
                // Appliquer le filtre aux enfants et mettre à jour l'indicateur de visibilité
                bool childVisible = FilterTreeViewItems(childItem, searchText);
                if (childVisible)
                {
                    hasVisibleChild = true;
                    item.IsExpanded = true; // Développer l'élément si un enfant est visible
                }
            }
        }

        // Si un enfant est visible, rendre visible cet élément
        if (hasVisibleChild)
        {
            item.Visibility = Visibility.Visible;
            itemVisible = true;
        }

        return itemVisible; // Retourner l'état de visibilité de l'élément
    }

    private static string NormalizeString(string input)
        {
            if (input == null) return string.Empty;

            // Remove diacritics (accents)
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Remove spaces, underscores, and hyphens
            return stringBuilder.ToString().ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        }

    //--------------------- Gestion développement synchronisé ----------------------------------------------//

    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        var item = e.OriginalSource as TreeViewItem;
        if (item is not null)
        {
            SynchronizeTreeViewItemExpansion(treeView1, item);
            SynchronizeTreeViewItemExpansion(treeView2, item);
        }
    }

    private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
    {
        var item = e.OriginalSource as TreeViewItem;
        if (item is not null)
        {
            SynchronizeTreeViewItemExpansion(treeView1, item);
            SynchronizeTreeViewItemExpansion(treeView2, item);
        }
    }

    private static void SynchronizeTreeViewItemExpansion(TreeView targetTreeView, TreeViewItem sourceItem)
    {
        string? itemPath = GetItemPath(sourceItem);
        if (itemPath == null)
        {
            // Gérer le cas où itemPath est null
            return;
        }

        TreeViewItem? targetItem = FindTreeViewItemByPath(targetTreeView, itemPath);
        if (targetItem != null)
        {
            targetItem.IsExpanded = sourceItem.IsExpanded;
            for (int i = 0; i < sourceItem.Items.Count; i++)
            {
                TreeViewItem? sourceChildItem = sourceItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                TreeViewItem? targetChildItem = targetItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

                if (sourceChildItem is not null && targetChildItem is not null)
                {
                    SynchronizeTreeViewItemExpansion(targetTreeView, sourceChildItem);
                }
            }
        }
    }

    private static string? GetItemPath(TreeViewItem item)
    {
        if (item.Header is not StackPanel headerStack)
        {
            return null;
        }

        var textBlock = headerStack.Children.OfType<TextBlock>().FirstOrDefault();
        if (textBlock == null)
        {
            return null;
        }

        string? path = textBlock.Text;
        var parent = item.Parent as TreeViewItem;

        while (parent != null)
        {
            if (parent.Header is not StackPanel parentHeaderStack)
            {
                return null;
            }

            var parentTextBlock = parentHeaderStack.Children.OfType<TextBlock>().FirstOrDefault();
            if (parentTextBlock == null)
            {
                return null;
            }

            path = parentTextBlock.Text + "\\&" + path;
            parent = parent.Parent as TreeViewItem;
        }
        return path;
    }

    private static TreeViewItem? FindTreeViewItemByPath(TreeView treeView, string path)
    {
        _ = path ?? throw new ArgumentNullException(nameof(path));

        string[] parts = path.Split("\\&");
        ItemCollection items = treeView.Items;
        TreeViewItem? currentItem = null;

        foreach (string part in parts)
        {
            currentItem = null;
            foreach (object item in items)
            {
                TreeViewItem? treeViewItem = item as TreeViewItem;
                if (treeViewItem is not null && treeViewItem.Header is StackPanel headerStack)
                {
                    var textBlock = headerStack.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null && textBlock.Text == part)
                    {
                        currentItem = treeViewItem;
                        items = treeViewItem.Items;
                        break;
                    }
                }
            }
            if (currentItem == null) return null;
        }
        return currentItem;
    }


    //--------------------- Gestion développement/rétractation bouton ----------------------------------------------//

    private bool isTreeViewExpanded = false;

    private void btnCollapseAndToggle_Click(object sender, RoutedEventArgs e)
    {
        if (isTreeViewExpanded)
        {
            rotateTransform.Angle = -90;
            rotateTransform2.Angle = -90;
            CollapseAllTreeViewItems(treeView1.Items);
            CollapseAllTreeViewItems(treeView2.Items);
        }
        else
        {
            rotateTransform.Angle = 0;
            rotateTransform2.Angle = 0;
            ExpandAllTreeViewItems(treeView1.Items);
            ExpandAllTreeViewItems(treeView2.Items);
        }

        isTreeViewExpanded = !isTreeViewExpanded; // Inverser l'état
    }

    private void CollapseAllTreeViewItems(ItemCollection items)
    {
        foreach (object obj in items)
        {
            if (obj is TreeViewItem item)
            {
                item.IsExpanded = false;
                CollapseAllTreeViewItems(item.Items);
            }
        }
    }

    private void ExpandAllTreeViewItems(ItemCollection items)
    {
        foreach (object obj in items)
        {
            if (obj is TreeViewItem item)
            {
                item.IsExpanded = true;
                ExpandAllTreeViewItems(item.Items);
            }
        }
    }

}


public class TreeItem
    {
        public string Name { get; set; }
        public ObservableCollection<TreeItem> Children { get; set; }

        public TreeItem(string name) : this(name, new ObservableCollection<TreeItem>())
        {
        }

        private TreeItem(string name, ObservableCollection<TreeItem> children)
        {
            Name = name;
            Children = children;
        }
    }

