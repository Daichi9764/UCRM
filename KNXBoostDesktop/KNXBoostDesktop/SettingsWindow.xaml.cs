using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

// ReSharper disable ConvertToUsingDeclaration


namespace KNXBoostDesktop
{
    public partial class SettingsWindow
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        private readonly bool _emkFileExists;
        
        public bool EnableDeeplTranslation { get; private set; } // Activation ou non de la traduction deepL
        public byte[] DeeplKey { get; private set; } // Clé API DeepL
        public string TranslationLang { get; private set; } // Langue de traduction des adresses de groupe
        public bool RemoveUnusedGroupAddresses { get; private set; } // Activation ou non de la fonctionnalité de nettoyage des adresses de groupe
        public bool EnableLightTheme { get; private set; } // Thème de l'application (sombre/clair)
        public string AppLang { get; private set; } // Langue de l'application (français par défaut)
        
        
        
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Constructeur par défaut. Charge les paramètres contenus dans le fichier appSettings et les affiche également
        // dans la fenêtre de paramétrage de l'application. Si la valeur est incorrecte ou vide, une valeur par défaut
        // est affectée.
        public SettingsWindow()
        {
            Uri iconUri = new ("./resources/settingsIcon.png", UriKind.RelativeOrAbsolute);
            Icon = BitmapFrame.Create(iconUri);
            
            // Initialement, l'application dispose des paramètres par défaut, qui seront potentiellement modifiés après par
            // la lecture du fichier settings. Cela permet d'éviter un crash si le fichier 
            EnableDeeplTranslation = false;
            TranslationLang = "FR";
            RemoveUnusedGroupAddresses = false;
            EnableLightTheme = true;
            AppLang = "FR";
            DeeplKey = Convert.FromBase64String("");

            const string settingsPath = "./appSettings"; // Chemin du fichier paramètres
            
            // Vérification que le fichier contenant la main key existe
            _emkFileExists = File.Exists("./emk");
            
            // Si le fichier contenant la main key n'existe pas
            if (!_emkFileExists)
            {
                App.ConsoleAndLogWriteLine(
                    "Main key file not found. Generating a new one and resetting the settings file...");

                // Génération aléatoire d'une main key de taille 32 octets
                EncryptAndStoreMainKey(Convert.FromBase64String(GenerateRandomKey(32)));

                try
                {
                    if (File.Exists(settingsPath)) File.Delete(settingsPath); // Réinitialisation du fichier paramètres
                    if (File.Exists("./ei")) File.Delete("./ei"); // Suppression du vecteur d'initialisation du cryptage (non récupérable via la main key)
                    if (File.Exists("./ek")) File.Delete("./ek"); // Suppression de la clé de cryptage (non récupérable via la main key)
                }
                // Si l'un des fichiers n'est pas accessible
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Erreur : impossible d'accéder au fichier 'ei', 'ek' ou 'appSettings'. Veuillez vérifier qu'ils " +
                                    "ne sont pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 1", "Erreur", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(1);
                }
            }

