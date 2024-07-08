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
using System.Windows.Shell;
using Microsoft.Win32;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace KNXBoostDesktop;

public partial class MainWindow 

{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    //private readonly string xmlFilePath1 = App.Fm?.ProjectFolderPath + "GroupAddresses.xml"; 
    private string _xmlFilePath1 = "";
    
    private string _xmlFilePath2 = "";

    private bool lightThemeON;

    private LoadingWindow loadingWindow;
    
    private MainViewModel ViewModel { get; set; }


    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        
        Uri iconUri = new ("pack://application:,,,/resources/BOOST-2.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);
        
        UpdateWindowContents();
        
        LocationChanged += MainWindow_LocationChanged;
    }
    
    private void MainWindow_LocationChanged(object sender, EventArgs e)
    {
        // Mettre à jour la position de la LoadingWindow lorsque MainWindow est déplacée
        if (loadingWindow != null && loadingWindow.IsVisible)
        {
            loadingWindow.UpdatePosition(Left, Top);
        }
    }

    public void UpdateWindowContents()
    {
        // Traduction de la fenêtre principale
        switch (App.DisplayElements?.SettingsWindow?.AppLang)
        {
            // Arabe
            case "AR":
                TxtSearch1.Text = "بحث...";
                ButtonChargerProject.Content = "تحميل مشروع جديد";
                ButtonExportProject.Content = "تصدير المشروع المعدل";
                TextBlockAdressesGauche.Text = "عناوين المجموعة الأصلية";
                TextBlockAdressesDroite.Text = "عناوين المجموعة المعدلة";
                break;

            // Bulgare
            case "BG":
                TxtSearch1.Text = "Търсене...";
                ButtonChargerProject.Content = "Зареждане на нов проект";
                ButtonExportProject.Content = "Експортиране на модифицирания проект";
                TextBlockAdressesGauche.Text = "Оригинални групови адреси";
                TextBlockAdressesDroite.Text = "Модифицирани групови адреси";
                break;

            // Tchèque
            case "CS":
                TxtSearch1.Text = "Hledat...";
                ButtonChargerProject.Content = "Načíst nový projekt";
                ButtonExportProject.Content = "Exportovat upravený projekt";
                TextBlockAdressesGauche.Text = "Původní skupinové adresy";
                TextBlockAdressesDroite.Text = "Upravené skupinové adresy";
                break;

            // Danois
            case "DA":
                TxtSearch1.Text = "Søg...";
                ButtonChargerProject.Content = "Indlæs et nyt projekt";
                ButtonExportProject.Content = "Eksporter det ændrede projekt";
                TextBlockAdressesGauche.Text = "Originale gruppeadresser";
                TextBlockAdressesDroite.Text = "Ændrede gruppeadresser";
                break;

            // Allemand
            case "DE":
                TxtSearch1.Text = "Suchen...";
                ButtonChargerProject.Content = "Neues Projekt laden";
                ButtonExportProject.Content = "Geändertes Projekt exportieren";
                TextBlockAdressesGauche.Text = "Ursprüngliche Gruppenadressen";
                TextBlockAdressesDroite.Text = "Geänderte Gruppenadressen";
                break;

            // Grec
            case "EL":
                TxtSearch1.Text = "Αναζήτηση...";
                ButtonChargerProject.Content = "Φόρτωση νέου έργου";
                ButtonExportProject.Content = "Εξαγωγή τροποποιημένου έργου";
                TextBlockAdressesGauche.Text = "Πρωτότυπες ομαδικές διευθύνσεις";
                TextBlockAdressesDroite.Text = "Τροποποιημένες ομαδικές διευθύνσεις";
                break;

            // Anglais
            case "EN":
                TxtSearch1.Text = "Search...";
                ButtonChargerProject.Content = "Load a new project";
                ButtonExportProject.Content = "Export the modified project";
                TextBlockAdressesGauche.Text = "Original Group Addresses";
                TextBlockAdressesDroite.Text = "Modified Group Addresses";
                break;

            // Espagnol
            case "ES":
                TxtSearch1.Text = "Buscar...";
                ButtonChargerProject.Content = "Cargar un nuevo proyecto";
                ButtonExportProject.Content = "Exportar el proyecto modificado";
                TextBlockAdressesGauche.Text = "Direcciones de grupo originales";
                TextBlockAdressesDroite.Text = "Direcciones de grupo modificadas";
                break;

            // Estonien
            case "ET":
                TxtSearch1.Text = "Otsi...";
                ButtonChargerProject.Content = "Laadi uus projekt";
                ButtonExportProject.Content = "Ekspordi muudetud projekt";
                TextBlockAdressesGauche.Text = "Algupärased grupiaadressid";
                TextBlockAdressesDroite.Text = "Muudetud grupiaadressid";
                break;

            // Finnois
            case "FI":
                TxtSearch1.Text = "Hae...";
                ButtonChargerProject.Content = "Lataa uusi projekti";
                ButtonExportProject.Content = "Vie muutettu projekti";
                TextBlockAdressesGauche.Text = "Alkuperäiset ryhmäosoitteet";
                TextBlockAdressesDroite.Text = "Muutetut ryhmäosoitteet";
                break;

            // Hongrois
            case "HU":
                TxtSearch1.Text = "Keresés...";
                ButtonChargerProject.Content = "Új projekt betöltése";
                ButtonExportProject.Content = "A módosított projekt exportálása";
                TextBlockAdressesGauche.Text = "Eredeti csoportcímek";
                TextBlockAdressesDroite.Text = "Módosított csoportcímek";
                break;

            // Indonésien
            case "ID":
                TxtSearch1.Text = "Cari...";
                ButtonChargerProject.Content = "Muat proyek baru";
                ButtonExportProject.Content = "Ekspor proyek yang dimodifikasi";
                TextBlockAdressesGauche.Text = "Alamat Grup Asli";
                TextBlockAdressesDroite.Text = "Alamat Grup yang Dimodifikasi";
                break;

            // Italien
            case "IT":
                TxtSearch1.Text = "Cerca...";
                ButtonChargerProject.Content = "Carica un nuovo progetto";
                ButtonExportProject.Content = "Esporta il progetto modificato";
                TextBlockAdressesGauche.Text = "Indirizzi di gruppo originali";
                TextBlockAdressesDroite.Text = "Indirizzi di gruppo modificati";
                break;

            // Japonais
            case "JA":
                TxtSearch1.Text = "検索...";
                ButtonChargerProject.Content = "新しいプロジェクトをロード";
                ButtonExportProject.Content = "変更されたプロジェクトをエクスポート";
                TextBlockAdressesGauche.Text = "元のグループアドレス";
                TextBlockAdressesDroite.Text = "変更されたグループアドレス";
                break;

            // Coréen
            case "KO":
                TxtSearch1.Text = "검색...";
                ButtonChargerProject.Content = "새 프로젝트 로드";
                ButtonExportProject.Content = "수정된 프로젝트 내보내기";
                TextBlockAdressesGauche.Text = "원본 그룹 주소";
                TextBlockAdressesDroite.Text = "수정된 그룹 주소";
                break;

            // Letton
            case "LV":
                TxtSearch1.Text = "Meklēt...";
                ButtonChargerProject.Content = "Ielādēt jaunu projektu";
                ButtonExportProject.Content = "Eksportēt modificēto projektu";
                TextBlockAdressesGauche.Text = "Oriģinālās grupu adreses";
                TextBlockAdressesDroite.Text = "Modificētās grupu adreses";
                break;

            // Lituanien
            case "LT":
                TxtSearch1.Text = "Ieškoti...";
                ButtonChargerProject.Content = "Įkelti naują projektą";
                ButtonExportProject.Content = "Eksportuoti pakeistą projektą";
                TextBlockAdressesGauche.Text = "Originalūs grupių adresai";
                TextBlockAdressesDroite.Text = "Modifikuoti grupių adresai";
                break;

            // Norvégien
            case "NB":
                TxtSearch1.Text = "Søk...";
                ButtonChargerProject.Content = "Last inn et nytt prosjekt";
                ButtonExportProject.Content = "Eksporter det endrede prosjektet";
                TextBlockAdressesGauche.Text = "Opprinnelige gruppeadresser";
                TextBlockAdressesDroite.Text = "Endrede gruppeadresser";
                break;

            // Néerlandais
            case "NL":
                TxtSearch1.Text = "Zoeken...";
                ButtonChargerProject.Content = "Laad een nieuw project";
                ButtonExportProject.Content = "Exporteer het gewijzigde project";
                TextBlockAdressesGauche.Text = "Originele groepadressen";
                TextBlockAdressesDroite.Text = "Gewijzigde groepadressen";
                break;

            // Polonais
            case "PL":
                TxtSearch1.Text = "Szukaj...";
                ButtonChargerProject.Content = "Załaduj nowy projekt";
                ButtonExportProject.Content = "Eksportuj zmodyfikowany projekt";
                TextBlockAdressesGauche.Text = "Oryginalne adresy grup";
                TextBlockAdressesDroite.Text = "Zmodyfikowane adresy grup";
                break;

            // Portugais
            case "PT":
                TxtSearch1.Text = "Pesquisar...";
                ButtonChargerProject.Content = "Carregar um novo projeto";
                ButtonExportProject.Content = "Exportar o projeto modificado";
                TextBlockAdressesGauche.Text = "Endereços de grupo originais";
                TextBlockAdressesDroite.Text = "Endereços de grupo modificados";
                break;

            // Roumain
            case "RO":
                TxtSearch1.Text = "Căutare...";
                ButtonChargerProject.Content = "Încărcați un proiect nou";
                ButtonExportProject.Content = "Exportați proiectul modificat";
                TextBlockAdressesGauche.Text = "Adresele grupului original";
                TextBlockAdressesDroite.Text = "Adresele grupului modificate";
                break;

            // Russe
            case "RU":
                TxtSearch1.Text = "Поиск...";
                ButtonChargerProject.Content = "Загрузить новый проект";
                ButtonExportProject.Content = "Экспортировать измененный проект";
                TextBlockAdressesGauche.Text = "Оригинальные групповые адреса";
                TextBlockAdressesDroite.Text = "Измененные групповые адреса";
                break;

            // Slovaque
            case "SK":
                TxtSearch1.Text = "Hľadať...";
                ButtonChargerProject.Content = "Načítať nový projekt";
                ButtonExportProject.Content = "Exportovať upravený projekt";
                TextBlockAdressesGauche.Text = "Pôvodné skupinové adresy";
                TextBlockAdressesDroite.Text = "Upravené skupinové adresy";
                break;

            // Slovène
            case "SL":
                TxtSearch1.Text = "Iskanje...";
                ButtonChargerProject.Content = "Naloži nov projekt";
                ButtonExportProject.Content = "Izvozi spremenjeni projekt";
                TextBlockAdressesGauche.Text = "Izvirni naslovi skupin";
                TextBlockAdressesDroite.Text = "Spremenjeni naslovi skupin";
                break;

            // Suédois
            case "SV":
                TxtSearch1.Text = "Sök...";
                ButtonChargerProject.Content = "Ladda ett nytt projekt";
                ButtonExportProject.Content = "Exportera det modifierade projektet";
                TextBlockAdressesGauche.Text = "Ursprungliga gruppadresser";
                TextBlockAdressesDroite.Text = "Ändrade gruppadresser";
                break;

            // Turc
            case "TR":
                TxtSearch1.Text = "Ara...";
                ButtonChargerProject.Content = "Yeni bir proje yükle";
                ButtonExportProject.Content = "Değiştirilen projeyi dışa aktar";
                TextBlockAdressesGauche.Text = "Orijinal Grup Adresleri";
                TextBlockAdressesDroite.Text = "Değiştirilen Grup Adresleri";
                break;

            // Ukrainien
            case "UK":
                TxtSearch1.Text = "Пошук...";
                ButtonChargerProject.Content = "Завантажити новий проект";
                ButtonExportProject.Content = "Експортувати змінений проект";
                TextBlockAdressesGauche.Text = "Оригінальні групові адреси";
                TextBlockAdressesDroite.Text = "Змінені групові адреси";
                break;

            // Chinois simplifié
            case "ZH":
                TxtSearch1.Text = "搜索...";
                ButtonChargerProject.Content = "加载新项目";
                ButtonExportProject.Content = "导出修改后的项目";
                TextBlockAdressesGauche.Text = "原始组地址";
                TextBlockAdressesDroite.Text = "修改后的组地址";
                break;

            // Langue par défaut (français)
            default:
                TxtSearch1.Text = "Chercher...";
                ButtonChargerProject.Content = "Charger un nouveau projet";
                ButtonExportProject.Content = "Exporter le projet modifié";
                TextBlockAdressesGauche.Text = "Adresses de Groupe Originales";
                TextBlockAdressesDroite.Text = "Adresses de Groupe Modifiées";
                break;
        }
        
        string buttonTextColor;
        string panelTextColor;
        string titleBarColor;
        string buttonColor;
        
        string settingsButtonColor;
        string logoColor;
        string borderColor;
        string borderPanelColor;
        
        string panelBackgroundColor;
        string backgroundColor;
        
            
        if (App.DisplayElements != null && App.DisplayElements.SettingsWindow.EnableLightTheme)
        {
            buttonTextColor = "#FFFFFF";
            panelTextColor = "#000000";
            titleBarColor = "#369026";
            buttonColor = "#4071B4";
            panelBackgroundColor = "#FFFFFF";
            backgroundColor = "#F5F5F5";

            settingsButtonColor = "#FFFFFF";
            logoColor = "#000000";
            borderColor = "#D7D7D7";

            borderPanelColor = "#D7D7D7";
            
            ButtonSettings.Style = (Style)FindResource("SettingsButtonLight");
            BtnToggleArrowGauche.Style = (Style)FindResource("ToggleButtonStyle");
            BtnToggleArrowDroite.Style = (Style)FindResource("ToggleButtonStyle");

            ApplyStyleToTreeViewItems(TreeViewGauche, "TreeViewItemStyle2");
            ApplyStyleToTreeViewItems(TreeViewDroite, "TreeViewItemStyle2");
            
            //TreeViewGauche.Style = (Style)FindResource("MyTreeViewStyle");
        }
        else
        {
            backgroundColor = "#313131";
            panelBackgroundColor = "#262626";

            panelTextColor = "#FFFFFF";
            
            settingsButtonColor = "#262626";
            logoColor = "#FFFFFF";
            borderColor = "#434343";

            borderPanelColor = "#525252";
            
            ButtonSettings.Style = (Style)FindResource("SettingsButtonDark");
            BtnToggleArrowGauche.Style = (Style)FindResource("ToggleButtonStyleDark");
            BtnToggleArrowDroite.Style = (Style)FindResource("ToggleButtonStyleDark");

            ApplyStyleToTreeViewItems(TreeViewGauche, "TreeViewItemStyleDark");
            ApplyStyleToTreeViewItems(TreeViewDroite, "TreeViewItemStyleDark");
        }
        
        // Panneaux et arrière-plan
        MainGrid.Background = ConvertStringColor(backgroundColor);
        ScrollViewerGauche.Background = ConvertStringColor(panelBackgroundColor);
        ScrollViewerDroite.Background = ConvertStringColor(panelBackgroundColor);
        
        // Bouton paramètre
        BrushSettings1.Brush = ConvertStringColor(logoColor);
        BrushSettings2.Brush = ConvertStringColor(logoColor);
        ButtonSettings.Background = ConvertStringColor(settingsButtonColor);
        ButtonSettings.BorderBrush = ConvertStringColor(borderColor);
        
        // Recherche
        Recherche.BorderBrush = ConvertStringColor(borderColor);
        Recherche.Background = ConvertStringColor(panelBackgroundColor);
        LogoRecherche.Brush = ConvertStringColor(logoColor);
        
        // Panel
        TextBlockAdressesGauche.Foreground = ConvertStringColor(panelTextColor);
        TextBlockAdressesDroite.Foreground = ConvertStringColor(panelTextColor);
        ChevronPanGauche.Brush = ConvertStringColor(logoColor);
        ChevronPanDroite.Brush = ConvertStringColor(logoColor);
        ScrollViewerGauche.Background = ConvertStringColor(panelBackgroundColor);
        ScrollViewerDroite.Background = ConvertStringColor(panelBackgroundColor);
        TreeViewGauche.Foreground = ConvertStringColor(panelTextColor);
        TreeViewDroite.Foreground = ConvertStringColor(panelTextColor);
        BorderPanGauche.BorderBrush = ConvertStringColor(borderPanelColor);
        BorderPanDroit.BorderBrush = ConvertStringColor(borderPanelColor);
        BorderTitrePanneauGauche.BorderBrush = ConvertStringColor(borderPanelColor);
        BorderTitrePanneauDroite.BorderBrush = ConvertStringColor(borderPanelColor);
        AjusteurPan.Background = ConvertStringColor(borderPanelColor);
    }   
    
