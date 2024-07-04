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
using Microsoft.Win32;

namespace KNXBoostDesktop;

public partial class MainWindow 

{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    //private readonly string xmlFilePath1 = App.Fm?.ProjectFolderPath + "GroupAddresses.xml"; 
    private string _xmlFilePath1 = "";
    
    private string _xmlFilePath2 = "";

    private MainViewModel ViewModel { get; set; }


    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainWindow()
    {   
        InitializeComponent();
        
        ViewModel = new MainViewModel();
        DataContext = ViewModel;

        //Title = $"{App.AppName} v{App.AppVersion}";
        Title = "";
        
        Uri iconUri = new ("pack://application:,,,/resources/BOOST-2.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);
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
            _xmlFilePath1 = $"{App.Fm.ProjectFolderPath}/GroupAddresses.xml";
            _xmlFilePath2 = App.Fm.ProjectFolderPath + "UpdatedGroupAddresses.xml"; 
            //Define the project path
            ExportUpdatedNameAddresses.Export(App.Fm.ZeroXmlPath ?? throw new InvalidOperationException(), App.Fm.ProjectFolderPath + "/GroupAddresses.xml");
            ExportUpdatedNameAddresses.Export(App.Fm.ProjectFolderPath + "/0_updated.xml",App.Fm?.ProjectFolderPath + "/UpdatedGroupAddresses.xml");
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
        LoadXmlFile(_xmlFilePath1, TreeViewGauche);
        LoadXmlFile(_xmlFilePath2, TreeViewDroite);
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
            var treeNode = CreateTreeViewItemFromXmlNode(xmlNode, level);

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
        var stack = new StackPanel { Orientation = Orientation.Horizontal };

        // Définir l'icône en fonction du niveau
        var icon = new Image
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 5, 0),
            Source = level switch
            {
                0 => new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level.png")),
                1 => new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level2.png")),
                2 => new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level3.png")),
                _ => new BitmapImage(new Uri("pack://application:,,,/resources/Icon_level3.png"))
            }
        };

        var text = new TextBlock { Text = ((XmlElement)xmlNode).GetAttribute("Name") };

        stack.Children.Add(icon);
        stack.Children.Add(text);