            try
            {
                // Si le fichier de paramétrage n'existe pas, on le crée
                // Note : comme File.Create ouvre un stream vers le fichier à la création, on le ferme directement avec Close().
                if (!File.Exists(settingsPath)) File.Create(settingsPath).Close();
            }
            // Si le programme n'a pas accès en écriture pour créer le fichier
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Erreur: impossible d'accéder au fichier de paramétrage de l'application. Veuillez vérifier qu'il " +
                                "n'est pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 1", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }
            // Si la longueur du path est incorrecte ou que des caractères non supportés sont présents
            catch (ArgumentException)
            {
                MessageBox.Show($"Erreur: des caractères non supportés sont présents dans le chemin d'accès du fichier de paramétrage" +
                                $"({settingsPath}. Impossible d'accéder au fichier.\nCode erreur: 2", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(2);
            }
            // Aucune idée de la raison
            catch (IOException)
            {
                MessageBox.Show($"Erreur: Erreur I/O lors de l'ouverture du fichier de paramétrage.\nCode erreur: 3", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(3);
            }

            // Déclaration du stream pour la lecture du fichier appSettings, initialement null
            StreamReader? reader = null;
            
            try
            {
                // Création du stream
                reader = new StreamReader(settingsPath);
            }
            // Aucune idée de la raison
            catch (IOException)
            {
                MessageBox.Show($"Erreur: Erreur I/O lors de l'ouverture du fichier de paramétrage.\nCode erreur: 3", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(3);
            }

            try
            {
                // On parcourt toutes les lignes tant qu'elle n'est pas 'null'
                while (reader!.ReadLine() is { } line)
                {
                    // Créer un HashSet avec tous les codes de langue valides
                    HashSet<string> validLanguageCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "AR", "BG", "CS", "DA", "DE", "EL", "EN", "ES", "ET", "FI",
                        "HU", "ID", "IT", "JA", "KO", "LT", "LV", "NB", "NL", "PL",
                        "PT", "RO", "RU", "SK", "SL", "SV", "TR", "UK", "ZH"
                    };
                    
                    // On coupe la ligne en deux morceaux : la partie avant le ' : ' qui contient le type de paramètre contenu dans la ligne,
                    // la partie après qui contient la valeur du paramètre
                    var parts = line.Split(':');

                    // S'il n'y a pas de ' : ' ou qu'il n'y a rien après les deux points, on skip car la ligne nous intéresse pas
                    if (parts.Length < 2) continue;

                    var parameter = parts[0].Trim().ToLower();
                    var value = parts[1].Trim();

                    switch (parameter)
                    {
                        case "enable deepl translation":
                            try
                            {
                                // On essaie de cast la valeur en booléen
                                EnableDeeplTranslation = bool.Parse(value);
                            }
                            // Si l'utilisateur n'a pas écrit dans le fichier paramètres un string s'apparentant à true ou false
                            catch (FormatException)
                            {
                                App.ConsoleAndLogWriteLine(
                                    "Error: Could not parse boolean value of the activation of the deepL translation, restoring default value");
                            }

                            break;

                        case "deepl key [encrypted]":
                            // On récupère la clé DeepL encryptée
                            try
                            {
                                // On tente de la convertir en byte[] à partir d'un string en base 64
                                DeeplKey = Convert.FromBase64String(value);
                            }
                            // Si le format de la clé n'est pas correct
                            catch (FormatException)
                            {
                                App.ConsoleAndLogWriteLine($"Error: The format of {value} is incorrect and cannot be decrypted into a DeepL API Key. " +
                                                           $"Restoring default value.");
                            }
                            break;

                        case "translation lang":
                            // Vérifier si value est un code de langue valide, si elle est valide, on assigne la valeur, sinon on met la langue par défaut
                            TranslationLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
                            break;

                        case "remove unused group addresses":
                            try
                            {
                                // On tente de caster le string en booléen
                                RemoveUnusedGroupAddresses = bool.Parse(value);
                            }
                            // Si l'utilisateur n'a pas écrit dans le fichier paramètres un string s'apparentant à true ou false
                            catch (FormatException)
                            {
                                App.ConsoleAndLogWriteLine(
                                    "Error: Could not parse boolean value of the activation of the function to remove unused group addresses, restoring default value");
                            }

                            break;

                        case "theme":
                            // Si la valeur n'est pas dark, on mettra toujours le thème clair (en cas d'erreur, ou si la value est "light")
                            EnableLightTheme = !value.Equals("dark", StringComparison.CurrentCultureIgnoreCase);
                            break;

                        case "application language":
                            // Vérifier si value est un code de langue valide, si elle est valide, on assigne la valeur, sinon on met la langue par défaut
                            AppLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
                            break;
                    }
                }

            }
            // Si l'application a manqué de mémoire pendant la récupération des lignes
            catch (OutOfMemoryException)
            {
                App.ConsoleAndLogWriteLine("Error: The program does not have sufficient memory to run. Please try closing a few applications before trying again.");
                return;
            }
            // Aucune idée de la raison
            catch (IOException)
            {
                App.ConsoleAndLogWriteLine("Error: An I/O error occured while reading the settings file.");
                return;
            }
            finally
            {
                reader?.Close(); // Fermeture du stream de lecture
                SaveSettings(); // Mise à jour du fichier appSettings
            }
            
            InitializeComponent(); // Initialisation de la fenêtre de paramétrage

            UpdateWindowContents(); // Affichage des paramètres dans la fenêtre
        }
        
        
        // Fonction s'exécutant à la fermeture de la fenêtre de paramètres
        private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
        {
            e.Cancel = true; // Pour éviter de tuer l'instance de SettingsWindow, on annule la fermeture
            UpdateWindowContents(true); // Mise à jour du contenu de la fenêtre pour remettre les valeurs précédentes
            Hide(); // On masque la fenêtre à la place
        }


        // Fonction permettant de sauvegarder les paramètres dans le fichier appSettings
        private void SaveSettings()
        {
            // Création du stream d'écriture du fichier appSettings
            var writer = new StreamWriter("./appSettings");
            
            // Ecriture de toutes les lignes du fichier
            try
            {
                writer.WriteLine(
                    "-----------------------------------------------------------------------------------------");
                writer.WriteLine(
                    "|                                KNXBOOSTDESKTOP SETTINGS                               |");
                writer.WriteLine(
                    "-----------------------------------------------------------------------------------------");

                writer.Write("enable deepL translation : ");
                writer.WriteLine(EnableDeeplTranslation);

                writer.Write("deepL Key [ENCRYPTED] : ");
                writer.WriteLine(Convert.ToBase64String(DeeplKey));

                writer.Write("translation lang : ");
                writer.WriteLine(TranslationLang);

                writer.Write("remove unused group addresses : ");
                writer.WriteLine(RemoveUnusedGroupAddresses);

                writer.Write("theme : ");
                writer.WriteLine(EnableLightTheme ? "light" : "dark");

                writer.Write("application language : ");
                writer.WriteLine(AppLang);

                writer.WriteLine(
                    "-----------------------------------------------------------------------------------------");
                writer.WriteLine(
                    "Available languages: AR, BG, CS, DA, DE, EL, EN, ES, ET, FI, FR, HU, ID, IT, JA, KO, LT, LV, NB, NL, PL, PT, RO, RU, SK, SL, SV, TR, UK, ZH\n");
                writer.Write(
                    "/!\\ WARNING:\nAny value that you modify in this file and that is not correct will be replaced by a default value.");
            }
            // Aucune idée de la raison
            catch (IOException)
            {
                App.ConsoleAndLogWriteLine("Error: an I/O error occured while writing appSettings.");
            }
            // Si le buffer d'écriture est plein
            catch (NotSupportedException)
            {
                App.ConsoleAndLogWriteLine("Error: the streamwriter buffer for appSettings is full. Flushing it.");
                writer.Flush(); // Vidage du buffer
            }
            // Si le stream a été fermé pendant l'écriture
            catch (ObjectDisposedException)
            {
                App.ConsoleAndLogWriteLine("Error: the streamwriter for appSettings was closed before finishing the writing operation.");
            }
            finally
            {
                // Fermeture du stream d'écriture
                writer.Close();
            }
        }

        
        // Fonction permettant de mettre à jour les champs dans la fenêtre de paramétrage
        private void UpdateWindowContents(bool isClosing = false)
        {
            EnableTranslationCheckBox.IsChecked = EnableDeeplTranslation; // Cochage/décochage
            
            if ((_emkFileExists)&&(!isClosing)) DeeplApiKeyTextBox.Text = DecryptStringFromBytes(DeeplKey);

            // Si la langue de traduction ou de l'application n'est pas le français, on désélectionne le français dans le combobox
            // pour sélectionner la langue voulue
            if ((TranslationLang != "FR")||(AppLang != "FR"))
            {
                FrTranslationComboBoxItem.IsSelected = (TranslationLang == "FR"); // Sélection/Désélection
                FrAppLanguageComboBoxItem.IsSelected = (AppLang == "FR"); // Sélection/Désélection
    
                // Sélection du langage de traduction
                foreach (ComboBoxItem item in TranslationLanguageComboBox.Items) // Parcours de toutes les possibilités de langue
                {
                    if (!item.Content.ToString()!.StartsWith(TranslationLang)) continue; // Si la langue n'est pas celle que l'on veut, on skip
                    item.IsSelected = true; // Sélection de la langue
                    break; // Si on a trouvé la langue, on peut quitter la boucle
                }
                
                // Sélection du langage de l'application (même fonctionnement que le code ci-dessus)
                foreach (ComboBoxItem item in AppLanguageComboBox.Items)
                {
                    if (!item.Content.ToString()!.StartsWith(AppLang)) continue;
                    item.IsSelected = true;
                    break;
                }
            }

            RemoveUnusedAddressesCheckBox.IsChecked = RemoveUnusedGroupAddresses; // Cochage/décochage

            // Sélection du thème clair ou sombre
            LightThemeComboBoxItem.IsSelected = EnableLightTheme;
            DarkThemeComboBoxItem.IsSelected = !EnableLightTheme;
            
            // Traduction du menu settings
            switch (AppLang)
            {
                // Arabe
                case "AR":
                    SettingsWindowTopTitle.Text = "الإعدادات";
                    TranslationTitle.Text = "ترجمة";
                    EnableTranslationCheckBox.Content = "تمكين الترجمة";
                    DeeplApiKeyText.Text = "مفتاح API لـ DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(اضغط هنا للحصول على مفتاح مجاناً)");

                    TranslationLanguageComboBoxText.Text = "لغة الترجمة:";

                    GroupAddressManagementTitle.Text = "إدارة عناوين المجموعة";
                    RemoveUnusedAddressesCheckBox.Content = "إزالة العناوين غير المستخدمة";

                    AppSettingsTitle.Text = "إعدادات التطبيق";
                    ThemeTextBox.Text = "سمة:";
                    LightThemeComboBoxItem.Content = "فاتح (افتراضي)";
                    DarkThemeComboBoxItem.Content = "داكن";

                    AppLanguageTextBlock.Text = "لغة التطبيق:";

                    SaveButtonText.Text = "حفظ";
                    CancelButtonText.Text = "إلغاء";
                    break;

                // Bulgare
                case "BG":
                    SettingsWindowTopTitle.Text = "Настройки";
                    TranslationTitle.Text = "Превод";
                    EnableTranslationCheckBox.Content = "Разреши превод";
                    DeeplApiKeyText.Text = "API ключ за DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Кликнете тук, за да получите ключ безплатно)");

                    TranslationLanguageComboBoxText.Text = "Език на превод:";

                    GroupAddressManagementTitle.Text = "Управление на групови адреси";
                    RemoveUnusedAddressesCheckBox.Content = "Премахване на неизползвани адреси";

                    AppSettingsTitle.Text = "Настройки на приложението";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Светла (по подразбиране)";
                    DarkThemeComboBoxItem.Content = "Тъмна";

                    AppLanguageTextBlock.Text = "Език на приложението:";

                    SaveButtonText.Text = "Запази";
                    CancelButtonText.Text = "Отказ";
                    break;

                // Tchèque
                case "CS":
                    SettingsWindowTopTitle.Text = "Nastavení";
                    TranslationTitle.Text = "Překlad";
                    EnableTranslationCheckBox.Content = "Povolit překlad";
                    DeeplApiKeyText.Text = "API klíč pro DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikněte sem a získejte klíč zdarma)");

                    TranslationLanguageComboBoxText.Text = "Jazyk překladu:";

                    GroupAddressManagementTitle.Text = "Správa skupinových adres";
                    RemoveUnusedAddressesCheckBox.Content = "Odstranit nepoužívané adresy";

                    AppSettingsTitle.Text = "Nastavení aplikace";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Světlé (výchozí)";
                    DarkThemeComboBoxItem.Content = "Tmavé";

                    AppLanguageTextBlock.Text = "Jazyk aplikace:";

                    SaveButtonText.Text = "Uložit";
                    CancelButtonText.Text = "Zrušit";
                    break;

                // Danois
                case "DA":
                    SettingsWindowTopTitle.Text = "Indstillinger";
                    TranslationTitle.Text = "Oversættelse";
                    EnableTranslationCheckBox.Content = "Aktiver oversættelse";
                    DeeplApiKeyText.Text = "DeepL API-nøgle:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik her for at få en gratis nøgle)");

                    TranslationLanguageComboBoxText.Text = "Oversættelsessprog:";

                    GroupAddressManagementTitle.Text = "Administration af gruppeadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrugte adresser";

                    AppSettingsTitle.Text = "Applikationsindstillinger";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Lys (standard)";
                    DarkThemeComboBoxItem.Content = "Mørk";

                    AppLanguageTextBlock.Text = "Applikationssprog:";

                    SaveButtonText.Text = "Gem";
                    CancelButtonText.Text = "Annuller";
                    break;

                // Allemand
                case "DE":
                    SettingsWindowTopTitle.Text = "Einstellungen";
                    TranslationTitle.Text = "Übersetzung";
                    EnableTranslationCheckBox.Content = "Übersetzung aktivieren";
                    DeeplApiKeyText.Text = "DeepL API-Schlüssel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klicken Sie hier, um einen Schlüssel kostenlos zu erhalten)");

                    TranslationLanguageComboBoxText.Text = "Übersetzungssprache:";

                    GroupAddressManagementTitle.Text = "Gruppenadressenverwaltung";
                    RemoveUnusedAddressesCheckBox.Content = "Nicht verwendete Adressen entfernen";

                    AppSettingsTitle.Text = "App-Einstellungen";
                    ThemeTextBox.Text = "Thema:";
                    LightThemeComboBoxItem.Content = "Hell (Standard)";
                    DarkThemeComboBoxItem.Content = "Dunkel";

                    AppLanguageTextBlock.Text = "App-Sprache:";

                    SaveButtonText.Text = "Speichern";
                    CancelButtonText.Text = "Abbrechen";
                    break;

                // Grec
                case "EL":
                    SettingsWindowTopTitle.Text = "Ρυθμίσεις";
                    TranslationTitle.Text = "Μετάφραση";
                    EnableTranslationCheckBox.Content = "Ενεργοποίηση μετάφρασης";
                    DeeplApiKeyText.Text = "API κλειδί για DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Κάντε κλικ εδώ για να λάβετε ένα κλειδί δωρεάν)");

                    TranslationLanguageComboBoxText.Text = "Γλώσσα μετάφρασης:";

                    GroupAddressManagementTitle.Text = "Διαχείριση ομαδικών διευθύνσεων";
                    RemoveUnusedAddressesCheckBox.Content = "Αφαίρεση αχρησιμοποίητων διευθύνσεων";

                    AppSettingsTitle.Text = "Ρυθμίσεις εφαρμογής";
                    ThemeTextBox.Text = "Θέμα:";
                    LightThemeComboBoxItem.Content = "Φωτεινό (προεπιλεγμένο)";
                    DarkThemeComboBoxItem.Content = "Σκοτεινό";

                    AppLanguageTextBlock.Text = "Γλώσσα εφαρμογής:";

                    SaveButtonText.Text = "Αποθήκευση";
                    CancelButtonText.Text = "Άκυρο";
                    break;

                // Anglais
                case "EN":
                    SettingsWindowTopTitle.Text = "Settings";
                    TranslationTitle.Text = "Translation";
                    EnableTranslationCheckBox.Content = "Enable translation";
                    DeeplApiKeyText.Text = "DeepL API Key:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Click here to get a free key)");

                    TranslationLanguageComboBoxText.Text = "Translation Language:";

                    GroupAddressManagementTitle.Text = "Group Address Management";
                    RemoveUnusedAddressesCheckBox.Content = "Remove Unused Addresses";

                    AppSettingsTitle.Text = "Application Settings";
                    ThemeTextBox.Text = "Theme:";
                    LightThemeComboBoxItem.Content = "Light (default)";
                    DarkThemeComboBoxItem.Content = "Dark";

                    AppLanguageTextBlock.Text = "Application Language:";

                    SaveButtonText.Text = "Save";
                    CancelButtonText.Text = "Cancel";
                    break;

                // Espagnol
                case "ES":
                    SettingsWindowTopTitle.Text = "Configuraciones";
                    TranslationTitle.Text = "Traducción";
                    EnableTranslationCheckBox.Content = "Habilitar traducción";
                    DeeplApiKeyText.Text = "Clave API de DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Haga clic aquí para obtener una clave gratuita)");

                    TranslationLanguageComboBoxText.Text = "Idioma de traducción:";

                    GroupAddressManagementTitle.Text = "Gestión de direcciones de grupo";
                    RemoveUnusedAddressesCheckBox.Content = "Eliminar direcciones no utilizadas";

                    AppSettingsTitle.Text = "Configuraciones de la aplicación";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Claro (predeterminado)";
                    DarkThemeComboBoxItem.Content = "Oscuro";

                    AppLanguageTextBlock.Text = "Idioma de la aplicación:";

                    SaveButtonText.Text = "Guardar";
                    CancelButtonText.Text = "Cancelar";
                    break;

                // Estonien
                case "ET":
                    SettingsWindowTopTitle.Text = "Seaded";
                    TranslationTitle.Text = "Tõlge";
                    EnableTranslationCheckBox.Content = "Luba tõlge";
                    DeeplApiKeyText.Text = "DeepL API võti:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klõpsake siin, et saada tasuta võti)");

                    TranslationLanguageComboBoxText.Text = "Tõlke keel:";

                    GroupAddressManagementTitle.Text = "Rühma aadresside haldamine";
                    RemoveUnusedAddressesCheckBox.Content = "Eemalda kasutamata aadressid";

                    AppSettingsTitle.Text = "Rakenduse seaded";
                    ThemeTextBox.Text = "Teema:";
                    LightThemeComboBoxItem.Content = "Hele (vaikimisi)";
                    DarkThemeComboBoxItem.Content = "Tume";

                    AppLanguageTextBlock.Text = "Rakenduse keel:";

                    SaveButtonText.Text = "Salvesta";
                    CancelButtonText.Text = "Tühista";
                    break;

                // Finnois
                case "FI":
                    SettingsWindowTopTitle.Text = "Asetukset";
                    TranslationTitle.Text = "Käännös";
                    EnableTranslationCheckBox.Content = "Ota käännös käyttöön";
                    DeeplApiKeyText.Text = "DeepL API-avain:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Napsauta tästä saadaksesi ilmaisen avaimen)");

                    TranslationLanguageComboBoxText.Text = "Käännöskieli:";

                    GroupAddressManagementTitle.Text = "Ryhmän osoitteiden hallinta";
                    RemoveUnusedAddressesCheckBox.Content = "Poista käyttämättömät osoitteet";

                    AppSettingsTitle.Text = "Sovelluksen asetukset";
                    ThemeTextBox.Text = "Teema:";
                    LightThemeComboBoxItem.Content = "Vaalea (oletus)";
                    DarkThemeComboBoxItem.Content = "Tumma";

                    AppLanguageTextBlock.Text = "Sovelluksen kieli:";

                    SaveButtonText.Text = "Tallenna";
                    CancelButtonText.Text = "Peruuta";
                    break;

                // Hongrois
                case "HU":
                    SettingsWindowTopTitle.Text = "Beállítások";
                    TranslationTitle.Text = "Fordítás";
                    EnableTranslationCheckBox.Content = "Fordítás engedélyezése";
                    DeeplApiKeyText.Text = "DeepL API kulcs:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kattintson ide, hogy ingyenes kulcsot kapjon)");

                    TranslationLanguageComboBoxText.Text = "Fordítási nyelv:";

                    GroupAddressManagementTitle.Text = "Csoportos címkezelés";
                    RemoveUnusedAddressesCheckBox.Content = "Nem használt címek eltávolítása";

                    AppSettingsTitle.Text = "Alkalmazás beállításai";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Világos (alapértelmezett)";
                    DarkThemeComboBoxItem.Content = "Sötét";

                    AppLanguageTextBlock.Text = "Alkalmazás nyelve:";

                    SaveButtonText.Text = "Mentés";
                    CancelButtonText.Text = "Mégse";
                    break;

                // Indonésien
                case "ID":
                    SettingsWindowTopTitle.Text = "Pengaturan";
                    TranslationTitle.Text = "Terjemahan";
                    EnableTranslationCheckBox.Content = "Aktifkan terjemahan";
                    DeeplApiKeyText.Text = "Kunci API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik di sini untuk mendapatkan kunci gratis)");

                    TranslationLanguageComboBoxText.Text = "Bahasa Terjemahan:";

                    GroupAddressManagementTitle.Text = "Pengelolaan Alamat Grup";
                    RemoveUnusedAddressesCheckBox.Content = "Hapus Alamat yang Tidak Digunakan";

                    AppSettingsTitle.Text = "Pengaturan Aplikasi";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Terang (default)";
                    DarkThemeComboBoxItem.Content = "Gelap";

                    AppLanguageTextBlock.Text = "Bahasa Aplikasi:";

                    SaveButtonText.Text = "Simpan";
                    CancelButtonText.Text = "Batal";
                    break;

                // Italien
                case "IT":
                    SettingsWindowTopTitle.Text = "Impostazioni";
                    TranslationTitle.Text = "Traduzione";
                    EnableTranslationCheckBox.Content = "Abilita traduzione";
                    DeeplApiKeyText.Text = "Chiave API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Clicca qui per ottenere una chiave gratuita)");

                    TranslationLanguageComboBoxText.Text = "Lingua di traduzione:";

                    GroupAddressManagementTitle.Text = "Gestione degli indirizzi di gruppo";
                    RemoveUnusedAddressesCheckBox.Content = "Rimuovi indirizzi non utilizzati";

                    AppSettingsTitle.Text = "Impostazioni dell'applicazione";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Chiaro (predefinito)";
                    DarkThemeComboBoxItem.Content = "Scuro";

                    AppLanguageTextBlock.Text = "Lingua dell'applicazione:";

                    SaveButtonText.Text = "Salva";
                    CancelButtonText.Text = "Annulla";
                    break;

                // Japonais
                case "JA":
                    SettingsWindowTopTitle.Text = "設定";
                    TranslationTitle.Text = "翻訳";
                    EnableTranslationCheckBox.Content = "翻訳を有効にする";
                    DeeplApiKeyText.Text = "DeepL APIキー:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(無料のキーを取得するにはここをクリック)");

                    TranslationLanguageComboBoxText.Text = "翻訳言語:";

                    GroupAddressManagementTitle.Text = "グループアドレス管理";
                    RemoveUnusedAddressesCheckBox.Content = "使用されていないアドレスを削除";

                    AppSettingsTitle.Text = "アプリケーション設定";
                    ThemeTextBox.Text = "テーマ:";
                    LightThemeComboBoxItem.Content = "ライト (デフォルト)";
                    DarkThemeComboBoxItem.Content = "ダーク";

                    AppLanguageTextBlock.Text = "アプリケーションの言語:";

                    SaveButtonText.Text = "保存";
                    CancelButtonText.Text = "キャンセル";
                    break;

                // Coréen
                case "KO":
                    SettingsWindowTopTitle.Text = "설정";
                    TranslationTitle.Text = "번역";
                    EnableTranslationCheckBox.Content = "번역 활성화";
                    DeeplApiKeyText.Text = "DeepL API 키:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(무료 키를 얻으려면 여기를 클릭하세요)");

                    TranslationLanguageComboBoxText.Text = "번역 언어:";

                    GroupAddressManagementTitle.Text = "그룹 주소 관리";
                    RemoveUnusedAddressesCheckBox.Content = "사용하지 않는 주소 제거";

                    AppSettingsTitle.Text = "애플리케이션 설정";
                    ThemeTextBox.Text = "테마:";
                    LightThemeComboBoxItem.Content = "밝게 (기본)";
                    DarkThemeComboBoxItem.Content = "어둡게";

                    AppLanguageTextBlock.Text = "애플리케이션 언어:";

                    SaveButtonText.Text = "저장";
                    CancelButtonText.Text = "취소";
                    break;

                // Letton
                case "LV":
                    SettingsWindowTopTitle.Text = "Iestatījumi";
                    TranslationTitle.Text = "Tulkot";
                    EnableTranslationCheckBox.Content = "Iespējot tulkojumu";
                    DeeplApiKeyText.Text = "DeepL API atslēga:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Noklikšķiniet šeit, lai iegūtu bezmaksas atslēgu)");

                    TranslationLanguageComboBoxText.Text = "Tulkot valodu:";

                    GroupAddressManagementTitle.Text = "Grupas adrešu pārvaldība";
                    RemoveUnusedAddressesCheckBox.Content = "Noņemt neizmantotās adreses";

                    AppSettingsTitle.Text = "Lietotnes iestatījumi";
                    ThemeTextBox.Text = "Tēma:";
                    LightThemeComboBoxItem.Content = "Gaišs (noklusējums)";
                    DarkThemeComboBoxItem.Content = "Tumšs";

                    AppLanguageTextBlock.Text = "Lietotnes valoda:";

                    SaveButtonText.Text = "Saglabāt";
                    CancelButtonText.Text = "Atcelt";
                    break;

                // Lituanien
                case "LT":
                    SettingsWindowTopTitle.Text = "Nustatymai";
                    TranslationTitle.Text = "Vertimas";
                    EnableTranslationCheckBox.Content = "Įjungti vertimą";
                    DeeplApiKeyText.Text = "DeepL API raktas:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Spustelėkite čia, kad gautumėte nemokamą raktą)");

                    TranslationLanguageComboBoxText.Text = "Vertimo kalba:";

                    GroupAddressManagementTitle.Text = "Grupės adresų tvarkymas";
                    RemoveUnusedAddressesCheckBox.Content = "Pašalinti nenaudojamus adresus";

                    AppSettingsTitle.Text = "Programos nustatymai";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Šviesus (numatytasis)";
                    DarkThemeComboBoxItem.Content = "Tamsus";

                    AppLanguageTextBlock.Text = "Programos kalba:";

                    SaveButtonText.Text = "Išsaugoti";
                    CancelButtonText.Text = "Atšaukti";
                    break;

                // Norvégien
                case "NB":
                    SettingsWindowTopTitle.Text = "Innstillinger";
                    TranslationTitle.Text = "Oversettelse";
                    EnableTranslationCheckBox.Content = "Aktiver oversettelse";
                    DeeplApiKeyText.Text = "DeepL API-nøkkel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikk her for å få en gratis nøkkel)");

                    TranslationLanguageComboBoxText.Text = "Oversettelsesspråk:";

                    GroupAddressManagementTitle.Text = "Administrasjon av gruppeadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrukte adresser";

                    AppSettingsTitle.Text = "App-innstillinger";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Lys (standard)";
                    DarkThemeComboBoxItem.Content = "Mørk";

                    AppLanguageTextBlock.Text = "Applikasjonsspråk:";

                    SaveButtonText.Text = "Lagre";
                    CancelButtonText.Text = "Avbryt";
                    break;

                // Néerlandais
                case "NL":
                    SettingsWindowTopTitle.Text = "Instellingen";
                    TranslationTitle.Text = "Vertaling";
                    EnableTranslationCheckBox.Content = "Vertaling inschakelen";
                    DeeplApiKeyText.Text = "DeepL API-sleutel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik hier om een gratis sleutel te krijgen)");

                    TranslationLanguageComboBoxText.Text = "Vertaaltaal:";

                    GroupAddressManagementTitle.Text = "Beheer van groepsadressen";
                    RemoveUnusedAddressesCheckBox.Content = "Verwijder ongebruikte adressen";

                    AppSettingsTitle.Text = "App-instellingen";
                    ThemeTextBox.Text = "Thema:";
                    LightThemeComboBoxItem.Content = "Licht (standaard)";
                    DarkThemeComboBoxItem.Content = "Donker";

                    AppLanguageTextBlock.Text = "App-taal:";

                    SaveButtonText.Text = "Opslaan";
                    CancelButtonText.Text = "Annuleren";
                    break;

                // Polonais
                case "PL":
                    SettingsWindowTopTitle.Text = "Ustawienia";
                    TranslationTitle.Text = "Tłumaczenie";
                    EnableTranslationCheckBox.Content = "Włącz tłumaczenie";
                    DeeplApiKeyText.Text = "Klucz API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknij tutaj, aby otrzymać darmowy klucz)");

                    TranslationLanguageComboBoxText.Text = "Język tłumaczenia:";

                    GroupAddressManagementTitle.Text = "Zarządzanie adresami grup";
                    RemoveUnusedAddressesCheckBox.Content = "Usuń nieużywane adresy";

                    AppSettingsTitle.Text = "Ustawienia aplikacji";
                    ThemeTextBox.Text = "Motyw:";
                    LightThemeComboBoxItem.Content = "Jasny (domyślny)";
                    DarkThemeComboBoxItem.Content = "Ciemny";

                    AppLanguageTextBlock.Text = "Język aplikacji:";

                    SaveButtonText.Text = "Zapisz";
                    CancelButtonText.Text = "Anuluj";
                    break;

                // Portugais
                case "PT":
                    SettingsWindowTopTitle.Text = "Configurações";
                    TranslationTitle.Text = "Tradução";
                    EnableTranslationCheckBox.Content = "Ativar tradução";
                    DeeplApiKeyText.Text = "Chave API do DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Clique aqui para obter uma chave gratuita)");

                    TranslationLanguageComboBoxText.Text = "Idioma da tradução:";

                    GroupAddressManagementTitle.Text = "Gerenciamento de endereços de grupo";
                    RemoveUnusedAddressesCheckBox.Content = "Remover endereços não utilizados";

                    AppSettingsTitle.Text = "Configurações do aplicativo";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Claro (padrão)";
                    DarkThemeComboBoxItem.Content = "Escuro";

                    AppLanguageTextBlock.Text = "Idioma do aplicativo:";

                    SaveButtonText.Text = "Salvar";
                    CancelButtonText.Text = "Cancelar";
                    break;

                // Roumain
                case "RO":
                    SettingsWindowTopTitle.Text = "Setări";
                    TranslationTitle.Text = "Traducere";
                    EnableTranslationCheckBox.Content = "Activare traducere";
                    DeeplApiKeyText.Text = "Cheie API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Faceți clic aici pentru a obține o cheie gratuit)");

                    TranslationLanguageComboBoxText.Text = "Limba de traducere:";

                    GroupAddressManagementTitle.Text = "Gestionarea adreselor de grup";
                    RemoveUnusedAddressesCheckBox.Content = "Eliminați adresele neutilizate";

                    AppSettingsTitle.Text = "Setările aplicației";
                    ThemeTextBox.Text = "Temă:";
                    LightThemeComboBoxItem.Content = "Luminoasă (implicit)";
                    DarkThemeComboBoxItem.Content = "Întunecată";

                    AppLanguageTextBlock.Text = "Limba aplicației:";

                    SaveButtonText.Text = "Salvați";
                    CancelButtonText.Text = "Anulați";
                    break;

                // Russe
                case "RU":
                    SettingsWindowTopTitle.Text = "Настройки";
                    TranslationTitle.Text = "Перевод";
                    EnableTranslationCheckBox.Content = "Включить перевод";
                    DeeplApiKeyText.Text = "Ключ API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Нажмите здесь, чтобы получить бесплатный ключ)");

                    TranslationLanguageComboBoxText.Text = "Язык перевода:";

                    GroupAddressManagementTitle.Text = "Управление групповыми адресами";
                    RemoveUnusedAddressesCheckBox.Content = "Удалить неиспользуемые адреса";

                    AppSettingsTitle.Text = "Настройки приложения";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Светлая (по умолчанию)";
                    DarkThemeComboBoxItem.Content = "Тёмная";

                    AppLanguageTextBlock.Text = "Язык приложения:";

                    SaveButtonText.Text = "Сохранить";
                    CancelButtonText.Text = "Отменить";
                    break;

                // Slovaque
                case "SK":
                    SettingsWindowTopTitle.Text = "Nastavenia";
                    TranslationTitle.Text = "Preklad";
                    EnableTranslationCheckBox.Content = "Povoliť preklad";
                    DeeplApiKeyText.Text = "API kľúč pre DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite sem a získajte kľúč zadarmo)");

                    TranslationLanguageComboBoxText.Text = "Jazyk prekladu:";

                    GroupAddressManagementTitle.Text = "Správa skupinových adries";
                    RemoveUnusedAddressesCheckBox.Content = "Odstrániť nepoužívané adresy";

                    AppSettingsTitle.Text = "Nastavenia aplikácie";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Svetlá (predvolená)";
                    DarkThemeComboBoxItem.Content = "Tmavá";

                    AppLanguageTextBlock.Text = "Jazyk aplikácie:";

                    SaveButtonText.Text = "Uložiť";
                    CancelButtonText.Text = "Zrušiť";
                    break;

                // Slovène
                case "SL":
                    SettingsWindowTopTitle.Text = "Nastavitve";
                    TranslationTitle.Text = "Prevajanje";
                    EnableTranslationCheckBox.Content = "Omogoči prevajanje";
                    DeeplApiKeyText.Text = "API ključ za DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite tukaj za brezplačen ključ)");

                    TranslationLanguageComboBoxText.Text = "Jezik prevajanja:";

                    GroupAddressManagementTitle.Text = "Upravljanje skupinskih naslovov";
                    RemoveUnusedAddressesCheckBox.Content = "Odstrani neuporabljene naslove";

                    AppSettingsTitle.Text = "Nastavitve aplikacije";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Svetlo (privzeto)";
                    DarkThemeComboBoxItem.Content = "Temno";

                    AppLanguageTextBlock.Text = "Jezik aplikacije:";

                    SaveButtonText.Text = "Shrani";
                    CancelButtonText.Text = "Prekliči";
                    break;

                // Suédois
                case "SV":
                    SettingsWindowTopTitle.Text = "Inställningar";
                    TranslationTitle.Text = "Översättning";
                    EnableTranslationCheckBox.Content = "Aktivera översättning";
                    DeeplApiKeyText.Text = "DeepL API-nyckel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klicka här för att få en gratis nyckel)");

                    TranslationLanguageComboBoxText.Text = "Översättningsspråk:";

                    GroupAddressManagementTitle.Text = "Hantera gruppadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Ta bort oanvända adresser";

                    AppSettingsTitle.Text = "Applikationsinställningar";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Ljus (standard)";
                    DarkThemeComboBoxItem.Content = "Mörk";

                    AppLanguageTextBlock.Text = "Applikationsspråk:";

                    SaveButtonText.Text = "Spara";
                    CancelButtonText.Text = "Avbryt";
                    break;

                // Turc
                case "TR":
                    SettingsWindowTopTitle.Text = "Ayarlar";
                    TranslationTitle.Text = "Çeviri";
                    EnableTranslationCheckBox.Content = "Çeviriyi etkinleştir";
                    DeeplApiKeyText.Text = "DeepL API Anahtarı:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Ücretsiz anahtar almak için buraya tıklayın)");

                    TranslationLanguageComboBoxText.Text = "Çeviri Dili:";

                    GroupAddressManagementTitle.Text = "Grup Adres Yönetimi";
                    RemoveUnusedAddressesCheckBox.Content = "Kullanılmayan Adresleri Kaldır";

                    AppSettingsTitle.Text = "Uygulama Ayarları";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Açık (varsayılan)";
                    DarkThemeComboBoxItem.Content = "Koyu";

                    AppLanguageTextBlock.Text = "Uygulama Dili:";

                    SaveButtonText.Text = "Kaydet";
                    CancelButtonText.Text = "İptal";
                    break;

                // Ukrainien
                case "UK":
                    SettingsWindowTopTitle.Text = "Налаштування";
                    TranslationTitle.Text = "Переклад";
                    EnableTranslationCheckBox.Content = "Увімкнути переклад";
                    DeeplApiKeyText.Text = "Ключ API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Натисніть тут, щоб отримати безкоштовний ключ)");

                    TranslationLanguageComboBoxText.Text = "Мова перекладу:";

                    GroupAddressManagementTitle.Text = "Управління груповими адресами";
                    RemoveUnusedAddressesCheckBox.Content = "Видалити невикористані адреси";

                    AppSettingsTitle.Text = "Налаштування додатка";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Світла (за замовчуванням)";
                    DarkThemeComboBoxItem.Content = "Темна";

                    AppLanguageTextBlock.Text = "Мова додатка:";

                    SaveButtonText.Text = "Зберегти";
                    CancelButtonText.Text = "Скасувати";
                    break;

                // Chinois simplifié
                case "ZH":
                    SettingsWindowTopTitle.Text = "设置";
                    TranslationTitle.Text = "翻译";
                    EnableTranslationCheckBox.Content = "启用翻译";
                    DeeplApiKeyText.Text = "DeepL API密钥:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(点击这里获取免费的密钥)");

                    TranslationLanguageComboBoxText.Text = "翻译语言:";

                    GroupAddressManagementTitle.Text = "组地址管理";
                    RemoveUnusedAddressesCheckBox.Content = "删除未使用的地址";

                    AppSettingsTitle.Text = "应用程序设置";
                    ThemeTextBox.Text = "主题:";
                    LightThemeComboBoxItem.Content = "浅色 (默认)";
                    DarkThemeComboBoxItem.Content = "深色";

                    AppLanguageTextBlock.Text = "应用程序语言:";

                    SaveButtonText.Text = "保存";
                    CancelButtonText.Text = "取消";
                    break;

	            // Langue par défaut (français)
                default:
		            SettingsWindowTopTitle.Text = "Paramètres";
                    TranslationTitle.Text = "Traduction";
                    EnableTranslationCheckBox.Content = "Activer la traduction";
                    DeeplApiKeyText.Text = "Clé API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Cliquez ici pour obtenir une clé gratuitement)");

                    TranslationLanguageComboBoxText.Text = "Langue de traduction:";

                    GroupAddressManagementTitle.Text = "Gestion des adresses de groupe";
                    RemoveUnusedAddressesCheckBox.Content = "Supprimer les adresses inutilisées";

                    AppSettingsTitle.Text = "Paramètres de l'application";
                    ThemeTextBox.Text = "Thème:";
                    LightThemeComboBoxItem.Content = "Clair (par défaut)";
                    DarkThemeComboBoxItem.Content = "Sombre";

                    AppLanguageTextBlock.Text = "Langue de l'application:";

                    SaveButtonText.Text = "Enregistrer";
                    CancelButtonText.Text = "Annuler";
                    break;

            }
        }

        
        // ----- GESTION DES BOUTONS -----
        // Fonction s'exécutant lors du clic sur le bouton sauvegarder
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
            EnableDeeplTranslation = (bool) EnableTranslationCheckBox.IsChecked!;
            DeeplKey = EncryptStringToBytes(DeeplApiKeyTextBox.Text);
            TranslationLang = TranslationLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            RemoveUnusedGroupAddresses = (bool) RemoveUnusedAddressesCheckBox.IsChecked!;
            EnableLightTheme = LightThemeComboBoxItem.IsSelected;
            AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            
            // Sauvegarde des paramètres dans le fichier appSettings
            App.ConsoleAndLogWriteLine($"Saving application settings at {Path.GetFullPath("./appSettings")}");
            SaveSettings();
            App.ConsoleAndLogWriteLine("Settings saved successfully");
            
            // Mise à jour éventuellement du contenu pour update la langue du menu
            UpdateWindowContents();
            
            // Masquage de la fenêtre de paramètres
            Hide();
        }

        
        // Fonction s'exécutant lors du clic sur le bouton annuler
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateWindowContents(); // Restauration des paramètres précédents dans la fenêtre de paramétrage
            Hide(); // Masquage de la fenêtre de paramétrage
        }

        
        // ----- GESTION DE DES CASES A COCHER -----
        // Fonction s'activant quand on coche l'activation de la traduction DeepL
        private void EnableTranslation(object sender, RoutedEventArgs e)
        {
            // On affiche la textbox qui permet à l'utilisateur d'entrer la clé API DeepL
            TextDeeplApiStackPanel.Visibility = Visibility.Visible;
            DeeplApiKeyTextBox.Visibility = Visibility.Visible;

            // On affiche le menu déroulant de sélection de la langue de traduction
            TranslationLanguageComboBoxText.Visibility = Visibility.Visible;
            TranslationLanguageComboBox.Visibility = Visibility.Visible;

            // Ajustement de la taille de la fenêtre pour que les nouveaux éléments affichés aient de la place
            Height += 95;
        }

        
        // Fonction s'activant quand on décoche l'activation de la traduction DeepL
        private void DisableTranslation(object sender, RoutedEventArgs e)
        {
            // On masque la textbox qui permet à l'utilisateur d'entrer la clé API DeepL
            TextDeeplApiStackPanel.Visibility = Visibility.Collapsed;
            DeeplApiKeyTextBox.Visibility = Visibility.Collapsed;

            // On masque le menu déroulant de sélection de la langue de traduction
            TranslationLanguageComboBoxText.Visibility = Visibility.Collapsed;
            TranslationLanguageComboBox.Visibility = Visibility.Collapsed;

            // Ajustement de la taille de la fenêtre
            Height -= 95;
        }
        
        
        // ----- GESTION DE L'ENCRYPTION -----
        // Fonction permettant d'encrypter un string donné avec une clé et un iv créés dynamiquement
        // Attention : cette fonction permet de stocker UN SEUL string (ici la clé deepL) à la fois. Si vous encryptez un autre string,
        // le combo clé/iv pour le premier string sera perdu.
        private static byte[] EncryptStringToBytes(string plainText)
        {
            // Générer une nouvelle clé et IV pour l'encryption
            using (Aes aesAlg = Aes.Create())
            {
                try
                {
                    // Chiffrer les nouvelles clés et IV avec la clé principale
                    var encryptedNewKey = EncryptKeyOrIv(Convert.ToBase64String(aesAlg.Key));
                    var encryptedNewIv = EncryptKeyOrIv(Convert.ToBase64String(aesAlg.IV));

                    // Stocker les clés chiffrées (par exemple dans un fichier ou une base de données)
                    // Note : si les fichiers n'existent pas, ils sont automatiquement créés par la fonction WriteAllText
                    File.WriteAllText("./ek", encryptedNewKey);
                    File.WriteAllText("./ei", encryptedNewIv);
                }
                // Aucune idée de la raison
                catch (IOException)
                {
                    App.ConsoleAndLogWriteLine("Error: I/O exception occured while writing 'ek' or 'ei' files, encrypted data will be lost");
                }
                // Si les fichiers 'ek' et 'ei' ne sont pas accessibles en écriture
                catch (UnauthorizedAccessException)
                {
                    App.ConsoleAndLogWriteLine("Error: cannot get writing access to 'ek' and 'ei' files, encrypted data will be lost");
                }

                // Chiffrer les données avec les nouvelles clés et IV
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                try
                {
                    // Création du stream de cryptage
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                // Cryptage de la donnée depuis le stream
                                swEncrypt.Write(plainText);
                            }
                            
                            // Renvoi de la donnée encryptée sous forme d'un tableau d'octets
                            return msEncrypt.ToArray();
                        }
                    }
                }
                // Si le stream est invalide
                catch (ArgumentException)
                {
                    App.ConsoleAndLogWriteLine("Error: MemoryStream for encryption is invalid. Could not encrypt the data.");
                    return Convert.FromBase64String(""); // On retourne un tableau d'octets vide
                }
                // Aucune idée de la raison
                catch (IOException)
                {
                    return Convert.FromBase64String(""); // On retourne un tableau d'octets vide
                } 
                // Si le writer est fermé, ou son buffer est plein
                catch (ObjectDisposedException)
                {
                    App.ConsoleAndLogWriteLine("Error: Encryption writer was closed before encrypting the data, or its buffer is full. Could not encrypt the data.");
                    return Convert.FromBase64String(""); // On retourne un tableau d'octets vide
                }
                // Pas sûr d'avoir saisi la différence avec l'exception précédente
                catch (NotSupportedException)
                {
                    App.ConsoleAndLogWriteLine("Error: Encryption writer was closed before encrypting the data, or its buffer is full. Could not encrypt the data.");
                    return Convert.FromBase64String(""); // On retourne un tableau d'octets vide
                }
            }
        }

        
        // Fonction permettant de décrypter un string donné à partir de la clé et de l'iv chiffrés
        public string DecryptStringFromBytes(byte[] encryptedString)
        {
            // Si les fichiers des clés n'existent pas, on retourne un string vide
            if (!File.Exists("./ek") || !File.Exists("./ei"))
            {
                App.ConsoleAndLogWriteLine("Error: encryption keys could not be retrieved to decrypt the DeepL API Key. Restoring default value.");
                return "";
            }

            string encryptedKey = "";
            string encryptedIv = "";

            try
            {
                // Lire les clés chiffrées
                encryptedKey = File.ReadAllText("./ek");
                encryptedIv = File.ReadAllText("./ei");
            }
            // Si la longueur du path est invalide ou contient des caractères non supportés
            catch (ArgumentException)
            {
                App.ConsoleAndLogWriteLine($"Error: the paths {Path.GetFullPath("./ek")} and/or {Path.GetFullPath("./ei")} contains unsupported characters. " +
                                           $"Encryption keys could not be retrieved.");
            }
            // Aucune idée de la raison
            catch (IOException)
            {
                App.ConsoleAndLogWriteLine($"Error: I/O exception occured while reading 'ei' and 'ek' files. " +
                                           $"Encryption keys could not be retrieved.");
            }
            // Si les fichiers ne sont pas accessibles en lecture
            catch (UnauthorizedAccessException)
            {
                App.ConsoleAndLogWriteLine($"Error: no authorization to read 'ei' and 'ek' files. " +
                                           $"Encryption keys could not be retrieved. Please try again or try running the program in administrator mode.");
            }
            

            // Déchiffrer les clés et IV avec la clé principale
            var decryptedKey = DecryptKeyOrIv(encryptedKey);
            var decryptedIv = DecryptKeyOrIv(encryptedIv);

            try
            {
                // Déchiffrer les données avec les clés et IV déchiffrés
                using (var aesAlg = Aes.Create())
                {
                    // Assignation des clés
                    aesAlg.Key = Convert.FromBase64String(decryptedKey);
                    aesAlg.IV = Convert.FromBase64String(decryptedIv);

                    // Création du décrypteur
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Création du stream de décryptage
                    using (var msDecrypt = new MemoryStream(encryptedString))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            // Si le stream est 'null'
            catch (ArgumentNullException)
            {
                App.ConsoleAndLogWriteLine("Error: decryption stream is null or the keys are null. Data could not be decrypted.");
                return "";
            }
            // Si le format des clés est invalide
            catch (FormatException)
            {
                App.ConsoleAndLogWriteLine("Error: the encryption keys are not in the right format. Data could not be decrypted.");
                return "";
            }
            // Si le stream ne permet pas la lecture
            catch (ArgumentException)
            {
                App.ConsoleAndLogWriteLine("Error: the stream does not allow reading. Data could not be decrypted.");
                return "";
            }
            // Si le programme n'a pas accès à assez de mémoire RAM
            catch (OutOfMemoryException)
            {
                App.ConsoleAndLogWriteLine("Error: the program does not have access to enough RAM. " +
                                           "Data could not be decrypted. Please try closing a few applications before trying again.");
                return "";
            }
            // Aucune idée de la raison
            catch (IOException)
            {
                App.ConsoleAndLogWriteLine("Error: I/O error occured while attempting to decrypt the data. Data could not be decrypted.");
                return "";
            }
        }
        
        
        // Fonction permettant d'encrypter et de stocker dans un fichier la clé principale de chiffrement
        private static void EncryptAndStoreMainKey(byte[] mainkey)
        {
            byte[] encryptedMainKeyBytes;
            
            try
            {
                // Utilisation de la DPAPI pour encrypter la clé en fonction de l'ordinateur et de la session Windows
                encryptedMainKeyBytes = ProtectedData.Protect(mainkey, null, DataProtectionScope.CurrentUser);
            }
            // Si par hasard la main key est "null"
            catch (ArgumentNullException)
            {
                App.ConsoleAndLogWriteLine("Error: The main key is null. Aborting storing operation.");
                return;
            }
            // Si l'encryption échoue
            catch (CryptographicException)
            {
                App.ConsoleAndLogWriteLine("Error: Could not encrypt main key.");
                return;
            }
            // Si l'OS ne supporte pas cet algorithme d'encryption
            catch (NotSupportedException)
            {
                App.ConsoleAndLogWriteLine($"Error: The system does not support the methods used to encrypt the main key.");
                return;
            }
            // Si le process n'a plus assez de RAM disponible pour l'encryption
            catch (OutOfMemoryException)
            {
                App.ConsoleAndLogWriteLine("Error: The application ran out of memory during the encryption of the main key.");
                return;
            }

            try
            {
                // Stockage de la clé encryptée dans un fichier
                File.WriteAllBytes("./emk", encryptedMainKeyBytes);
            }
            // Si le path est incorrect
            catch (ArgumentException)
            {
                App.ConsoleAndLogWriteLine($"Error: The path size for {Path.GetFullPath("./emk")} is invalid, or it contains characters that are not supported. " +
                                           $"Cancelling the current storage operation. Please try again.");
            }
            // Aucune idée de la raison de cette exception
            catch (IOException)
            {
                App.ConsoleAndLogWriteLine($"Error: an I/O exception occured while writing {Path.GetFullPath("./emk")}.");
            }
            // Si le fichier ne peut pas être accédé en écriture
            catch (UnauthorizedAccessException)
            {
                App.ConsoleAndLogWriteLine($"Error: Cannot write into {Path.GetFullPath("./emk")}. Cancelling the current storage operation.");
            }
            // Si le format du path n'est pas correct
            catch (NotSupportedException)
            {
                App.ConsoleAndLogWriteLine($"Error: The path format of {Path.GetFullPath("./emk")} is incorrect. Cancelling the current storage operation.");
            }
            // Si l'application n'a pas les autorisations nécessaires pour écrire
            catch (SecurityException)
            {
                App.ConsoleAndLogWriteLine($"Error: No permission to access and/or write to {Path.GetFullPath("./emk")}. Cancelling the current storage operation.");
            }
        }

        
        // Fonction permettant de récupérer la clé principale chiffrée et de la déchiffrer
        private static string RetrieveAndDecryptMainKey()
        {
            byte[] encryptedMainKeyBytes;
            try
            {
                // Lecture de la clé encryptée dans le fichier emk
                encryptedMainKeyBytes = File.ReadAllBytes("./emk");
            }
            // Si le path est incorrect
            catch (ArgumentException)
            {
                App.ConsoleAndLogWriteLine($"Error: The path size for {Path.GetFullPath("./emk")} is invalid, or it contains characters that are not supported. " +
                                           $"Cancelling the current reading operation. Please try again.");
                return "";
            }
            // Aucune idée de la raison de cette exception
            catch (IOException)
            {
                App.ConsoleAndLogWriteLine($"Error: an I/O exception occured while reading {Path.GetFullPath("./emk")}.");
                return "";
            }
            // Si le fichier ne peut pas être accédé en écriture
            catch (UnauthorizedAccessException)
            {
                App.ConsoleAndLogWriteLine($"Error: Cannot read into {Path.GetFullPath("./emk")}. Cancelling the current reading operation.");
                return "";
            }
            // Si le format du path n'est pas correct
            catch (NotSupportedException)
            {
                App.ConsoleAndLogWriteLine($"Error: The path format of {Path.GetFullPath("./emk")} is incorrect. Cancelling the current reading operation.");
                return "";
            }
            // Si l'application n'a pas les autorisations nécessaires pour écrire
            catch (SecurityException)
            {
                App.ConsoleAndLogWriteLine($"Error: No permission to access and/or read into {Path.GetFullPath("./emk")}. Cancelling the current reading operation.");
                return "";
            }

            byte[] mainKeyBytes;
            
            try
            {
                // Utilisation de DPAPI pour décrypter la clé
                mainKeyBytes = ProtectedData.Unprotect(encryptedMainKeyBytes, null, DataProtectionScope.CurrentUser);
            }
            // Si la donnée encryptée est null
            catch (ArgumentException)
            {
                App.ConsoleAndLogWriteLine("Error: the encrypted main key is null, cannot decrypt it.");
                return "";
            }
            // Si le décryptage échoue
            catch (CryptographicException)
            {
                App.ConsoleAndLogWriteLine("Error: the decryption of the main key failed.");
                return "";
            }
            // Si l'algorithme de décryptage n'est pas supporté
            catch (NotSupportedException)
            {
                App.ConsoleAndLogWriteLine("Error: the decryption algorithm is not supported by the current operating system. Could not decrypt the main key.");
                return "";
            }
            catch (OutOfMemoryException)
            {
                App.ConsoleAndLogWriteLine("Error: the program ran out of RAM while decrypting the main key. Could not decrypt it.");
                return "";
            }
            
            // Conversion en un string
            return Convert.ToBase64String(mainKeyBytes);
        }
        
        
        // Fonction permettant d'encrypter la clé ou l'iv à partir de la clé principale de chiffrement
        private static string EncryptKeyOrIv(string keyOrIv)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(RetrieveAndDecryptMainKey());
                aesAlg.IV = new byte[16]; // IV de 16 octets rempli de zéros

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                
                try
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(keyOrIv);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
                // Si le stream est invalide
                catch (ArgumentException)
                {
                    App.ConsoleAndLogWriteLine("Error: MemoryStream for encryption is invalid. Could not encrypt the data.");
                    return ""; // On retourne un string vide
                }
                // Aucune idée de la raison
                catch (IOException)
                {
                    return ""; // On retourne un string vide
                } 
                // Si le writer est fermé, ou son buffer est plein
                catch (ObjectDisposedException)
                {
                    App.ConsoleAndLogWriteLine("Error: Encryption writer was closed before encrypting the data, or its buffer is full. Could not encrypt the data.");
                    return ""; // On retourne un string vide
                }
                // Pas sûr d'avoir saisi la différence avec l'exception précédente
                catch (NotSupportedException)
                {
                    App.ConsoleAndLogWriteLine("Error: Encryption writer was closed before encrypting the data, or its buffer is full. Could not encrypt the data.");
                    return ""; // On retourne un string vide
                }
            }
        }

        
        // Fonction permettant de décrypter la clé ou l'iv à partir de la clé principale de chiffrement
        private static string DecryptKeyOrIv(string cipherText)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(RetrieveAndDecryptMainKey());
                aesAlg.IV = new byte[16]; // IV de 16 octets rempli de zéros

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                try
                {
                    using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                // Si le stream est 'null'
                catch (ArgumentNullException)
                {
                    App.ConsoleAndLogWriteLine("Error: decryption stream is null or the keys are null. Data could not be decrypted.");
                    return "";
                }
                // Si le format des clés est invalide
                catch (FormatException)
                {
                    App.ConsoleAndLogWriteLine("Error: the encryption keys are not in the right format. Data could not be decrypted.");
                    return "";
                }
                // Si le stream ne permet pas la lecture
                catch (ArgumentException)
                {
                    App.ConsoleAndLogWriteLine("Error: the stream does not allow reading. Data could not be decrypted.");
                    return "";
                }
                // Si le programme n'a pas accès à assez de mémoire RAM
                catch (OutOfMemoryException)
                {
                    App.ConsoleAndLogWriteLine("Error: the program does not have access to enough RAM. " +
                                               "Data could not be decrypted. Please try closing a few applications before trying again.");
                    return "";
                }
                // Aucune idée de la raison
                catch (IOException)
                {
                    App.ConsoleAndLogWriteLine("Error: I/O error occured while attempting to decrypt the data. Data could not be decrypted.");
                    return "";
                }
            }
        }
        
        
        // Fonction permettant de générer des clés aléatoires
        private static string GenerateRandomKey(int length)
        {
            // Tableau contenant tous les caractères admissibles dans une clé d'encryption
            var chars ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

            // Si la longueur est de 0 ou moins, on retourne un string vide
            if (length <= 0) return "";
            
            var result = new StringBuilder(length);
            var random = new Random();

            // Tant que la taille de la clé voulue n'est pas atteinte
            for (var i = 0; i < length; i++)
            {
                // On rajoute au string un caractère aléatoire depuis la table des caractères admissibles
                result.Append(chars[random.Next(chars.Length)]);
            }

            // Renvoi de la clé générée sous forme de string
            return result.ToString();
        }
        
        
        // ----- GESTION DES LIENS HYPERTEXTE -----
        // Fonction gérant le clic sur un lien hypertexte
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
            }
            catch (InvalidOperationException)
            {
                App.ConsoleAndLogWriteLine("Error: cannot redirect to the clicked link.");
            }
            catch (ArgumentException)
            {
                App.ConsoleAndLogWriteLine("Error: cannot redirect to the clicked link.");
            }
            catch (PlatformNotSupportedException)
            {
                App.ConsoleAndLogWriteLine("Error: cannot redirect to the clicked link.");
            }
            
            e.Handled = true;
        }

        
        // Fonction permettant d'effectuer des actions quand une touche spécifique du clavier est appuyée
        private void SettingsWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Si on appuie sur échap, on ferme la fenêtre et on annule les modifications
                case Key.Escape:
                    UpdateWindowContents(); // Restauration des paramètres précédents dans la fenêtre de paramétrage
                    Hide(); // Masquage de la fenêtre de paramétrage
                    break;
                
                // Si on appuie sur entrée, on sauvegarde les modifications et on ferme
                case Key.Enter:
                    // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
                    EnableDeeplTranslation = (bool) EnableTranslationCheckBox.IsChecked!;
                    DeeplKey = EncryptStringToBytes(DeeplApiKeyTextBox.Text);
                    TranslationLang = TranslationLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
                    RemoveUnusedGroupAddresses = (bool) RemoveUnusedAddressesCheckBox.IsChecked!;
                    EnableLightTheme = LightThemeComboBoxItem.IsSelected;
                    AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            
                    // Sauvegarde des paramètres dans le fichier appSettings
                    App.ConsoleAndLogWriteLine($"Saving application settings at {Path.GetFullPath("./appSettings")}");
                    SaveSettings();
                    App.ConsoleAndLogWriteLine("Settings saved successfully");
            
                    // Mise à jour éventuellement du contenu pour update la langue du menu
                    UpdateWindowContents();
            
                    // Masquage de la fenêtre de paramètres
                    Hide();
                    break;
            }
        }
    }
}