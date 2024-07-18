using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;


namespace KNXBoostDesktop;

/// <summary>
/// Fenêtre pour renommer une adresse de groupe.
/// </summary>
public partial class GroupAddressRenameWindow
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Obtient le résultat de la boîte de dialogue. True si l'utilisateur a cliqué sur "sauvegarder", False sinon.
    /// </summary>
    public new bool? DialogResult { get; private set; } // True si l'utilisateur a cliqué sur sauvegarder, False sinon
    
    /// <summary>
    /// Obtient l'adresse modifiée par l'utilisateur.
    /// </summary>
    public string NewAddress { get; private set; } // Adresse modifiée par l'utilisateur
    public string SavedAddress { get; private set; } // Adresse issue du logiciel sauvegardée pour reset

    private string xmlRenameFilePath = "";




    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="GroupAddressRenameWindow"/>.
    /// </summary>
    public GroupAddressRenameWindow()
    {
        NewAddress = "";
        SavedAddress = "";
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        InitializeComponent();
    }
    

    /// <summary>
    /// Met à jour le contenu de la fenêtre en fonction de la langue de l'application.
    /// </summary>
    public void UpdateWindowContents()
    {
        switch (App.DisplayElements?.SettingsWindow!.AppLang)
        {
            // Arabe
            case "AR":
                GroupAddressRenameWindowTopTitle.Text = "إعادة تسمية عنوان المجموعة";
                BeforeText.Text = "عنوان المجموعة";
                AfterText.Text = "إعادة التسمية إلى...";
                SaveButtonText.Text = "حفظ";
                CancelButtonText.Text = "إلغاء";
                break;

            // Bulgare
            case "BG":
                GroupAddressRenameWindowTopTitle.Text = "Преименуване на адрес на групата";
                BeforeText.Text = "Адрес на групата";
                AfterText.Text = "Преименуване на...";
                SaveButtonText.Text = "Запазване";
                CancelButtonText.Text = "Отказ";
                break;

            // Tchèque
            case "CS":
                GroupAddressRenameWindowTopTitle.Text = "Přejmenovat adresu skupiny";
                BeforeText.Text = "Adresa skupiny";
                AfterText.Text = "Přejmenovat na...";
                SaveButtonText.Text = "Uložit";
                CancelButtonText.Text = "Zrušit";
                break;

            // Danois
            case "DA":
                GroupAddressRenameWindowTopTitle.Text = "Omdøb gruppeadresse";
                BeforeText.Text = "Gruppeadresse";
                AfterText.Text = "Omdøb til...";
                SaveButtonText.Text = "Gem";
                CancelButtonText.Text = "Annuller";
                break;

            // Allemand
            case "DE":
                GroupAddressRenameWindowTopTitle.Text = "Gruppenadresse umbenennen";
                BeforeText.Text = "Gruppenadresse";
                AfterText.Text = "Umbenennen in...";
                SaveButtonText.Text = "Speichern";
                CancelButtonText.Text = "Abbrechen";
                break;

            // Grec
            case "EL":
                GroupAddressRenameWindowTopTitle.Text = "Μετονομασία διεύθυνσης ομάδας";
                BeforeText.Text = "Διεύθυνση ομάδας";
                AfterText.Text = "Μετονομασία σε...";
                SaveButtonText.Text = "Αποθήκευση";
                CancelButtonText.Text = "Ακύρωση";
                break;

            // Anglais
            case "EN":
                GroupAddressRenameWindowTopTitle.Text = "Rename Group Address";
                BeforeText.Text = "Original Group Address";
                AfterText.Text = "Rename to...";
                SaveButtonText.Text = "Save";
                CancelButtonText.Text = "Cancel";
                break;

            // Espagnol
            case "ES":
                GroupAddressRenameWindowTopTitle.Text = "Renombrar dirección de grupo";
                BeforeText.Text = "Dirección de grupo";
                AfterText.Text = "Renombrar a...";
                SaveButtonText.Text = "Guardar";
                CancelButtonText.Text = "Cancelar";
                break;

            // Estonien
            case "ET":
                GroupAddressRenameWindowTopTitle.Text = "Grupi aadressi ümbernimetamine";
                BeforeText.Text = "Grupi aadress";
                AfterText.Text = "Nimeta ümber...";
                SaveButtonText.Text = "Salvesta";
                CancelButtonText.Text = "Tühista";
                break;

            // Finnois
            case "FI":
                GroupAddressRenameWindowTopTitle.Text = "Nimeä ryhmäosoite uudelleen";
                BeforeText.Text = "Ryhmäosoite";
                AfterText.Text = "Nimeä uudelleen...";
                SaveButtonText.Text = "Tallenna";
                CancelButtonText.Text = "Peruuta";
                break;

            // Hongrois
            case "HU":
                GroupAddressRenameWindowTopTitle.Text = "Csoportcím átnevezése";
                BeforeText.Text = "Csoportcím";
                AfterText.Text = "Átnevezés erre...";
                SaveButtonText.Text = "Mentés";
                CancelButtonText.Text = "Mégse";
                break;

            // Indonésien
            case "ID":
                GroupAddressRenameWindowTopTitle.Text = "Ganti Nama Alamat Grup";
                BeforeText.Text = "Alamat Grup";
                AfterText.Text = "Ganti nama menjadi...";
                SaveButtonText.Text = "Simpan";
                CancelButtonText.Text = "Batal";
                break;

            // Italien
            case "IT":
                GroupAddressRenameWindowTopTitle.Text = "Rinomina indirizzo di gruppo";
                BeforeText.Text = "Indirizzo di gruppo";
                AfterText.Text = "Rinomina in...";
                SaveButtonText.Text = "Salva";
                CancelButtonText.Text = "Annulla";
                break;

            // Japonais
            case "JA":
                GroupAddressRenameWindowTopTitle.Text = "グループアドレスの名前を変更";
                BeforeText.Text = "グループアドレス";
                AfterText.Text = "名前を変更...";
                SaveButtonText.Text = "保存";
                CancelButtonText.Text = "キャンセル";
                break;

            // Coréen
            case "KO":
                GroupAddressRenameWindowTopTitle.Text = "그룹 주소 이름 바꾸기";
                BeforeText.Text = "그룹 주소";
                AfterText.Text = "다음으로 이름 변경...";
                SaveButtonText.Text = "저장";
                CancelButtonText.Text = "취소";
                break;

            // Letton
            case "LV":
                GroupAddressRenameWindowTopTitle.Text = "Pārdēvēt grupas adresi";
                BeforeText.Text = "Grupas adrese";
                AfterText.Text = "Pārdēvēt uz...";
                SaveButtonText.Text = "Saglabāt";
                CancelButtonText.Text = "Atcelt";
                break;

            // Lituanien
            case "LT":
                GroupAddressRenameWindowTopTitle.Text = "Pervardyti grupės adresą";
                BeforeText.Text = "Grupės adresas";
                AfterText.Text = "Pervardyti į...";
                SaveButtonText.Text = "Išsaugoti";
                CancelButtonText.Text = "Atšaukti";
                break;

            // Norvégien
            case "NB":
                GroupAddressRenameWindowTopTitle.Text = "Gi nytt navn til gruppeadresse";
                BeforeText.Text = "Gruppeadresse";
                AfterText.Text = "Gi nytt navn til...";
                SaveButtonText.Text = "Lagre";
                CancelButtonText.Text = "Avbryt";
                break;

            // Néerlandais
            case "NL":
                GroupAddressRenameWindowTopTitle.Text = "Groepsadres hernoemen";
                BeforeText.Text = "Groepsadres";
                AfterText.Text = "Hernoemen naar...";
                SaveButtonText.Text = "Opslaan";
                CancelButtonText.Text = "Annuleren";
                break;

            // Polonais
            case "PL":
                GroupAddressRenameWindowTopTitle.Text = "Zmień nazwę adresu grupy";
                BeforeText.Text = "Adres grupy";
                AfterText.Text = "Zmień nazwę na...";
                SaveButtonText.Text = "Zapisz";
                CancelButtonText.Text = "Anuluj";
                break;

            // Portugais
            case "PT":
                GroupAddressRenameWindowTopTitle.Text = "Renomear endereço do grupo";
                BeforeText.Text = "Endereço do grupo";
                AfterText.Text = "Renomear para...";
                SaveButtonText.Text = "Salvar";
                CancelButtonText.Text = "Cancelar";
                break;

            // Roumain
            case "RO":
                GroupAddressRenameWindowTopTitle.Text = "Redenumiți adresa grupului";
                BeforeText.Text = "Adresa grupului";
                AfterText.Text = "Redenumiți în...";
                SaveButtonText.Text = "Salvați";
                CancelButtonText.Text = "Anulați";
                break;

            // Russe
            case "RU":
                GroupAddressRenameWindowTopTitle.Text = "Переименовать адрес группы";
                BeforeText.Text = "Адрес группы";
                AfterText.Text = "Переименовать в...";
                SaveButtonText.Text = "Сохранить";
                CancelButtonText.Text = "Отмена";
                break;

            // Slovaque
            case "SK":
                GroupAddressRenameWindowTopTitle.Text = "Premenovať adresu skupiny";
                BeforeText.Text = "Adresa skupiny";
                AfterText.Text = "Premenovať na...";
                SaveButtonText.Text = "Uložiť";
                CancelButtonText.Text = "Zrušiť";
                break;

            // Slovène
            case "SL":
                GroupAddressRenameWindowTopTitle.Text = "Preimenuj naslov skupine";
                BeforeText.Text = "Naslov skupine";
                AfterText.Text = "Preimenuj v...";
                SaveButtonText.Text = "Shrani";
                CancelButtonText.Text = "Prekliči";
                break;

            // Suédois
            case "SV":
                GroupAddressRenameWindowTopTitle.Text = "Byt namn på gruppadress";
                BeforeText.Text = "Gruppadress";
                AfterText.Text = "Byt namn till...";
                SaveButtonText.Text = "Spara";
                CancelButtonText.Text = "Avbryt";
                break;

            // Turc
            case "TR":
                GroupAddressRenameWindowTopTitle.Text = "Grup Adresini Yeniden Adlandır";
                BeforeText.Text = "Grup Adresi";
                AfterText.Text = "Yeniden adlandır...";
                SaveButtonText.Text = "Kaydet";
                CancelButtonText.Text = "İptal";
                break;

            // Ukrainien
            case "UK":
                GroupAddressRenameWindowTopTitle.Text = "Перейменувати адресу групи";
                BeforeText.Text = "Адреса групи";
                AfterText.Text = "Перейменувати на...";
                SaveButtonText.Text = "Зберегти";
                CancelButtonText.Text = "Скасувати";
                break;

            // Chinois simplifié
            case "ZH":
                GroupAddressRenameWindowTopTitle.Text = "重命名组地址";
                BeforeText.Text = "组地址";
                AfterText.Text = "重命名为...";
                SaveButtonText.Text = "保存";
                CancelButtonText.Text = "取消";
                break;

            // Langue par défaut (français)
            default:
                GroupAddressRenameWindowTopTitle.Text = "Renommer l'adresse de groupe";
                BeforeText.Text = "Adresse de groupe orignale ";
                AfterText.Text = "Renommer en...";
                SaveButtonText.Text = "Enregistrer";
                CancelButtonText.Text = "Annuler";
                break;
        }

        if (App.DisplayElements!.SettingsWindow!.EnableLightTheme)
        {
            var txtColor = MainWindow.ConvertStringColor("#000000");
            
            MainGrid.Background = MainWindow.ConvertStringColor("#F5F5F5");
            DrawingBrush1.Brush = txtColor;
            DrawingBrush2.Brush = txtColor;
            GroupAddressRenameWindowTopTitle.Foreground = txtColor;
            HeaderPath.Stroke = MainWindow.ConvertStringColor("#D7D7D7");
            MainContentPanel.Background = MainWindow.ConvertStringColor("#FFFFFF");
            MainContentBorder.BorderBrush = MainWindow.ConvertStringColor("#D7D7D7");
            BeforeText.Foreground = txtColor;
            BeforeTextBox.Style = (Style)FindResource("TextBoxFocusStyleLight");
            BeforeTextBox.Foreground = txtColor;
            BeforeTextBox.Background = MainWindow.ConvertStringColor("#F5F5F5");
            BeforeTextBox.BorderBrush = MainWindow.ConvertStringColor("#b8b8b8");
            AfterText.Foreground = txtColor;
            AfterTextBox.Style = (Style)FindResource("TextBoxFocusStyleLight");
            AfterTextBox.Foreground = txtColor;
            AfterTextBox.Background = MainWindow.ConvertStringColor("#FFFFFF");
            AfterTextBox.BorderBrush = MainWindow.ConvertStringColor("#b8b8b8");
            FooterPath.Stroke = MainWindow.ConvertStringColor("#D7D7D7");
            SettingsWindowFooter.Background = MainWindow.ConvertStringColor("#FFFFFF");

            SaveButton.Style = (Style)FindResource("BottomButtonLight");
            SaveButtonText.Foreground = txtColor;
            SaveButtonDrawing.Brush = txtColor;
            CancelButton.Style = (Style)FindResource("BottomButtonLight");
            CancelButtonText.Foreground = txtColor;
            CancelButtonDrawing.Brush = txtColor;
            AfterTextBoxButton.Style = (Style)FindResource("SquareButtonStyleLight");
            AfterTextBoxButton.Foreground = txtColor;
        }
        else
        {
            var txtColor = MainWindow.ConvertStringColor("#E3DED4");
            
            MainGrid.Background = MainWindow.ConvertStringColor("#313131");
            DrawingBrush1.Brush = txtColor;
            DrawingBrush2.Brush = txtColor;
            GroupAddressRenameWindowTopTitle.Foreground = txtColor;
            HeaderPath.Stroke = MainWindow.ConvertStringColor("#434343");
            MainContentPanel.Background = MainWindow.ConvertStringColor("#262626");
            MainContentBorder.BorderBrush = MainWindow.ConvertStringColor("#434343");
            BeforeText.Foreground = txtColor;
            BeforeTextBox.Style = (Style)FindResource("TextBoxFocusStyleDark");
            BeforeTextBox.Foreground = txtColor;
            BeforeTextBox.Background = MainWindow.ConvertStringColor("#313131");
            BeforeTextBox.BorderBrush = MainWindow.ConvertStringColor("#434343");
            AfterText.Foreground = txtColor;
            AfterTextBox.Style = (Style)FindResource("TextBoxFocusStyleDark");
            AfterTextBox.Foreground = txtColor;
            AfterTextBox.Background = MainWindow.ConvertStringColor("#262626");
            AfterTextBox.BorderBrush = MainWindow.ConvertStringColor("#434343");
            FooterPath.Stroke = MainWindow.ConvertStringColor("#434343");
            SettingsWindowFooter.Background = MainWindow.ConvertStringColor("#262626");
            
            SaveButton.Style = (Style)FindResource("BottomButtonDark");
            SaveButtonText.Foreground = txtColor;
            SaveButtonDrawing.Brush = txtColor;
            CancelButton.Style = (Style)FindResource("BottomButtonDark");
            CancelButtonText.Foreground = txtColor;
            CancelButtonDrawing.Brush = txtColor;
            AfterTextBoxButton.Style = (Style)FindResource("SquareButtonStyleDark");
            AfterTextBoxButton.Foreground = txtColor;
        }
        
        ApplyScaling(App.DisplayElements.SettingsWindow.AppScaleFactor/100f);
    }


    /// <summary>
    /// Définit l'adresse actuelle à renommer.
    /// </summary>
    /// <param name="address">L'adresse actuelle de groupe.</param>
    public void SetAddress(string addressOriginale, string addressModifiée)
    {
        BeforeTextBox.Text = addressOriginale;
        AfterTextBox.Text = addressModifiée;
        SavedAddress = addressModifiée; 
    }

    public void SetPath(string _xmlRenameFilePath)
    {
        xmlRenameFilePath = _xmlRenameFilePath;
    }


    /// <summary>
    /// Permet de déplacer la fenêtre en cliquant et en maintenant le bouton gauche de la souris enfoncé.
    /// </summary>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    
    /// <summary>
    /// Gestionnaire d'événements pour le bouton "Annuler". Annule les modifications et masque la fenêtre.
    /// </summary>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;

        NewAddress = "";
        
        UpdateWindowContents(); // Restauration des paramètres précédents dans la fenêtre de paramétrage
        Hide(); // Masquage de la fenêtre de renommage
    }

    
    /// <summary>
    /// Gestionnaire d'événements pour le bouton "Sauvegarder". Sauvegarde les modifications et masque la fenêtre.
    /// </summary>
    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;

        NewAddress = AfterTextBox.Text; // Sauvegarde du texte modifié par l'utilisateur
        
        Hide(); // Masquage de la fenêtre de renommage
    }
    
    
    /// <summary>
    /// Gestionnaire d'événements pour la fermeture de la fenêtre de paramètres. Annule la fermeture et masque la fenêtre à la place.
    /// </summary>
    private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true; // Pour éviter de tuer l'instance de SettingsWindow, on annule la fermeture
        UpdateWindowContents(); // Mise à jour du contenu de la fenêtre pour remettre les valeurs précédentes
        Hide(); // On masque la fenêtre à la place
    }


    private void Reset(object? sender, RoutedEventArgs e)
    {
        try
        {
            string path = xmlRenameFilePath; // Remplacez par le chemin réel de votre fichier XML

            // Vérifier si le fichier existe
            if (!File.Exists(path))
            {
                AfterTextBox.Text = SavedAddress; // Utiliser SavedAddress si le fichier n'existe pas
                App.ConsoleAndLogWriteLine($"File not created. Reset with '{SavedAddress}'");
                return;
            }

            // Charger le document XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            // Rechercher l'élément <Change> où OriginalAddress correspond à BeforeTextBox.Text
            XmlNodeList changeNodes = xmlDoc.SelectNodes($"/Root/Change[@OriginalAddress='{BeforeTextBox.Text}']");

            if (changeNodes != null && changeNodes.Count > 0)
            {
                // Initialiser la variable pour stocker le OldAddress le plus ancien
                string oldestOldAddress = null;
                DateTime oldestTimeStamp = DateTime.MaxValue;

                // Parcourir les nodes pour trouver le OldAddress le plus ancien
                foreach (XmlNode changeNode in changeNodes)
                {
                    // Récupérer OldAddress et TimeStamp de chaque node
                    string oldAddress = changeNode.Attributes["OldAddress"]?.Value;
                    DateTime timeStamp = DateTime.Parse(changeNode.Attributes["TimeStamp"]?.Value);

                    // Vérifier si le TimeStamp est plus ancien que celui actuellement enregistré
                    if (timeStamp < oldestTimeStamp)
                    {
                        oldestTimeStamp = timeStamp;
                        oldestOldAddress = oldAddress;
                    }
                }

                // Mettre à jour AfterTextBox.Text avec le OldAddress le plus ancien trouvé
                AfterTextBox.Text = oldestOldAddress;

                App.ConsoleAndLogWriteLine($"Reset successful: After Address set to '{oldestOldAddress}' based on Original Address '{BeforeTextBox.Text}'");
            }
            else
            {
                // Gérer le cas où aucun élément correspondant n'est trouvé
                AfterTextBox.Text = SavedAddress; // Utiliser SavedAddress si aucune correspondance n'est trouvée
                App.ConsoleAndLogWriteLine($"Address not modified yet. Reset with '{SavedAddress}'");
            }
        }
        catch (Exception ex)
        {
            // En cas d'exception, utiliser SavedAddress
            App.ConsoleAndLogWriteLine($"Error during reset: {ex.Message}");
            AfterTextBox.Text = SavedAddress;
        }
    }
    
    
    
    public void ApplyScaling(float scale)
    {
        AddressRenameWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
        
        Height = 275 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 275 * scale;
        Width = 500 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 500 * scale;
    }
}