    //--------------------- Gestion des boutons -----------------------------------------------------//

    private async void ImportProjectButtonClick(object sender, RoutedEventArgs e)
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
            
            // Créer et configurer la LoadingWindow
            loadingWindow = App.DisplayElements!.LoadingWindow = new LoadingWindow
            {
                Owner = this // Définir la fenêtre principale comme propriétaire de la fenêtre de chargement
            };
            
            ShowOverlay();
            await ExecuteLongRunningTask();
            HideOverlay();

            ViewModel.IsProjectImported = true;
        }
        else
        {
            App.ConsoleAndLogWriteLine("User aborted the file selection operation");
        }
    }
    
    private async Task ExecuteLongRunningTask()
    {
        if (App.DisplayElements!.SettingsWindow!.EnableLightTheme)
        {
            loadingWindow.SetLightMode();
        }
        else
        {
            loadingWindow.SetDarKMode();
        }
        
        App.DisplayElements.ShowLoadingWindow();
        
        TaskbarInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

        try
        {
            // Exécuter les tâches
            await Task.Run(async () =>
            {
                string task, loadingFinished;
                
                // Traduction de la fenêtre de chargement
                switch (App.DisplayElements?.SettingsWindow?.AppLang)
                {
                    // Arabe
                    case "AR":
                        task = "مهمة";
                        loadingFinished = "التحميل انتهى!";
                        break;

                    // Bulgare
                    case "BG":
                        task = "Задача";
                        loadingFinished = "Зареждането приключи!";
                        break;

                    // Tchèque
                    case "CS":
                        task = "Úkol";
                        loadingFinished = "Načítání dokončeno!";
                        break;

                    // Danois
                    case "DA":
                        task = "Opgave";
                        loadingFinished = "Indlæsning afsluttet!";
                        break;

                    // Allemand
                    case "DE":
                        task = "Aufgabe";
                        loadingFinished = "Laden abgeschlossen!";
                        break;

                    // Grec
                    case "EL":
                        task = "Εργασία";
                        loadingFinished = "Η φόρτωση ολοκληρώθηκε!";
                        break;

                    // Anglais
                    case "EN":
                        task = "Task";
                        loadingFinished = "Loading finished!";
                        break;

                    // Espagnol
                    case "ES":
                        task = "Tarea";
                        loadingFinished = "¡Carga terminada!";
                        break;

                    // Estonien
                    case "ET":
                        task = "Ülesanne";
                        loadingFinished = "Laadimine lõppenud!";
                        break;

                    // Finnois
                    case "FI":
                        task = "Tehtävä";
                        loadingFinished = "Lataus valmis!";
                        break;

                    // Hongrois
                    case "HU":
                        task = "Feladat";
                        loadingFinished = "Betöltés kész!";
                        break;

                    // Indonésien
                    case "ID":
                        task = "Tugas";
                        loadingFinished = "Pemuatan selesai!";
                        break;

                    // Italien
                    case "IT":
                        task = "Compito";
                        loadingFinished = "Caricamento completato!";
                        break;

                    // Japonais
                    case "JA":
                        task = "タスク";
                        loadingFinished = "読み込み完了！";
                        break;

                    // Coréen
                    case "KO":
                        task = "작업";
                        loadingFinished = "로드 완료!";
                        break;

                    // Letton
                    case "LV":
                        task = "Uzdevums";
                        loadingFinished = "Ielāde pabeigta!";
                        break;

                    // Lituanien
                    case "LT":
                        task = "Užduotis";
                        loadingFinished = "Įkėlimas baigtas!";
                        break;

                    // Norvégien
                    case "NB":
                        task = "Oppgave";
                        loadingFinished = "Laster ferdig!";
                        break;

                    // Néerlandais
                    case "NL":
                        task = "Taak";
                        loadingFinished = "Laden voltooid!";
                        break;

                    // Polonais
                    case "PL":
                        task = "Zadanie";
                        loadingFinished = "Ładowanie zakończone!";
                        break;

                    // Portugais
                    case "PT":
                        task = "Tarefa";
                        loadingFinished = "Carregamento concluído!";
                        break;

                    // Roumain
                    case "RO":
                        task = "Sarcină";
                        loadingFinished = "Încărcare terminată!";
                        break;

                    // Russe
                    case "RU":
                        task = "Задача";
                        loadingFinished = "Загрузка завершена!";
                        break;

                    // Slovaque
                    case "SK":
                        task = "Úloha";
                        loadingFinished = "Načítanie dokončené!";
                        break;

                    // Slovène
                    case "SL":
                        task = "Naloga";
                        loadingFinished = "Nalaganje končano!";
                        break;

                    // Suédois
                    case "SV":
                        task = "Uppgift";
                        loadingFinished = "Inläsning klar!";
                        break;

                    // Turc
                    case "TR":
                        task = "Görev";
                        loadingFinished = "Yükleme tamamlandı!";
                        break;

                    // Ukrainien
                    case "UK":
                        task = "Завдання";
                        loadingFinished = "Завантаження завершено!";
                        break;

                    // Chinois simplifié
                    case "ZH":
                        task = "任务";
                        loadingFinished = "加载完成！";
                        break;

                    // Langue par défaut (français)
                    default:
                        task = "Tâche";
                        loadingFinished = "Chargement terminé !";
                        break;
                }

                
                loadingWindow.UpdateTaskName($"{task} 1/4");
                await App.Fm.FindZeroXml(loadingWindow).ConfigureAwait(false);
                loadingWindow.UpdateTaskName($"{task} 2/4");
                await MyNameCorrector.CorrectName(loadingWindow).ConfigureAwait(false);
                
                _xmlFilePath1 = $"{App.Fm?.ProjectFolderPath}/GroupAddresses.xml";
                _xmlFilePath2 = App.Fm?.ProjectFolderPath + "UpdatedGroupAddresses.xml"; 
                //Define the project path
                loadingWindow.UpdateTaskName($"{task} 3/4");
                if (App.DisplayElements != null && App.DisplayElements.SettingsWindow!.RemoveUnusedGroupAddresses)
                {
                    await ExportUpdatedNameAddresses.Export(App.Fm?.ProjectFolderPath + "/0_original.xml",App.Fm?.ProjectFolderPath + "/GroupAddresses.xml", loadingWindow).ConfigureAwait(false);
                }
                else
                {
                    await ExportUpdatedNameAddresses.Export(App.Fm?.ZeroXmlPath!,App.Fm?.ProjectFolderPath + "/GroupAddresses.xml", loadingWindow).ConfigureAwait(false);
                }
                loadingWindow.UpdateTaskName($"{task} 3/4");
                await ExportUpdatedNameAddresses.Export(App.Fm?.ProjectFolderPath + "/0_updated.xml",App.Fm?.ProjectFolderPath + "/UpdatedGroupAddresses.xml", loadingWindow).ConfigureAwait(false);

                await LoadXmlFiles(loadingWindow).ConfigureAwait(false);

                // Mettre à jour l'interface utilisateur depuis le thread principal
                Dispatcher.Invoke(() =>
                {
                    loadingWindow.UpdateTaskName(loadingFinished);
                    loadingWindow.MarkActivityComplete();
                    loadingWindow.CompleteActivity();
                });
            });
        }
        finally
        {
            // Mettre à jour l'état de la barre des tâches et masquer l'overlay
            Dispatcher.Invoke(() =>
            {
                TaskbarInfo.ProgressState = TaskbarItemProgressState.None;
                loadingWindow.CloseAfterDelay(2000).ConfigureAwait(false);
            });
        }
    }
    
    private new void ShowOverlay()
    {
        if (App.DisplayElements!.SettingsWindow!.EnableLightTheme)
        {
            OverlayLight.Visibility = Visibility.Visible;
            MainContent.IsEnabled = false;
        }
        else
        {
            OverlayDark.Visibility = Visibility.Visible;
            MainContent.IsEnabled = false;
        }
    }
    
    private new void HideOverlay()
    {
        if (App.DisplayElements!.SettingsWindow!.EnableLightTheme)
        {
            OverlayLight.Visibility = Visibility.Collapsed;
            MainContent.IsEnabled = true;
        }
        else
        {
            OverlayDark.Visibility = Visibility.Collapsed;
            MainContent.IsEnabled = true;
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

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
        }
        else
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleWindowState()
    {
        if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowState = WindowState.Normal;
        }
    }
    
    private void OpenGroupAddressFileButtonClick(object sender, RoutedEventArgs e)
    {
        App.ConsoleAndLogWriteLine($"Opening {App.Fm?.ProjectFolderPath}UpdatedGroupAddresses.xml externally");

        // Résoudre le chemin absolu
        string absoluteFilePath = Path.GetFullPath($"{App.Fm?.ProjectFolderPath}UpdatedGroupAddresses.xml");

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
        string sourceFilePath = $"{App.Fm?.ProjectFolderPath}UpdatedGroupAddresses.xml";
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
            FileName = "UpdatedGroupAddresses.xml", // Nom de fichier par défaut
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
        // Vérifie si la fenêtre de paramètres est déjà ouverte
        if (App.DisplayElements!.SettingsWindow != null && App.DisplayElements.SettingsWindow.IsVisible)
        {
            // Si la fenêtre est déjà ouverte, la met au premier plan
            App.DisplayElements.SettingsWindow.Activate();
            App.DisplayElements.SettingsWindow.Focus();
        }
        else
        {
            // Sinon, affiche la fenêtre de paramètres
            App.DisplayElements.ShowSettingsWindow();
        }

        Keyboard.ClearFocus(); // On dé-sélectionne le bouton paramètres dans mainwindow
    }

    
    private void ApplyStyleToTreeViewItems(TreeView treeView, string style)
    {
        foreach (var item in treeView.Items)
        {
            if (treeView.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                ApplyStyleRecursive(treeViewItem, style);
            }
        }
    }

    // Méthode récursive pour appliquer le style à un TreeViewItem et ses enfants
    private void ApplyStyleRecursive(TreeViewItem item, string style)
    {
        item.Style = FindResource(style) as Style;

        // Parcourir les enfants récursivement
        foreach (var subItem in item.Items)
        {
            if (item.ItemContainerGenerator.ContainerFromItem(subItem) is TreeViewItem subTreeViewItem)
            {
                ApplyStyleRecursive(subTreeViewItem, style);
            }
        }
    }
    
    public static SolidColorBrush ConvertStringColor(string colorInput)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorInput));
    }
    
    //--------------------- Gestion de l'affichage à partir de fichiers -------------------------------//

    private async Task LoadXmlFiles(LoadingWindow loadingWindow)
    {            
        await LoadXmlFile(_xmlFilePath1, TreeViewGauche);
        await LoadXmlFile(_xmlFilePath2, TreeViewDroite);
    }

    private async Task LoadXmlFile(string filePath, TreeView treeView)
    {
        try
        {
            XmlDocument xmlDoc = await Task.Run(() =>
            {
                XmlDocument doc = new();
                doc.Load(filePath);
                return doc;
            });

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeView.Items.Clear();

                // Ajouter tous les nœuds récursivement
                if (xmlDoc.DocumentElement != null)
                {
                    foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                    {
                        AddNodeRecursively(node, treeView.Items, 0);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            // Afficher le message d'erreur sur le thread principal
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

    }

    private void AddNodeRecursively(XmlNode xmlNode, ItemCollection parentItems, int level)
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

    private TreeViewItem CreateTreeViewItemFromXmlNode(XmlNode xmlNode, int level)
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

        var treeNode = new TreeViewItem
        {
            Header = stack,
        };

        if (App.DisplayElements!.SettingsWindow!.EnableLightTheme)
        {
            treeNode.Style = (Style)FindResource("TreeViewItemStyle2");
        }
        else
        {
            treeNode.Style = (Style)FindResource("TreeViewItemStyleDark");
        }
        
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
        tb.Foreground = App.DisplayElements.SettingsWindow.EnableLightTheme ? 
            new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        // Utiliser un Dispatcher pour s'assurer que le TextBox a réellement perdu le focus
        tb?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!string.IsNullOrWhiteSpace(tb.Text)) return;
            tb.Text = "Chercher...";
            tb.Foreground = App.DisplayElements.SettingsWindow.EnableLightTheme ? 
                new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.DarkGray);
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
        if (TxtSearch1.Text != "Chercher...")
        {
            string normalizedSearchText = NormalizeString(TxtSearch1.Text);

            // Filtrer et masquer les éléments des deux TreeView basés sur le texte de recherche
            int itemCount = Math.Min(TreeViewGauche.Items.Count, TreeViewDroite.Items.Count);
            for (int i = 0; i < itemCount; i++)
            {
                if (TreeViewGauche.Items[i] is TreeViewItem item1 && TreeViewDroite.Items[i] is TreeViewItem item2)
                {
                    item1.Visibility = Visibility.Visible;
                    item2.Visibility = Visibility.Visible;
                    FilterTreeViewItems(item1, item2, normalizedSearchText);
                }

            }
        }
    }

    private static bool FilterTreeViewItems(TreeViewItem item1, TreeViewItem item2 , string searchText)
    {
        bool item1Visible = false; // Indicateur pour déterminer si l'élément est visible
        bool item2Visible = false;

        // Extraire le texte du TextBlock dans le Header du TreeViewItem
        string? headerText1 = null;
        string? headerText2 = null;

        if (item1.Header is StackPanel headerStack1 && item2.Header is StackPanel headerStack2 )
        {
            var textBlock1 = headerStack1.Children.OfType<TextBlock>().FirstOrDefault();
            var textBlock2 = headerStack2.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBlock1 != null && textBlock1 != null)
            {
                headerText1 = textBlock1.Text;
                headerText2 = textBlock2.Text;
            }
        }

        if (headerText1 == null && headerText2 == null)
        {
            item1.Visibility = Visibility.Collapsed;
            item2.Visibility = Visibility.Collapsed;

            return false; // Si l'entête est null, l'élément n'est pas visible
        }

        string normalizedHeader1 = NormalizeString(headerText1);
        string normalizedHeader2 = NormalizeString(headerText2);

        // Vérifier si l'élément correspond au texte de recherche
        if (normalizedHeader1.Contains(searchText, StringComparison.OrdinalIgnoreCase) || normalizedHeader2.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            item1.Visibility = Visibility.Visible; // Rendre visible l'élément
            item1.IsExpanded = true; // Développer l'élément pour montrer les enfants correspondants
            item1Visible = true; // Indiquer que l'élément est visible

            item2.Visibility = Visibility.Visible; // Rendre visible l'élément
            item2.IsExpanded = true; // Développer l'élément pour montrer les enfants correspondants
            item2Visible = true; // Indiquer que l'élément est visible
        }
        else
        {
            item1.Visibility = Visibility.Collapsed; // Masquer l'élément si le texte ne correspond pas
            item2.Visibility = Visibility.Collapsed; // Masquer l'élément si le texte ne correspond pas

        }


        bool hasVisibleChild = false;

        int itemCount = Math.Min(item1.Items.Count, item2.Items.Count);
        for (int i = 0; i < itemCount; i++)
        {
           
            if ((item1.Items[i] is TreeViewItem childItem1) && (item2.Items[i] is TreeViewItem childItem2))
            {
                // Appliquer le filtre aux enfants et mettre à jour l'indicateur de visibilité
                bool childVisible = FilterTreeViewItems(childItem1, childItem2, searchText);
                if (childVisible)
                {
                    hasVisibleChild = true;
                    item1.IsExpanded = true; // Développer l'élément si un enfant est visible
                    item2.IsExpanded = true;
                }
            }

        }

        // Si un enfant est visible, rendre visible cet élément
        if (hasVisibleChild)
        {
            item1.Visibility = Visibility.Visible;
            item1Visible = true;
            item2.Visibility = Visibility.Visible;
            item2Visible = true;
        }


        return item1Visible; // Retourner l'état de visibilité de l'élément
    }

    private static void SynchronizeVisibilityWithOtherTreeView(TreeViewItem item, TreeView otherTreeView)
    {
        foreach (object obj in otherTreeView.Items)
        {
            if (obj is TreeViewItem otherItem && item.Header.ToString() == otherItem.Header.ToString())
            {
                if (item.Visibility == Visibility.Visible)
                {
                    otherItem.Visibility = Visibility.Visible;
                }

                break;
            }
        }
    }

    private static string NormalizeString(string? input)
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

