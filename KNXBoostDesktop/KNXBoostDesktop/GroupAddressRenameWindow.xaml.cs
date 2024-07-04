using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace KNXBoostDesktop;

public partial class GroupAddressRenameWindow
{
    public new bool? DialogResult;
    
    public GroupAddressRenameWindow()
    {
        InitializeComponent();

        UpdateWindowContents();
    }

    public void UpdateWindowContents()
    {
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
                GroupAddressRenameWindowTopTitle.Text = "Renommer l'adresse de groupe";
                SaveButtonText.Text = "Enregistrer";
                CancelButtonText.Text = "Annuler";
                break;

                if (App.DisplayElements.SettingsWindow.EnableLightTheme)
                {
                    
                }
                else
                {
                    
                }
        }
    }

    public void setAddress(string address)
    {
        BeforeTextBox.Text = address;
        AfterTextBox.Text = address;
    }
    
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        UpdateWindowContents(); // Restauration des paramètres précédents dans la fenêtre de paramétrage
        Hide(); // Masquage de la fenêtre de renommage
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Hide(); // Masquage de la fenêtre de renommage
    }
    
    private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true; // Pour éviter de tuer l'instance de SettingsWindow, on annule la fermeture
        UpdateWindowContents(); // Mise à jour du contenu de la fenêtre pour remettre les valeurs précédentes
        Hide(); // On masque la fenêtre à la place
    }
}