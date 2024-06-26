using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable ConvertToUsingDeclaration

namespace KNXBoostDesktop
{
    public partial class SettingsWindow
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        private static readonly string SettingsPath = $"./appSettings"; // Chemin du fichier paramètres
        
        public new bool DialogResult { get; private set; } // Permet de savoir si l'utilisateur a entré l'information voulue ou s'il a fermé la fenêtre/annulé

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
        public SettingsWindow()
        {
            // Initialement, l'application dispose des paramètres par défaut, qui seront potentiellement modifiés après par
            // la lecture du fichier settings. Cela permet d'éviter un crash si le fichier 
            EnableDeeplTranslation = false;
            TranslationLang = "FR";
            RemoveUnusedGroupAddresses = false;
            EnableLightTheme = true;
            AppLang = "FR";
            DeeplKey = Convert.FromBase64String("");

            
            _emkFileExists = File.Exists("./emk");
            
            if (!_emkFileExists)
            {
                App.ConsoleAndLogWriteLine(
                    "Main key file not found. Generating a new one and resetting the settings file...");

                // Génération aléatoire d'une main key
                EncryptAndStoreMainKey(Convert.FromBase64String(GenerateRandomKey(32)));
                
                if (File.Exists("./appSettings")) File.Delete("./appSettings");
                if (File.Exists("./ei")) File.Delete("./ei");
                if (File.Exists("./ek")) File.Delete("./ek");
            }
            
            // Si le fichier de paramétrage n'existe pas, on le crée
            // Note: comme File.Create ouvre un stream vers le fichier à la création, on le ferme directement.
            if (!File.Exists(SettingsPath)) File.Create(SettingsPath).Close();
            
            StreamReader reader = new(SettingsPath);
            
            try
            {
                while (reader.ReadLine() is { } line)
                {
                    // Créer un HashSet avec tous les codes de langue valides
                    HashSet<string> validLanguageCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "AR", "BG", "CS", "DA", "DE", "EL", "EN", "ES", "ET", "FI",
                        "HU", "ID", "IT", "JA", "KO", "LT", "LV", "NB", "NL", "PL",
                        "PT", "RO", "RU", "SK", "SL", "SV", "TR", "UK", "ZH"
                    };
                    
                    string[] parts = line.Split(':');
                    
                    // S'il n'y a pas de : ou qu'il n'y a rien après les deux points, on skip car la ligne nous intéresse pas
                    if (parts.Length < 2) continue;

                    string key = parts[0].Trim().ToLower();
                    string value = parts[1].Trim();
                    
                    switch (key)
                    {
                        case "enable deepl translation":
                            try
                            {
                                EnableDeeplTranslation = bool.Parse(value);
                            }
                            // Si l'utilisateur n'a pas écrit dans le fichier paramètres un string s'apparentant à true ou false
                            catch (FormatException)
                            {
                                App.ConsoleAndLogWriteLine("Could not parse boolean value of the activation of the deepL translation, restoring default value");
                            }
                            break;

                        case "deepl key [encrypted]":
                            // On récupère la clé DeepL encryptée
                            DeeplKey = Convert.FromBase64String(value);
                            break;

                        case "translation lang":
                            // Vérifier si value est un code de langue valide, si elle est valide on assigne la valeur, sinon on met la langue par défaut
                            TranslationLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
                            break;

                        case "remove unused group addresses":
                            try
                            {
                                RemoveUnusedGroupAddresses = bool.Parse(value);
                            }
                            // Si l'utilisateur n'a pas écrit dans le fichier paramètres un string s'apparentant à true ou false
                            catch (FormatException)
                            {
                                App.ConsoleAndLogWriteLine("Could not parse boolean value of the activation of the function to remove unused group addresses, restoring default value");
                            }
                            break;

                        case "theme":
                            // Si la valeur n'est pas dark, on mettra toujours le thème clair (en cas d'erreur, ou si la value est "light")
                            EnableLightTheme = !value.Equals("dark", StringComparison.CurrentCultureIgnoreCase);
                            break;

                        case "application language":
                            // Vérifier si value est un code de langue valide, si elle est valide on assigne la valeur, sinon on met la langue par défaut
                            AppLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
                            break;
                    }
                }
                
            }
            finally
            {
                reader.Close();
                SaveSettings();
            }
            
