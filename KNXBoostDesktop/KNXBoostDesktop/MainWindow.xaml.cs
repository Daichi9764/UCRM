/***************************************************************************
 * Nom du Projet : NomDuProjet
 * Fichier       : NomDuFichier.cs
 * Auteur        : Votre Nom
 * Date          : 12/06/2024
 * Version       : 1.0.0
 *
 * Description :
 * Ce fichier contient [Description du fichier et son rôle dans le projet].
 * [Donnez des détails supplémentaires sur le contenu et la fonctionnalité
 * du fichier, les classes principales, les méthodes ou toute autre
 * information pertinente].
 *
 * Historique des Modifications :
 * -------------------------------------------------------------------------
 * Date        | Auteur       | Version  | Description
 * -------------------------------------------------------------------------
 * 12/06/2024  | Votre Nom    | 1.0.0    | Création initiale du fichier.
 * [Date]      | [Nom]        | [Ver.]   | [Description de la modification]
 *
 * Remarques :
 * [Ajoutez ici toute information supplémentaire, des notes spéciales, des
 * avertissements ou des recommandations concernant ce fichier].
 *
 * **************************************************************************/

using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace KNXBoostDesktop;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title = "KNX Boost";

        Uri iconUri = new Uri("./icon.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);
    }

    private void ImportProjectButtonClick(object sender, RoutedEventArgs e)
    {
        
        // Créer une nouvelle instance de OpenFileDialog
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            // Définir des propriétés optionnelles
            Title = "Select a file",
            Filter = "ETS KNX Project File (*.knxproj)|*.knxproj|other file|*.*",
            FilterIndex = 1,
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        bool? result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            string selectedFilePath = openFileDialog.FileName;
            App.Fm.KnxprojSourceFilePath = selectedFilePath;
            App.Fm.KnxprojExportFolderPath = @"T:\Exported_dir\";
            App.Fm.ExtractProjectFiles();

            findZeroXmlButton.Visibility = Visibility.Visible;
        }
    }
    
    private void CloseProgramButtonClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void FindZeroXmlButtonClick(object sender, RoutedEventArgs e)
    {
        App.Fm.FindZeroXml();
        
        checkMarkImage.Visibility = Visibility.Visible;
        
        //findZeroXmlButton.Visibility = Visibility.Collapsed;
    }
}