        TreeViewItem treeNode = new() { Header = stack };
        return treeNode;
    }
    

    //-------------------- Gestion du scroll vertical synchronisé ------------------------------------//

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer changedScrollViewer) return;
        // Défilement horizontal
        if (changedScrollViewer == ScrollViewerGauche && e.HorizontalChange != 0)
        {
            ScrollViewerDroite.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
        }
        else if (changedScrollViewer == ScrollViewerDroite && e.HorizontalChange != 0)
        {
            ScrollViewerGauche.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
        }

        // Défilement vertical
        if (changedScrollViewer == ScrollViewerGauche && e.VerticalChange != 0)
        {
            ScrollViewerDroite.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
        }
        else if (changedScrollViewer == ScrollViewerDroite && e.VerticalChange != 0)
        {
            ScrollViewerGauche.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
        }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        // Vérifier si Ctrl est enfoncé
        var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        switch (isShiftPressed)
        {
            // Défilement horizontal
            case true when scrollViewer == ScrollViewerGauche:
                ScrollViewerGauche.ScrollToHorizontalOffset(ScrollViewerGauche.HorizontalOffset - e.Delta);
                break;
            case true:
            {
                if (scrollViewer == ScrollViewerDroite)
                {
                    ScrollViewerDroite.ScrollToHorizontalOffset(ScrollViewerDroite.HorizontalOffset - e.Delta);
                }

                break;
            }
            // Défilement vertical avec Ctrl enfoncé
            case false:
            {
                if (scrollViewer == ScrollViewerGauche)
                {
                    ScrollViewerGauche.ScrollToVerticalOffset(ScrollViewerGauche.VerticalOffset - e.Delta);
                }
                else if (scrollViewer == ScrollViewerDroite)
                {
                    ScrollViewerDroite.ScrollToVerticalOffset(ScrollViewerDroite.VerticalOffset - e.Delta);
                }

                e.Handled = true; // Indiquer que l'événement a été géré
                break;
            }
        }
    }
    
    
    //-------------------- Gestion de la barre de recherche ------------------------------------//
    
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        if (tb?.Text != "Chercher...") return;
        tb.Text = "";
        tb.Foreground = new SolidColorBrush(Colors.Black);
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        // Utiliser un Dispatcher pour s'assurer que le TextBox a réellement perdu le focus
        tb?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!string.IsNullOrWhiteSpace(tb.Text)) return;
            tb.Text = "Chercher...";
            tb.Foreground = new SolidColorBrush(Colors.Gray);
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
    
    private void txtSearch1_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Escape)) return;
        // Perdre le focus du TextBox
        TxtSearch1.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        // Pour indiquer que l'événement est géré
        e.Handled = true; 
    }

    //-------------------- Gestion de la recherche ---------------------------------------------------//

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (TxtSearch1.Text == "Chercher...") return;
        var normalizedSearchText = NormalizeString(TxtSearch1.Text);

        // Filtrer et masquer les éléments de la TreeView basé sur le texte de recherche
        foreach (var obj in TreeViewGauche.Items)
        {
            if (obj is not TreeViewItem item) continue;
            item.Visibility = Visibility.Visible;
            FilterTreeViewItems(item, normalizedSearchText);
        }
    }

    private static bool FilterTreeViewItems(TreeViewItem item, string searchText)
    {
        var itemVisible = false; // Indicateur pour déterminer si l'élément est visible

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

        var normalizedHeader = NormalizeString(headerText);

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
        var hasVisibleChild = false;
        foreach (var obj in item.Items)
        {
            if (obj is not TreeViewItem childItem) continue;
            // Appliquer le filtre aux enfants et mettre à jour l'indicateur de visibilité
            var childVisible = FilterTreeViewItems(childItem, searchText);
            
            if (!childVisible) continue;
            hasVisibleChild = true;
            item.IsExpanded = true; // Développer l'élément si un enfant est visible
        }

        // Si un enfant est visible, rendre visible cet élément
        if (!hasVisibleChild) return itemVisible; // Retourner l'état de visibilité de l'élément
        item.Visibility = Visibility.Visible;
        itemVisible = true;

        return itemVisible; // Retourner l'état de visibilité de l'élément
    }

    private static string NormalizeString(string? input)
    {
        if (input == null) return string.Empty;

        // Remove diacritics (accents)
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new();

        foreach (var c in normalizedString.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
        {
            stringBuilder.Append(c);
        }

        // Remove spaces, underscores, and hyphens
        return stringBuilder.ToString().ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    //--------------------- Gestion développement synchronisé ----------------------------------------------//

    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem item) return;
        SynchronizeTreeViewItemExpansion(TreeViewGauche, item);
        SynchronizeTreeViewItemExpansion(TreeViewDroite, item);
    }

    private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem item) return;
        SynchronizeTreeViewItemExpansion(TreeViewGauche, item);
        SynchronizeTreeViewItemExpansion(TreeViewDroite, item);
    }

    private static void SynchronizeTreeViewItemExpansion(TreeView targetTreeView, TreeViewItem sourceItem)
    {
        var itemPath = GetItemPath(sourceItem);
        if (itemPath == null) return; // Gérer le cas où itemPath est null
        
        var targetItem = FindTreeViewItemByPath(targetTreeView, itemPath);
        if (targetItem == null) return;
        targetItem.IsExpanded = sourceItem.IsExpanded;
        for (var i = 0; i < sourceItem.Items.Count; i++)
        {
            var sourceChildItem = sourceItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
            var targetChildItem = targetItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;

            if (sourceChildItem is not null && targetChildItem is not null)
            {
                SynchronizeTreeViewItemExpansion(targetTreeView, sourceChildItem);
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

        var path = textBlock.Text;
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

        var parts = path.Split("\\&");
        var items = treeView.Items;
        TreeViewItem? currentItem = null;

        foreach (var part in parts)
        {
            currentItem = null;
            foreach (var item in items)
            {
                if (item is not TreeViewItem { Header: StackPanel headerStack } treeViewItem) continue;
                var textBlock = headerStack.Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock == null || textBlock.Text != part) continue;
                currentItem = treeViewItem;
                items = treeViewItem.Items;
                break;
            }
            if (currentItem == null) return null;
        }
        return currentItem;
    }


    //--------------------- Gestion développement/rétractation bouton ----------------------------------------------//

    private bool _isTreeViewExpanded;

    private void btnCollapseAndToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_isTreeViewExpanded)
        {
            RotateTransform.Angle = -90;
            RotateTransform2.Angle = -90;
            CollapseAllTreeViewItems(TreeViewGauche.Items);
            CollapseAllTreeViewItems(TreeViewDroite.Items);
        }
        else
        {
            RotateTransform.Angle = 0;
            RotateTransform2.Angle = 0;
            ExpandAllTreeViewItems(TreeViewGauche.Items);
            ExpandAllTreeViewItems(TreeViewDroite.Items);
        }

        _isTreeViewExpanded = !_isTreeViewExpanded; // Inverser l'état
    }

    private static void CollapseAllTreeViewItems(ItemCollection items)
    {
        foreach (var obj in items)
        {
            if (obj is not TreeViewItem item) continue;
            item.IsExpanded = false;
            CollapseAllTreeViewItems(item.Items);
        }
    }

    private static void ExpandAllTreeViewItems(ItemCollection items)
    {
        foreach (var obj in items)
        {
            if (obj is not TreeViewItem item) continue;
            item.IsExpanded = true;
            ExpandAllTreeViewItems(item.Items);
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