            InitializeComponent();

            UpdateWindowContents();

        }
        
        
        private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            DialogResult = false;
            UpdateWindowContents();
            Hide();
        }


        private void SaveSettings()
        {
            StreamWriter writer = new StreamWriter(SettingsPath);
            
            try
            {
                writer.WriteLine("-----------------------------------------------------------------------------------------");
                writer.WriteLine("|                                KNXBOOSTDESKTOP SETTINGS                               |");
                writer.WriteLine("-----------------------------------------------------------------------------------------");
                
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
                
                writer.WriteLine("-----------------------------------------------------------------------------------------");
                writer.WriteLine("Available languages: AR, BG, CS, DA, DE, EL, EN, ES, ET, FI, FR, HU, ID, IT, JA, KO, LT, LV, NB, NL, PL, PT, RO, RU, SK, SL, SV, TR, UK, ZH\n");
                writer.Write("/!\\ WARNING:\nAny value that you modify in this file and that is not correct will be replaced by a default value.");
            }
            finally
            {
                writer.Close();
            }
        }

        
        private void UpdateWindowContents()
        {
            EnableTranslationCheckBox.IsChecked = EnableDeeplTranslation;
            
            if (_emkFileExists) DeeplApiKeyTextBox.Text = DecryptStringFromBytes(DeeplKey);

            // Si la langue de traduction ou de l'application n'est pas le français, on désélectionne le français dans le combobox
            // pour sélectionner la langue voulue
            if ((TranslationLang != "FR")||(AppLang != "FR"))
            {
                FrTranslationComboBoxItem.IsSelected = (TranslationLang == "FR");
                FrAppLanguageComboBoxItem.IsSelected = (AppLang == "FR");
    
                // Sélection du langage de traduction
                foreach (ComboBoxItem item in TranslationLanguageComboBox.Items)
                {
                    if (item.Content.ToString()!.StartsWith(TranslationLang))
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
                
                // Sélection du langage de l'application
                foreach (ComboBoxItem item in AppLanguageComboBox.Items)
                {
                    if (item.Content.ToString()!.StartsWith(AppLang))
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }

            RemoveUnusedAddressesCheckBox.IsChecked = RemoveUnusedGroupAddresses;

            lightThemeComboBoxItem.IsSelected = EnableLightTheme;
            darkThemeComboBoxItem.IsSelected = !EnableLightTheme;
        }

        
        // ----- GESTION DES BOUTONS -----
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
            EnableDeeplTranslation = (bool) EnableTranslationCheckBox.IsChecked!;
            DeeplKey = EncryptStringToBytes(DeeplApiKeyTextBox.Text);
            TranslationLang = TranslationLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];

            RemoveUnusedGroupAddresses = (bool) RemoveUnusedAddressesCheckBox.IsChecked!;

            EnableLightTheme = lightThemeComboBoxItem.IsSelected;
            
            AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            
            App.ConsoleAndLogWriteLine($"Saving application settings at {Path.GetFullPath(SettingsPath)}");
            SaveSettings();
            App.ConsoleAndLogWriteLine("Settings saved successfully");
            DialogResult = true;
            
            Hide();
        }

        
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateWindowContents();
            Hide();
        }

        
        // ----- GESTION DE DES CASES A COCHER -----
        private void EnableTranslation(object sender, RoutedEventArgs e)
        {
            DeeplApiKeyText.Visibility = Visibility.Visible;
            DeeplApiKeyTextBox.Visibility = Visibility.Visible;

            TranslationLanguageComboBoxText.Visibility = Visibility.Visible;
            TranslationLanguageComboBox.Visibility = Visibility.Visible;

            Height += 110;
        }

        
        private void DisableTranslation(object sender, RoutedEventArgs e)
        {
            DeeplApiKeyText.Visibility = Visibility.Collapsed;
            DeeplApiKeyTextBox.Visibility = Visibility.Collapsed;

            TranslationLanguageComboBoxText.Visibility = Visibility.Collapsed;
            TranslationLanguageComboBox.Visibility = Visibility.Collapsed;

            Height -= 110;
        }
        
        
        // ----- GESTION DE L'ENCRYPTION -----
        // Fonction permettant d'encrypter un string donné avec une clé et un iv créés dynamiquement
        // Attention: cette fonction permet de stocker UN SEUL string (ici la clé deepL) à la fois. Si vous encryptez un autre string,
        // le combo clé/iv pour le premier string sera perdu.
        private Byte[] EncryptStringToBytes(string plainText)
        {
            // Générer une nouvelle clé et IV
            using (Aes aesAlg = Aes.Create())
            {
                string newKey = Convert.ToBase64String(aesAlg.Key);
                string newIv = Convert.ToBase64String(aesAlg.IV);

                // Chiffrer les nouvelles clés et IV avec la clé principale
                string encryptedKey = EncryptKeyOrIv(newKey);
                string encryptedIv = EncryptKeyOrIv(newIv);

                // Stocker les clés chiffrées (par exemple dans un fichier ou une base de données)
                File.WriteAllText("./ek", encryptedKey);
                File.WriteAllText("./ei", encryptedIv);

                // Chiffrer les données avec les nouvelles clés et IV
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        
        // Fonction permettant de décrypter un string donné à partir de la clé et de l'iv chiffrés
        private string DecryptStringFromBytes(Byte[] encryptedString)
        {
            if (!File.Exists("./ek") || !File.Exists("./ei")) return "";
            
            // Lire les clés chiffrées
            string encryptedKey = File.ReadAllText("./ek");
            string encryptedIv = File.ReadAllText("./ei");

            // Déchiffrer les clés et IV avec la clé principale
            string decryptedKey = DecryptKeyOrIv(encryptedKey);
            string decryptedIv = DecryptKeyOrIv(encryptedIv);

            // Déchiffrer les données avec les clés et IV déchiffrés
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(decryptedKey);
                aesAlg.IV = Convert.FromBase64String(decryptedIv);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedString))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        
        
        // Fonction permettant d'encrypter et de stocker dans un fichier la clé principale de chiffrement
        private static void EncryptAndStoreMainKey(byte[] mainkey)
        {
            // Use DPAPI to encrypt the key
            byte[] encryptedMainKeyBytes = ProtectedData.Protect(mainkey, null, DataProtectionScope.CurrentUser);
            
            // Store the encrypted key in a file
            File.WriteAllBytes("./emk", encryptedMainKeyBytes);
        }

        
        // Fonction permettant de récupérer la clé principale chiffrée et de la déchiffrer
        private static string RetrieveAndDecryptMainKey()
        {
            // Read the encrypted key from the file
            byte[] encryptedMainKeyBytes = File.ReadAllBytes("./emk");

            // Use DPAPI to decrypt the key
            byte[] mainKeyBytes = ProtectedData.Unprotect(encryptedMainKeyBytes, null, DataProtectionScope.CurrentUser);

            // Convert the byte array back to a string
            return Convert.ToBase64String(mainKeyBytes);
        }
        
        
        // Fonction permettant d'encrypter la clé ou l'iv à partir de la clé principale de chiffrement
        private static string EncryptKeyOrIv(string keyOrIv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(RetrieveAndDecryptMainKey());
                aesAlg.IV = new byte[16]; // IV de 16 octets rempli de zéros

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(keyOrIv);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        
        // Fonction permettant de décrypter la clé ou l'iv à partir de la clé principale de chiffrement
        private static string DecryptKeyOrIv(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(RetrieveAndDecryptMainKey());
                aesAlg.IV = new byte[16]; // IV de 16 octets rempli de zéros

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        
        
        // Fonction permettant de générer des clés aléatoires
        private static string GenerateRandomKey(int length)
        {
            char[] chars ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
    }
}