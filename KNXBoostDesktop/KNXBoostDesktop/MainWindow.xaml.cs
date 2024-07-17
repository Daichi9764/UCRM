using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Windows;
using System.Windows.Interop;
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

    /// <summary>
    ///  Path to the original 0.xml file
    /// </summary>
    private string _xmlFilePath1 = "";

    /// <summary>
    ///  Path to the modified 0.xml file
    /// </summary>
    private string _xmlFilePath2 = "";

    /// <summary>
    ///  Path to the history file for the group address rename window
    /// </summary>
    private string _xmlRenameFilePath = "";

    /// <summary>
    ///  Texte "Rechercher ..." de la barre de recherche. A SUPPRIMER DES QUE POSSIBLE /!\
    /// </summary>
    private string _searchTextTranslate = "";

    /// <summary>
    /// Gets the main view model instance.
    /// </summary>
    private MainViewModel ViewModel { get; }
    
    /// <summary>
    /// Indicates whether the TreeView is expanded.
    /// </summary>
    private bool _isTreeViewExpanded;




    /* ------------------------------------------------------------------------------------------------
    --------------------------------------------- METHODES --------------------------------------------
    ------------------------------------------------------------------------------------------------ */

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up the render mode, data context, window icon, and updates the window contents.
    /// </summary>
    public MainWindow()
    {
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        InitializeComponent();

        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        
        Uri iconUri = new ("pack://application:,,,/resources/IconApp.ico", UriKind.RelativeOrAbsolute);
        Icon = BitmapFrame.Create(iconUri);
        
        UpdateWindowContents();
        
        LocationChanged += MainWindow_LocationChanged;
    }
    
    
    /// <summary>
    /// Handles the LocationChanged event of the MainWindow.
    /// This method will be called whenever the window's location changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void MainWindow_LocationChanged(object? sender, EventArgs e)
    {
        // Mettre à jour la position de la LoadingWindow lorsque MainWindow est déplacée
        if (App.DisplayElements != null && App.DisplayElements.LoadingWindow != null && App.DisplayElements.LoadingWindow.IsVisible)
        {
            App.DisplayElements.LoadingWindow.UpdatePosition(Left, Top);
        }
    }

    
    /// <summary>
    /// Updates the contents of the window, including theme and language.
    /// </summary>
    public void UpdateWindowContents()
    {
        // Traduction de la fenêtre principale
        switch (App.DisplayElements?.SettingsWindow?.AppLang)
        {
            // Arabe
            case "AR":
                _searchTextTranslate = "بحث...";
                TxtSearch1.Text = "بحث...";
                ButtonChargerProject.Content = "تحميل مشروع جديد";
                ButtonExportProject.Content = "تصدير المشروع المعدل";
                TextBlockAdressesGauche.Text = "عناوين المجموعة الأصلية";
                TextBlockAdressesDroite.Text = "عناوين المجموعة المعدلة";
                break;

            // Bulgare
            case "BG":
                _searchTextTranslate = "Търсене...";
                TxtSearch1.Text = "Търсене...";
                ButtonChargerProject.Content = "Зареждане на нов проект";
                ButtonExportProject.Content = "Експортиране на модифицирания проект";
                TextBlockAdressesGauche.Text = "Оригинални групови адреси";
                TextBlockAdressesDroite.Text = "Модифицирани групови адреси";
                break;

            // Tchèque
            case "CS":
                _searchTextTranslate = "Hledat...";
                TxtSearch1.Text = "Hledat...";
                ButtonChargerProject.Content = "Načíst nový projekt";
                ButtonExportProject.Content = "Exportovat upravený projekt";
                TextBlockAdressesGauche.Text = "Původní skupinové adresy";
                TextBlockAdressesDroite.Text = "Upravené skupinové adresy";
                break;

            // Danois
            case "DA":
                _searchTextTranslate = "Søg...";
                TxtSearch1.Text = "Søg...";
                ButtonChargerProject.Content = "Indlæs et nyt projekt";
                ButtonExportProject.Content = "Eksporter det ændrede projekt";
                TextBlockAdressesGauche.Text = "Originale gruppeadresser";
                TextBlockAdressesDroite.Text = "Ændrede gruppeadresser";
                break;

            // Allemand
            case "DE":
                _searchTextTranslate = "Suchen...";
                TxtSearch1.Text = "Suchen...";
                ButtonChargerProject.Content = "Neues Projekt laden";
                ButtonExportProject.Content = "Geändertes Projekt exportieren";
                TextBlockAdressesGauche.Text = "Ursprüngliche Gruppenadressen";
                TextBlockAdressesDroite.Text = "Geänderte Gruppenadressen";
                break;

            // Grec
            case "EL":
                _searchTextTranslate = "Αναζήτηση...";
                TxtSearch1.Text = "Αναζήτηση...";
                ButtonChargerProject.Content = "Φόρτωση νέου έργου";
                ButtonExportProject.Content = "Εξαγωγή τροποποιημένου έργου";
                TextBlockAdressesGauche.Text = "Πρωτότυπες ομαδικές διευθύνσεις";
                TextBlockAdressesDroite.Text = "Τροποποιημένες ομαδικές διευθύνσεις";
                break;

            // Anglais
            case "EN":
                _searchTextTranslate = "Search...";
                TxtSearch1.Text = "Search...";
                ButtonChargerProject.Content = "Load a new project";
                ButtonExportProject.Content = "Export the modified project";
                TextBlockAdressesGauche.Text = "Original Group Addresses";
                TextBlockAdressesDroite.Text = "Modified Group Addresses";
                break;

            // Espagnol
            case "ES":
                _searchTextTranslate = "Buscar...";
                TxtSearch1.Text = "Buscar...";
                ButtonChargerProject.Content = "Cargar un nuevo proyecto";
                ButtonExportProject.Content = "Exportar el proyecto modificado";
                TextBlockAdressesGauche.Text = "Direcciones de grupo originales";
                TextBlockAdressesDroite.Text = "Direcciones de grupo modificadas";
                break;

            // Estonien
            case "ET":
                _searchTextTranslate = "Otsi...";
                TxtSearch1.Text = "Otsi...";
                ButtonChargerProject.Content = "Laadi uus projekt";
                ButtonExportProject.Content = "Ekspordi muudetud projekt";
                TextBlockAdressesGauche.Text = "Algupärased grupiaadressid";
                TextBlockAdressesDroite.Text = "Muudetud grupiaadressid";
                break;

            // Finnois
            case "FI":
                _searchTextTranslate = "Hae...";
                TxtSearch1.Text = "Hae...";
                ButtonChargerProject.Content = "Lataa uusi projekti";
                ButtonExportProject.Content = "Vie muutettu projekti";
                TextBlockAdressesGauche.Text = "Alkuperäiset ryhmäosoitteet";
                TextBlockAdressesDroite.Text = "Muutetut ryhmäosoitteet";
                break;

            // Hongrois
            case "HU":
                _searchTextTranslate = "Keresés...";
                TxtSearch1.Text = "Keresés...";
                ButtonChargerProject.Content = "Új projekt betöltése";
                ButtonExportProject.Content = "A módosított projekt exportálása";
                TextBlockAdressesGauche.Text = "Eredeti csoportcímek";
                TextBlockAdressesDroite.Text = "Módosított csoportcímek";
                break;

            // Indonésien
            case "ID":
                _searchTextTranslate = "Cari...";
                TxtSearch1.Text = "Cari...";
                ButtonChargerProject.Content = "Muat proyek baru";
                ButtonExportProject.Content = "Ekspor proyek yang dimodifikasi";
                TextBlockAdressesGauche.Text = "Alamat Grup Asli";
                TextBlockAdressesDroite.Text = "Alamat Grup yang Dimodifikasi";
                break;

            // Italien
            case "IT":
                _searchTextTranslate = "Cerca...";
                TxtSearch1.Text = "Cerca...";
                ButtonChargerProject.Content = "Carica un nuovo progetto";
                ButtonExportProject.Content = "Esporta il progetto modificato";
                TextBlockAdressesGauche.Text = "Indirizzi di gruppo originali";
                TextBlockAdressesDroite.Text = "Indirizzi di gruppo modificati";
                break;

            // Japonais
            case "JA":
                _searchTextTranslate = "検索...";
                TxtSearch1.Text = "検索...";
                ButtonChargerProject.Content = "新しいプロジェクトをロード";
                ButtonExportProject.Content = "変更されたプロジェクトをエクスポート";
                TextBlockAdressesGauche.Text = "元のグループアドレス";
                TextBlockAdressesDroite.Text = "変更されたグループアドレス";
                break;

            // Coréen
            case "KO":
                _searchTextTranslate = "검색...";
                TxtSearch1.Text = "검색...";
                ButtonChargerProject.Content = "새 프로젝트 로드";
                ButtonExportProject.Content = "수정된 프로젝트 내보내기";
                TextBlockAdressesGauche.Text = "원본 그룹 주소";
                TextBlockAdressesDroite.Text = "수정된 그룹 주소";
                break;

            // Letton
            case "LV":
                _searchTextTranslate = "Meklēt...";
                TxtSearch1.Text = "Meklēt...";
                ButtonChargerProject.Content = "Ielādēt jaunu projektu";
                ButtonExportProject.Content = "Eksportēt modificēto projektu";
                TextBlockAdressesGauche.Text = "Oriģinālās grupu adreses";
                TextBlockAdressesDroite.Text = "Modificētās grupu adreses";
                break;

            // Lituanien
            case "LT":
                _searchTextTranslate = "Ieškoti...";
                TxtSearch1.Text = "Ieškoti...";
                ButtonChargerProject.Content = "Įkelti naują projektą";
                ButtonExportProject.Content = "Eksportuoti pakeistą projektą";
                TextBlockAdressesGauche.Text = "Originalūs grupių adresai";
                TextBlockAdressesDroite.Text = "Modifikuoti grupių adresai";
                break;

            // Norvégien
            case "NB":
                _searchTextTranslate = "Søk...";
                TxtSearch1.Text = "Søk...";
                ButtonChargerProject.Content = "Last inn et nytt prosjekt";
                ButtonExportProject.Content = "Eksporter det endrede prosjektet";
                TextBlockAdressesGauche.Text = "Opprinnelige gruppeadresser";
                TextBlockAdressesDroite.Text = "Endrede gruppeadresser";
                break;

            // Néerlandais
            case "NL":
                _searchTextTranslate = "Zoeken...";
                TxtSearch1.Text = "Zoeken...";
                ButtonChargerProject.Content = "Laad een nieuw project";
                ButtonExportProject.Content = "Exporteer het gewijzigde project";
                TextBlockAdressesGauche.Text = "Originele groepadressen";
                TextBlockAdressesDroite.Text = "Gewijzigde groepadressen";
                break;

            // Polonais
            case "PL":
                _searchTextTranslate = "Szukaj...";
                TxtSearch1.Text = "Szukaj...";
                ButtonChargerProject.Content = "Załaduj nowy projekt";
                ButtonExportProject.Content = "Eksportuj zmodyfikowany projekt";
                TextBlockAdressesGauche.Text = "Oryginalne adresy grup";
                TextBlockAdressesDroite.Text = "Zmodyfikowane adresy grup";
                break;

            // Portugais
            case "PT":
                _searchTextTranslate = "Pesquisar...";
                TxtSearch1.Text = "Pesquisar...";
                ButtonChargerProject.Content = "Carregar um novo projeto";
                ButtonExportProject.Content = "Exportar o projeto modificado";
                TextBlockAdressesGauche.Text = "Endereços de grupo originais";
                TextBlockAdressesDroite.Text = "Endereços de grupo modificados";
                break;

            // Roumain
            case "RO":
                _searchTextTranslate = "Căutare...";
                TxtSearch1.Text = "Căutare...";
                ButtonChargerProject.Content = "Încărcați un proiect nou";
                ButtonExportProject.Content = "Exportați proiectul modificat";
                TextBlockAdressesGauche.Text = "Adresele grupului original";
                TextBlockAdressesDroite.Text = "Adresele grupului modificate";
                break;

            // Russe
            case "RU":
                _searchTextTranslate = "Поиск...";
                TxtSearch1.Text = "Поиск...";
                ButtonChargerProject.Content = "Загрузить новый проект";
                ButtonExportProject.Content = "Экспортировать измененный проект";
                TextBlockAdressesGauche.Text = "Оригинальные групповые адреса";
                TextBlockAdressesDroite.Text = "Измененные групповые адреса";
                break;

            // Slovaque
            case "SK":
                _searchTextTranslate = "Hľadať...";
                TxtSearch1.Text = "Hľadať...";
                ButtonChargerProject.Content = "Načítať nový projekt";
                ButtonExportProject.Content = "Exportovať upravený projekt";
                TextBlockAdressesGauche.Text = "Pôvodné skupinové adresy";
                TextBlockAdressesDroite.Text = "Upravené skupinové adresy";
                break;

            // Slovène
            case "SL":
                _searchTextTranslate = "Iskanje...";
                TxtSearch1.Text = "Iskanje...";
                ButtonChargerProject.Content = "Naloži nov projekt";
                ButtonExportProject.Content = "Izvozi spremenjeni projekt";
                TextBlockAdressesGauche.Text = "Izvirni naslovi skupin";
                TextBlockAdressesDroite.Text = "Spremenjeni naslovi skupin";
                break;

            // Suédois
            case "SV":
                _searchTextTranslate = "Sök...";
                TxtSearch1.Text = "Sök...";
                ButtonChargerProject.Content = "Ladda ett nytt projekt";
                ButtonExportProject.Content = "Exportera det modifierade projektet";
                TextBlockAdressesGauche.Text = "Ursprungliga gruppadresser";
                TextBlockAdressesDroite.Text = "Ändrade gruppadresser";
                break;

            // Turc
            case "TR":
                _searchTextTranslate = "Ara...";
                TxtSearch1.Text = "Ara...";
                ButtonChargerProject.Content = "Yeni bir proje yükle";
                ButtonExportProject.Content = "Değiştirilen projeyi dışa aktar";
                TextBlockAdressesGauche.Text = "Orijinal Grup Adresleri";
                TextBlockAdressesDroite.Text = "Değiştirilen Grup Adresleri";
                break;

            // Ukrainien
            case "UK":
                _searchTextTranslate = "Пошук...";
                TxtSearch1.Text = "Пошук...";
                ButtonChargerProject.Content = "Завантажити новий проект";
                ButtonExportProject.Content = "Експортувати змінений проект";
                TextBlockAdressesGauche.Text = "Оригінальні групові адреси";
                TextBlockAdressesDroite.Text = "Змінені групові адреси";
                break;

            // Chinois simplifié
            case "ZH":
                _searchTextTranslate = "搜索...";
                TxtSearch1.Text = "搜索...";
                ButtonChargerProject.Content = "加载新项目";
                ButtonExportProject.Content = "导出修改后的项目";
                TextBlockAdressesGauche.Text = "原始组地址";
                TextBlockAdressesDroite.Text = "修改后的组地址";
                break;

            // Langue par défaut (français)
            default:
                _searchTextTranslate = "Chercher...";
                TxtSearch1.Text = "Chercher...";
                ButtonChargerProject.Content = "Charger un nouveau projet";
                ButtonExportProject.Content = "Exporter le projet modifié";
                TextBlockAdressesGauche.Text = "Adresses de Groupe Originales";
                TextBlockAdressesDroite.Text = "Adresses de Groupe Modifiées";
                break;
        }
        
        if (!string.IsNullOrEmpty(App.Fm?.ProjectName))
        {
            if (App.DisplayElements == null) return;
            App.DisplayElements.MainWindow.Title = App.DisplayElements.SettingsWindow!.AppLang switch
                {
                    // Arabe
                    "AR" => $"المشروع المستورد: {App.Fm.ProjectName}",
                    // Bulgare
                    "BG" => $"Импортиран проект: {App.Fm.ProjectName}",
                    // Tchèque
                    "CS" => $"Importovaný projekt: {App.Fm.ProjectName}",
                    // Danois
                    "DA" => $"Importerede projekt: {App.Fm.ProjectName}",
                    // Allemand
                    "DE" => $"Importiertes Projekt: {App.Fm.ProjectName}",
                    // Grec
                    "EL" => $"Εισαγόμενο έργο: {App.Fm.ProjectName}",
                    // Anglais
                    "EN" => $"Imported Project: {App.Fm.ProjectName}",
                    // Espagnol
                    "ES" => $"Proyecto importado: {App.Fm.ProjectName}",
                    // Estonien
                    "ET" => $"Imporditud projekt: {App.Fm.ProjectName}",
                    // Finnois
                    "FI" => $"Tuotu projekti: {App.Fm.ProjectName}",
                    // Hongrois
                    "HU" => $"Importált projekt: {App.Fm.ProjectName}",
                    // Indonésien
                    "ID" => $"Proyek yang diimpor: {App.Fm.ProjectName}",
                    // Italien
                    "IT" => $"Progetto importato: {App.Fm.ProjectName}",
                    // Japonais
                    "JA" => $"インポートされたプロジェクト: {App.Fm.ProjectName}",
                    // Coréen
                    "KO" => $"가져온 프로젝트: {App.Fm.ProjectName}",
                    // Letton
                    "LV" => $"Importēts projekts: {App.Fm.ProjectName}",
                    // Lituanien
                    "LT" => $"Importuotas projektas: {App.Fm.ProjectName}",
                    // Norvégien
                    "NB" => $"Importert prosjekt: {App.Fm.ProjectName}",
                    // Néerlandais
                    "NL" => $"Geïmporteerd project: {App.Fm.ProjectName}",
                    // Polonais
                    "PL" => $"Zaimportowany projekt: {App.Fm.ProjectName}",
                    // Portugais
                    "PT" => $"Projeto importado: {App.Fm.ProjectName}",
                    // Roumain
                    "RO" => $"Proiect importat: {App.Fm.ProjectName}",
                    // Russe
                    "RU" => $"Импортированный проект: {App.Fm.ProjectName}",
                    // Slovaque
                    "SK" => $"Importovaný projekt: {App.Fm.ProjectName}",
                    // Slovène
                    "SL" => $"Uvožen projekt: {App.Fm.ProjectName}",
                    // Suédois
                    "SV" => $"Importerade projekt: {App.Fm.ProjectName}",
                    // Turc
                    "TR" => $"İçe aktarılan proje: {App.Fm.ProjectName}",
                    // Ukrainien
                    "UK" => $"Імпортований проект: {App.Fm.ProjectName}",
                    // Chinois simplifié
                    "ZH" => $"导入项目: {App.Fm.ProjectName}",
                    // Cas par défaut (français)
                    _ => $"Projet importé : {App.Fm.ProjectName}"
                };
        }
        else
        {
            Title = App.DisplayElements?.SettingsWindow!.AppLang switch
        {
            // Arabe
            "AR" => "في انتظار فتح مشروع...",
            // Bulgare
            "BG" => "Изчакване за отваряне на проект...",
            // Tchèque
            "CS" => "Čekání na otevření projektu...",
            // Danois
            "DA" => "Venter på at åbne et projekt...",
            // Allemand
            "DE" => "Warten auf das Öffnen eines Projekts...",
            // Grec
            "EL" => "Αναμονή για άνοιγμα έργου...",
            // Anglais
            "EN" => "Waiting for a project to open...",
            // Espagnol
            "ES" => "Esperando a que se abra un proyecto...",
            // Estonien
            "ET" => "Ootab projekti avamist...",
            // Finnois
            "FI" => "Odotetaan projektin avaamista...",
            // Hongrois
            "HU" => "Projekt megnyitására várva...",
            // Indonésien
            "ID" => "Menunggu proyek dibuka...",
            // Italien
            "IT" => "In attesa dell'apertura di un progetto...",
            // Japonais
            "JA" => "プロジェクトのオープンを待っています...",
            // Coréen
            "KO" => "프로젝트 열기를 기다리는 중...",
            // Letton
            "LV" => "Gaida projekta atvēršanu...",
            // Lituanien
            "LT" => "Laukiama projekto atidarymo...",
            // Norvégien
            "NB" => "Venter på å åpne et prosjekt...",
            // Néerlandais
            "NL" => "Wachten op het openen van een project...",
            // Polonais
            "PL" => "Oczekiwanie na otwarcie projektu...",
            // Portugais
            "PT" => "Aguardando a abertura de um projeto...",
            // Roumain
            "RO" => "Așteptarea deschiderii unui proiect...",
            // Russe
            "RU" => "Ожидание открытия проекта...",
            // Slovaque
            "SK" => "Čaká sa na otvorenie projektu...",
            // Slovène
            "SL" => "Čakam na odprtje projekta...",
            // Suédois
            "SV" => "Väntar på att öppna ett projekt...",
            // Turc
            "TR" => "Projenin açılması bekleniyor...",
            // Ukrainien
            "UK" => "Очікування відкриття проекту...",
            // Chinois simplifié
            "ZH" => "等待项目打开...",
            // Cas par défaut (français)
            _ => "En attente de l'ouverture d'un projet..."
        };
        }
        
        string panelTextColor;
        
        string settingsButtonColor;
        string logoColor;
        string borderColor;
        string borderPanelColor;
        
        string panelBackgroundColor;
        string backgroundColor;
        
            
        if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableLightTheme)
        {
            panelTextColor = "#000000";
            panelBackgroundColor = "#FFFFFF";
            backgroundColor = "#F5F5F5";

            settingsButtonColor = "#FFFFFF";
            logoColor = "#000000";
            borderColor = "#D7D7D7";

            borderPanelColor = "#D7D7D7";
            
            ButtonSettings.Style = (Style)FindResource("SettingsButtonLight");
            BtnToggleArrowGauche.Style = (Style)FindResource("ToggleButtonStyle");
            BtnToggleArrowDroite.Style = (Style)FindResource("ToggleButtonStyle");

            ApplyStyleToTreeViewItems(TreeViewGauche, "TreeViewItemStyleLight");
            ApplyStyleToTreeViewItems(TreeViewDroite, "TreeViewItemStyleLight");
        }
        else
        {
            backgroundColor = "#313131";
            panelBackgroundColor = "#262626";

            panelTextColor = "#E3DED4";
            
            settingsButtonColor = "#262626";
            logoColor = "#E3DED4";
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

        if (App.DisplayElements != null && App.DisplayElements.SettingsWindow != null)
        {
            ApplyScaling(App.DisplayElements.SettingsWindow!.AppScaleFactor/100f);
        }
        
        const string filePath = "./runData.csv";
        LoadLoadingTimesFromCsv(filePath);
    }   
    
    
    
    //--------------------- Gestion des boutons -----------------------------------------------------//
    /// <summary>
    /// Handles the button click event to import a KNX project file.
    /// Displays an OpenFileDialog for the user to select the project file,
    /// extracts necessary files, shows a loading window during the import process,
    /// and updates the view model upon successful import.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private async void ImportProjectButtonClick(object sender, RoutedEventArgs e)
    {
        App.ConsoleAndLogWriteLine("Waiting for user to select KNX project file");
        
        // Créer une nouvelle instance de OpenFileDialog
        OpenFileDialog openFileDialog = new()
        {
            // Définir des propriétés optionnelles
            Title = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "اختر مشروع KNX للاستيراد",
                // Bulgare
                "BG" => "Изберете KNX проект за импортиране",
                // Tchèque
                "CS" => "Vyberte projekt KNX k importu",
                // Danois
                "DA" => "Vælg et KNX-projekt til import",
                // Allemand
                "DE" => "Wählen Sie ein KNX-Projekt zum Importieren",
                // Grec
                "EL" => "Επιλέξτε ένα έργο KNX προς εισαγωγή",
                // Anglais
                "EN" => "Select a KNX project to import",
                // Espagnol
                "ES" => "Seleccione un proyecto KNX para importar",
                // Estonien
                "ET" => "Valige KNX projekt importimiseks",
                // Finnois
                "FI" => "Valitse KNX-projekti tuominen",
                // Hongrois
                "HU" => "Válasszon egy KNX projektet importálásra",
                // Indonésien
                "ID" => "Pilih proyek KNX untuk diimpor",
                // Italien
                "IT" => "Seleziona un progetto KNX da importare",
                // Japonais
                "JA" => "インポートするKNXプロジェクトを選択",
                // Coréen
                "KO" => "가져올 KNX 프로젝트를 선택하세요",
                // Letton
                "LV" => "Izvēlieties KNX projektu importēšanai",
                // Lituanien
                "LT" => "Pasirinkite KNX projektą importuoti",
                // Norvégien
                "NB" => "Velg et KNX-prosjekt å importere",
                // Néerlandais
                "NL" => "Selecteer een KNX-project om te importeren",
                // Polonais
                "PL" => "Wybierz projekt KNX do importu",
                // Portugais
                "PT" => "Selecione um projeto KNX para importar",
                // Roumain
                "RO" => "Selectați un proiect KNX pentru import",
                // Russe
                "RU" => "Выберите проект KNX для импорта",
                // Slovaque
                "SK" => "Vyberte projekt KNX na import",
                // Slovène
                "SL" => "Izberite KNX projekt za uvoz",
                // Suédois
                "SV" => "Välj ett KNX-projekt att importera",
                // Turc
                "TR" => "İçe aktarılacak bir KNX projesi seçin",
                // Ukrainien
                "UK" => "Виберіть проект KNX для імпорту",
                // Chinois simplifié
                "ZH" => "选择要导入的KNX项目",
                // Cas par défaut (français)
                _ => "Sélectionnez un projet KNX à importer"
            },
            Filter = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "ملفات مشروع KNX|*.knxproj|جميع الملفات|*.*",
                // Bulgare
                "BG" => "KNX проектни файлове|*.knxproj|Всички файлове|*.*",
                // Tchèque
                "CS" => "Soubor projektu KNX|*.knxproj|Všechny soubory|*.*",
                // Danois
                "DA" => "KNX projektfiler|*.knxproj|Alle filer|*.*",
                // Allemand
                "DE" => "KNX-Projektdateien|*.knxproj|Alle Dateien|*.*",
                // Grec
                "EL" => "Αρχεία έργου KNX|*.knxproj|Όλα τα αρχεία|*.*",
                // Anglais
                "EN" => "KNX Project Files|*.knxproj|All Files|*.*",
                // Espagnol
                "ES" => "Archivos de proyecto KNX|*.knxproj|Todos los archivos|*.*",
                // Estonien
                "ET" => "KNX projekti failid|*.knxproj|Kõik failid|*.*",
                // Finnois
                "FI" => "KNX-projektitiedostot|*.knxproj|Kaikki tiedostot|*.*",
                // Hongrois
                "HU" => "KNX projektfájlok|*.knxproj|Minden fájl|*.*",
                // Indonésien
                "ID" => "File Proyek KNX|*.knxproj|Semua file|*.*",
                // Italien
                "IT" => "File di progetto KNX|*.knxproj|Tutti i file|*.*",
                // Japonais
                "JA" => "KNXプロジェクトファイル|*.knxproj|すべてのファイル|*.*",
                // Coréen
                "KO" => "KNX 프로젝트 파일|*.knxproj|모든 파일|*.*",
                // Letton
                "LV" => "KNX projekta faili|*.knxproj|Visi faili|*.*",
                // Lituanien
                "LT" => "KNX projekto failai|*.knxproj|Visi failai|*.*",
                // Norvégien
                "NB" => "KNX prosjektfiler|*.knxproj|Alle filer|*.*",
                // Néerlandais
                "NL" => "KNX-projectbestanden|*.knxproj|Alle bestanden|*.*",
                // Polonais
                "PL" => "Pliki projektu KNX|*.knxproj|Wszystkie pliki|*.*",
                // Portugais
                "PT" => "Arquivos de projeto KNX|*.knxproj|Todos os arquivos|*.*",
                // Roumain
                "RO" => "Fișiere proiect KNX|*.knxproj|Toate fișierele|*.*",
                // Russe
                "RU" => "Файлы проекта KNX|*.knxproj|Все файлы|*.*",
                // Slovaque
                "SK" => "Súbory projektu KNX|*.knxproj|Všetky súbory|*.*",
                // Slovène
                "SL" => "Datoteke projekta KNX|*.knxproj|Vse datoteke|*.*",
                // Suédois
                "SV" => "KNX-projektfiler|*.knxproj|Alla filer|*.*",
                // Turc
                "TR" => "KNX Proje Dosyaları|*.knxproj|Tüm Dosyalar|*.*",
                // Ukrainien
                "UK" => "Файли проекту KNX|*.knxproj|Усі файли|*.*",
                // Chinois simplifié
                "ZH" => "KNX 项目文件|*.knxproj|所有文件|*.*",
                // Cas par défaut (français)
                _ => "Fichiers projet ETS|*.knxproj|Tous les fichiers|*.*"
            },
            FilterIndex = 1,
            Multiselect = false
        };

        // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
        var result = openFileDialog.ShowDialog();

        if (result == true)
        {
            // Récupérer le chemin du fichier sélectionné
            App.ConsoleAndLogWriteLine($"File selected: {openFileDialog.FileName}");

            // Si le file manager n'existe pas ou que l'on n'a pas réussi à extraire les fichiers du projet, on annule l'opération
            if ((App.Fm == null)||(!App.Fm.ExtractProjectFiles(openFileDialog.FileName))) return;
            
            // Créer et configurer la LoadingWindow
            App.DisplayElements!.LoadingWindow = new LoadingWindow
            {
                Owner = this
            };
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Tâche qui met à jour l'affichage du temps écoulé toutes les 100ms
            var updateTask = Task.Run(async () =>
            {
                while (stopwatch.IsRunning)
                {
                    var elapsedTime = stopwatch.Elapsed;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.DisplayElements.LoadingWindow.TotalTime.Text = 
                        App.DisplayElements.SettingsWindow!.AppLang switch
                        {
                            // Arabe
                            "AR" => $"الوقت الإجمالي: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Bulgare
                            "BG" => $"Общо време: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Tchèque
                            "CS" => $"Celkový čas: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Danois
                            "DA" => $"Samlet tid: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Allemand
                            "DE" => $"Gesamtzeit: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Grec
                            "EL" => $"Συνολικός χρόνος: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Anglais
                            "EN" => $"Total time: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Espagnol
                            "ES" => $"Tiempo total: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Estonien
                            "ET" => $"Koguaeg: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Finnois
                            "FI" => $"Kokonaisaika: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Hongrois
                            "HU" => $"Összes idő: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Indonésien
                            "ID" => $"Total waktu: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Italien
                            "IT" => $"Tempo totale: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Japonais
                            "JA" => $"総時間: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Coréen
                            "KO" => $"총 시간: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Letton
                            "LV" => $"Kopējais laiks: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Lituanien
                            "LT" => $"Bendras laikas: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Norvégien
                            "NB" => $"Total tid: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Néerlandais
                            "NL" => $"Totale tijd: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Polonais
                            "PL" => $"Całkowity czas: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Portugais
                            "PT" => $"Tempo total: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Roumain
                            "RO" => $"Timp total: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Russe
                            "RU" => $"Общее время: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Slovaque
                            "SK" => $"Celkový čas: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Slovène
                            "SL" => $"Skupni čas: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Suédois
                            "SV" => $"Total tid: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Turc
                            "TR" => $"Toplam süre: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Ukrainien
                            "UK" => $"Загальний час: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Chinois simplifié
                            "ZH" => $"总时间: {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}",
                            // Cas par défaut (français)
                            _ => $"Temps total : {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}"
                        };

                    });
                    await Task.Delay(100); 
                }
            });
            
            ShowOverlay();
            await ExecuteLongRunningTask();
            HideOverlay();
            
            stopwatch.Stop();
            TimeSpan finalElapsedTime = stopwatch.Elapsed;

            Application.Current.Dispatcher.Invoke(() =>
            {
                App.DisplayElements.LoadingWindow.TotalTime.Text = 
                        App.DisplayElements.SettingsWindow!.AppLang switch
                        {
                            // Arabe
                            "AR" => $"الوقت الإجمالي: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Bulgare
                            "BG" => $"Общо време: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Tchèque
                            "CS" => $"Celkový čas: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Danois
                            "DA" => $"Samlet tid: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Allemand
                            "DE" => $"Gesamtzeit: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Grec
                            "EL" => $"Συνολικός χρόνος: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Anglais
                            "EN" => $"Total time: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Espagnol
                            "ES" => $"Tiempo total: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Estonien
                            "ET" => $"Koguaeg: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Finnois
                            "FI" => $"Kokonaisaika: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Hongrois
                            "HU" => $"Összes idő: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Indonésien
                            "ID" => $"Total waktu: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Italien
                            "IT" => $"Tempo totale: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Japonais
                            "JA" => $"総時間: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Coréen
                            "KO" => $"총 시간: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Letton
                            "LV" => $"Kopējais laiks: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Lituanien
                            "LT" => $"Bendras laikas: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Norvégien
                            "NB" => $"Total tid: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Néerlandais
                            "NL" => $"Totale tijd: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Polonais
                            "PL" => $"Całkowity czas: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Portugais
                            "PT" => $"Tempo total: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Roumain
                            "RO" => $"Timp total: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Russe
                            "RU" => $"Общее время: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Slovaque
                            "SK" => $"Celkový čas: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Slovène
                            "SL" => $"Skupni čas: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Suédois
                            "SV" => $"Total tid: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Turc
                            "TR" => $"Toplam süre: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Ukrainien
                            "UK" => $"Загальний час: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Chinois simplifié
                            "ZH" => $"总时间: {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}",
                            // Cas par défaut (français)
                            _ => $"Temps total : {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}"
                        };
            });

            const string filePath = "./runData.csv";
            var loadingTimes = LoadLoadingTimesFromCsv(filePath);
            loadingTimes?.Add(new LoadingTimeEntry
            {
                ProjectName = App.Fm.ProjectName,
                AddressCount = GroupAddressNameCorrector.totalAddresses,
                DeviceCount = GroupAddressNameCorrector.totalDevices,
                IsDeleted = App.DisplayElements.SettingsWindow != null &&
                            (bool)App.DisplayElements.SettingsWindow.RemoveUnusedAddressesCheckBox.IsChecked!,
                DeletedAddresses = GroupAddressNameCorrector.totalDeletedAddresses,
                IsTranslated = App.DisplayElements.SettingsWindow != null && 
                               (bool)App.DisplayElements.SettingsWindow.EnableTranslationCheckBox.IsChecked!,
                TotalLoadingTime = finalElapsedTime
            });

            // Attend la fin de la tâche de mise à jour (au cas où elle serait encore en cours)
            await updateTask;
            
            SaveLoadingTimesAsCsv(filePath, loadingTimes);
            //LoadLoadingTimesFromCsv(filePath);
            ViewModel.IsProjectImported = true;
        }
        else
        {
            App.ConsoleAndLogWriteLine("User aborted the file selection operation");
        }
    }

    public async void ReloadProject()
    {
        App.ConsoleAndLogWriteLine("Reaload");

            // Créer et configurer la LoadingWindow
            App.DisplayElements!.LoadingWindow = new LoadingWindow
            {
                Owner = this // Définir la fenêtre principale comme propriétaire de la fenêtre de chargement
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Tâche qui met à jour l'affichage du temps écoulé toutes les 100ms
            var updateTask = Task.Run(async () =>
            {
                while (stopwatch.IsRunning)
                {
                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    // Utiliser le Dispatcher pour mettre à jour l'UI sur le thread approprié
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.DisplayElements.LoadingWindow.TotalTime.Text =
                            $"Temps total : {(int)elapsedTime.TotalMinutes:D2}:{elapsedTime.Seconds:D2}";
                    });
                    await Task.Delay(100); // Met à jour toutes les 100ms
                }
            });

            ShowOverlay();
            await ExecuteLongRunningTask();
            HideOverlay();

            stopwatch.Stop();
            TimeSpan finalElapsedTime = stopwatch.Elapsed;

            // Mise à jour finale de l'affichage
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.DisplayElements.LoadingWindow.TotalTime.Text =
                    $"Temps total : {(int)finalElapsedTime.TotalMinutes:D2}:{finalElapsedTime.Seconds:D2}";
            });

            // Attend la fin de la tâche de mise à jour (au cas où elle serait encore en cours)
            await updateTask;

            ViewModel.IsProjectImported = true;

    }

    /// <summary>
    /// Handles the click event for the Open Console button.
    /// Opens the console window and ensures it scrolls to the bottom to display the latest messages.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OpenConsoleButtonClick(object sender, RoutedEventArgs e)
    {
        if (App.DisplayElements == null) return;

        App.ConsoleAndLogWriteLine("Opening console window");
        App.DisplayElements.ShowConsoleWindow();

        // Pour éviter qu'à la réouverture de la console, on ait quelques lignes de retard, on scrolle en bas dès l'ouverture
        if (App.DisplayElements.ConsoleWindow.IsVisible)
        {
            App.DisplayElements.ConsoleWindow.ConsoleTextBox.ScrollToEnd();
        }
    }

    /// <summary>
    /// Saves loading times to a CSV file.
    /// </summary>
    /// <param name="filePath">The full path of the CSV file where the data will be written.</param>
    /// <param name="loadingTimes">The list of loading time entries to save.</param>
    private static void SaveLoadingTimesAsCsv(string filePath, List<LoadingTimeEntry>? loadingTimes)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("ProjectName,AddressCount,DeviceCount,IsDeleted?,DeletedAddresses,IsTranslated?,TotalLoadingTime");
        if (loadingTimes == null) return;
            
        foreach (var entry in loadingTimes)
        {
            writer.WriteLine($"{App.Fm?.ProjectName}," +
                             $"{entry.AddressCount}," +
                             $"{entry.DeviceCount}," +
                             $"{entry.IsDeleted}," +
                             $"{entry.DeletedAddresses}," +
                             $"{entry.IsTranslated}," +
                             $"{(int)entry.TotalLoadingTime.TotalMinutes:D2}:{entry.TotalLoadingTime.Seconds:D2}");
        }
    }

    /// <summary>
    /// Loads loading time entries from a CSV file located at the specified file path.
    /// </summary>
    /// <param name="filePath">The file path of the CSV file.</param>
    /// <returns>A list of LoadingTimeEntry objects if successful; otherwise, null.</returns>
    private static List<LoadingTimeEntry>? LoadLoadingTimesFromCsv(string filePath)
    {
        var loadingTimes = new List<LoadingTimeEntry>();
        
        try
        {
            // Si le fichier de paramétrage n'existe pas, on le crée
            // Note : comme File.Create ouvre un stream vers le fichier à la création, on le ferme directement avec Close().
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }
        // Si le programme n'a pas accès en écriture pour créer le fichier
        catch (UnauthorizedAccessException)
        {
            // Définir les variables pour le texte du message et le titre du MessageBox
            var messageBoxText = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "خطأ: تعذر الوصول إلى ملف البيانات. يرجى التحقق من أنه ليس للقراءة فقط وحاول مرة أخرى، أو قم بتشغيل البرنامج كمسؤول.\nرمز الخطأ: 10",
                // Bulgare
                "BG" => "Грешка: Не може да се получи достъп до файла с данни. Моля, проверете дали файлът не е само за четене и опитайте отново, или стартирайте програмата като администратор.\nКод за грешка: 10",
                // Tchèque
                "CS" => "Chyba: Nelze získat přístup k datovému souboru. Zkontrolujte, zda není pouze ke čtení, a zkuste to znovu, nebo spusťte program jako správce.\nChybový kód: 10",
                // Danois
                "DA" => "Fejl: Kan ikke få adgang til datafilen. Kontroller venligst, at filen ikke er skrivebeskyttet, og prøv igen, eller start programmet som administrator.\nFejlkode: 10",
                // Allemand
                "DE" => "Fehler: Zugriff auf die Datendatei nicht möglich. Bitte überprüfen Sie, ob die Datei schreibgeschützt ist, und versuchen Sie es erneut, oder starten Sie das Programm als Administrator.\nFehlercode: 10",
                // Grec
                "EL" => "Σφάλμα: δεν είναι δυνατή η πρόσβαση στο αρχείο δεδομένων. Παρακαλώ ελέγξτε αν δεν είναι μόνο για ανάγνωση και προσπαθήστε ξανά, ή ξεκινήστε το πρόγραμμα ως διαχειριστής.\nΚωδικός σφάλματος: 10",
                // Anglais
                "EN" => "Error: Unable to access the data file. Please check if it is read-only and try again, or run the program as an administrator.\nError Code: 10",
                // Espagnol
                "ES" => "Error: No se puede acceder al archivo de datos. Por favor, verifique si el archivo es de solo lectura y vuelva a intentarlo, o ejecute el programa como administrador.\nCódigo de error: 10",
                // Estonien
                "ET" => "Viga: ei saa andmefailile juurde pääseda. Kontrollige, kas fail on ainult lugemiseks ja proovige uuesti või käivitage programm administraatorina.\nVeakood: 10",
                // Finnois
                "FI" => "Virhe: Data tiedostoon ei pääse käsiksi. Tarkista, ettei tiedosto ole vain luku -tilassa, ja yritä uudelleen tai käynnistä ohjelma järjestelmänvalvojana.\nVirhekoodi: 10",
                // Hongrois
                "HU" => "Hiba: Nem lehet hozzáférni az adatfájlhoz. Kérjük, ellenőrizze, hogy a fájl nem csak olvasásra van-e beállítva, és próbálja újra, vagy futtassa a programot rendszergazdai jogosultságokkal.\nHibakód: 10",
                // Indonésien
                "ID" => "Kesalahan: tidak dapat mengakses file data. Silakan periksa apakah file tersebut hanya-baca dan coba lagi, atau jalankan program sebagai administrator.\nKode kesalahan: 10",
                // Italien
                "IT" => "Errore: impossibile accedere al file dei dati. Verifica se il file è solo in lettura e riprova, oppure avvia il programma come amministratore.\nCodice errore: 10",
                // Japonais
                "JA" => "エラー: データファイルにアクセスできません。ファイルが読み取り専用でないか確認し、再試行するか、管理者としてプログラムを実行してください。\nエラーコード: 10",
                // Coréen
                "KO" => "오류: 데이터 파일에 액세스할 수 없습니다. 파일이 읽기 전용인지 확인하고 다시 시도하거나 관리자로 프로그램을 실행하세요.\n오류 코드: 10",
                // Letton
                "LV" => "Kļūda: nevar piekļūt datu failam. Lūdzu, pārbaudiet, vai fails nav tikai lasāms, un mēģiniet vēlreiz vai palaidiet programmu kā administrators.\nKļūdas kods: 10",
                // Lituanien
                "LT" => "Klaida: negalima prieiti prie duomenų failo. Patikrinkite, ar failas nėra tik skaitymui ir bandykite dar kartą arba paleiskite programą kaip administratorius.\nKlaidos kodas: 10",
                // Norvégien
                "NB" => "Feil: Kan ikke få tilgang til datafilen. Sjekk om filen er skrivebeskyttet og prøv igjen, eller kjør programmet som administrator.\nFeilkode: 10",
                // Néerlandais
                "NL" => "Fout: kan geen toegang krijgen tot het databestand. Controleer of het bestand alleen-lezen is en probeer het opnieuw, of voer het programma uit als administrator.\nFoutcode: 10",
                // Polonais
                "PL" => "Błąd: Nie można uzyskać dostępu do pliku danych. Sprawdź, czy plik nie jest tylko do odczytu, a następnie spróbuj ponownie lub uruchom program jako administrator.\nKod błędu: 10",
                // Portugais
                "PT" => "Erro: não foi possível acessar o arquivo de dados. Verifique se o arquivo é somente leitura e tente novamente, ou execute o programa como administrador.\nCódigo de erro: 10",
                // Roumain
                "RO" => "Eroare: Nu se poate accesa fișierul de date. Vă rugăm să verificați dacă fișierul este numai pentru citire și să încercați din nou sau să rulați programul ca administrator.\nCod eroare: 10",
                // Russe
                "RU" => "Ошибка: невозможно получить доступ к файлу данных. Проверьте, не является ли файл только для чтения, и попробуйте снова, или запустите программу от имени администратора.\nКод ошибки: 10",
                // Slovaque
                "SK" => "Chyba: nemožno získať prístup k dátovému súboru. Skontrolujte, či nie je súbor iba na čítanie, a skúste to znova, alebo spustite program ako správca.\nChybový kód: 10",
                // Slovène
                "SL" => "Napaka: dostop do podatkovne datoteke ni mogoč. Preverite, ali je datoteka samo za branje, in poskusite znova, ali zaženite program kot skrbnik.\nKoda napake: 10",
                // Suédois
                "SV" => "Fel: Kan inte komma åt datafilen. Kontrollera om filen är skrivskyddad och försök igen, eller kör programmet som administratör.\nFelkod: 10",
                // Turc
                "TR" => "Hata: Veri dosyasına erişilemiyor. Dosyanın salt okunur olup olmadığını kontrol edin ve tekrar deneyin veya programı yönetici olarak çalıştırın.\nHata Kodu: 10",
                // Ukrainien
                "UK" => "Помилка: неможливо отримати доступ до файлу даних. Будь ласка, перевірте, чи не є файл тільки для читання, і спробуйте ще раз або запустіть програму від імені адміністратора.\nКод помилки: 10",
                // Chinois simplifié
                "ZH" => "错误: 无法访问数据文件。请检查文件是否为只读，并重试，或者以管理员身份运行程序。\n错误代码: 10",
                // Cas par défaut (français)
                _ => "Erreur: impossible d'accéder au fichier de données. Veuillez vérifier qu'il n'est pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 10"
            };


            var caption = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "خطأ",
                // Bulgare
                "BG" => "Грешка",
                // Tchèque
                "CS" => "Chyba",
                // Danois
                "DA" => "Fejl",
                // Allemand
                "DE" => "Fehler",
                // Grec
                "EL" => "Σφάλμα",
                // Anglais
                "EN" => "Error",
                // Espagnol
                "ES" => "Error",
                // Estonien
                "ET" => "Viga",
                // Finnois
                "FI" => "Virhe",
                // Hongrois
                "HU" => "Hiba",
                // Indonésien
                "ID" => "Kesalahan",
                // Italien
                "IT" => "Errore",
                // Japonais
                "JA" => "エラー",
                // Coréen
                "KO" => "오류",
                // Letton
                "LV" => "Kļūda",
                // Lituanien
                "LT" => "Klaida",
                // Norvégien
                "NB" => "Feil",
                // Néerlandais
                "NL" => "Fout",
                // Polonais
                "PL" => "Błąd",
                // Portugais
                "PT" => "Erro",
                // Roumain
                "RO" => "Eroare",
                // Russe
                "RU" => "Ошибка",
                // Slovaque
                "SK" => "Chyba",
                // Slovène
                "SL" => "Napaka",
                // Suédois
                "SV" => "Fel",
                // Turc
                "TR" => "Hata",
                // Ukrainien
                "UK" => "Помилка",
                // Chinois simplifié
                "ZH" => "错误",
                // Cas par défaut (français)
                _ => "Erreur"
            };

            // Afficher le MessageBox avec les traductions appropriées
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(1);
        }
        // Si la longueur du path est incorrecte ou que des caractères non supportés sont présents
        catch (ArgumentException)
        {
            // Traductions des messages d'erreur et du titre en fonction de la langue
            var errorTitle = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                "AR" => "خطأ",
                "BG" => "Грешка",
                "CS" => "Chyba",
                "DA" => "Fejl",
                "DE" => "Fehler",
                "EL" => "Σφάλμα",
                "EN" => "Error",
                "ES" => "Error",
                "ET" => "Viga",
                "FI" => "Virhe",
                "HU" => "Hiba",
                "ID" => "Kesalahan",
                "IT" => "Errore",
                "JA" => "エラー",
                "KO" => "오류",
                "LV" => "Kļūda",
                "LT" => "Klaida",
                "NB" => "Feil",
                "NL" => "Fout",
                "PL" => "Błąd",
                "PT" => "Erro",
                "RO" => "Eroare",
                "RU" => "Ошибка",
                "SK" => "Chyba",
                "SL" => "Napaka",
                "SV" => "Fel",
                "TR" => "Hata",
                "UK" => "Помилка",
                "ZH" => "错误",
                _ => "Erreur"
            };

            var errorMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                "AR" => $"خطأ: هناك أحرف غير مدعومة في مسار ملف البيانات ({filePath}). تعذر الوصول إلى الملف.\nرمز الخطأ: 20",
                "BG" => $"Грешка: Съдържа неразрешени символи в пътя на файла с данни ({filePath}). Невъзможно е да се достъпи до файла.\nКод на грешката: 20",
                "CS" => $"Chyba: V cestě k datovému souboru ({filePath}) jsou přítomny nepodporované znaky. Nelze přistupovat k souboru.\nKód chyby: 20",
                "DA" => $"Fejl: Ugyldige tegn findes i stien til datafilen ({filePath}). Kan ikke få adgang til filen.\nFejlkode: 20",
                "DE" => $"Fehler: Im Pfad zur Datendatei ({filePath}) sind nicht unterstützte Zeichen vorhanden. Auf die Datei kann nicht zugegriffen werden.\nFehlercode: 20",
                "EL" => $"Σφάλμα: Υπάρχουν μη υποστηριγμένοι χαρακτήρες στη διαδρομή του αρχείου δεδομένων ({filePath}). Δεν είναι δυνατή η πρόσβαση στο αρχείο.\nΚωδικός σφάλματος: 20",
                "EN" => $"Error: Unsupported characters are present in the data file path ({filePath}). Unable to access the file.\nError code: 20",
                "ES" => $"Error: Hay caracteres no admitidos en la ruta del archivo de datos ({filePath}). No se puede acceder al archivo.\nCódigo de error: 20",
                "ET" => $"Viga: Andmefaili tee ({filePath}) sisaldab toetamatuid märke. Failile ei ole võimalik juurde pääseda.\nVigakood: 20",
                "FI" => $"Virhe: Tiedoston polussa ({filePath}) on tukemattomia merkkejä. Tiedostoon ei voi käyttää.\nVirhekoodi: 20",
                "HU" => $"Hiba: Az adatfájl elérési útvonalán ({filePath}) nem támogatott karakterek találhatók. A fájlhoz nem lehet hozzáférni.\nHibakód: 20",
                "ID" => $"Kesalahan: Karakter yang tidak didukung ada di jalur file data ({filePath}). Tidak dapat mengakses file.\nKode kesalahan: 20",
                "IT" => $"Errore: Sono presenti caratteri non supportati nel percorso del file di dati ({filePath}). Impossibile accedere al file.\nCodice errore: 20",
                "JA" => $"エラー: データファイルのパス ({filePath}) にサポートされていない文字が含まれています。ファイルにアクセスできません。\nエラーコード: 20",
                "KO" => $"오류: 데이터 파일 경로 ({filePath})에 지원되지 않는 문자가 포함되어 있습니다. 파일에 접근할 수 없습니다.\n오류 코드: 20",
                "LV" => $"Kļūda: Datu faila ceļā ({filePath}) ir neatbalstīti rakstzīmes. Nevar piekļūt failam.\nKļūdas kods: 20",
                "LT" => $"Klaida: Nustatymų failo kelias ({filePath}) turi nepalaikomų simbolių. Nepavyksta pasiekti failo.\nKlaidos kodas: 20",
                "NB" => $"Feil: Det finnes ikke-støttede tegn i stien til datafilen ({filePath}). Kan ikke få tilgang til filen.\nFeilkode: 20",
                "NL" => $"Fout: Onondersteunde tekens zijn aanwezig in het pad naar het databestand ({filePath}). Kan niet toegang krijgen tot het bestand.\nFoutcode: 20",
                "PL" => $"Błąd: W ścieżce pliku danych ({filePath}) znajdują się nieobsługiwane znaki. Nie można uzyskać dostępu do pliku.\nKod błędu: 20",
                "PT" => $"Erro: Caracteres não suportados estão presentes no caminho do arquivo de dados ({filePath}). Não é possível acessar o arquivo.\nCódigo de erro: 20",
                "RO" => $"Eroare: Caracterelor nesuportate sunt prezente în calea fișierului de date ({filePath}). Nu se poate accesa fișierul.\nCod eroare: 20",
                "RU" => $"Ошибка: В пути к файлу данных ({filePath}) присутствуют неподдерживаемые символы. Невозможно получить доступ к файлу.\nКод ошибки: 20",
                "SK" => $"Chyba: V ceste k súboru údajov ({filePath}) sú prítomné nepodporované znaky. Nie je možné pristupovať k súboru.\nKód chyby: 20",
                "SL" => $"Napaka: V poti do podatkovne datoteke ({filePath}) so prisotne nepodprte znake. Do datoteke ni mogoče dostopati.\nKoda napake: 20",
                "SV" => $"Fel: I datafilens sökväg ({filePath}) finns tecken som inte stöds. Kan inte komma åt filen.\nFelkod: 20",
                "TR" => $"Hata: Veri dosyası yolunda ({filePath}) desteklenmeyen karakterler bulunuyor. Dosyaya erişilemiyor.\nHata kodu: 20",
                "UK" => $"Помилка: У шляху до файлу даних ({filePath}) є непідтримувані символи. Не вдалося отримати доступ до файлу.\nКод помилки: 20",
                "ZH" => $"错误: 配置文件路径 ({filePath}) 中存在不支持的字符。无法访问文件。\n错误代码: 20",
                _ => $"Erreur: des caractères non supportés sont présents dans le chemin d'accès du fichier de données ({filePath}). Impossible d'accéder au fichier.\nCode erreur: 20"
            };


            // Affichage de la MessageBox avec le titre et le message traduits
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(2);
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            // Traductions du titre et du message d'erreur en fonction de la langue
            var ioErrorTitle = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                "AR" => "خطأ",
                "BG" => "Грешка",
                "CS" => "Chyba",
                "DA" => "Fejl",
                "DE" => "Fehler",
                "EL" => "Σφάλμα",
                "EN" => "Error",
                "ES" => "Error",
                "ET" => "Viga",
                "FI" => "Virhe",
                "HU" => "Hiba",
                "ID" => "Kesalahan",
                "IT" => "Errore",
                "JA" => "エラー",
                "KO" => "오류",
                "LV" => "Kļūda",
                "LT" => "Klaida",
                "NB" => "Feil",
                "NL" => "Fout",
                "PL" => "Błąd",
                "PT" => "Erro",
                "RO" => "Eroare",
                "RU" => "Ошибка",
                "SK" => "Chyba",
                "SL" => "Napaka",
                "SV" => "Fel",
                "TR" => "Hata",
                "UK" => "Помилка",
                "ZH" => "错误",
                _ => "Erreur"
            };

            var ioErrorMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                "AR" => $"خطأ: خطأ في الإدخال/الإخراج عند فتح ملف البيانات.\nرمز الخطأ: 30",
                "BG" => "Грешка: Грешка при четене/запис на файла с данни.\nКод на грешката: 30",
                "CS" => "Chyba: Chyba I/O při otevírání souboru s daty.\nKód chyby: 30",
                "DA" => "Fejl: I/O-fejl ved åbning af datafilen.\nFejlkode: 30",
                "DE" => "Fehler: I/O-Fehler beim Öffnen der Datendatei.\nFehlercode: 30",
                "EL" => "Σφάλμα: Σφάλμα I/O κατά το άνοιγμα του αρχείου δεδομένων.\nΚωδικός σφάλματος: 30",
                "EN" => "Error: I/O error while opening the data file.\nError code: 30",
                "ES" => "Error: Error de I/O al abrir el archivo de datos.\nCódigo de error: 30",
                "ET" => "Viga: I/O viga andmefaili avamisel.\nVigakood: 30",
                "FI" => "Virhe: I/O-virhe data tiedoston avaamisessa.\nVirhekoodi: 30",
                "HU" => "Hiba: I/O hiba az adatfájl megnyitásakor.\nHibakód: 30",
                "ID" => "Kesalahan: Kesalahan I/O saat membuka file data.\nKode kesalahan: 30",
                "IT" => "Errore: Errore I/O durante l'apertura del file di dati.\nCodice errore: 30",
                "JA" => "エラー: データファイルのオープン時にI/Oエラーが発生しました。\nエラーコード: 30",
                "KO" => "오류: 데이터 파일 열기 중 I/O 오류가 발생했습니다.\n오류 코드: 30",
                "LV" => "Kļūda: I/O kļūda atverot datu failu.\nKļūdas kods: 30",
                "LT" => "Klaida: I/O klaida atidarant duomenų failą.\nKlaidos kodas: 30",
                "NB" => "Feil: I/O-feil ved åpning av datafilen.\nFeilkode: 30",
                "NL" => "Fout: I/O-fout bij het openen van het databestand.\nFoutcode: 30",
                "PL" => "Błąd: Błąd I/O podczas otwierania pliku danych.\nKod błędu: 30",
                "PT" => "Erro: Erro de I/O ao abrir o arquivo de dados.\nCódigo de erro: 30",
                "RO" => "Eroare: Eroare I/O la deschiderea fișierului de date.\nCod eroare: 30",
                "RU" => "Ошибка: Ошибка ввода/вывода при открытии файла данных.\nКод ошибки: 30",
                "SK" => "Chyba: Chyba I/O pri otváraní súboru údajov.\nKód chyby: 30",
                "SL" => "Napaka: Napaka I/O pri odpiranju podatkovne datoteke.\nKoda napake: 30",
                "SV" => "Fel: I/O-fel vid öppning av datafilen.\nFelkod: 30",
                "TR" => "Hata: Veri dosyasını açarken I/O hatası oluştu.\nHata kodu: 30",
                "UK" => "Помилка: Помилка вводу/виводу під час відкриття файлу даних.\nКод помилки: 30",
                "ZH" => "错误: 打开数据文件时发生I/O错误。\n错误代码: 30",
                _ => "Erreur: Erreur I/O lors de l'ouverture du fichier de données.\nCode erreur: 30"
            };


            // Affichage de la MessageBox avec le titre et le message traduits
            MessageBox.Show(ioErrorMessage, ioErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(3);
        }
        
        StreamReader? reader = null;
        
        try
        {
            // Création du stream
            reader = new StreamReader(filePath);
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            // Traductions du titre et du message d'erreur en fonction de la langue
            var ioErrorTitle = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                "AR" => "خطأ",
                "BG" => "Грешка",
                "CS" => "Chyba",
                "DA" => "Fejl",
                "DE" => "Fehler",
                "EL" => "Σφάλμα",
                "EN" => "Error",
                "ES" => "Error",
                "ET" => "Viga",
                "FI" => "Virhe",
                "HU" => "Hiba",
                "ID" => "Kesalahan",
                "IT" => "Errore",
                "JA" => "エラー",
                "KO" => "오류",
                "LV" => "Kļūda",
                "LT" => "Klaida",
                "NB" => "Feil",
                "NL" => "Fout",
                "PL" => "Błąd",
                "PT" => "Erro",
                "RO" => "Eroare",
                "RU" => "Ошибка",
                "SK" => "Chyba",
                "SL" => "Napaka",
                "SV" => "Fel",
                "TR" => "Hata",
                "UK" => "Помилка",
                "ZH" => "错误",
                _ => "Erreur"
            };

            var ioErrorMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                "AR" => $"خطأ: خطأ في الإدخال/الإخراج عند فتح ملف الإعدادات.\nرمز الخطأ: 3",
                "BG" => "Грешка: Грешка при четене/запис на файла с настройки.\nКод на грешката: 3",
                "CS" => "Chyba: Chyba I/O při otevírání souboru nastavení.\nKód chyby: 3",
                "DA" => "Fejl: I/O-fejl ved åbning af konfigurationsfilen.\nFejlkode: 3",
                "DE" => "Fehler: I/O-Fehler beim Öffnen der Einstellungsdatei.\nFehlercode: 3",
                "EL" => "Σφάλμα: Σφάλμα I/O κατά το άνοιγμα του αρχείου ρυθμίσεων.\nΚωδικός σφάλματος: 3",
                "EN" => "Error: I/O error while opening the settings file.\nError code: 3",
                "ES" => "Error: Error de I/O al abrir el archivo de configuración.\nCódigo de error: 3",
                "ET" => "Viga: I/O viga seadistusfaili avamisel.\nVigakood: 3",
                "FI" => "Virhe: I/O-virhe asetustiedoston avaamisessa.\nVirhekoodi: 3",
                "HU" => "Hiba: I/O hiba a beállítási fájl megnyitásakor.\nHibakód: 3",
                "ID" => "Kesalahan: Kesalahan I/O saat membuka file pengaturan.\nKode kesalahan: 3",
                "IT" => "Errore: Errore I/O durante l'apertura del file di configurazione.\nCodice errore: 3",
                "JA" => "エラー: 設定ファイルのオープン時にI/Oエラーが発生しました。\nエラーコード: 3",
                "KO" => "오류: 설정 파일 열기 중 I/O 오류가 발생했습니다.\n오류 코드: 3",
                "LV" => "Kļūda: I/O kļūda atverot iestatījumu failu.\nKļūdas kods: 3",
                "LT" => "Klaida: I/O klaida atidarant nustatymų failą.\nKlaidos kodas: 3",
                "NB" => "Feil: I/O-feil ved åpning av innstillingsfilen.\nFeilkode: 3",
                "NL" => "Fout: I/O-fout bij het openen van het instellingenbestand.\nFoutcode: 3",
                "PL" => "Błąd: Błąd I/O podczas otwierania pliku konfiguracyjnego.\nKod błędu: 3",
                "PT" => "Erro: Erro de I/O ao abrir o arquivo de configuração.\nCódigo de erro: 3",
                "RO" => "Eroare: Eroare I/O la deschiderea fișierului de configurare.\nCod eroare: 3",
                "RU" => "Ошибка: Ошибка ввода/вывода при открытии файла настроек.\nКод ошибки: 3",
                "SK" => "Chyba: Chyba I/O pri otváraní súboru nastavení.\nKód chyby: 3",
                "SL" => "Napaka: Napaka I/O pri odpiranju konfiguracijske datoteke.\nKoda napake: 3",
                "SV" => "Fel: I/O-fel vid öppning av inställningsfilen.\nFelkod: 3",
                "TR" => "Hata: Ayar dosyasını açarken I/O hatası oluştu.\nHata kodu: 3",
                "UK" => "Помилка: Помилка вводу/виводу під час відкриття файлу налаштувань.\nКод помилки: 3",
                "ZH" => "错误: 打开设置文件时发生I/O错误。\n错误代码: 3",
                _ => "Erreur: Erreur I/O lors de l'ouverture du fichier de paramétrage.\nCode erreur: 3"
            };

            // Affichage de la MessageBox avec le titre et le message traduits
            MessageBox.Show(ioErrorMessage, ioErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            Application.Current.Shutdown(3);
        }

        try
        {            
            var headerLine = reader?.ReadLine();
            while (reader?.ReadLine() is { } line)
            {
                // Vérifier si la ligne est vide ou seulement composée d'espaces
                if (string.IsNullOrWhiteSpace(line)) continue; // Ignorer les lignes vides
                
                var parts = line.Split(',');
                if (parts.Length < 7) continue;

                var projectName = parts[0];
                var addressCount = int.Parse(parts[1]);
                var deviceCount = int.Parse(parts[2]);
                var isDeleted = bool.Parse(parts[3]);
                var deletedAddresses = int.Parse(parts[4]);
                var isTranslated = bool.Parse(parts[5]);
                var timeParts = parts[6].Split(':');
                var minutes = int.Parse(timeParts[0]);
                var seconds = int.Parse(timeParts[1]);
                var totalLoadingTime = new TimeSpan(0, minutes, seconds);

                loadingTimes.Add(new LoadingTimeEntry
                {
                    ProjectName = projectName,
                    AddressCount = addressCount,
                    DeviceCount = deviceCount,
                    IsDeleted = isDeleted,
                    DeletedAddresses = deletedAddresses,
                    IsTranslated = isTranslated,
                    TotalLoadingTime = totalLoadingTime
                });
            }
            var allLines = File.ReadAllLines(filePath).ToList();
            

        }
        catch (OutOfMemoryException)
        {
            App.ConsoleAndLogWriteLine("Error: The program does not have sufficient memory to run. Please try closing a few applications before trying again.");
            return null;
        }
        // Aucune idée de la raison
        catch (IOException)
        {
            App.ConsoleAndLogWriteLine("Error: An I/O error occured while reading the data file.");
            return null;
        }
        finally
        {
            reader?.Close(); // Fermeture du stream de lecture
        }
        return loadingTimes;
    }
    
    
    /// <summary>
    /// Handles the button click event to export the updated project file to a selected destination.
    /// Checks if the source file exists, prompts the user to select a destination using SaveFileDialog,
    /// and copies the source file to the selected location. Logs relevant information and handles exceptions.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ExportModifiedProjectButtonClick(object sender, RoutedEventArgs e)
    {
        var sourceFilePath = $"{App.Fm?.ProjectFolderPath}UpdatedGroupAddresses.xml";
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
            Title = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "حفظ ملف عناوين المجموعة المحدثة باسم...",
                // Bulgare
                "BG" => "Запазване на актуализирания файл с адреси на групи като...",
                // Tchèque
                "CS" => "Uložit aktualizovaný soubor skupinových adres jako...",
                // Danois
                "DA" => "Gem den opdaterede gruppeadressedatafil som...",
                // Allemand
                "DE" => "Aktualisierte Gruppenadressdatei speichern unter...",
                // Grec
                "EL" => "Αποθήκευση του ενημερωμένου αρχείου διευθύνσεων ομάδας ως...",
                // Anglais
                "EN" => "Save updated group address file as...",
                // Espagnol
                "ES" => "Guardar archivo de direcciones de grupo actualizado como...",
                // Estonien
                "ET" => "Salvesta värskendatud rühma aadresside fail nimega...",
                // Finnois
                "FI" => "Tallenna päivitetty ryhmäosoitetiedosto nimellä...",
                // Hongrois
                "HU" => "Mentse a frissített csoportcím fájlt másként...",
                // Indonésien
                "ID" => "Simpan file alamat grup yang diperbarui sebagai...",
                // Italien
                "IT" => "Salva il file degli indirizzi del gruppo aggiornato come...",
                // Japonais
                "JA" => "更新されたグループアドレスファイルを名前を付けて保存...",
                // Coréen
                "KO" => "업데이트된 그룹 주소 파일을 다음 이름으로 저장...",
                // Letton
                "LV" => "Saglabāt atjaunināto grupas adrešu failu kā...",
                // Lituanien
                "LT" => "Išsaugoti atnaujintą grupės adresų failą kaip...",
                // Norvégien
                "NB" => "Lagre oppdatert gruppeadressefil som...",
                // Néerlandais
                "NL" => "Bijgewerkt groepsadresbestand opslaan als...",
                // Polonais
                "PL" => "Zapisz zaktualizowany plik adresów grupowych jako...",
                // Portugais
                "PT" => "Salvar arquivo de endereços de grupo atualizado como...",
                // Roumain
                "RO" => "Salvează fișierul actualizat de adrese de grup ca...",
                // Russe
                "RU" => "Сохранить обновленный файл адресов группы как...",
                // Slovaque
                "SK" => "Uložiť aktualizovaný súbor skupinových adries ako...",
                // Slovène
                "SL" => "Shrani posodobljeno datoteko skupinskih naslovov kot...",
                // Suédois
                "SV" => "Spara uppdaterad gruppadressfil som...",
                // Turc
                "TR" => "Güncellenmiş grup adres dosyasını farklı kaydet...",
                // Ukrainien
                "UK" => "Зберегти оновлений файл групових адрес як...",
                // Chinois simplifié
                "ZH" => "将更新的组地址文件另存为...",
                // Cas par défaut (français)
                _ => "Enregistrer le fichier d'adresses de groupe modifiées sous..."
            },
            FileName = "UpdatedGroupAddresses.xml", // Nom de fichier par défaut
            DefaultExt = ".xml", // Extension par défaut
            Filter = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "ملفات XML|*.xml|كل الملفات|*.*",
                // Bulgare
                "BG" => "XML файлове|*.xml|Всички файлове|*.*",
                // Tchèque
                "CS" => "XML soubory|*.xml|Všechny soubory|*.*",
                // Danois
                "DA" => "XML-filer|*.xml|Alle filer|*.*",
                // Allemand
                "DE" => "XML-Dateien|*.xml|Alle Dateien|*.*",
                // Grec
                "EL" => "Αρχεία XML|*.xml|Όλα τα αρχεία|*.*",
                // Anglais
                "EN" => "XML Files|*.xml|All Files|*.*",
                // Espagnol
                "ES" => "Archivos XML|*.xml|Todos los archivos|*.*",
                // Estonien
                "ET" => "XML-failid|*.xml|Kõik failid|*.*",
                // Finnois
                "FI" => "XML-tiedostot|*.xml|Kaikki tiedostot|*.*",
                // Hongrois
                "HU" => "XML fájlok|*.xml|Minden fájl|*.*",
                // Indonésien
                "ID" => "File XML|*.xml|Semua file|*.*",
                // Italien
                "IT" => "File XML|*.xml|Tutti i file|*.*",
                // Japonais
                "JA" => "XMLファイル|*.xml|すべてのファイル|*.*",
                // Coréen
                "KO" => "XML 파일|*.xml|모든 파일|*.*",
                // Letton
                "LV" => "XML faili|*.xml|Visi faili|*.*",
                // Lituanien
                "LT" => "XML failai|*.xml|Visi failai|*.*",
                // Norvégien
                "NB" => "XML-filer|*.xml|Alle filer|*.*",
                // Néerlandais
                "NL" => "XML-bestanden|*.xml|Alle bestanden|*.*",
                // Polonais
                "PL" => "Pliki XML|*.xml|Wszystkie pliki|*.*",
                // Portugais
                "PT" => "Arquivos XML|*.xml|Todos os arquivos|*.*",
                // Roumain
                "RO" => "Fișiere XML|*.xml|Toate fișierele|*.*",
                // Russe
                "RU" => "XML-файлы|*.xml|Все файлы|*.*",
                // Slovaque
                "SK" => "XML súbory|*.xml|Všetky súbory|*.*",
                // Slovène
                "SL" => "XML datoteke|*.xml|Vse datoteke|*.*",
                // Suédois
                "SV" => "XML-filer|*.xml|Alla filer|*.*",
                // Turc
                "TR" => "XML Dosyaları|*.xml|Tüm Dosyalar|*.*",
                // Ukrainien
                "UK" => "XML-файли|*.xml|Всі файли|*.*",
                // Chinois simplifié
                "ZH" => "XML 文件|*.xml|所有文件|*.*",
                // Cas par défaut (français)
                _ => "Fichiers XML|*.xml|Tous les fichiers|*.*"
            }

        };

        // Afficher le dialogue et vérifier si l'utilisateur a sélectionné un emplacement
        var result = saveFileDialog.ShowDialog();

        if (result != true) return;
        // Chemin du fichier sélectionné par l'utilisateur
        App.ConsoleAndLogWriteLine($"Destination path selected: {saveFileDialog.FileName}");

        try
        {
            // Copier le fichier source à l'emplacement sélectionné par l'utilisateur
            File.Copy(sourceFilePath, saveFileDialog.FileName, true);
            App.ConsoleAndLogWriteLine($"File saved successfully at {saveFileDialog.FileName}.");
        }
        catch (Exception ex)
        {
            // Gérer les exceptions et afficher un message d'erreur
            App.ConsoleAndLogWriteLine($"Failed to save the file: {ex.Message}");
        }
    }

    
    /// <summary>
    /// Handles the closing event of the main window by shutting down the application.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void ClosingMainWindow(object sender, CancelEventArgs e)
    {
        Application.Current.Shutdown();
    }
    

    /// <summary>
    /// Handles the button click event to open the application settings window.
    /// Checks if the settings window is already open and brings it to the front if so,
    /// otherwise, displays the settings window.
    /// Clears keyboard focus from the button after opening the settings window.
    /// </summary>
    /// <param name="sender">The object that raised the event (typically the button).</param>
    /// <param name="e">The event data.</param>
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

        Keyboard.ClearFocus(); // On désélectionne le bouton paramètres dans mainwindow
    }

    
    
    //--------------------- Gestion de la fenêtre de chargement -----------------------------------------------------//
    /// <summary>
    /// Executes a long-running task asynchronously, displaying progress in a loading window.
    /// The task includes showing a progress indicator in the taskbar,
    /// and performing multiple asynchronous operations including file extraction, data processing, and UI updates.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteLongRunningTask()
    {
        if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableLightTheme)
        {
            App.DisplayElements.LoadingWindow?.SetLightMode();
        }
        else
        {
            App.DisplayElements?.LoadingWindow?.SetDarKMode();
        }

        App.DisplayElements?.ShowLoadingWindow();

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


                if (App.Fm != null)
                {
                    await App.Fm.FindZeroXml().ConfigureAwait(false);
                    App.DisplayElements?.LoadingWindow?.UpdateTaskName($"{task} 1/3");
                    await GroupAddressNameCorrector.CorrectName().ConfigureAwait(false);

                    _xmlFilePath1 = $"{App.Fm.ProjectFolderPath}/GroupAddresses.xml";
                    _xmlFilePath2 = App.Fm.ProjectFolderPath + "UpdatedGroupAddresses.xml";
                    _xmlRenameFilePath = App.Fm.ProjectFolderPath + "RenamedAddressesHistory.xml";

                    //Define the project path
                    if (App.DisplayElements != null)
                    {
                        App.DisplayElements.LoadingWindow?.UpdateTaskName($"{task} 3/3");
                        if (App.DisplayElements.SettingsWindow!.RemoveUnusedGroupAddresses)
                        {
                            await ExportUpdatedNameAddresses.Export(App.Fm.ProjectFolderPath + "/0_original.xml",
                                App.Fm.ProjectFolderPath + "/GroupAddresses.xml").ConfigureAwait(false);
                        }
                        else
                        {
                            await ExportUpdatedNameAddresses
                                .Export(App.Fm.ZeroXmlPath, App.Fm.ProjectFolderPath + "/GroupAddresses.xml")
                                .ConfigureAwait(false);
                        }

                    }

                    await ExportUpdatedNameAddresses.Export(App.Fm.ProjectFolderPath + "/0_updated.xml",
                        App.Fm.ProjectFolderPath + "/UpdatedGroupAddresses.xml").ConfigureAwait(false);
                }

                await LoadXmlFiles().ConfigureAwait(false);

                // Mettre à jour l'interface utilisateur depuis le thread principal
                Dispatcher.Invoke(() =>
                {
                    App.DisplayElements?.LoadingWindow?.UpdateTaskName(loadingFinished);
                    App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
                    App.DisplayElements?.LoadingWindow?.CompleteActivity();
                });
            });
        }
        finally
        {
            // Mettre à jour l'état de la barre des tâches et masquer l'overlay
            Dispatcher.Invoke(() =>
            {
                TaskbarInfo.ProgressState = TaskbarItemProgressState.None;
                App.DisplayElements?.LoadingWindow?.CloseAfterDelay(2000).ConfigureAwait(false);
            });
        }
    }

    
    /// <summary>
    /// Shows an overlay on the main content area based on the application's theme settings.
    /// Disables user interaction with the main content while the overlay is visible.
    /// </summary>
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

    
    /// <summary>
    /// Hides the overlay from the main content area based on the application's theme settings.
    /// Enables user interaction with the main content after hiding the overlay.
    /// </summary>
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

    
    
    //--------------------- Gestion de la fenêtre de renommage -----------------------------------------------------//
    /// <summary>
    /// Handles the button click event to initiate the renaming of a selected item's address.
    /// Checks if an item is selected in the TreeView and if it is a leaf node.
    /// Extracts the text of the selected item and opens a dialog window to rename the address.
    /// If the user confirms the renaming, updates the item's text in the TreeView and XML file.
    /// </summary>
    /// <param name="sender">The object that raised the event (typically the button).</param>
    /// <param name="e">The event data.</param>
    private void RenameWindow(object sender, RoutedEventArgs e)
    {
        if (TreeViewDroite.SelectedItem is not TreeViewItem selectedItem) return;
        
        // Vérifier si l'élément sélectionné est un élément de dernier niveau
        if (selectedItem.Items.Count != 0) return;
        
        // Extraire le texte de l'élément sélectionné
        var stackPanel = selectedItem.Header as StackPanel;
        var textBlock = stackPanel?.Children.OfType<TextBlock>().FirstOrDefault();
        
        if (textBlock == null) return;
        
        // Obtenir le chemin de l'élément sélectionné
        var itemPath = GetItemPath(selectedItem);
        
        if (itemPath == null) return;
        
        // Trouver l'élément correspondant dans TreeViewGauche
        var correspondingItem = FindTreeViewItemByPath(TreeViewGauche, itemPath);
        
        if (correspondingItem == null) return;
        
        // Extraire le texte de l'élément correspondant
        var correspondingStackPanel = correspondingItem.Header as StackPanel;
        var correspondingTextBlock = correspondingStackPanel?.Children.OfType<TextBlock>().FirstOrDefault();
        
        if (correspondingTextBlock == null) return;
                            
        var originalAddress = correspondingTextBlock.Text;
        var editedAddress = textBlock.Text;

        var result = App.DisplayElements?.ShowGroupAddressRenameWindow(originalAddress, editedAddress, _xmlRenameFilePath);
                                
        if (result != true) return;
        
        var newAddress = App.DisplayElements!.GroupAddressRenameWindow.NewAddress;
        textBlock.Text = newAddress;
        
        //Sauvegarder les modfications
        SaveModifiedAdress(_xmlRenameFilePath, editedAddress, newAddress, originalAddress);

        // Renommer l'adresse dans le fichier XML
        RenameAddressInXmlFile(_xmlFilePath2, editedAddress, newAddress);
    }

    
    /// <summary>
    /// Saves a record of an address modification in an XML file in order to add a "reset" property.
    /// If the file does not exist, it creates a new XML file and adds the modification.
    /// Each modification entry includes the old address, new address, and the timestamp of the modification.
    /// </summary>
    /// <param name="filePath">The path to the XML file where the modifications will be saved.</param>
    /// <param name="oldAddress">The original address before modification.</param>
    /// <param name="newAddress">The new address that replaces the old address.</param>
    /// <param name="originalAddress">The original group address.</param>
    private void SaveModifiedAdress(string filePath, string oldAddress, string newAddress, string originalAddress)
    {
        try
        {
            var xmlDoc = new XmlDocument();

            // Vérifier si le fichier XML existe
            if (!File.Exists(filePath))
            {
                // Si le fichier n'existe pas, créer un nouveau document XML
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                // Créer l'élément racine
                var rootElement = xmlDoc.CreateElement("Root");
                xmlDoc.AppendChild(rootElement);

                // Enregistrer le nouveau fichier XML
                xmlDoc.Save(filePath);
            }
            else
            {
                // Charger le document XML existant
                xmlDoc.Load(filePath);
            }

            // Créer un nouvel élément <Change> avec les attributs OldAddress, NewAddress et la date de modification
            var changeElement = xmlDoc.CreateElement("Change");
            changeElement.SetAttribute("OldAddress", oldAddress);
            changeElement.SetAttribute("NewAddress", newAddress);
            changeElement.SetAttribute("OriginalAddress", originalAddress);
            changeElement.SetAttribute("TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Ajouter l'élément <Change> sous l'élément racine du document
            xmlDoc.DocumentElement?.AppendChild(changeElement);

            // Enregistrer les modifications dans le fichier XML
            xmlDoc.Save(filePath);

            App.ConsoleAndLogWriteLine($"Change saved: Old Address '{oldAddress}' -> New Address '{newAddress}'");
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"Error updating XML file: {ex.Message}");
        }
    }


    /// <summary>
    /// Renames an address entry in an XML file based on the provided old address and new address.
    /// Uses XPath to locate and update the XML element corresponding to the old address.
    /// Saves the modified XML file after updating the address.
    /// </summary>
    /// <param name="filePath">The path to the XML file.</param>
    /// <param name="oldAddress">The current address to be renamed.</param>
    /// <param name="newAddress">The new address to replace the old address.</param>
    private void RenameAddressInXmlFile(string filePath, string oldAddress, string newAddress)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_xmlFilePath2);

            // Déclaration d'un gestionnaire de noms pour l'espace de noms par défaut
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ga", "http://knx.org/xml/ga-export/01");

            // Utiliser XPath pour trouver l'élément avec l'adresse à renommer
            var nodeToRename = xmlDoc.SelectSingleNode($"//ga:GroupAddress[@Name='{oldAddress}']", nsManager);
            if (nodeToRename?.Attributes != null)
            {
                nodeToRename.Attributes["Name"]!.Value = newAddress;
                xmlDoc.Save(filePath);
                App.ConsoleAndLogWriteLine($"Address '{oldAddress}' renamed to '{newAddress}' in the XML file.");
            }
            else
            {
                App.ConsoleAndLogWriteLine($"Address '{oldAddress}' not found in the XML file.");
            }
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"Error updating XML file: {ex.Message}");
        }
    }


    
    //--------------------- Changement de thème -----------------------------------------------------//
    /// <summary>
    /// Applies a specified style to all TreeView items and their children recursively.
    /// </summary>
    /// <param name="treeView">The TreeView whose items should be styled.</param>
    /// <param name="style">The name of the style to apply from application resources.</param>
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


    /// <summary>
    /// Recursively applies a specified style to a TreeViewItem and its child items.
    /// </summary>
    /// <param name="item">The TreeViewItem to apply the style to.</param>
    /// <param name="style">The name of the style to apply from application resources.</param>    
    private void ApplyStyleRecursive(TreeViewItem item, string style)
    {
        item.Style = FindResource(style) as Style;

        // Ensure the TreeViewItem is expanded to generate child containers
        item.IsExpanded = true;
        item.UpdateLayout(); // Force update layout to generate child containers

        item.ItemContainerGenerator.StatusChanged += (_, _) =>
        {
            if (item.ItemContainerGenerator.Status !=
                System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) return;
            
            foreach (var subItem in item.Items)
            {
                if (item.ItemContainerGenerator.ContainerFromItem(subItem) is TreeViewItem subTreeViewItem)
                {
                    ApplyStyleRecursive(subTreeViewItem, style);
                }
            }
        };

        // Apply style to already generated child items
        foreach (var subItem in item.Items)
        {
            if (item.ItemContainerGenerator.ContainerFromItem(subItem) is TreeViewItem subTreeViewItem)
            {
                ApplyStyleRecursive(subTreeViewItem, style);
            }
        }
    }


    /// <summary>
    /// Converts a string representation of a color to a SolidColorBrush.
    /// </summary>
    /// <param name="colorInput">The string representation of the color (e.g., "#RRGGBB" or "ColorName").</param>
    /// <returns>A SolidColorBrush representing the converted color.</returns>
    public static SolidColorBrush ConvertStringColor(string colorInput)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorInput));
    }


    
    //--------------------- Gestion de l'affichage à partir de fichiers -------------------------------//
    /// <summary>
    /// Asynchronously loads XML files into two TreeViews.
    /// </summary>
    private async Task LoadXmlFiles()
    {
        await LoadXmlFile(_xmlFilePath1, TreeViewGauche);
        await LoadXmlFile(_xmlFilePath2, TreeViewDroite);

        TreeViewGauche.SelectedItemChanged += TreeViewGauche_SelectedItemChanged;
        TreeViewDroite.SelectedItemChanged += TreeViewDroite_SelectedItemChanged;
    }

    
    /// <summary>
    /// Asynchronously loads an XML file into a specified TreeView.
    /// </summary>
    /// <param name="filePath">The path to the XML file to load.</param>
    /// <param name="treeView">The TreeView where XML nodes should be displayed.</param>
    private async Task LoadXmlFile(string filePath, TreeView treeView)
    {
        try
        {
            var xmlDoc = await Task.Run(() =>
            {
                XmlDocument doc = new();
                doc.Load(filePath);
                return doc;
            });

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeView.Items.Clear();

                // Ajouter tous les nœuds récursivement
                if (xmlDoc.DocumentElement == null) return;
                
                var index = 0;
                
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    AddNodeRecursively(node, treeView.Items, 0, index++);
                }
            });
        }
        catch (Exception ex)
        {
            // Afficher le message d'erreur sur le thread principal
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var caption = App.DisplayElements?.SettingsWindow!.AppLang switch
                {
                    // Arabe
                    "AR" => "خطأ",
                    // Bulgare
                    "BG" => "Грешка",
                    // Tchèque
                    "CS" => "Chyba",
                    // Danois
                    "DA" => "Fejl",
                    // Allemand
                    "DE" => "Fehler",
                    // Grec
                    "EL" => "Σφάλμα",
                    // Anglais
                    "EN" => "Error",
                    // Espagnol
                    "ES" => "Error",
                    // Estonien
                    "ET" => "Viga",
                    // Finnois
                    "FI" => "Virhe",
                    // Hongrois
                    "HU" => "Hiba",
                    // Indonésien
                    "ID" => "Kesalahan",
                    // Italien
                    "IT" => "Errore",
                    // Japonais
                    "JA" => "エラー",
                    // Coréen
                    "KO" => "오류",
                    // Letton
                    "LV" => "Kļūda",
                    // Lituanien
                    "LT" => "Klaida",
                    // Norvégien
                    "NB" => "Feil",
                    // Néerlandais
                    "NL" => "Fout",
                    // Polonais
                    "PL" => "Błąd",
                    // Portugais
                    "PT" => "Erro",
                    // Roumain
                    "RO" => "Eroare",
                    // Russe
                    "RU" => "Ошибка",
                    // Slovaque
                    "SK" => "Chyba",
                    // Slovène
                    "SL" => "Napaka",
                    // Suédois
                    "SV" => "Fel",
                    // Turc
                    "TR" => "Hata",
                    // Ukrainien
                    "UK" => "Помилка",
                    // Chinois simplifié
                    "ZH" => "错误",
                    // Cas par défaut (français)
                    _ => "Erreur"
                };
                
                MessageBox.Show(ex.Message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    
    /// <summary>
    /// Adds XML nodes recursively to a TreeView.
    /// </summary>
    /// <param name="xmlNode">The XML node to add.</param>
    /// <param name="parentItems">The parent collection of TreeView items.</param>
    /// <param name="level">The depth level of the current XML node.</param>
    /// <param name="index">The index of the current XML node among its siblings.</param>
    private void AddNodeRecursively(XmlNode xmlNode, ItemCollection parentItems, int level, int index)
    {
        if (xmlNode.NodeType != XmlNodeType.Element) return;
        var treeNode = CreateTreeViewItemFromXmlNode(xmlNode, level, index);

        parentItems.Add(treeNode);
        // Parcourir récursivement les enfants
        int childIndex = 0;
        foreach (XmlNode childNode in xmlNode.ChildNodes)
        {
            AddNodeRecursively(childNode, treeNode.Items, level + 1, childIndex++);
        }
    }

    
    /// <summary>
    /// Creates a TreeViewItem from an XML node, with its corresponding image.
    /// </summary>
    /// <param name="xmlNode">The XML node to create a TreeViewItem from.</param>
    /// <param name="level">The depth level of the XML node.</param>
    /// <param name="index">The index of the XML node among its siblings.</param>
    /// <returns>A TreeViewItem representing the XML node.</returns>
    private TreeViewItem CreateTreeViewItemFromXmlNode(XmlNode xmlNode, int level, int index)
    {
        var stack = new StackPanel { Orientation = Orientation.Horizontal };

        // Définir l'icône en fonction du niveau
        var drawingImageKey = level switch
        {
            0 => "Icon_level1",
            1 => "Icon_level2",
            2 => "Icon_level3",
            _ => "Icon_level3"
        };

        var drawingImage = Application.Current.Resources[drawingImageKey] as DrawingImage;

        var icon = new Image
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 5, 0),
            Source = drawingImage
        };


        var text = new TextBlock
        {
            Text = ((XmlElement)xmlNode).GetAttribute("Name"),
            FontSize = 12

        };

      

        stack.Children.Add(icon);
        stack.Children.Add(text);

        var treeNode = new TreeViewItem
        {
            Header = stack,
            Tag = GetNodePath(level, index)
        };

        if (App.DisplayElements!.SettingsWindow!.EnableLightTheme)
        {
            treeNode.Style = (Style)FindResource("TreeViewItemStyleLight");
        }
        else
        {
            treeNode.Style = (Style)FindResource("TreeViewItemStyleDark");
        }

        return treeNode;
    }

    
    /// <summary>
    /// Generates a unique path for the node based on its level and index.
    /// </summary>
    /// <param name="level">The depth level of the node.</param>
    /// <param name="index">The index of the node among its siblings.</param>
    /// <returns>A string representing the unique path of the node.</returns>
    private static string GetNodePath(int level, int index)
    {
        return $"{level}-{index}";
    }


    
    //-------------------- Gestion du scroll vertical synchronisé ------------------------------------//
    /// <summary>
    /// Handles the ScrollChanged event of the ScrollViewer to synchronize scrolling between two ScrollViewer instances.
    /// </summary>
    /// <param name="sender">The object that raised the event (should be a ScrollViewer).</param>
    /// <param name="e">The event data.</param>
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

    
    /// <summary>
    /// Handles the PreviewMouseWheel event of the ScrollViewer to manage scrolling behavior based on Shift key press.
    /// </summary>
    /// <param name="sender">The object that raised the event (should be a ScrollViewer).</param>
    /// <param name="e">The event data.</param>
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

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
    /// <summary>
    /// Handles the GotFocus event of a TextBox by clearing its content if it matches the search text and adjusting its appearance.
    /// </summary>
    /// <param name="sender">The TextBox that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        if (tb?.Text != _searchTextTranslate) return;
        tb.Text = "";
        tb.Foreground = App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableLightTheme ? 
            new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
    }

    
    /// <summary>
    /// Handles the LostFocus event of a TextBox by restoring the initial search text if the TextBox is empty and adjusting its appearance.
    /// </summary>
    /// <param name="sender">The TextBox that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        // Utiliser un Dispatcher pour s'assurer que le TextBox a réellement perdu le focus
        tb?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!string.IsNullOrWhiteSpace(tb.Text)) return;
            tb.Text = _searchTextTranslate;
            tb.Foreground = App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableLightTheme ? 
                new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.DarkGray);
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
    
    
    /// <summary>
    /// Handles the PreviewKeyDown event of the search TextBox to move focus away and handle specific key presses (Enter or Escape).
    /// </summary>
    /// <param name="sender">The TextBox that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void txtSearch1_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Vérifier si la touche pressée est 'Enter' ou 'Escape'
        if (e.Key is not (Key.Enter or Key.Escape)) return;

        // Perdre le focus du TextBox
        TxtSearch1.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

        // Indiquer que l'événement est géré pour éviter le traitement supplémentaire
        e.Handled = true;
    }
    
    
    
    //-------------------- Gestion de la recherche ---------------------------------------------------//
    /// <summary>
    /// Handles the TextChanged event of the search TextBox to filter and hide items in two TreeViews based on the search text.
    /// </summary>
    /// <param name="sender">The TextBox that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Vérifier si le texte de recherche est égal au texte de recherche initial, si oui, ne rien faire
        if (TxtSearch1.Text == _searchTextTranslate) return;

        // Normaliser le texte de recherche
        var normalizedSearchText = NormalizeString(TxtSearch1.Text);

        // Déterminer le nombre d'éléments à comparer dans les deux TreeViews
        var itemCount = Math.Min(TreeViewGauche.Items.Count, TreeViewDroite.Items.Count);

        // Parcourir les éléments des deux TreeViews
        for (var i = 0; i < itemCount; i++)
        {
            // Vérifier si les éléments sont de type TreeViewItem
            if (TreeViewGauche.Items[i] is not TreeViewItem item1 ||
                TreeViewDroite.Items[i] is not TreeViewItem item2) continue;

            // Rétablir la visibilité des éléments
            item1.Visibility = Visibility.Visible;
            item2.Visibility = Visibility.Visible;

            // Filtrer les éléments dans les deux TreeViews en fonction du texte de recherche normalisé
            FilterTreeViewItems(item1, item2, normalizedSearchText);
        }
    }

    
    /// <summary>
    /// Filters two TreeViewItems based on the search text and shows or hides them accordingly.
    /// </summary>
    /// <param name="item1">The first TreeViewItem to filter.</param>
    /// <param name="item2">The second TreeViewItem to filter.</param>
    /// <param name="searchText">The normalized search text to filter against.</param>
    /// <returns>True if at least one of the TreeViewItems is visible after filtering, false otherwise.</returns>
    private static bool FilterTreeViewItems(TreeViewItem item1, TreeViewItem item2 , string searchText)
    {
        var item1Visible = false; // Indicateur pour déterminer si l'élément est visible

        // Extraire le texte du TextBlock dans le Header du TreeViewItem
        string? headerText1 = null;
        string? headerText2 = null;

        if (item1.Header is StackPanel headerStack1 && item2.Header is StackPanel headerStack2 )
        {
            var textBlock1 = headerStack1.Children.OfType<TextBlock>().FirstOrDefault();
            var textBlock2 = headerStack2.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBlock1 != null)
            {
                headerText1 = textBlock1.Text;
                headerText2 = textBlock2?.Text;
            }
        }

        if (headerText1 == null && headerText2 == null)
        {
            item1.Visibility = Visibility.Collapsed;
            item2.Visibility = Visibility.Collapsed;

            return false; // Si l'entête est null, l'élément n'est pas visible
        }

        var normalizedHeader1 = NormalizeString(headerText1);
        var normalizedHeader2 = NormalizeString(headerText2);

        // Vérifier si l'élément correspond au texte de recherche
        if (normalizedHeader1.Contains(searchText, StringComparison.OrdinalIgnoreCase) || normalizedHeader2.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            item1.Visibility = Visibility.Visible; // Rendre visible l'élément
            item1.IsExpanded = true; // Développer l'élément pour montrer les enfants correspondants
            item1Visible = true; // Indiquer que l'élément est visible

            item2.Visibility = Visibility.Visible; // Rendre visible l'élément
            item2.IsExpanded = true; // Développer l'élément pour montrer les enfants correspondants
        }
        else
        {
            item1.Visibility = Visibility.Collapsed; // Masquer l'élément si le texte ne correspond pas
            item2.Visibility = Visibility.Collapsed; // Masquer l'élément si le texte ne correspond pas
        }
        
        var hasVisibleChild = false;

        var itemCount = Math.Min(item1.Items.Count, item2.Items.Count);
        for (var i = 0; i < itemCount; i++)
        {
            if ((item1.Items[i] is not TreeViewItem childItem1) ||
                (item2.Items[i] is not TreeViewItem childItem2)) continue;
            
            // Appliquer le filtre aux enfants et mettre à jour l'indicateur de visibilité
            var childVisible = FilterTreeViewItems(childItem1, childItem2, searchText);
            if (!childVisible) continue;
            hasVisibleChild = true;
            item1.IsExpanded = true; // Développer l'élément si un enfant est visible
            item2.IsExpanded = true;

        }

        // Si un enfant est visible, rendre visible cet élément
        if (!hasVisibleChild) return item1Visible; // Retourner l'état de visibilité de l'élément
        
        item1.Visibility = Visibility.Visible;
        item1Visible = true;
        item2.Visibility = Visibility.Visible;
        
        return item1Visible; // Retourner l'état de visibilité de l'élément
    }

    
    /// <summary>
    /// Normalizes a string by removing diacritics (accents), non-spacing marks, spaces, underscores, and hyphens.
    /// </summary>
    /// <param name="input">The input string to normalize.</param>
    /// <returns>The normalized string with diacritics removed, converted to lowercase, and spaces, underscores, and hyphens removed.</returns>
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
    /// <summary>
    /// Handles the expanded event of a TreeViewItem by synchronizing expansion state with corresponding items in two TreeViews.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem item) return;
        SynchronizeTreeViewItemExpansion(TreeViewGauche, item);
        SynchronizeTreeViewItemExpansion(TreeViewDroite, item);
    }

    
    /// <summary>
    /// Handles the collapsed event of a TreeViewItem by synchronizing expansion state with corresponding items in two TreeViews.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem item) return;
        SynchronizeTreeViewItemExpansion(TreeViewGauche, item);
        SynchronizeTreeViewItemExpansion(TreeViewDroite, item);
    }

    
    /// <summary>
    /// Synchronizes the expansion state of a TreeViewItem between two TreeViews based on item path.
    /// </summary>
    /// <param name="targetTreeView">The target TreeView to synchronize with.</param>
    /// <param name="sourceItem">The source TreeViewItem whose expansion state is being synchronized.</param>
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
    

    /// <summary>
    /// Retrieves the path of a TreeViewItem by traversing its parent items recursively up to the root.
    /// </summary>
    /// <param name="item">The TreeViewItem whose path is to be retrieved.</param>
    /// <returns>The path of the TreeViewItem as a string, or null if the path cannot be determined.</returns>
    private static string? GetItemPath(TreeViewItem item)
    {
        var path = item.Tag as string;
        if (path == null)
        {
            return null;
        }

        var parent = item.Parent as TreeViewItem;
        while (parent != null)
        {
            var parentPath = parent.Tag as string;
            if (parentPath == null)
            {
                return null;
            }

            path = parentPath + "\\" + path;
            parent = parent.Parent as TreeViewItem;
        }
        return path;
    }


    /// <summary>
    /// Finds a TreeViewItem in a TreeView based on a given path.
    /// </summary>
    /// <param name="treeView">The TreeView in which to search for the item.</param>
    /// <param name="path">The path of the TreeViewItem to find.</param>
    /// <returns>The TreeViewItem found, or null if not found.</returns>
    private static TreeViewItem? FindTreeViewItemByPath(TreeView treeView, string path)
    {
        _ = path ?? throw new ArgumentNullException(nameof(path));

        var parts = path.Split("\\");
        var items = treeView.Items;
        TreeViewItem? currentItem = null;

        foreach (var part in parts)
        {
            currentItem = null;
            foreach (var item in items)
            {
                if (item is not TreeViewItem treeViewItem) continue;
                var itemPath = treeViewItem.Tag as string;
                if (itemPath == null || itemPath != part) continue;
                currentItem = treeViewItem;
                items = treeViewItem.Items;
                break;
            }
            if (currentItem == null) return null;
        }
        return currentItem;
    }

    
    
    //--------------------- Gestion sélection synchronisée ----------------------------------------------//
    /// <summary>
    /// Handles the SelectedItemChanged event of the TreeViewGauche to synchronize selection with TreeViewDroite.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void TreeViewGauche_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // Synchronize selection from TreeViewGauche to TreeViewDroite
        SynchronizeTreeViewSelection(TreeViewGauche, TreeViewDroite);
    }

    
    /// <summary>
    /// Handles the SelectedItemChanged event of the TreeViewDroite to synchronize selection with TreeViewGauche.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void TreeViewDroite_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        // Synchronize selection from TreeViewDroite to TreeViewGauche
        SynchronizeTreeViewSelection(TreeViewDroite, TreeViewGauche);
    }

    
    /// <summary>
    /// Synchronizes the selection between two TreeViews.
    /// </summary>
    /// <param name="sourceTreeView">The source TreeView from which selection is made.</param>
    /// <param name="targetTreeView">The target TreeView to synchronize selection with.</param>
    private static void SynchronizeTreeViewSelection(TreeView sourceTreeView, TreeView targetTreeView)
    {
        // Récupérer l'élément sélectionné dans le TreeView source
        var selectedSourceItem = sourceTreeView.SelectedItem as TreeViewItem;
        if (selectedSourceItem == null) return;

        // Récupérer le chemin de l'élément sélectionné
        var itemPath = GetItemPath(selectedSourceItem);
        if (itemPath == null) return;

        // Trouver l'élément correspondant dans le TreeView cible
        var targetItem = FindTreeViewItemByPath(targetTreeView, itemPath);
        if (targetItem == null) return;

        // Sélectionner l'élément correspondant dans le TreeView cible
        targetItem.IsSelected = true;
    }


    
    //--------------------- Gestion développement/rétractation bouton ----------------------------------------------//
    /// <summary>
    /// Handles the click event of the collapse/expand toggle button.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
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

    
    /// <summary>
    /// Recursively collapses all TreeView items starting from the specified collection.
    /// </summary>
    /// <param name="items">The collection of items to collapse.</param>
    private static void CollapseAllTreeViewItems(ItemCollection items)
    {
        foreach (var obj in items)
        {
            if (obj is not TreeViewItem item) continue;
            item.IsExpanded = false;
            CollapseAllTreeViewItems(item.Items);
        }
    }

    
    /// <summary>
    /// Recursively expands all TreeView items starting from the specified collection.
    /// </summary>
    /// <param name="items">The collection of items to expand.</param>
    private static void ExpandAllTreeViewItems(ItemCollection items)
    {
        foreach (var obj in items)
        {
            if (obj is not TreeViewItem item) continue;
            item.IsExpanded = true;
            ExpandAllTreeViewItems(item.Items);
        }
    }
    
    
    
    //-------------------------------------------- Logique de scaling ----------------------------------------------//
    public void ApplyScaling(double scale)
    {
        // Créez un ScaleTransform avec la valeur de l'échelle
        var scaleTransform = new ScaleTransform(scale, scale);

        // Appliquez la transformation à l'ensemble de la fenêtre
        LayoutTransform = scaleTransform;
    }

}