using System.Windows;
using System.Windows.Input;

namespace KNXBoostDesktop;

public partial class GroupAddressRenameWindow : Window
{
    public GroupAddressRenameWindow()
    {
        InitializeComponent();

        switch (App.DisplayElements?.SettingsWindow.AppLang)
        {
            // Arabe
            case "AR":
                break;

            // Bulgare
            case "BG":
                break;

            // Tchèque
            case "CS":
                break;

            // Danois
            case "DA":
                break;

            // Allemand
            case "DE":
                break;

            // Grec
            case "EL":
                break;

            // Anglais
            case "EN":
                break;

            // Espagnol
            case "ES":
                break;

            // Estonien
            case "ET":
                break;

            // Finnois
            case "FI":
                break;

            // Hongrois
            case "HU":
                break;

            // Indonésien
            case "ID":
                break;

            // Italien
            case "IT":
                break;

            // Japonais
            case "JA":
                break;

            // Coréen
            case "KO":
                break;

            // Letton
            case "LV":
                break;

            // Lituanien
            case "LT":
                break;

            // Norvégien
            case "NB":
                break;

            // Néerlandais
            case "NL":
                break;

            // Polonais
            case "PL":
                break;

            // Portugais
            case "PT":
                break;

            // Roumain
            case "RO":
                break;

            // Russe
            case "RU":
                break;

            // Slovaque
            case "SK":
                break;

            // Slovène
            case "SL":
                break;

            // Suédois
            case "SV":
                break;

            // Turc
            case "TR":
                break;

            // Ukrainien
            case "UK":
                break;

            // Chinois simplifié
            case "ZH":
                break;

            // Langue par défaut (français)
            default:
                SaveButtonText.Text = "Enregistrer";
                CancelButtonText.Text = "Annuler";
                break;
        }
        
        
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}