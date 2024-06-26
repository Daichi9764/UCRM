﻿using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
            lightThemeComboBoxItem.IsSelected = EnableLightTheme;
            darkThemeComboBoxItem.IsSelected = !EnableLightTheme;
            
            // Traduction du menu settings
            switch (AppLang)
            {
                // Arabe
                case "AR":
                    Title = "الإعدادات";

                    EnableTranslationCheckBox.Content = "تفعيل الترجمة";
                    DeeplApiKeyText.Text = "مفتاح API لـ DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(انقر هنا للحصول عليها مجانًا)");

                    TranslationLanguageComboBoxText.Text = "لغة الترجمة:";

                    RemoveUnusedAddressesCheckBox.Content = "إزالة العناوين غير المستخدمة";

                    ThemeTextBox.Text = "السمة:";
                    lightThemeComboBoxItem.Content = "فاتح (افتراضي)";
                    darkThemeComboBoxItem.Content = "داكن";

                    AppLanguageTextBlock.Text = "لغة التطبيق:";

                    SaveButton.Content = "حفظ";
                    CancelButton.Content = "إلغاء";
                    break;

                // Bulgare
                case "BG":
                    Title = "Настройки";

                    EnableTranslationCheckBox.Content = "Активиране на превод";
                    DeeplApiKeyText.Text = "API ключ за DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Кликнете тук, за да го получите безплатно)");

                    TranslationLanguageComboBoxText.Text = "Език за превод:";

                    RemoveUnusedAddressesCheckBox.Content = "Премахване на неизползвани адреси";

                    ThemeTextBox.Text = "Тема:";
                    lightThemeComboBoxItem.Content = "Светла (по подразбиране)";
                    darkThemeComboBoxItem.Content = "Тъмна";

                    AppLanguageTextBlock.Text = "Език на приложението:";

                    SaveButton.Content = "Запазване";
                    CancelButton.Content = "Отказ";
                    break;

                // Tchèque
                case "CS":
                    Title = "Nastavení";

                    EnableTranslationCheckBox.Content = "Povolit překlad";
                    DeeplApiKeyText.Text = "API klíč pro DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikněte zde pro získání zdarma)");

                    TranslationLanguageComboBoxText.Text = "Jazyk překladu:";

                    RemoveUnusedAddressesCheckBox.Content = "Odstranit nepoužívané adresy";

                    ThemeTextBox.Text = "Téma:";
                    lightThemeComboBoxItem.Content = "Světlé (výchozí)";
                    darkThemeComboBoxItem.Content = "Tmavé";

                    AppLanguageTextBlock.Text = "Jazyk aplikace:";

                    SaveButton.Content = "Uložit";
                    CancelButton.Content = "Zrušit";
                    break;

                // Danois
                case "DA":
                    Title = "Indstillinger";

                    EnableTranslationCheckBox.Content = "Aktiver oversættelse";
                    DeeplApiKeyText.Text = "DeepL API nøgle :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik her for at få det gratis)");

                    TranslationLanguageComboBoxText.Text = "Oversættelsessprog:";

                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrugte adresser";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Lys (standard)";
                    darkThemeComboBoxItem.Content = "Mørk";

                    AppLanguageTextBlock.Text = "App sprog:";

                    SaveButton.Content = "Gemme";
                    CancelButton.Content = "Annuller";
                    break;

                // Allemand
                case "DE":
                    Title = "Einstellungen";

                    EnableTranslationCheckBox.Content = "Übersetzung aktivieren";
                    DeeplApiKeyText.Text = "DeepL API-Schlüssel :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klicken Sie hier, um es kostenlos zu erhalten)");

                    TranslationLanguageComboBoxText.Text = "Übersetzungssprache:";

                    RemoveUnusedAddressesCheckBox.Content = "Unbenutzte Adressen entfernen";

                    ThemeTextBox.Text = "Thema:";
                    lightThemeComboBoxItem.Content = "Hell (Standard)";
                    darkThemeComboBoxItem.Content = "Dunkel";

                    AppLanguageTextBlock.Text = "App-Sprache:";

                    SaveButton.Content = "Speichern";
                    CancelButton.Content = "Abbrechen";
                    break;

                // Grec
                case "EL":
                    Title = "Ρυθμίσεις";

                    EnableTranslationCheckBox.Content = "Ενεργοποίηση μετάφρασης";
                    DeeplApiKeyText.Text = "Κλειδί API για DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Κάντε κλικ εδώ για να το αποκτήσετε δωρεάν)");

                    TranslationLanguageComboBoxText.Text = "Γλώσσα μετάφρασης:";

                    RemoveUnusedAddressesCheckBox.Content = "Κατάργηση αχρησιμοποίητων διευθύνσεων";

                    ThemeTextBox.Text = "Θέμα:";
                    lightThemeComboBoxItem.Content = "Φωτεινό (προεπιλογή)";
                    darkThemeComboBoxItem.Content = "Σκούρο";

                    AppLanguageTextBlock.Text = "Γλώσσα εφαρμογής:";

                    SaveButton.Content = "Αποθήκευση";
                    CancelButton.Content = "Ακύρωση";
                    break;

                // Anglais
                case "EN":
                    Title = "Settings";

                    EnableTranslationCheckBox.Content = "Enable Translation";
                    DeeplApiKeyText.Text = "DeepL API Key :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Click here to get one for free)");

                    TranslationLanguageComboBoxText.Text = "Translation Language:";

                    RemoveUnusedAddressesCheckBox.Content = "Remove Unused Addresses";

                    ThemeTextBox.Text = "Theme:";
                    lightThemeComboBoxItem.Content = "Light (default)";
                    darkThemeComboBoxItem.Content = "Dark";

                    AppLanguageTextBlock.Text = "Application Language:";

                    SaveButton.Content = "Save";
                    CancelButton.Content = "Cancel";
                    break;

                // Espagnol
                case "ES":
                    Title = "Configuración";

                    EnableTranslationCheckBox.Content = "Habilitar traducción";
                    DeeplApiKeyText.Text = "Clave API de DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Haga clic aquí para obtener una gratis)");

                    TranslationLanguageComboBoxText.Text = "Idioma de traducción:";

                    RemoveUnusedAddressesCheckBox.Content = "Eliminar direcciones no utilizadas";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Claro (predeterminado)";
                    darkThemeComboBoxItem.Content = "Oscuro";

                    AppLanguageTextBlock.Text = "Idioma de la aplicación:";

                    SaveButton.Content = "Guardar";
                    CancelButton.Content = "Cancelar";
                    break;

                // Estonien
                case "ET":
                    Title = "Seaded";

                    EnableTranslationCheckBox.Content = "Luba tõlge";
                    DeeplApiKeyText.Text = "DeepL API võti :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klõpsake siin, et saada see tasuta)");

                    TranslationLanguageComboBoxText.Text = "Tõlke keel:";

                    RemoveUnusedAddressesCheckBox.Content = "Eemaldage kasutamata aadressid";

                    ThemeTextBox.Text = "Teema:";
                    lightThemeComboBoxItem.Content = "Hele (vaikimisi)";
                    darkThemeComboBoxItem.Content = "Tume";

                    AppLanguageTextBlock.Text = "Rakenduse keel:";

                    SaveButton.Content = "Salvesta";
                    CancelButton.Content = "Tühista";
                    break;

                // Finnois
                case "FI":
                    Title = "Asetukset";

                    EnableTranslationCheckBox.Content = "Ota käännös käyttöön";
                    DeeplApiKeyText.Text = "DeepL API-avain :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Napsauta tästä saadaksesi sen ilmaiseksi)");

                    TranslationLanguageComboBoxText.Text = "Käännöskieli:";

                    RemoveUnusedAddressesCheckBox.Content = "Poista käyttämättömät osoitteet";

                    ThemeTextBox.Text = "Teema:";
                    lightThemeComboBoxItem.Content = "Vaalea (oletus)";
                    darkThemeComboBoxItem.Content = "Tumma";

                    AppLanguageTextBlock.Text = "Sovelluksen kieli:";

                    SaveButton.Content = "Tallenna";
                    CancelButton.Content = "Peruuta";
                    break;

                // Hongrois
                case "HU":
                    Title = "Beállítások";

                    EnableTranslationCheckBox.Content = "Fordítás engedélyezése";
                    DeeplApiKeyText.Text = "DeepL API kulcs :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kattintson ide az ingyenes eléréshez)");

                    TranslationLanguageComboBoxText.Text = "Fordítási nyelv:";

                    RemoveUnusedAddressesCheckBox.Content = "Nem használt címek eltávolítása";

                    ThemeTextBox.Text = "Téma:";
                    lightThemeComboBoxItem.Content = "Világos (alapértelmezett)";
                    darkThemeComboBoxItem.Content = "Sötét";

                    AppLanguageTextBlock.Text = "Alkalmazás nyelve:";

                    SaveButton.Content = "Mentés";
                    CancelButton.Content = "Mégse";
                    break;

                // Indonésien
                case "ID":
                    Title = "Pengaturan";

                    EnableTranslationCheckBox.Content = "Aktifkan Terjemahan";
                    DeeplApiKeyText.Text = "Kunci API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik di sini untuk mendapatkannya secara gratis)");

                    TranslationLanguageComboBoxText.Text = "Bahasa terjemahan:";

                    RemoveUnusedAddressesCheckBox.Content = "Hapus Alamat yang Tidak Digunakan";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Terang (default)";
                    darkThemeComboBoxItem.Content = "Gelap";

                    AppLanguageTextBlock.Text = "Bahasa Aplikasi:";

                    SaveButton.Content = "Simpan";
                    CancelButton.Content = "Batal";
                    break;

                // Italien
                case "IT":
                    Title = "Impostazioni";

                    EnableTranslationCheckBox.Content = "Abilita Traduzione";
                    DeeplApiKeyText.Text = "Chiave API di DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Clicca qui per ottenerlo gratuitamente)");

                    TranslationLanguageComboBoxText.Text = "Lingua di traduzione:";

                    RemoveUnusedAddressesCheckBox.Content = "Rimuovi indirizzi inutilizzati";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Chiaro (predefinito)";
                    darkThemeComboBoxItem.Content = "Scuro";

                    AppLanguageTextBlock.Text = "Lingua dell'applicazione:";

                    SaveButton.Content = "Salva";
                    CancelButton.Content = "Annulla";
                    break;

                // Japonais
                case "JA":
                    Title = "設定";

                    EnableTranslationCheckBox.Content = "翻訳を有効にする";
                    DeeplApiKeyText.Text = "DeepL APIキー :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(無料で取得するにはここをクリック)");

                    TranslationLanguageComboBoxText.Text = "翻訳言語:";

                    RemoveUnusedAddressesCheckBox.Content = "未使用のアドレスを削除";

                    ThemeTextBox.Text = "テーマ:";
                    lightThemeComboBoxItem.Content = "ライト (デフォルト)";
                    darkThemeComboBoxItem.Content = "ダーク";

                    AppLanguageTextBlock.Text = "アプリケーションの言語:";

                    SaveButton.Content = "保存";
                    CancelButton.Content = "キャンセル";
                    break;

                // Coréen
                case "KO":
                    Title = "설정";

                    EnableTranslationCheckBox.Content = "번역 활성화";
                    DeeplApiKeyText.Text = "DeepL API 키 :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(무료로 받으려면 여기를 클릭하세요)");

                    TranslationLanguageComboBoxText.Text = "번역 언어:";

                    RemoveUnusedAddressesCheckBox.Content = "사용되지 않는 주소 제거";

                    ThemeTextBox.Text = "테마:";
                    lightThemeComboBoxItem.Content = "라이트 (기본값)";
                    darkThemeComboBoxItem.Content = "다크";

                    AppLanguageTextBlock.Text = "응용 프로그램 언어:";

                    SaveButton.Content = "저장";
                    CancelButton.Content = "취소";
                    break;

                // Lituanien
                case "LT":
                    Title = "Nustatymai";

                    EnableTranslationCheckBox.Content = "Įjungti vertimą";
                    DeeplApiKeyText.Text = "DeepL API raktas :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Spustelėkite čia, kad gautumėte nemokamai)");

                    TranslationLanguageComboBoxText.Text = "Vertimo kalba:";

                    RemoveUnusedAddressesCheckBox.Content = "Pašalinti nenaudojamus adresus";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Šviesus (numatytasis)";
                    darkThemeComboBoxItem.Content = "Tamsus";

                    AppLanguageTextBlock.Text = "Programos kalba:";

                    SaveButton.Content = "Išsaugoti";
                    CancelButton.Content = "Atšaukti";
                    break;

                // Letton
                case "LV":
                    Title = "Iestatījumi";

                    EnableTranslationCheckBox.Content = "Iespējot tulkošanu";
                    DeeplApiKeyText.Text = "DeepL API atslēga :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikšķiniet šeit, lai to iegūtu bez maksas)");

                    TranslationLanguageComboBoxText.Text = "Tulkošanas valoda:";

                    RemoveUnusedAddressesCheckBox.Content = "Noņemt neizmantotās adreses";

                    ThemeTextBox.Text = "Tēma:";
                    lightThemeComboBoxItem.Content = "Gaišs (noklusējuma)";
                    darkThemeComboBoxItem.Content = "Tumšs";

                    AppLanguageTextBlock.Text = "Lietotnes valoda:";

                    SaveButton.Content = "Saglabāt";
                    CancelButton.Content = "Atcelt";
                    break;

                // Bokmål norvégien
                case "NB":
                    Title = "Innstillinger";

                    EnableTranslationCheckBox.Content = "Aktiver oversettelse";
                    DeeplApiKeyText.Text = "DeepL API-nøkkel :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikk her for å få en gratis)");

                    TranslationLanguageComboBoxText.Text = "Oversettelsesspråk:";

                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrukte adresser";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Lys (standard)";
                    darkThemeComboBoxItem.Content = "Mørk";

                    AppLanguageTextBlock.Text = "App-språk:";

                    SaveButton.Content = "Lagre";
                    CancelButton.Content = "Avbryt";
                    break;

                // Néerlandais
                case "NL":
                    Title = "Instellingen";

                    EnableTranslationCheckBox.Content = "Vertaling inschakelen";
                    DeeplApiKeyText.Text = "DeepL API-sleutel :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik hier om er gratis een te krijgen)");

                    TranslationLanguageComboBoxText.Text = "Vertaling Taal:";

                    RemoveUnusedAddressesCheckBox.Content = "Ongebruikte adressen verwijderen";

                    ThemeTextBox.Text = "Thema:";
                    lightThemeComboBoxItem.Content = "Licht (standaard)";
                    darkThemeComboBoxItem.Content = "Donker";

                    AppLanguageTextBlock.Text = "App-taal:";

                    SaveButton.Content = "Opslaan";
                    CancelButton.Content = "Annuleren";
                    break;

                // Polonais
                case "PL":
                    Title = "Ustawienia";

                    EnableTranslationCheckBox.Content = "Włącz tłumaczenie";
                    DeeplApiKeyText.Text = "Klucz API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknij tutaj, aby otrzymać za darmo)");

                    TranslationLanguageComboBoxText.Text = "Język tłumaczenia:";

                    RemoveUnusedAddressesCheckBox.Content = "Usuń nieużywane adresy";

                    ThemeTextBox.Text = "Temat:";
                    lightThemeComboBoxItem.Content = "Jasny (domyślny)";
                    darkThemeComboBoxItem.Content = "Ciemny";

                    AppLanguageTextBlock.Text = "Język aplikacji:";

                    SaveButton.Content = "Zapisz";
                    CancelButton.Content = "Anuluj";
                    break;

                // Portugais
                case "PT":
                    Title = "Configurações";

                    EnableTranslationCheckBox.Content = "Ativar Tradução";
                    DeeplApiKeyText.Text = "Chave API do DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Clique aqui para obter gratuitamente)");

                    TranslationLanguageComboBoxText.Text = "Idioma de Tradução:";

                    RemoveUnusedAddressesCheckBox.Content = "Remover Endereços Não Utilizados";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Claro (padrão)";
                    darkThemeComboBoxItem.Content = "Escuro";

                    AppLanguageTextBlock.Text = "Idioma do Aplicativo:";

                    SaveButton.Content = "Salvar";
                    CancelButton.Content = "Cancelar";
                    break;

                // Roumain
                case "RO":
                    Title = "Setări";

                    EnableTranslationCheckBox.Content = "Activați traducerea";
                    DeeplApiKeyText.Text = "Cheie API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Faceți clic aici pentru a obține gratuit)");

                    TranslationLanguageComboBoxText.Text = "Limba traducerii:";

                    RemoveUnusedAddressesCheckBox.Content = "Eliminați adresele neutilizate";

                    ThemeTextBox.Text = "Temă:";
                    lightThemeComboBoxItem.Content = "Luminos (implicit)";
                    darkThemeComboBoxItem.Content = "Întunecat";

                    AppLanguageTextBlock.Text = "Limba aplicației:";

                    SaveButton.Content = "Salvați";
                    CancelButton.Content = "Anulați";
                    break;

                // Russe
                case "RU":
                    Title = "Настройки";

                    EnableTranslationCheckBox.Content = "Включить перевод";
                    DeeplApiKeyText.Text = "API-ключ DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Нажмите здесь, чтобы получить бесплатно)");

                    TranslationLanguageComboBoxText.Text = "Язык перевода:";

                    RemoveUnusedAddressesCheckBox.Content = "Удалить неиспользуемые адреса";

                    ThemeTextBox.Text = "Тема:";
                    lightThemeComboBoxItem.Content = "Светлый (по умолчанию)";
                    darkThemeComboBoxItem.Content = "Темный";

                    AppLanguageTextBlock.Text = "Язык приложения:";

                    SaveButton.Content = "Сохранить";
                    CancelButton.Content = "Отмена";
                    break;

                // Slovaque
                case "SK":
                    Title = "Nastavenia";

                    EnableTranslationCheckBox.Content = "Povoliť preklad";
                    DeeplApiKeyText.Text = "DeepL API kľúč :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite sem a získajte zadarmo)");

                    TranslationLanguageComboBoxText.Text = "Prekladací jazyk:";

                    RemoveUnusedAddressesCheckBox.Content = "Odstrániť nepoužívané adresy";

                    ThemeTextBox.Text = "Téma:";
                    lightThemeComboBoxItem.Content = "Svetlá (predvolená)";
                    darkThemeComboBoxItem.Content = "Tmavá";

                    AppLanguageTextBlock.Text = "Jazyk aplikácie:";

                    SaveButton.Content = "Uložiť";
                    CancelButton.Content = "Zrušiť";
                    break;

                // Slovène
                case "SL":
                    Title = "Nastavitve";

                    EnableTranslationCheckBox.Content = "Omogoči prevod";
                    DeeplApiKeyText.Text = "DeepL API ključ :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite tukaj za brezplačno pridobitev)");

                    TranslationLanguageComboBoxText.Text = "Jezik prevajanja:";

                    RemoveUnusedAddressesCheckBox.Content = "Odstrani neuporabljene naslove";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Svetla (privzeta)";
                    darkThemeComboBoxItem.Content = "Temna";

                    AppLanguageTextBlock.Text = "Jezik aplikacije:";

                    SaveButton.Content = "Shrani";
                    CancelButton.Content = "Prekliči";
                    break;

                // Suédois
                case "SV":
                    Title = "Inställningar";

                    EnableTranslationCheckBox.Content = "Aktivera översättning";
                    DeeplApiKeyText.Text = "DeepL API-nyckel :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klicka här för att få det gratis)");

                    TranslationLanguageComboBoxText.Text = "Översättningsspråk:";

                    RemoveUnusedAddressesCheckBox.Content = "Ta bort oanvända adresser";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Ljus (standard)";
                    darkThemeComboBoxItem.Content = "Mörk";

                    AppLanguageTextBlock.Text = "App-språk:";

                    SaveButton.Content = "Spara";
                    CancelButton.Content = "Avbryt";
                    break;

                // Turc
                case "TR":
                    Title = "Ayarlar";

                    EnableTranslationCheckBox.Content = "Çeviriyi Etkinleştir";
                    DeeplApiKeyText.Text = "DeepL API Anahtarı :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Ücretsiz almak için buraya tıklayın)");

                    TranslationLanguageComboBoxText.Text = "Çeviri Dili:";

                    RemoveUnusedAddressesCheckBox.Content = "Kullanılmayan Adresleri Kaldır";

                    ThemeTextBox.Text = "Tema:";
                    lightThemeComboBoxItem.Content = "Açık (varsayılan)";
                    darkThemeComboBoxItem.Content = "Koyu";

                    AppLanguageTextBlock.Text = "Uygulama Dili:";

                    SaveButton.Content = "Kaydet";
                    CancelButton.Content = "İptal";
                    break;

                // Ukrainien
                case "UK":
                    Title = "Налаштування";

                    EnableTranslationCheckBox.Content = "Увімкнути переклад";
                    DeeplApiKeyText.Text = "Ключ API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Натисніть тут, щоб отримати безкоштовно)");

                    TranslationLanguageComboBoxText.Text = "Мова перекладу:";

                    RemoveUnusedAddressesCheckBox.Content = "Видалити невикористані адреси";

                    ThemeTextBox.Text = "Тема:";
                    lightThemeComboBoxItem.Content = "Світла (за замовчуванням)";
                    darkThemeComboBoxItem.Content = "Темна";

                    AppLanguageTextBlock.Text = "Мова програми:";

                    SaveButton.Content = "Зберегти";
                    CancelButton.Content = "Скасувати";
                    break;

                // Chinois
                case "ZH":
                    Title = "设置";

                    EnableTranslationCheckBox.Content = "启用翻译";
                    DeeplApiKeyText.Text = "DeepL API密钥 :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(点击这里免费获取)");

                    TranslationLanguageComboBoxText.Text = "翻译语言:";

                    RemoveUnusedAddressesCheckBox.Content = "删除未使用的地址";

                    ThemeTextBox.Text = "主题:";
                    lightThemeComboBoxItem.Content = "浅色 (默认)";
                    darkThemeComboBoxItem.Content = "深色";

                    AppLanguageTextBlock.Text = "应用语言:";

                    SaveButton.Content = "保存";
                    CancelButton.Content = "取消";
                    break;

                // Langue par défaut: le français
                default:
                    Title = "Paramètres";

                    EnableTranslationCheckBox.Content = "Activer la traduction";
                    DeeplApiKeyText.Text = "Clé API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Cliquez ici pour en obtenir une gratuitement)");

                    TranslationLanguageComboBoxText.Text = "Langue de traduction:";

                    RemoveUnusedAddressesCheckBox.Content = "Supprimer les adresses de groupe non associées";

                    ThemeTextBox.Text = "Thème:";
                    lightThemeComboBoxItem.Content = "Clair (par défaut)";
                    darkThemeComboBoxItem.Content = "Sombre";

                    AppLanguageTextBlock.Text = "Langue de l'application:";

                    SaveButton.Content = "Sauvegarder";
                    CancelButton.Content = "Annuler";
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
            EnableLightTheme = lightThemeComboBoxItem.IsSelected;
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
    }
}