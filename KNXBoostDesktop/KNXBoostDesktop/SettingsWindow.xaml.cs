using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Win32;

// ReSharper disable ConvertToUsingDeclaration


namespace KNXBoostDesktop
{
    public partial class SettingsWindow
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        /// <summary>
        /// Gets or sets a value indicating whether DeepL translation is enabled.
        /// </summary>
        public bool EnableDeeplTranslation { get; private set; } // Activation ou non de la traduction deepL
        
        /// <summary>
        /// Gets or sets the DeepL API key used for accessing the DeepL translation service. Warning: the key is not
        /// stored in memory using plain text. It is encrypted to improve safety.
        /// </summary>
        public byte[] DeeplKey { get; private set; } // Clé API DeepL
        
        /// <summary>
        /// Gets or sets a value indicating whether automatic detection of the source language by DeepL is enabled.
        /// </summary>
        public bool EnableAutomaticSourceLangDetection { get; private set; } // Activation ou non de la détection automatique de la langue par DeepL
        
        /// <summary>
        /// Gets or sets the source language for translating group addresses.
        /// </summary>
        public string TranslationSourceLang { get; private set; } // Langue de source pour la traduction des adresses de groupe
        
        /// <summary>
        /// Gets or sets the destination language for translating group addresses.
        /// </summary>
        public string TranslationDestinationLang { get; private set; } // Langue de destination pour la traduction des adresses de groupe
        
        /// <summary>
        /// Gets or sets a value indicating whether the feature to clean up unused group addresses is enabled.
        /// </summary>
        public bool RemoveUnusedGroupAddresses { get; private set; } // Activation ou non de la fonctionnalité de nettoyage des adresses de groupe
        
        /// <summary>
        /// Gets or sets a value indicating whether the light theme is enabled for the application.
        /// </summary>
        public bool EnableLightTheme { get; private set; } // Thème de l'application (sombre/clair)
        
        /// <summary>
        /// Gets or sets the application language, with French as the default.
        /// </summary>
        public string AppLang { get; private set; } // Langue de l'application (français par défaut)
        
        
        /// <summary>
        ///  Gets or sets the scale factor for the content of every window of the application
        /// </summary>
        public int AppScaleFactor { get; private set; } // Facteur d'échelle pour les fenêtres de l'application
        
        
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Constructeur par défaut. Charge les paramètres contenus dans le fichier appSettings et les affiche également
        // dans la fenêtre de paramétrage de l'application. Si la valeur est incorrecte ou vide, une valeur par défaut
        // est affectée.
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class,
        /// loading and applying settings from the appSettings file, and setting default values where necessary.
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent(); // Initialisation de la fenêtre de paramétrage
            
            // Initialement, l'application dispose des paramètres par défaut, qui seront potentiellement modifiés après par
            // la lecture du fichier settings. Cela permet d'éviter un crash si le fichier 
            EnableDeeplTranslation = false;
            TranslationDestinationLang = "FR";
            EnableAutomaticSourceLangDetection = true;
            TranslationSourceLang = "FR";
            RemoveUnusedGroupAddresses = true;
            EnableLightTheme = true;
            AppLang = "FR";
            DeeplKey = Convert.FromBase64String("");
            AppScaleFactor = 100;

            const string settingsPath = "./appSettings"; // Chemin du fichier paramètres
            
            // Si le fichier contenant la main key n'existe pas
            if (!File.Exists("./emk"))
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
                    var messageBoxText = App.DisplayElements?.SettingsWindow!.AppLang switch
                    {
                        // Arabe
                        "AR" => "خطأ: لا يمكن الوصول إلى الملف 'ei', 'ek' أو 'appSettings'. يرجى التحقق من أنها ليست للقراءة فقط وإعادة المحاولة، أو تشغيل البرنامج كمسؤول.\nرمز الخطأ: 1",
                        // Bulgare
                        "BG" => "Грешка: Не може да се получи достъп до файла 'ei', 'ek' или 'appSettings'. Моля, уверете се, че не са само за четене и опитайте отново, или стартирайте програмата като администратор.\nКод на грешката: 1",
                        // Tchèque
                        "CS" => "Chyba: Nelze přistoupit k souboru 'ei', 'ek' nebo 'appSettings'. Zkontrolujte, zda nejsou pouze ke čtení, a zkuste to znovu, nebo spusťte program jako správce.\nChybový kód: 1",
                        // Danois
                        "DA" => "Fejl: Kan ikke få adgang til filen 'ei', 'ek' eller 'appSettings'. Kontroller, at de ikke er skrivebeskyttede, og prøv igen, eller kør programmet som administrator.\nFejlkode: 1",
                        // Allemand
                        "DE" => "Fehler: Zugriff auf die Datei 'ei', 'ek' oder 'appSettings' nicht möglich. Bitte überprüfen Sie, ob die Dateien nur lesbar sind und versuchen Sie es erneut oder starten Sie das Programm als Administrator.\nFehlercode: 1",
                        // Grec
                        "EL" => "Σφάλμα: Αδυναμία πρόσβασης στο αρχείο 'ei', 'ek' ή 'appSettings'. Ελέγξτε αν δεν είναι μόνο για ανάγνωση και δοκιμάστε ξανά ή εκκινήστε το πρόγραμμα ως διαχειριστής.\nΚωδικός σφάλματος: 1",
                        // Anglais
                        "EN" => "Error: Unable to access the file 'ei', 'ek', or 'appSettings'. Please check that they are not read-only and try again, or run the program as an administrator.\nError code: 1",
                        // Espagnol
                        "ES" => "Error: No se puede acceder al archivo 'ei', 'ek' o 'appSettings'. Verifique que no sean de solo lectura y vuelva a intentarlo, o ejecute el programa como administrador.\nCódigo de error: 1",
                        // Estonien
                        "ET" => "Viga: ei pääse faili 'ei', 'ek' või 'appSettings'. Palun kontrollige, et need ei oleks ainult lugemiseks ja proovige uuesti või käivitage programm administraatorina.\nVeakood: 1",
                        // Finnois
                        "FI" => "Virhe: Ei pääse tiedostoon 'ei', 'ek' tai 'appSettings'. Tarkista, etteivät ne ole vain luku- ja yritä uudelleen, tai suorita ohjelma järjestelmänvalvojana.\nVirhekoodi: 1",
                        // Hongrois
                        "HU" => "Hiba: Nem lehet hozzáférni az 'ei', 'ek' vagy 'appSettings' fájlhoz. Kérjük, ellenőrizze, hogy nem csak olvashatóak-e, és próbálja újra, vagy futtassa a programot rendszergazdaként.\nHibakód: 1",
                        // Indonésien
                        "ID" => "Kesalahan: Tidak dapat mengakses file 'ei', 'ek', atau 'appSettings'. Silakan periksa apakah file tersebut hanya-baca dan coba lagi, atau jalankan program sebagai administrator.\nKode kesalahan: 1",
                        // Italien
                        "IT" => "Errore: Impossibile accedere al file 'ei', 'ek' o 'appSettings'. Verifica che non siano di sola lettura e riprova, oppure esegui il programma come amministratore.\nCodice errore: 1",
                        // Japonais
                        "JA" => "エラー: 'ei'、'ek'、または 'appSettings' ファイルにアクセスできません。ファイルが読み取り専用でないことを確認し、もう一度試してください。あるいは、管理者としてプログラムを実行してください。\nエラーコード: 1",
                        // Coréen
                        "KO" => "오류: 'ei', 'ek' 또는 'appSettings' 파일에 접근할 수 없습니다. 파일이 읽기 전용이 아닌지 확인하고 다시 시도하거나 프로그램을 관리자 권한으로 실행하십시오.\n오류 코드: 1",
                        // Letton
                        "LV" => "Kļūda: nevar piekļūt failam 'ei', 'ek' vai 'appSettings'. Lūdzu, pārliecinieties, ka faili nav tikai lasāmi un mēģiniet vēlreiz, vai palaidiet programmu kā administrators.\nKļūdas kods: 1",
                        // Lituanien
                        "LT" => "Klaida: Nepavyksta pasiekti failui 'ei', 'ek' arba 'appSettings'. Patikrinkite, ar failai nėra tik skaitymo režime, ir bandykite dar kartą arba paleiskite programą kaip administratorių.\nKlaidos kodas: 1",
                        // Norvégien
                        "NB" => "Feil: Kan ikke få tilgang til filen 'ei', 'ek' eller 'appSettings'. Kontroller at de ikke er skrivebeskyttede og prøv igjen, eller kjør programmet som administrator.\nFeilkode: 1",
                        // Néerlandais
                        "NL" => "Fout: Kan geen toegang krijgen tot het bestand 'ei', 'ek' of 'appSettings'. Controleer of ze niet alleen-lezen zijn en probeer het opnieuw, of voer het programma uit als administrator.\nFoutcode: 1",
                        // Polonais
                        "PL" => "Błąd: Nie można uzyskać dostępu do pliku 'ei', 'ek' lub 'appSettings'. Sprawdź, czy nie są one tylko do odczytu i spróbuj ponownie, lub uruchom program jako administrator.\nKod błędu: 1",
                        // Portugais
                        "PT" => "Erro: Não é possível acessar o arquivo 'ei', 'ek' ou 'appSettings'. Verifique se eles não são somente leitura e tente novamente, ou execute o programa como administrador.\nCódigo de erro: 1",
                        // Roumain
                        "RO" => "Eroare: Nu se poate accesa fișierul 'ei', 'ek' sau 'appSettings'. Vă rugăm să verificați dacă nu sunt doar pentru citire și încercați din nou sau rulați programul ca administrator.\nCod eroare: 1",
                        // Russe
                        "RU" => "Ошибка: Не удалось получить доступ к файлам 'ei', 'ek' или 'appSettings'. Пожалуйста, убедитесь, что файлы не являются только для чтения, и попробуйте снова, или запустите программу от имени администратора.\nКод ошибки: 1",
                        // Slovaque
                        "SK" => "Chyba: Nepodarilo sa získať prístup k súboru 'ei', 'ek' alebo 'appSettings'. Skontrolujte, či nie sú len na čítanie, a skúste to znova alebo spustite program ako správca.\nChybový kód: 1",
                        // Slovène
                        "SL" => "Napaka: Ni mogoče dostopati do datoteke 'ei', 'ek' ali 'appSettings'. Preverite, ali niso samo za branje, in poskusite znova ali zaženite program kot skrbnik.\nKoda napake: 1",
                        // Suédois
                        "SV" => "Fel: Kan inte få åtkomst till filen 'ei', 'ek' eller 'appSettings'. Kontrollera att de inte är skrivskyddade och försök igen, eller kör programmet som administratör.\nFelkod: 1",
                        // Turc
                        "TR" => "Hata: 'ei', 'ek' veya 'appSettings' dosyasına erişilemiyor. Dosyaların sadece okunur olmadığını kontrol edin ve tekrar deneyin veya programı yönetici olarak çalıştırın.\nHata kodu: 1",
                        // Ukrainien
                        "UK" => "Помилка: Неможливо отримати доступ до файлів 'ei', 'ek' або 'appSettings'. Перевірте, чи не є вони лише для читання, і спробуйте ще раз, або запустіть програму від імені адміністратора.\nКод помилки: 1",
                        // Chinois simplifié
                        "ZH" => "错误：无法访问文件 'ei'、'ek' 或 'appSettings'。请检查文件是否为只读，然后重试，或以管理员身份运行程序。\n错误代码：1",
                        // Cas par défaut (français)
                        _ => "Erreur : impossible d'accéder au fichier 'ei', 'ek' ou 'appSettings'. Veuillez vérifier qu'ils ne sont pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 1"
                    };

                    var messageBoxCaption = App.DisplayElements?.SettingsWindow!.AppLang switch
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

                    // Affichage du MessageBox avec la traduction appropriée
                    MessageBox.Show(messageBoxText, messageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);

                    Application.Current.Shutdown(1);
                }
            }

            try
            {
                // Si le fichier de paramétrage n'existe pas, on le crée
                // Note : comme File.Create ouvre un stream vers le fichier à la création, on le ferme directement avec Close().
                if (!File.Exists(settingsPath))
                {
                    File.Create(settingsPath).Close();

                    // Le thème appliqué par défaut est le même que celui de windows
                    EnableLightTheme = DetectWindowsTheme();
                    
                    // La langue de l'application appliquée est la même que celle de windows
                    AppLang = DetectWindowsLanguage();
                }
            }
            // Si le programme n'a pas accès en écriture pour créer le fichier
            catch (UnauthorizedAccessException)
            {
                // Définir les variables pour le texte du message et le titre du MessageBox
                var messageBoxText = App.DisplayElements?.SettingsWindow!.AppLang switch
                {
                    // Arabe
                    "AR" => "خطأ: تعذر الوصول إلى ملف إعدادات التطبيق. يرجى التحقق من أنه ليس للقراءة فقط وحاول مرة أخرى، أو قم بتشغيل البرنامج كمسؤول.\nرمز الخطأ: 1",
                    // Bulgare
                    "BG" => "Грешка: Не може да се получи достъп до конфигурационния файл на приложението. Моля, проверете дали файлът не е само за четене и опитайте отново, или стартирайте програмата като администратор.\nКод за грешка: 1",
                    // Tchèque
                    "CS" => "Chyba: Nelze získat přístup k konfiguračnímu souboru aplikace. Zkontrolujte, zda není pouze ke čtení, a zkuste to znovu, nebo spusťte program jako správce.\nChybový kód: 1",
                    // Danois
                    "DA" => "Fejl: Kan ikke få adgang til applikationskonfigurationsfilen. Kontroller venligst, at filen ikke er skrivebeskyttet, og prøv igen, eller start programmet som administrator.\nFejlkode: 1",
                    // Allemand
                    "DE" => "Fehler: Zugriff auf die Konfigurationsdatei der Anwendung nicht möglich. Bitte überprüfen Sie, ob die Datei schreibgeschützt ist, und versuchen Sie es erneut, oder starten Sie das Programm als Administrator.\nFehlercode: 1",
                    // Grec
                    "EL" => "Σφάλμα: δεν είναι δυνατή η πρόσβαση στο αρχείο ρυθμίσεων της εφαρμογής. Παρακαλώ ελέγξτε αν δεν είναι μόνο για ανάγνωση και προσπαθήστε ξανά, ή ξεκινήστε το πρόγραμμα ως διαχειριστής.\nΚωδικός σφάλματος: 1",
                    // Anglais
                    "EN" => "Error: Unable to access the application configuration file. Please check if it is read-only and try again, or run the program as an administrator.\nError Code: 1",
                    // Espagnol
                    "ES" => "Error: No se puede acceder al archivo de configuración de la aplicación. Por favor, verifique si el archivo es de solo lectura y vuelva a intentarlo, o ejecute el programa como administrador.\nCódigo de error: 1",
                    // Estonien
                    "ET" => "Viga: rakenduse konfiguratsioonifailile ei saa juurde pääseda. Kontrollige, kas fail on ainult lugemiseks ja proovige uuesti või käivitage programm administraatorina.\nVeakood: 1",
                    // Finnois
                    "FI" => "Virhe: Sovelluksen asetustiedostoon ei pääse käsiksi. Tarkista, ettei tiedosto ole vain luku -tilassa, ja yritä uudelleen tai käynnistä ohjelma järjestelmänvalvojana.\nVirhekoodi: 1",
                    // Hongrois
                    "HU" => "Hiba: Nem lehet hozzáférni az alkalmazás konfigurációs fájljához. Kérjük, ellenőrizze, hogy a fájl nem csak olvasásra van-e beállítva, és próbálja újra, vagy futtassa a programot rendszergazdai jogosultságokkal.\nHibakód: 1",
                    // Indonésien
                    "ID" => "Kesalahan: tidak dapat mengakses file konfigurasi aplikasi. Silakan periksa apakah file tersebut hanya-baca dan coba lagi, atau jalankan program sebagai administrator.\nKode kesalahan: 1",
                    // Italien
                    "IT" => "Errore: impossibile accedere al file di configurazione dell'applicazione. Verifica se il file è solo in lettura e riprova, oppure avvia il programma come amministratore.\nCodice errore: 1",
                    // Japonais
                    "JA" => "エラー: アプリケーションの設定ファイルにアクセスできません。ファイルが読み取り専用でないか確認し、再試行するか、管理者としてプログラムを実行してください。\nエラーコード: 1",
                    // Coréen
                    "KO" => "오류: 애플리케이션 구성 파일에 액세스할 수 없습니다. 파일이 읽기 전용인지 확인하고 다시 시도하거나 관리자로 프로그램을 실행하세요.\n오류 코드: 1",
                    // Letton
                    "LV" => "Kļūda: nevar piekļūt lietojumprogrammas konfigurācijas failam. Lūdzu, pārbaudiet, vai fails nav tikai lasāms, un mēģiniet vēlreiz vai palaidiet programmu kā administrators.\nKļūdas kods: 1",
                    // Lituanien
                    "LT" => "Klaida: negalima prieiti prie programos konfigūracijos failo. Patikrinkite, ar failas nėra tik skaitymui ir bandykite dar kartą arba paleiskite programą kaip administratorius.\nKlaidos kodas: 1",
                    // Norvégien
                    "NB" => "Feil: Kan ikke få tilgang til applikasjonskonfigurasjonsfilen. Sjekk om filen er skrivebeskyttet og prøv igjen, eller kjør programmet som administrator.\nFeilkode: 1",
                    // Néerlandais
                    "NL" => "Fout: kan geen toegang krijgen tot het configuratiebestand van de applicatie. Controleer of het bestand alleen-lezen is en probeer het opnieuw, of voer het programma uit als administrator.\nFoutcode: 1",
                    // Polonais
                    "PL" => "Błąd: Nie można uzyskać dostępu do pliku konfiguracyjnego aplikacji. Sprawdź, czy plik nie jest tylko do odczytu, a następnie spróbuj ponownie lub uruchom program jako administrator.\nKod błędu: 1",
                    // Portugais
                    "PT" => "Erro: não foi possível acessar o arquivo de configuração do aplicativo. Verifique se o arquivo é somente leitura e tente novamente, ou execute o programa como administrador.\nCódigo de erro: 1",
                    // Roumain
                    "RO" => "Eroare: Nu se poate accesa fișierul de configurare al aplicației. Vă rugăm să verificați dacă fișierul este numai pentru citire și să încercați din nou sau să rulați programul ca administrator.\nCod eroare: 1",
                    // Russe
                    "RU" => "Ошибка: невозможно получить доступ к файлу конфигурации приложения. Проверьте, не является ли файл только для чтения, и попробуйте снова, или запустите программу от имени администратора.\nКод ошибки: 1",
                    // Slovaque
                    "SK" => "Chyba: nemožno získať prístup k konfiguračnému súboru aplikácie. Skontrolujte, či nie je súbor iba na čítanie, a skúste to znova, alebo spustite program ako správca.\nChybový kód: 1",
                    // Slovène
                    "SL" => "Napaka: dostop do konfiguracijske datoteke aplikacije ni mogoč. Preverite, ali je datoteka samo za branje, in poskusite znova, ali zaženite program kot skrbnik.\nKoda napake: 1",
                    // Suédois
                    "SV" => "Fel: Kan inte komma åt konfigurationsfilen för applikationen. Kontrollera om filen är skrivskyddad och försök igen, eller kör programmet som administratör.\nFelkod: 1",
                    // Turc
                    "TR" => "Hata: Uygulama yapılandırma dosyasına erişilemiyor. Dosyanın salt okunur olup olmadığını kontrol edin ve tekrar deneyin veya programı yönetici olarak çalıştırın.\nHata Kodu: 1",
                    // Ukrainien
                    "UK" => "Помилка: неможливо отримати доступ до файлу конфігурації програми. Будь ласка, перевірте, чи не є файл тільки для читання, і спробуйте ще раз або запустіть програму від імені адміністратора.\nКод помилки: 1",
                    // Chinois simplifié
                    "ZH" => "错误: 无法访问应用程序配置文件。请检查文件是否为只读，并重试，或者以管理员身份运行程序。\n错误代码: 1",
                    // Cas par défaut (français)
                    _ => "Erreur: impossible d'accéder au fichier de paramétrage de l'application. Veuillez vérifier qu'il n'est pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 1"
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
                    "AR" => $"خطأ: هناك أحرف غير مدعومة في مسار ملف الإعدادات ({settingsPath}). تعذر الوصول إلى الملف.\nرمز الخطأ: 2",
                    "BG" => $"Грешка: Съдържа неразрешени символи в пътя на файла с настройки ({settingsPath}). Невъзможно е да се достъпи до файла.\nКод на грешката: 2",
                    "CS" => $"Chyba: V cestě k souboru nastavení ({settingsPath}) jsou přítomny nepodporované znaky. Nelze přistupovat k souboru.\nKód chyby: 2",
                    "DA" => $"Fejl: Ugyldige tegn findes i stien til konfigurationsfilen ({settingsPath}). Kan ikke få adgang til filen.\nFejlkode: 2",
                    "DE" => $"Fehler: Im Pfad zur Einstellungsdatei ({settingsPath}) sind nicht unterstützte Zeichen vorhanden. Auf die Datei kann nicht zugegriffen werden.\nFehlercode: 2",
                    "EL" => $"Σφάλμα: Υπάρχουν μη υποστηριγμένοι χαρακτήρες στη διαδρομή του αρχείου ρυθμίσεων ({settingsPath}). Δεν είναι δυνατή η πρόσβαση στο αρχείο.\nΚωδικός σφάλματος: 2",
                    "EN" => $"Error: Unsupported characters are present in the settings file path ({settingsPath}). Unable to access the file.\nError code: 2",
                    "ES" => $"Error: Hay caracteres no admitidos en la ruta del archivo de configuración ({settingsPath}). No se puede acceder al archivo.\nCódigo de error: 2",
                    "ET" => $"Viga: Seadistusfaili tee ({settingsPath}) sisaldab toetamatuid märke. Failile ei ole võimalik juurde pääseda.\nVigakood: 2",
                    "FI" => $"Virhe: Asetustiedoston polussa ({settingsPath}) on tukemattomia merkkejä. Tiedostoon ei voi käyttää.\nVirhekoodi: 2",
                    "HU" => $"Hiba: Az beállítási fájl elérési útvonalán ({settingsPath}) nem támogatott karakterek találhatók. A fájlhoz nem lehet hozzáférni.\nHibakód: 2",
                    "ID" => $"Kesalahan: Karakter yang tidak didukung ada di jalur file pengaturan ({settingsPath}). Tidak dapat mengakses file.\nKode kesalahan: 2",
                    "IT" => $"Errore: Sono presenti caratteri non supportati nel percorso del file di configurazione ({settingsPath}). Impossibile accedere al file.\nCodice errore: 2",
                    "JA" => $"エラー: 設定ファイルのパス ({settingsPath}) にサポートされていない文字が含まれています。ファイルにアクセスできません。\nエラーコード: 2",
                    "KO" => $"오류: 설정 파일 경로 ({settingsPath})에 지원되지 않는 문자가 포함되어 있습니다. 파일에 접근할 수 없습니다.\n오류 코드: 2",
                    "LV" => $"Kļūda: Iestatījumu faila ceļā ({settingsPath}) ir neatbalstīti rakstzīmes. Nevar piekļūt failam.\nKļūdas kods: 2",
                    "LT" => $"Klaida: Nustatymų failo kelias ({settingsPath}) turi nepalaikomų simbolių. Nepavyksta pasiekti failo.\nKlaidos kodas: 2",
                    "NB" => $"Feil: Det finnes ikke-støttede tegn i stien til innstillingsfilen ({settingsPath}). Kan ikke få tilgang til filen.\nFeilkode: 2",
                    "NL" => $"Fout: Onondersteunde tekens zijn aanwezig in het pad naar het instellingenbestand ({settingsPath}). Kan niet toegang krijgen tot het bestand.\nFoutcode: 2",
                    "PL" => $"Błąd: W ścieżce pliku ustawień ({settingsPath}) znajdują się nieobsługiwane znaki. Nie można uzyskać dostępu do pliku.\nKod błędu: 2",
                    "PT" => $"Erro: Caracteres não suportados estão presentes no caminho do arquivo de configuração ({settingsPath}). Não é possível acessar o arquivo.\nCódigo de erro: 2",
                    "RO" => $"Eroare: Caracterelor nesuportate sunt prezente în calea fișierului de configurare ({settingsPath}). Nu se poate accesa fișierul.\nCod eroare: 2",
                    "RU" => $"Ошибка: В пути к файлу настроек ({settingsPath}) присутствуют неподдерживаемые символы. Невозможно получить доступ к файлу.\nКод ошибки: 2",
                    "SK" => $"Chyba: V ceste k súboru nastavení ({settingsPath}) sú prítomné nepodporované znaky. Nie je možné pristupovať k súboru.\nKód chyby: 2",
                    "SL" => $"Napaka: V poti do konfiguracijske datoteke ({settingsPath}) so prisotne nepodprte znake. Do datoteke ni mogoče dostopati.\nKoda napake: 2",
                    "SV" => $"Fel: I inställningsfilens sökväg ({settingsPath}) finns tecken som inte stöds. Kan inte komma åt filen.\nFelkod: 2",
                    "TR" => $"Hata: Ayar dosyası yolunda ({settingsPath}) desteklenmeyen karakterler bulunuyor. Dosyaya erişilemiyor.\nHata kodu: 2",
                    "UK" => $"Помилка: У шляху до файлу налаштувань ({settingsPath}) є непідтримувані символи. Не вдалося отримати доступ до файлу.\nКод помилки: 2",
                    "ZH" => $"错误: 配置文件路径 ({settingsPath}) 中存在不支持的字符。无法访问文件。\n错误代码: 2",
                    _ => $"Erreur: des caractères non supportés sont présents dans le chemin d'accès du fichier de paramétrage ({settingsPath}). Impossible d'accéder au fichier.\nCode erreur: 2"
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
                // On parcourt toutes les lignes tant qu'elle n'est pas 'null'
                while (reader!.ReadLine() is { } line)
                {
                    // Créer un HashSet avec tous les codes de langue valides
                    var validLanguageCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
                        
                        case "enable automatic source lang detection for translation":
                            try
                            {
                                // On essaie de cast la valeur en booléen
                                EnableAutomaticSourceLangDetection = bool.Parse(value);
                            }
                            // Si l'utilisateur n'a pas écrit dans le fichier paramètres un string s'apparentant à true ou false
                            catch (FormatException)
                            {
                                App.ConsoleAndLogWriteLine(
                                    "Error: Could not parse boolean value of the activation of the automatic detection of the " +
                                    "translation source language, restoring default value");
                            }
                            break;

                        case "source translation lang":
                            // Vérifier si value est un code de langue valide, si elle est valide, on assigne la valeur, sinon on met la langue par défaut
                            TranslationSourceLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
                            break;
                        
                        case "destination translation lang":
                            // Vérifier si value est un code de langue valide, si elle est valide, on assigne la valeur, sinon on met la langue par défaut
                            TranslationDestinationLang = validLanguageCodes.Contains(value.ToUpper()) ? value : "FR";
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
                        
                        case "window scale factor":
                            try
                            {
                                AppScaleFactor = Convert.ToInt32(value) > 300 || Convert.ToInt32(value) < 50 ? 100 : Convert.ToInt32(value);
                                if (AppScaleFactor <= 100)
                                {
                                    ApplyScaling(AppScaleFactor/100f - 0.1f);
                                }
                                else
                                {
                                    ApplyScaling(AppScaleFactor/100f - 0.2f);
                                }
                            }
                            catch (Exception)
                            {
                                App.ConsoleAndLogWriteLine("Error: Could not parse the integer value of the window scale factor. Restoring default value (100%).");
                            }
                            
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

            AppVersionTextBlock.Text = $"{App.AppName} v{App.AppVersion.ToString(CultureInfo.InvariantCulture)} (build {App.AppBuild})";

            UpdateWindowContents(); // Affichage des paramètres dans la fenêtre
        }
        
        
        // Fonction s'exécutant à la fermeture de la fenêtre de paramètres
        /// <summary>
        /// Handles the settings window closing event by canceling the closure, restoring previous settings, and hiding the window.
        /// </summary>
        private void ClosingSettingsWindow(object? sender, CancelEventArgs e)
        {
            e.Cancel = true; // Pour éviter de tuer l'instance de SettingsWindow, on annule la fermeture
            UpdateWindowContents(true); // Mise à jour du contenu de la fenêtre pour remettre les valeurs précédentes
            Hide(); // On masque la fenêtre à la place
        }


        // Fonction permettant de sauvegarder les paramètres dans le fichier appSettings
        /// <summary>
        /// Saves the application settings to the appSettings file, handling potential I/O errors during the process.
        /// </summary>
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

                writer.Write("source translation lang : ");
                writer.WriteLine(TranslationSourceLang);
                
                writer.Write("enable automatic source lang detection for translation : ");
                writer.WriteLine(EnableAutomaticSourceLangDetection);
                
                writer.Write("destination translation lang : ");
                writer.WriteLine(TranslationDestinationLang);

                writer.Write("remove unused group addresses : ");
                writer.WriteLine(RemoveUnusedGroupAddresses);

                writer.Write("theme : ");
                writer.WriteLine(EnableLightTheme ? "light" : "dark");

                writer.Write("application language : ");
                writer.WriteLine(AppLang);
                
                writer.Write("window scale factor : ");
                writer.WriteLine(AppScaleFactor);

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
        /// <summary>
        /// Updates the contents (texts, textboxes, checkboxes, ...) of the settingswindow accordingly to the application settings.
        /// </summary>
        private void UpdateWindowContents(bool isClosing = false)
        {
            EnableTranslationCheckBox.IsChecked = EnableDeeplTranslation; // Cochage/décochage
            
            if ((File.Exists("./emk"))&&(!isClosing)) DeeplApiKeyTextBox.Text = DecryptStringFromBytes(DeeplKey); // Décryptage de la clé DeepL

            EnableAutomaticTranslationLangDetectionCheckbox.IsChecked = EnableAutomaticSourceLangDetection; // Cochage/Décochage

            // Si la langue de traduction ou de l'application n'est pas le français, on désélectionne le français dans le combobox
            // pour sélectionner la langue voulue
            if (TranslationDestinationLang != "FR")
            {
                FrDestinationTranslationComboBoxItem.IsSelected = (TranslationDestinationLang == "FR"); // Sélection/Désélection

                // Sélection du langage destination de traduction
                foreach (ComboBoxItem item in TranslationLanguageDestinationComboBox.Items) // Parcours de toutes les possibilités de langue
                {
                    if (!item.Content.ToString()!.StartsWith(TranslationDestinationLang)) continue; // Si la langue n'est pas celle que l'on veut, on skip
                    item.IsSelected = true; // Sélection de la langue
                    break; // Si on a trouvé la langue, on peut quitter la boucle
                }
            }
            
            if (TranslationSourceLang != "FR")
            {
                FrSourceTranslationComboBoxItem.IsSelected = (TranslationSourceLang == "FR"); // Sélection/Désélection
                
                // Sélection du langage source de traduction
                foreach (ComboBoxItem item in TranslationSourceLanguageComboBox.Items) // Parcours de toutes les possibilités de langue
                {
                    if (!item.Content.ToString()!.StartsWith(TranslationSourceLang)) continue; // Si la langue n'est pas celle que l'on veut, on skip
                    item.IsSelected = true; // Sélection de la langue
                    break; // Si on a trouvé la langue, on peut quitter la boucle
                }
            }
            
            if (AppLang != "FR")
            {
                FrAppLanguageComboBoxItem.IsSelected = (AppLang == "FR"); // Sélection/Désélection

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
            
            // Mise à jour du slider
            ScaleSlider.Value = AppScaleFactor;
            
            // Traduction du menu settings
            switch (AppLang)
            {
                // Arabe
                case "AR":
                    SettingsWindowTopTitle.Text = "الإعدادات";
                    TranslationTitle.Text = "ترجمة";
                    EnableTranslationCheckBox.Content = "تفعيل الترجمة";
                    DeeplApiKeyText.Text = "مفتاح API الخاص بـ DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(انقر هنا للحصول على مفتاح مجانًا)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ar/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "تمكين الكشف التلقائي عن اللغة للترجمة";
                    TranslationSourceLanguageComboBoxText.Text = "لغة المصدر للترجمة:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "لغة الوجهة للترجمة:";

                    GroupAddressManagementTitle.Text = "إدارة عناوين المجموعة";
                    RemoveUnusedAddressesCheckBox.Content = "إزالة العناوين غير المستخدمة";

                    AppSettingsTitle.Text = "إعدادات التطبيق";
                    ThemeTextBox.Text = "السمة:";
                    LightThemeComboBoxItem.Content = "فاتح (افتراضي)";
                    DarkThemeComboBoxItem.Content = "داكن";

                    AppLanguageTextBlock.Text = "لغة التطبيق:";

                    SaveButtonText.Text = "حفظ";
                    CancelButtonText.Text = "إلغاء";
                    
                    MenuDebug.Text = "قائمة التصحيح";
                    AddInfosOsCheckBox.Content = "تضمين معلومات نظام التشغيل";
                    AddInfosHardCheckBox.Content = "تضمين معلومات أجهزة الكمبيوتر";
                    AddImportedFilesCheckBox.Content = "تضمين الملفات المستوردة منذ الإطلاق";
                    IncludeAddressListCheckBox.Content = "تضمين قائمة العناوين المحذوفة من المشاريع";
                    CreateArchiveDebugText.Text = "إنشاء ملف التصحيح";
                    
                    OngletParametresGeneraux.Header = "الإعدادات العامة";
                    OngletDebug.Header = "تصحيح الأخطاء";
                    break;

                // Bulgare
                case "BG":
                    SettingsWindowTopTitle.Text = "Настройки";
                    TranslationTitle.Text = "Превод";
                    EnableTranslationCheckBox.Content = "Активиране на превода";
                    DeeplApiKeyText.Text = "DeepL API ключ:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Кликнете тук за безплатен ключ)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Активиране на автоматичното разпознаване на езика за превод";
                    TranslationSourceLanguageComboBoxText.Text = "Изходен език за превод:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Целеви език за превод:";

                    GroupAddressManagementTitle.Text = "Управление на групови адреси";
                    RemoveUnusedAddressesCheckBox.Content = "Премахване на неизползваните адреси";

                    AppSettingsTitle.Text = "Настройки на приложението";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Светла (по подразбиране)";
                    DarkThemeComboBoxItem.Content = "Тъмна";

                    AppLanguageTextBlock.Text = "Език на приложението:";

                    SaveButtonText.Text = "Запази";
                    CancelButtonText.Text = "Отмени";
                    
                    MenuDebug.Text = "Меню за отстраняване на грешки";
                    AddInfosOsCheckBox.Content = "Включване на информация за операционната система";
                    AddInfosHardCheckBox.Content = "Включване на информация за хардуера";
                    AddImportedFilesCheckBox.Content = "Включване на импортираните файлове от стартиране";
                    IncludeAddressListCheckBox.Content = "Включване на списъка с изтрити адреси";
                    CreateArchiveDebugText.Text = "Създаване на файл за отстраняване на грешки";
                    
                    OngletParametresGeneraux.Header = "Общи настройки";
                    OngletDebug.Header = "Отстраняване на грешки";
                    break;

                // Tchèque
                case "CS":
                    SettingsWindowTopTitle.Text = "Nastavení";
                    TranslationTitle.Text = "Překlad";
                    EnableTranslationCheckBox.Content = "Povolit překlad";
                    DeeplApiKeyText.Text = "API klíč DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikněte sem a získejte klíč zdarma)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/cs/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Povolit automatickou detekci jazyka pro překlad";
                    TranslationSourceLanguageComboBoxText.Text = "Zdrojový jazyk překladu:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Cílový jazyk překladu:";

                    GroupAddressManagementTitle.Text = "Správa skupinových adres";
                    RemoveUnusedAddressesCheckBox.Content = "Odebrat nepoužívané adresy";

                    AppSettingsTitle.Text = "Nastavení aplikace";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Světlé (výchozí)";
                    DarkThemeComboBoxItem.Content = "Tmavé";

                    AppLanguageTextBlock.Text = "Jazyk aplikace:";

                    SaveButtonText.Text = "Uložit";
                    CancelButtonText.Text = "Zrušit";
                    
                    MenuDebug.Text = "Ladicí nabídka";
                    AddInfosOsCheckBox.Content = "Zahrnout informace o operačním systému";
                    AddInfosHardCheckBox.Content = "Zahrnout informace o hardwaru";
                    AddImportedFilesCheckBox.Content = "Zahrnout importované soubory od spuštění";
                    IncludeAddressListCheckBox.Content = "Zahrnout seznam odstraněných adres skupin v projektech";
                    CreateArchiveDebugText.Text = "Vytvořit ladicí soubor";
                    
                    OngletParametresGeneraux.Header = "Obecná nastavení";
                    OngletDebug.Header = "Ladění";
                    break;

                // Danois
                case "DA":
                    SettingsWindowTopTitle.Text = "Indstillinger";
                    TranslationTitle.Text = "Oversættelse";
                    EnableTranslationCheckBox.Content = "Aktiver oversættelse";
                    DeeplApiKeyText.Text = "DeepL API nøgle:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik her for at få en gratis nøgle)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktiver automatisk sprogdetektion til oversættelse";
                    TranslationSourceLanguageComboBoxText.Text = "Kildesprog for oversættelse:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Målsprog for oversættelse:";

                    GroupAddressManagementTitle.Text = "Administration af gruppeadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrugte adresser";

                    AppSettingsTitle.Text = "Applikationsindstillinger";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Lyst (standard)";
                    DarkThemeComboBoxItem.Content = "Mørkt";

                    AppLanguageTextBlock.Text = "Applikationssprog:";

                    SaveButtonText.Text = "Gem";
                    CancelButtonText.Text = "Annuller";
                    
                    MenuDebug.Text = "Fejlfindingsmenu";
                    AddInfosOsCheckBox.Content = "Inkluder oplysninger om operativsystemet";
                    AddInfosHardCheckBox.Content = "Inkluder hardwareoplysninger";
                    AddImportedFilesCheckBox.Content = "Inkluder importerede filer siden opstart";
                    IncludeAddressListCheckBox.Content = "Inkluder listen over slettede gruppeadresser i projekter";
                    CreateArchiveDebugText.Text = "Opret fejlfindingsfil";
                    
                    OngletParametresGeneraux.Header = "Generelle indstillinger";
                    OngletDebug.Header = "Fejlfinding";
                    break;

                // Allemand
                case "DE":
                    SettingsWindowTopTitle.Text = "Einstellungen";
                    TranslationTitle.Text = "Übersetzung";
                    EnableTranslationCheckBox.Content = "Übersetzung aktivieren";
                    DeeplApiKeyText.Text = "DeepL API-Schlüssel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Hier klicken, um einen kostenlosen Schlüssel zu erhalten)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/de/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Automatische Spracherkennung für Übersetzungen aktivieren";
                    TranslationSourceLanguageComboBoxText.Text = "Quellsprache der Übersetzung:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Zielsprache der Übersetzung:";

                    GroupAddressManagementTitle.Text = "Verwaltung von Gruppenadressen";
                    RemoveUnusedAddressesCheckBox.Content = "Unbenutzte Adressen entfernen";

                    AppSettingsTitle.Text = "App-Einstellungen";
                    ThemeTextBox.Text = "Thema:";
                    LightThemeComboBoxItem.Content = "Hell (Standard)";
                    DarkThemeComboBoxItem.Content = "Dunkel";

                    AppLanguageTextBlock.Text = "App-Sprache:";

                    SaveButtonText.Text = "Speichern";
                    CancelButtonText.Text = "Abbrechen";
                    
                    MenuDebug.Text = "Debug-Menü";
                    AddInfosOsCheckBox.Content = "Betriebssysteminformationen einbeziehen";
                    AddInfosHardCheckBox.Content = "Hardwareinformationen einbeziehen";
                    AddImportedFilesCheckBox.Content = "Seit dem Start importierte Dateien einbeziehen";
                    IncludeAddressListCheckBox.Content = "Liste der gelöschten Gruppenadressen in Projekten einbeziehen";
                    CreateArchiveDebugText.Text = "Debug-Datei erstellen";
                    
                    MenuDebug.Text = "Μενού αποσφαλμάτωσης";
                    AddInfosOsCheckBox.Content = "Συμπερίληψη πληροφοριών λειτουργικού συστήματος";
                    AddInfosHardCheckBox.Content = "Συμπερίληψη πληροφοριών υλικού υπολογιστή";
                    AddImportedFilesCheckBox.Content = "Συμπερίληψη εισαγόμενων αρχείων από την εκκίνηση";
                    IncludeAddressListCheckBox.Content = "Συμπερίληψη της λίστας διαγραμμένων διευθύνσεων ομάδων στα έργα";
                    CreateArchiveDebugText.Text = "Δημιουργία αρχείου αποσφαλμάτωσης";
                    
                    OngletParametresGeneraux.Header = "Allgemeine Einstellungen";
                    OngletDebug.Header = "Fehlerbehebung";
                    break;

                // Grec
                case "EL":
                    SettingsWindowTopTitle.Text = "Ρυθμίσεις";
                    TranslationTitle.Text = "Μετάφραση";
                    EnableTranslationCheckBox.Content = "Ενεργοποίηση μετάφρασης";
                    DeeplApiKeyText.Text = "Κλειδί API του DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Κάντε κλικ εδώ για να αποκτήσετε ένα δωρεάν κλειδί)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Ενεργοποίηση αυτόματης ανίχνευσης γλώσσας για μετάφραση";
                    TranslationSourceLanguageComboBoxText.Text = "Γλώσσα προέλευσης για μετάφραση:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Γλώσσα προορισμού για μετάφραση:";

                    GroupAddressManagementTitle.Text = "Διαχείριση διευθύνσεων ομάδας";
                    RemoveUnusedAddressesCheckBox.Content = "Αφαίρεση αχρησιμοποίητων διευθύνσεων";

                    AppSettingsTitle.Text = "Ρυθμίσεις εφαρμογής";
                    ThemeTextBox.Text = "Θέμα:";
                    LightThemeComboBoxItem.Content = "Φωτεινό (προεπιλογή)";
                    DarkThemeComboBoxItem.Content = "Σκοτεινό";

                    AppLanguageTextBlock.Text = "Γλώσσα εφαρμογής:";

                    SaveButtonText.Text = "Αποθήκευση";
                    CancelButtonText.Text = "Ακύρωση";
                    
                    OngletParametresGeneraux.Header = "Γενικές ρυθμίσεις";
                    OngletDebug.Header = "Αποσφαλμάτωση";
                    break;

                // Anglais
                case "EN":
                    SettingsWindowTopTitle.Text = "Settings";
                    TranslationTitle.Text = "Translation";
                    EnableTranslationCheckBox.Content = "Enable Translation";
                    DeeplApiKeyText.Text = "DeepL API Key:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Click here to get a free key)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Enable automatic language detection for translation";
                    TranslationSourceLanguageComboBoxText.Text = "Source language for translation:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Target language for translation:";

                    GroupAddressManagementTitle.Text = "Group Address Management";
                    RemoveUnusedAddressesCheckBox.Content = "Remove unused addresses";

                    AppSettingsTitle.Text = "Application Settings";
                    ThemeTextBox.Text = "Theme:";
                    LightThemeComboBoxItem.Content = "Light (default)";
                    DarkThemeComboBoxItem.Content = "Dark";

                    AppLanguageTextBlock.Text = "Application Language:";

                    SaveButtonText.Text = "Save";
                    CancelButtonText.Text = "Cancel";
                    
                    MenuDebug.Text = "Debug Menu";
                    AddInfosOsCheckBox.Content = "Include OS information";
                    AddInfosHardCheckBox.Content = "Include hardware information";
                    AddImportedFilesCheckBox.Content = "Include files imported since launch";
                    IncludeAddressListCheckBox.Content = "Include list of deleted group addresses in projects";
                    CreateArchiveDebugText.Text = "Create debug file";
                    
                    OngletParametresGeneraux.Header = "General Settings";
                    OngletDebug.Header = "Debugging";
                    break;

                // Espagnol
                case "ES":
                    SettingsWindowTopTitle.Text = "Configuración";
                    TranslationTitle.Text = "Traducción";
                    EnableTranslationCheckBox.Content = "Habilitar traducción";
                    DeeplApiKeyText.Text = "Clave API de DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Haga clic aquí para obtener una clave gratis)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/es/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Habilitar detección automática de idioma para la traducción";
                    TranslationSourceLanguageComboBoxText.Text = "Idioma de origen para la traducción:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Idioma de destino para la traducción:";

                    GroupAddressManagementTitle.Text = "Gestión de direcciones de grupo";
                    RemoveUnusedAddressesCheckBox.Content = "Eliminar direcciones no utilizadas";

                    AppSettingsTitle.Text = "Configuración de la aplicación";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Claro (predeterminado)";
                    DarkThemeComboBoxItem.Content = "Oscuro";

                    AppLanguageTextBlock.Text = "Idioma de la aplicación:";

                    SaveButtonText.Text = "Guardar";
                    CancelButtonText.Text = "Cancelar";
                    
                    MenuDebug.Text = "Menú de depuración";
                    AddInfosOsCheckBox.Content = "Incluir información del sistema operativo";
                    AddInfosHardCheckBox.Content = "Incluir información de hardware";
                    AddImportedFilesCheckBox.Content = "Incluir archivos importados desde el inicio";
                    IncludeAddressListCheckBox.Content = "Incluir lista de direcciones de grupo eliminadas en los proyectos";
                    CreateArchiveDebugText.Text = "Crear archivo de depuración";
                    
                    OngletParametresGeneraux.Header = "Configuración general";
                    OngletDebug.Header = "Depuración";
                    break;

                // Estonien
                case "ET":
                    SettingsWindowTopTitle.Text = "Seaded";
                    TranslationTitle.Text = "Tõlge";
                    EnableTranslationCheckBox.Content = "Luba tõlge";
                    DeeplApiKeyText.Text = "DeepL API võti:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikkige siia, et saada tasuta võti)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Luba tõlke jaoks automaatne keele tuvastamine";
                    TranslationSourceLanguageComboBoxText.Text = "Tõlke lähtekeel:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Tõlke sihtkeel:";

                    GroupAddressManagementTitle.Text = "Grupi aadresside haldamine";
                    RemoveUnusedAddressesCheckBox.Content = "Eemalda kasutamata aadressid";

                    AppSettingsTitle.Text = "Rakenduse seaded";
                    ThemeTextBox.Text = "Teema:";
                    LightThemeComboBoxItem.Content = "Hele (vaikimisi)";
                    DarkThemeComboBoxItem.Content = "Tume";

                    AppLanguageTextBlock.Text = "Rakenduse keel:";

                    SaveButtonText.Text = "Salvesta";
                    CancelButtonText.Text = "Tühista";
                    
                    MenuDebug.Text = "Silumisvalik";
                    AddInfosOsCheckBox.Content = "Kaasa operatsioonisüsteemi teave";
                    AddInfosHardCheckBox.Content = "Kaasa riistvara teave";
                    AddImportedFilesCheckBox.Content = "Kaasa imporditud failid käivitamisest alates";
                    IncludeAddressListCheckBox.Content = "Kaasa projektidest kustutatud rühma aadresside nimekiri";
                    CreateArchiveDebugText.Text = "Loo silumisfail";
                    
                    OngletParametresGeneraux.Header = "Üldised seaded";
                    OngletDebug.Header = "Silmamine";
                    break;

                // Finnois
                case "FI":
                    SettingsWindowTopTitle.Text = "Asetukset";
                    TranslationTitle.Text = "Käännös";
                    EnableTranslationCheckBox.Content = "Ota käännös käyttöön";
                    DeeplApiKeyText.Text = "DeepL API-avain:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Napsauta tästä saadaksesi ilmaisen avaimen)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Ota kielen automaattinen tunnistus käyttöön käännöstä varten";
                    TranslationSourceLanguageComboBoxText.Text = "Käännöksen lähdekieli:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Käännöksen kohdekieli:";

                    GroupAddressManagementTitle.Text = "Ryhmäosoitteiden hallinta";
                    RemoveUnusedAddressesCheckBox.Content = "Poista käyttämättömät osoitteet";

                    AppSettingsTitle.Text = "Sovelluksen asetukset";
                    ThemeTextBox.Text = "Teema:";
                    LightThemeComboBoxItem.Content = "Vaalea (oletus)";
                    DarkThemeComboBoxItem.Content = "Tumma";

                    AppLanguageTextBlock.Text = "Sovelluksen kieli:";

                    SaveButtonText.Text = "Tallenna";
                    CancelButtonText.Text = "Peruuta";
                    
                    MenuDebug.Text = "Vianmääritysvalikko";
                    AddInfosOsCheckBox.Content = "Sisällytä käyttöjärjestelmän tiedot";
                    AddInfosHardCheckBox.Content = "Sisällytä laitteistotiedot";
                    AddImportedFilesCheckBox.Content = "Sisällytä käynnistyksen jälkeen tuodut tiedostot";
                    IncludeAddressListCheckBox.Content = "Sisällytä projektien poistetut ryhmäosoitteet";
                    CreateArchiveDebugText.Text = "Luo vianmääritystiedosto";
                    
                    OngletParametresGeneraux.Header = "Yleiset asetukset";
                    OngletDebug.Header = "Vianmääritys";
                    break;

                // Hongrois
                case "HU":
                    SettingsWindowTopTitle.Text = "Beállítások";
                    TranslationTitle.Text = "Fordítás";
                    EnableTranslationCheckBox.Content = "Fordítás engedélyezése";
                    DeeplApiKeyText.Text = "DeepL API kulcs:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kattintson ide, hogy ingyenes kulcsot szerezzen)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Automatikus nyelvfelismerés engedélyezése a fordításhoz";
                    TranslationSourceLanguageComboBoxText.Text = "Forrásnyelv a fordításhoz:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Cél nyelv a fordításhoz:";

                    GroupAddressManagementTitle.Text = "Csoportcím kezelés";
                    RemoveUnusedAddressesCheckBox.Content = "Nem használt címek eltávolítása";

                    AppSettingsTitle.Text = "Alkalmazás beállításai";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Világos (alapértelmezett)";
                    DarkThemeComboBoxItem.Content = "Sötét";

                    AppLanguageTextBlock.Text = "Alkalmazás nyelve:";

                    SaveButtonText.Text = "Mentés";
                    CancelButtonText.Text = "Mégse";
                    
                    MenuDebug.Text = "Hibakeresési menü";
                    AddInfosOsCheckBox.Content = "Tartalmazza az operációs rendszer adatait";
                    AddInfosHardCheckBox.Content = "Tartalmazza a hardveradatokat";
                    AddImportedFilesCheckBox.Content = "Tartalmazza az indítás óta importált fájlokat";
                    IncludeAddressListCheckBox.Content = "Tartalmazza a projektek törölt csoportcímek listáját";
                    CreateArchiveDebugText.Text = "Hibakeresési fájl létrehozása";
                    
                    OngletParametresGeneraux.Header = "Általános beállítások";
                    OngletDebug.Header = "Hibakeresés";
                    break;

                // Indonésien
                case "ID":
                    SettingsWindowTopTitle.Text = "Pengaturan";
                    TranslationTitle.Text = "Terjemahan";
                    EnableTranslationCheckBox.Content = "Aktifkan Terjemahan";
                    DeeplApiKeyText.Text = "Kunci API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik di sini untuk mendapatkan kunci gratis)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/id/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktifkan deteksi bahasa otomatis untuk terjemahan";
                    TranslationSourceLanguageComboBoxText.Text = "Bahasa sumber untuk terjemahan:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Bahasa tujuan untuk terjemahan:";

                    GroupAddressManagementTitle.Text = "Manajemen Alamat Grup";
                    RemoveUnusedAddressesCheckBox.Content = "Hapus alamat yang tidak digunakan";

                    AppSettingsTitle.Text = "Pengaturan Aplikasi";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Terang (default)";
                    DarkThemeComboBoxItem.Content = "Gelap";

                    AppLanguageTextBlock.Text = "Bahasa Aplikasi:";

                    SaveButtonText.Text = "Simpan";
                    CancelButtonText.Text = "Batal";
                    
                    MenuDebug.Text = "Menu Debug";
                    AddInfosOsCheckBox.Content = "Sertakan informasi OS";
                    AddInfosHardCheckBox.Content = "Sertakan informasi hardware";
                    AddImportedFilesCheckBox.Content = "Sertakan file yang diimpor sejak peluncuran";
                    IncludeAddressListCheckBox.Content = "Sertakan daftar alamat grup yang dihapus pada proyek";
                    CreateArchiveDebugText.Text = "Buat file debug";
                    
                    OngletParametresGeneraux.Header = "Pengaturan umum";
                    OngletDebug.Header = "Debugging";
                    break;

                // Italien
                case "IT":
                    SettingsWindowTopTitle.Text = "Impostazioni";
                    TranslationTitle.Text = "Traduzione";
                    EnableTranslationCheckBox.Content = "Abilita Traduzione";
                    DeeplApiKeyText.Text = "Chiave API di DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Fai clic qui per ottenere una chiave gratuita)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/it/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Abilita rilevamento automatico della lingua per la traduzione";
                    TranslationSourceLanguageComboBoxText.Text = "Lingua di origine per la traduzione:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Lingua di destinazione per la traduzione:";

                    GroupAddressManagementTitle.Text = "Gestione degli indirizzi di gruppo";
                    RemoveUnusedAddressesCheckBox.Content = "Rimuovi indirizzi non utilizzati";

                    AppSettingsTitle.Text = "Impostazioni dell'applicazione";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Chiaro (predefinito)";
                    DarkThemeComboBoxItem.Content = "Scuro";

                    AppLanguageTextBlock.Text = "Lingua dell'applicazione:";

                    SaveButtonText.Text = "Salva";
                    CancelButtonText.Text = "Annulla";
                    
                    MenuDebug.Text = "Menu di debug";
                    AddInfosOsCheckBox.Content = "Includi informazioni sul sistema operativo";
                    AddInfosHardCheckBox.Content = "Includi informazioni sull'hardware";
                    AddImportedFilesCheckBox.Content = "Includi file importati dal lancio";
                    IncludeAddressListCheckBox.Content = "Includi elenco degli indirizzi di gruppo eliminati nei progetti";
                    CreateArchiveDebugText.Text = "Crea file di debug";
                    
                    OngletParametresGeneraux.Header = "Impostazioni generali";
                    OngletDebug.Header = "Debug";
                    break;

                // Japonais
                case "JA":
                    SettingsWindowTopTitle.Text = "設定";
                    TranslationTitle.Text = "翻訳";
                    EnableTranslationCheckBox.Content = "翻訳を有効にする";
                    DeeplApiKeyText.Text = "DeepL APIキー:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(ここをクリックして無料のキーを取得)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ja/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "翻訳のための自動言語検出を有効にする";
                    TranslationSourceLanguageComboBoxText.Text = "翻訳のソース言語:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "翻訳のターゲット言語:";

                    GroupAddressManagementTitle.Text = "グループアドレス管理";
                    RemoveUnusedAddressesCheckBox.Content = "使用されていないアドレスを削除";

                    AppSettingsTitle.Text = "アプリケーション設定";
                    ThemeTextBox.Text = "テーマ:";
                    LightThemeComboBoxItem.Content = "ライト（デフォルト）";
                    DarkThemeComboBoxItem.Content = "ダーク";

                    AppLanguageTextBlock.Text = "アプリケーションの言語:";

                    SaveButtonText.Text = "保存";
                    CancelButtonText.Text = "キャンセル";
                    
                    MenuDebug.Text = "デバッグメニュー";
                    AddInfosOsCheckBox.Content = "OS情報を含む";
                    AddInfosHardCheckBox.Content = "ハードウェア情報を含む";
                    AddImportedFilesCheckBox.Content = "起動以降にインポートされたファイルを含む";
                    IncludeAddressListCheckBox.Content = "プロジェクトで削除されたグループアドレスのリストを含む";
                    CreateArchiveDebugText.Text = "デバッグファイルを作成";
                    
                    OngletParametresGeneraux.Header = "一般設定";
                    OngletDebug.Header = "デバッグ";
                    break;

                // Coréen
                case "KO":
                    SettingsWindowTopTitle.Text = "설정";
                    TranslationTitle.Text = "번역";
                    EnableTranslationCheckBox.Content = "번역 활성화";
                    DeeplApiKeyText.Text = "DeepL API 키:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(무료 키를 받으려면 여기를 클릭하세요)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ko/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "번역을 위한 자동 언어 감지 활성화";
                    TranslationSourceLanguageComboBoxText.Text = "번역 소스 언어:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "번역 대상 언어:";

                    GroupAddressManagementTitle.Text = "그룹 주소 관리";
                    RemoveUnusedAddressesCheckBox.Content = "사용하지 않는 주소 제거";

                    AppSettingsTitle.Text = "애플리케이션 설정";
                    ThemeTextBox.Text = "테마:";
                    LightThemeComboBoxItem.Content = "라이트 (기본값)";
                    DarkThemeComboBoxItem.Content = "다크";

                    AppLanguageTextBlock.Text = "애플리케이션 언어:";

                    SaveButtonText.Text = "저장";
                    CancelButtonText.Text = "취소";
                    
                    MenuDebug.Text = "디버그 메뉴";
                    AddInfosOsCheckBox.Content = "운영 체제 정보를 포함";
                    AddInfosHardCheckBox.Content = "하드웨어 정보를 포함";
                    AddImportedFilesCheckBox.Content = "실행 후 가져온 파일 포함";
                    IncludeAddressListCheckBox.Content = "프로젝트에서 삭제된 그룹 주소 목록 포함";
                    CreateArchiveDebugText.Text = "디버그 파일 생성";
                    
                    OngletParametresGeneraux.Header = "일반 설정";
                    OngletDebug.Header = "디버깅";
                    break;

                // Letton
                case "LV":
                    SettingsWindowTopTitle.Text = "Iestatījumi";
                    TranslationTitle.Text = "Tulkot";
                    EnableTranslationCheckBox.Content = "Iespējot tulkošanu";
                    DeeplApiKeyText.Text = "DeepL API atslēga:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Noklikšķiniet šeit, lai iegūtu bezmaksas atslēgu)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Iespējot automātisku valodas noteikšanu tulkošanai";
                    TranslationSourceLanguageComboBoxText.Text = "Tulkojuma avota valoda:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Tulkojuma mērķa valoda:";

                    GroupAddressManagementTitle.Text = "Grupas adreses pārvaldība";
                    RemoveUnusedAddressesCheckBox.Content = "Noņemt neizmantotās adreses";

                    AppSettingsTitle.Text = "Lietotnes iestatījumi";
                    ThemeTextBox.Text = "Tēma:";
                    LightThemeComboBoxItem.Content = "Gaišs (noklusējums)";
                    DarkThemeComboBoxItem.Content = "Tumšs";

                    AppLanguageTextBlock.Text = "Lietotnes valoda:";

                    SaveButtonText.Text = "Saglabāt";
                    CancelButtonText.Text = "Atcelt";
                    
                    MenuDebug.Text = "Atkļūdošanas izvēlne";
                    AddInfosOsCheckBox.Content = "Iekļaut OS informāciju";
                    AddInfosHardCheckBox.Content = "Iekļaut aparatūras informāciju";
                    AddImportedFilesCheckBox.Content = "Iekļaut kopš palaišanas importētos failus";
                    IncludeAddressListCheckBox.Content = "Iekļaut projektu dzēsto grupu adrešu sarakstu";
                    CreateArchiveDebugText.Text = "Izveidot atkļūdošanas failu";
                    
                    OngletParametresGeneraux.Header = "Vispārīgie iestatījumi";
                    OngletDebug.Header = "Atkļūdošana";
                    break;

                // Lituanien
                case "LT":
                    SettingsWindowTopTitle.Text = "Nustatymai";
                    TranslationTitle.Text = "Vertimas";
                    EnableTranslationCheckBox.Content = "Įjungti vertimą";
                    DeeplApiKeyText.Text = "DeepL API raktas:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Spustelėkite čia, kad gautumėte nemokamą raktą)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Įjungti automatinį kalbos aptikimą vertimui";
                    TranslationSourceLanguageComboBoxText.Text = "Vertimo šaltinio kalba:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Vertimo tikslinė kalba:";

                    GroupAddressManagementTitle.Text = "Grupės adresų valdymas";
                    RemoveUnusedAddressesCheckBox.Content = "Pašalinti nenaudojamus adresus";

                    AppSettingsTitle.Text = "Programėlės nustatymai";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Šviesi (numatytoji)";
                    DarkThemeComboBoxItem.Content = "Tamsi";

                    AppLanguageTextBlock.Text = "Programėlės kalba:";

                    SaveButtonText.Text = "Išsaugoti";
                    CancelButtonText.Text = "Atšaukti";
                    
                    MenuDebug.Text = "Derinimo meniu";
                    AddInfosOsCheckBox.Content = "Įtraukti OS informaciją";
                    AddInfosHardCheckBox.Content = "Įtraukti aparatūros informaciją";
                    AddImportedFilesCheckBox.Content = "Įtraukti nuo paleidimo importuotus failus";
                    IncludeAddressListCheckBox.Content = "Įtraukti iš projektų ištrintų grupių adresų sąrašą";
                    CreateArchiveDebugText.Text = "Sukurti derinimo failą";
                    
                    OngletParametresGeneraux.Header = "Bendrieji nustatymai";
                    OngletDebug.Header = "Derinimas";
                    break;

                // Norvégien
                case "NB":
                    SettingsWindowTopTitle.Text = "Innstillinger";
                    TranslationTitle.Text = "Oversettelse";
                    EnableTranslationCheckBox.Content = "Aktiver oversettelse";
                    DeeplApiKeyText.Text = "DeepL API-nøkkel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikk her for å få en gratis nøkkel)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktiver automatisk språkgjenkjenning for oversettelse";
                    TranslationSourceLanguageComboBoxText.Text = "Kildespråk for oversettelse:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Målspråk for oversettelse:";

                    GroupAddressManagementTitle.Text = "Administrasjon av gruppeadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrukte adresser";

                    AppSettingsTitle.Text = "Programinnstillinger";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Lys (standard)";
                    DarkThemeComboBoxItem.Content = "Mørk";

                    AppLanguageTextBlock.Text = "Applikasjonsspråk:";

                    SaveButtonText.Text = "Lagre";
                    CancelButtonText.Text = "Avbryt";
                    
                    MenuDebug.Text = "Feilsøkingsmeny";
                    AddInfosOsCheckBox.Content = "Inkluder OS-informasjon";
                    AddInfosHardCheckBox.Content = "Inkluder maskinvareinformasjon";
                    AddImportedFilesCheckBox.Content = "Inkluder filer importert siden oppstart";
                    IncludeAddressListCheckBox.Content = "Inkluder listen over slettede gruppeadresser i prosjekter";
                    CreateArchiveDebugText.Text = "Opprett feilsøkingsfil";
                    
                    OngletParametresGeneraux.Header = "Generelle innstillinger";
                    OngletDebug.Header = "Feilsøking";
                    break;

                // Néerlandais
                case "NL":
                    SettingsWindowTopTitle.Text = "Instellingen";
                    TranslationTitle.Text = "Vertaling";
                    EnableTranslationCheckBox.Content = "Vertaling inschakelen";
                    DeeplApiKeyText.Text = "DeepL API-sleutel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik hier om een gratis sleutel te krijgen)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/nl/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Automatische taalherkenning voor vertaling inschakelen";
                    TranslationSourceLanguageComboBoxText.Text = "Bron taal voor vertaling:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Doeltaal voor vertaling:";

                    GroupAddressManagementTitle.Text = "Groepsadresbeheer";
                    RemoveUnusedAddressesCheckBox.Content = "Verwijder ongebruikte adressen";

                    AppSettingsTitle.Text = "Applicatie-instellingen";
                    ThemeTextBox.Text = "Thema:";
                    LightThemeComboBoxItem.Content = "Licht (standaard)";
                    DarkThemeComboBoxItem.Content = "Donker";

                    AppLanguageTextBlock.Text = "Applicatietaal:";

                    SaveButtonText.Text = "Opslaan";
                    CancelButtonText.Text = "Annuleren";
                    
                    MenuDebug.Text = "Debug-menu";
                    AddInfosOsCheckBox.Content = "OS-informatie opnemen";
                    AddInfosHardCheckBox.Content = "Hardware-informatie opnemen";
                    AddImportedFilesCheckBox.Content = "Opgenomen geïmporteerde bestanden sinds de lancering";
                    IncludeAddressListCheckBox.Content = "Lijst met verwijderde groepsadressen in projecten opnemen";
                    CreateArchiveDebugText.Text = "Maak een debug-bestand";
                    
                    OngletParametresGeneraux.Header = "Algemene instellingen";
                    OngletDebug.Header = "Foutopsporing";
                    break;

                // Polonais
                case "PL":
                    SettingsWindowTopTitle.Text = "Ustawienia";
                    TranslationTitle.Text = "Tłumaczenie";
                    EnableTranslationCheckBox.Content = "Włącz tłumaczenie";
                    DeeplApiKeyText.Text = "Klucz API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknij tutaj, aby uzyskać darmowy klucz)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/pl/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Włącz automatyczne wykrywanie języka do tłumaczenia";
                    TranslationSourceLanguageComboBoxText.Text = "Język źródłowy do tłumaczenia:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Język docelowy do tłumaczenia:";

                    GroupAddressManagementTitle.Text = "Zarządzanie adresami grup";
                    RemoveUnusedAddressesCheckBox.Content = "Usuń nieużywane adresy";

                    AppSettingsTitle.Text = "Ustawienia aplikacji";
                    ThemeTextBox.Text = "Motyw:";
                    LightThemeComboBoxItem.Content = "Jasny (domyślny)";
                    DarkThemeComboBoxItem.Content = "Ciemny";

                    AppLanguageTextBlock.Text = "Język aplikacji:";

                    SaveButtonText.Text = "Zapisz";
                    CancelButtonText.Text = "Anuluj";
                    
                    MenuDebug.Text = "Menu debugowania";
                    AddInfosOsCheckBox.Content = "Uwzględnij informacje o systemie operacyjnym";
                    AddInfosHardCheckBox.Content = "Uwzględnij informacje o sprzęcie";
                    AddImportedFilesCheckBox.Content = "Uwzględnij pliki zaimportowane od uruchomienia";
                    IncludeAddressListCheckBox.Content = "Uwzględnij listę usuniętych adresów grupowych w projektach";
                    CreateArchiveDebugText.Text = "Utwórz plik debugowania";
                    
                    OngletParametresGeneraux.Header = "Ustawienia ogólne";
                    OngletDebug.Header = "Debugowanie";
                    break;

                // Portugais
                case "PT":
                    SettingsWindowTopTitle.Text = "Configurações";
                    TranslationTitle.Text = "Tradução";
                    EnableTranslationCheckBox.Content = "Ativar tradução";
                    DeeplApiKeyText.Text = "Chave API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Clique aqui para obter uma chave gratuita)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/pt/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Ativar detecção automática de idioma para tradução";
                    TranslationSourceLanguageComboBoxText.Text = "Idioma de origem para tradução:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Idioma de destino para tradução:";

                    GroupAddressManagementTitle.Text = "Gerenciamento de endereços de grupo";
                    RemoveUnusedAddressesCheckBox.Content = "Remover endereços não utilizados";

                    AppSettingsTitle.Text = "Configurações do aplicativo";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Claro (padrão)";
                    DarkThemeComboBoxItem.Content = "Escuro";

                    AppLanguageTextBlock.Text = "Idioma do aplicativo:";

                    SaveButtonText.Text = "Salvar";
                    CancelButtonText.Text = "Cancelar";
                    
                    MenuDebug.Text = "Menu de depuração";
                    AddInfosOsCheckBox.Content = "Incluir informações do sistema operacional";
                    AddInfosHardCheckBox.Content = "Incluir informações de hardware";
                    AddImportedFilesCheckBox.Content = "Incluir arquivos importados desde o lançamento";
                    IncludeAddressListCheckBox.Content = "Incluir lista de endereços de grupo excluídos em projetos";
                    CreateArchiveDebugText.Text = "Criar arquivo de depuração";
                    
                    OngletParametresGeneraux.Header = "Configurações gerais";
                    OngletDebug.Header = "Depuração";
                    break;

                // Roumain
                case "RO":
                    SettingsWindowTopTitle.Text = "Setări";
                    TranslationTitle.Text = "Traducere";
                    EnableTranslationCheckBox.Content = "Activează traducerea";
                    DeeplApiKeyText.Text = "Cheie API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Faceți clic aici pentru a obține o cheie gratuită)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Activează detectarea automată a limbii pentru traducere";
                    TranslationSourceLanguageComboBoxText.Text = "Limbă sursă pentru traducere:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Limbă destinație pentru traducere:";

                    GroupAddressManagementTitle.Text = "Gestionarea adreselor de grup";
                    RemoveUnusedAddressesCheckBox.Content = "Eliminați adresele neutilizate";

                    AppSettingsTitle.Text = "Setările aplicației";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Luminoasă (implicit)";
                    DarkThemeComboBoxItem.Content = "Întunecată";

                    AppLanguageTextBlock.Text = "Limba aplicației:";

                    SaveButtonText.Text = "Salvează";
                    CancelButtonText.Text = "Anulează";
                    
                    MenuDebug.Text = "Meniu depanare";
                    AddInfosOsCheckBox.Content = "Includeți informații despre sistemul de operare";
                    AddInfosHardCheckBox.Content = "Includeți informații despre hardware";
                    AddImportedFilesCheckBox.Content = "Includeți fișierele importate de la lansare";
                    IncludeAddressListCheckBox.Content = "Includeți lista adreselor de grup șterse în proiecte";
                    CreateArchiveDebugText.Text = "Creați fișierul de depanare";
                    
                    OngletParametresGeneraux.Header = "Setări generale";
                    OngletDebug.Header = "Depanare";
                    break;

                // Slovaque
                case "SK":
                    SettingsWindowTopTitle.Text = "Nastavenia";
                    TranslationTitle.Text = "Preklad";
                    EnableTranslationCheckBox.Content = "Povoliť preklad";
                    DeeplApiKeyText.Text = "DeepL API kľúč:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknutím sem získate bezplatný kľúč)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Povoliť automatické rozpoznávanie jazyka pre preklad";
                    TranslationSourceLanguageComboBoxText.Text = "Zdrojový jazyk pre preklad:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Cieľový jazyk pre preklad:";

                    GroupAddressManagementTitle.Text = "Správa skupinových adries";
                    RemoveUnusedAddressesCheckBox.Content = "Odstrániť nepoužívané adresy";

                    AppSettingsTitle.Text = "Nastavenia aplikácie";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Svetlý (predvolený)";
                    DarkThemeComboBoxItem.Content = "Tmavý";

                    AppLanguageTextBlock.Text = "Jazyk aplikácie:";

                    SaveButtonText.Text = "Uložiť";
                    CancelButtonText.Text = "Zrušiť";
                    
                    MenuDebug.Text = "Ladiace menu";
                    AddInfosOsCheckBox.Content = "Zahrnúť informácie o operačnom systéme";
                    AddInfosHardCheckBox.Content = "Zahrnúť informácie o hardvéri";
                    AddImportedFilesCheckBox.Content = "Zahrnúť súbory importované od spustenia";
                    IncludeAddressListCheckBox.Content = "Zahrnúť zoznam odstránených skupinových adries v projektoch";
                    CreateArchiveDebugText.Text = "Vytvoriť súbor na ladenie";
                    
                    OngletParametresGeneraux.Header = "Všeobecné nastavenia";
                    OngletDebug.Header = "Ladenie";
                    break;

                // Slovène
                case "SL":
                    SettingsWindowTopTitle.Text = "Nastavitve";
                    TranslationTitle.Text = "Prevod";
                    EnableTranslationCheckBox.Content = "Omogoči prevajanje";
                    DeeplApiKeyText.Text = "DeepL API ključ:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite tukaj za brezplačen ključ)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Omogoči samodejno zaznavanje jezika za prevajanje";
                    TranslationSourceLanguageComboBoxText.Text = "Izvorni jezik za prevod:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Ciljni jezik za prevod:";

                    GroupAddressManagementTitle.Text = "Upravljanje naslovov skupine";
                    RemoveUnusedAddressesCheckBox.Content = "Odstrani neuporabljene naslove";

                    AppSettingsTitle.Text = "Nastavitve aplikacije";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Svetlo (privzeto)";
                    DarkThemeComboBoxItem.Content = "Temno";

                    AppLanguageTextBlock.Text = "Jezik aplikacije:";

                    SaveButtonText.Text = "Shrani";
                    CancelButtonText.Text = "Prekliči";
                    
                    MenuDebug.Text = "Meni za odpravljanje napak";
                    AddInfosOsCheckBox.Content = "Vključi informacije o operacijskem sistemu";
                    AddInfosHardCheckBox.Content = "Vključi informacije o strojni opremi";
                    AddImportedFilesCheckBox.Content = "Vključi uvožene datoteke od zagona";
                    IncludeAddressListCheckBox.Content = "Vključi seznam izbrisanih naslovov skupin v projektih";
                    CreateArchiveDebugText.Text = "Ustvari datoteko za odpravljanje napak";
                    
                    OngletParametresGeneraux.Header = "Splošne nastavitve";
                    OngletDebug.Header = "Odpravljanje napak";
                    break;

                // Suédois
                case "SV":
                    SettingsWindowTopTitle.Text = "Inställningar";
                    TranslationTitle.Text = "Översättning";
                    EnableTranslationCheckBox.Content = "Aktivera översättning";
                    DeeplApiKeyText.Text = "DeepL API-nyckel:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klicka här för att få en gratis nyckel)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/sv/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktivera automatisk språkigenkänning för översättning";
                    TranslationSourceLanguageComboBoxText.Text = "Källspråk för översättning:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Målspråk för översättning:";

                    GroupAddressManagementTitle.Text = "Hantera gruppadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Ta bort oanvända adresser";

                    AppSettingsTitle.Text = "Programinställningar";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Ljus (standard)";
                    DarkThemeComboBoxItem.Content = "Mörk";

                    AppLanguageTextBlock.Text = "Applikationsspråk:";

                    SaveButtonText.Text = "Spara";
                    CancelButtonText.Text = "Avbryt";
                    
                    MenuDebug.Text = "Felsökningsmeny";
                    AddInfosOsCheckBox.Content = "Inkludera OS-information";
                    AddInfosHardCheckBox.Content = "Inkludera hårdvaruinformation";
                    AddImportedFilesCheckBox.Content = "Inkludera importerade filer sedan start";
                    IncludeAddressListCheckBox.Content = "Inkludera lista över raderade gruppadresser i projekt";
                    CreateArchiveDebugText.Text = "Skapa felsökningsfil";
                    
                    OngletParametresGeneraux.Header = "Allmänna inställningar";
                    OngletDebug.Header = "Felsökning";
                    break;

                // Turc
                case "TR":
                    SettingsWindowTopTitle.Text = "Ayarlar";
                    TranslationTitle.Text = "Çeviri";
                    EnableTranslationCheckBox.Content = "Çeviriyi etkinleştir";
                    DeeplApiKeyText.Text = "DeepL API anahtarı:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Ücretsiz anahtar almak için buraya tıklayın)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/tr/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Çeviri için otomatik dil algılamayı etkinleştir";
                    TranslationSourceLanguageComboBoxText.Text = "Çeviri için kaynak dil:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Çeviri için hedef dil:";

                    GroupAddressManagementTitle.Text = "Grup adres yönetimi";
                    RemoveUnusedAddressesCheckBox.Content = "Kullanılmayan adresleri kaldır";

                    AppSettingsTitle.Text = "Uygulama Ayarları";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Açık (varsayılan)";
                    DarkThemeComboBoxItem.Content = "Koyu";

                    AppLanguageTextBlock.Text = "Uygulama Dili:";

                    SaveButtonText.Text = "Kaydet";
                    CancelButtonText.Text = "İptal";
                    
                    MenuDebug.Text = "Hata Ayıklama Menüsü";
                    AddInfosOsCheckBox.Content = "OS bilgilerini ekle";
                    AddInfosHardCheckBox.Content = "Donanım bilgilerini ekle";
                    AddImportedFilesCheckBox.Content = "Başlangıçtan bu yana içe aktarılan dosyaları ekle";
                    IncludeAddressListCheckBox.Content = "Projelerde silinen grup adresleri listesini ekle";
                    CreateArchiveDebugText.Text = "Hata ayıklama dosyası oluştur";
                    
                    OngletParametresGeneraux.Header = "Genel Ayarlar";
                    OngletDebug.Header = "Hata Ayıklama";
                    break;

                // Ukrainien
                case "UK":
                    SettingsWindowTopTitle.Text = "Налаштування";
                    TranslationTitle.Text = "Переклад";
                    EnableTranslationCheckBox.Content = "Увімкнути переклад";
                    DeeplApiKeyText.Text = "Ключ API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Натисніть тут, щоб отримати безкоштовний ключ)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/uk/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Увімкнути автоматичне визначення мови для перекладу";
                    TranslationSourceLanguageComboBoxText.Text = "Мова джерела для перекладу:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Мова призначення для перекладу:";

                    GroupAddressManagementTitle.Text = "Управління адресами групи";
                    RemoveUnusedAddressesCheckBox.Content = "Видалити невикористані адреси";

                    AppSettingsTitle.Text = "Налаштування додатка";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Світла (за замовчуванням)";
                    DarkThemeComboBoxItem.Content = "Темна";

                    AppLanguageTextBlock.Text = "Мова додатка:";

                    SaveButtonText.Text = "Зберегти";
                    CancelButtonText.Text = "Скасувати";
                    
                    MenuDebug.Text = "Меню налагодження";
                    AddInfosOsCheckBox.Content = "Включити інформацію про ОС";
                    AddInfosHardCheckBox.Content = "Включити інформацію про апаратне забезпечення";
                    AddImportedFilesCheckBox.Content = "Включити файли, імпортовані з моменту запуску";
                    IncludeAddressListCheckBox.Content = "Включити список видалених групових адрес у проектах";
                    CreateArchiveDebugText.Text = "Створити файл налагодження";
                    
                    OngletParametresGeneraux.Header = "Загальні налаштування";
                    OngletDebug.Header = "Відлагодження";
                    break;
                
                // Russe
                case "RU":
                    SettingsWindowTopTitle.Text = "Настройки";
                    TranslationTitle.Text = "Перевод";
                    EnableTranslationCheckBox.Content = "Включить перевод";
                    DeeplApiKeyText.Text = "Ключ API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Нажмите здесь, чтобы получить бесплатный ключ)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ru/pro-api");
    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Включить автоматическое определение языка для перевода";
                    TranslationSourceLanguageComboBoxText.Text = "Исходный язык для перевода:";
    
                    TranslationDestinationLanguageComboBoxText.Text = "Язык назначения для перевода:";

                    GroupAddressManagementTitle.Text = "Управление адресами групп";
                    RemoveUnusedAddressesCheckBox.Content = "Удалить неиспользуемые адреса";

                    AppSettingsTitle.Text = "Настройки приложения";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Светлая (по умолчанию)";
                    DarkThemeComboBoxItem.Content = "Темная";

                    AppLanguageTextBlock.Text = "Язык приложения:";

                    SaveButtonText.Text = "Сохранить";
                    CancelButtonText.Text = "Отменить";
                    
                    MenuDebug.Text = "Меню отладки";
                    AddInfosOsCheckBox.Content = "Включить информацию о ОС";
                    AddInfosHardCheckBox.Content = "Включить информацию о оборудовании";
                    AddImportedFilesCheckBox.Content = "Включить файлы, импортированные с момента запуска";
                    IncludeAddressListCheckBox.Content = "Включить список удаленных групповых адресов в проектах";
                    CreateArchiveDebugText.Text = "Создать файл отладки";
                    
                    OngletParametresGeneraux.Header = "Общие настройки";
                    OngletDebug.Header = "Отладка";
                    break;

                // Chinois simplifié
                case "ZH":
                    SettingsWindowTopTitle.Text = "设置";
                    TranslationTitle.Text = "翻译";
                    EnableTranslationCheckBox.Content = "启用翻译";
                    DeeplApiKeyText.Text = "DeepL API 密钥:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(点击这里获取免费密钥)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/zh/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "启用自动语言检测进行翻译";
                    TranslationSourceLanguageComboBoxText.Text = "翻译源语言：";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "翻译目标语言：";

                    GroupAddressManagementTitle.Text = "组地址管理";
                    RemoveUnusedAddressesCheckBox.Content = "删除未使用的地址";

                    AppSettingsTitle.Text = "应用设置";
                    ThemeTextBox.Text = "主题：";
                    LightThemeComboBoxItem.Content = "浅色（默认）";
                    DarkThemeComboBoxItem.Content = "深色";

                    AppLanguageTextBlock.Text = "应用语言：";

                    SaveButtonText.Text = "保存";
                    CancelButtonText.Text = "取消";
                    
                    MenuDebug.Text = "调试菜单";
                    AddInfosOsCheckBox.Content = "包括操作系统信息";
                    AddInfosHardCheckBox.Content = "包括硬件信息";
                    AddImportedFilesCheckBox.Content = "包括启动以来导入的文件";
                    IncludeAddressListCheckBox.Content = "包括项目中已删除的组地址列表";
                    CreateArchiveDebugText.Text = "创建调试文件";
                    
                    OngletParametresGeneraux.Header = "常规设置";
                    OngletDebug.Header = "调试";
                    break;

	            // Langue par défaut (français)
                default:
		            SettingsWindowTopTitle.Text = "Paramètres";
                    TranslationTitle.Text = "Traduction";
                    EnableTranslationCheckBox.Content = "Activer la traduction";
                    DeeplApiKeyText.Text = "Clé API DeepL :";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Cliquez ici pour obtenir une clé gratuitement)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/fr/pro-api");
                    
                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Activer la détection automatique de la langue pour la traduction";
                    TranslationSourceLanguageComboBoxText.Text = "Langue source de la traduction:";
                    
                    TranslationDestinationLanguageComboBoxText.Text = "Langue de destination de la traduction:";

                    GroupAddressManagementTitle.Text = "Gestion des adresses de groupe";
                    RemoveUnusedAddressesCheckBox.Content = "Supprimer les adresses inutilisées";

                    AppSettingsTitle.Text = "Paramètres de l'application";
                    ThemeTextBox.Text = "Thème:";
                    LightThemeComboBoxItem.Content = "Clair (par défaut)";
                    DarkThemeComboBoxItem.Content = "Sombre";

                    AppLanguageTextBlock.Text = "Langue de l'application:";
                    
                    MenuDebug.Text = "Menu de débogage";
                    AddInfosOsCheckBox.Content = "Inclure les informations sur le système d'exploitation";
                    AddInfosHardCheckBox.Content = "Inclure les informations sur le matériel de l'ordinateur";
                    AddImportedFilesCheckBox.Content = "Inclure les fichiers des projets importés depuis le lancement";
                    IncludeAddressListCheckBox.Content = "Inclure la liste des adresses de groupe supprimées sur les projets";

                    CreateArchiveDebugText.Text = "Créer le fichier de débogage";

                    OngletParametresGeneraux.Header = "Paramètres généraux";
                    OngletDebug.Header = "Débogage";
                    
                    OngletInformations.Header = "Informations";
                    InformationsText.Text =
                        "Application réalisée par\n\nNathan BRUGIERE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE et Maxime OLIVEIRA LOPES\n\nPartenariat entre l'INSA de Toulouse et l'UCRM.";
                        
                    SaveButtonText.Text = "Enregistrer";
                    CancelButtonText.Text = "Annuler";
                    
                    break;
            }

            string textColor;
            string darkBackgroundColor;
            string deepDarkBackgroundColor;
            string pathColor;
            string textboxBackgroundColor;

            var checkboxStyle = (Style)FindResource("CheckboxLightThemeStyle");
            Brush borderBrush;
            
            if (EnableLightTheme) // Si le thème clair est actif,
            {
                textColor = "#000000";
                darkBackgroundColor = "#F5F5F5";
                deepDarkBackgroundColor = "#FFFFFF";
                pathColor = "#D7D7D7";
                textboxBackgroundColor = "#FFFFFF";
                borderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b8b8b8"));
                
                TranslationSourceLanguageComboBox.Style = (Style)FindResource("LightComboBoxStyle");
                TranslationLanguageDestinationComboBox.Style = (Style)FindResource("LightComboBoxStyle");
                ThemeComboBox.Style = (Style)FindResource("LightComboBoxStyle");
                AppLanguageComboBox.Style = (Style)FindResource("LightComboBoxStyle");
                ScaleSlider.Style = (Style)FindResource("LightSlider");
                SaveButton.Style = (Style)FindResource("BottomButtonLight");
                CancelButton.Style = (Style)FindResource("BottomButtonLight");
                CreateArchiveDebugButton.Style = (Style)FindResource("BottomButtonLight");

                OngletParametresGeneraux.Style = (Style)FindResource("LightOnglet");
                OngletDebug.Style = (Style)FindResource("LightOnglet");
                OngletInformations.Style = (Style)FindResource("LightOnglet");
                IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked! ? 
                    MainWindow.ConvertStringColor(textColor) : new SolidColorBrush(Colors.Gray);
            }
            else // Sinon, on met le thème sombre
            {
                textColor = "#E3DED4";
                darkBackgroundColor = "#313131";
                deepDarkBackgroundColor = "#262626";
                pathColor = "#434343";
                textboxBackgroundColor = "#262626";
                checkboxStyle = (Style)FindResource("CheckboxDarkThemeStyle");
                borderBrush = (Brush)FindResource("DarkThemeCheckBoxBorderBrush");
                
                TranslationSourceLanguageComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
                TranslationLanguageDestinationComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
                ThemeComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
                AppLanguageComboBox.Style = (Style)FindResource("DarkComboBoxStyle");
                ScaleSlider.Style = (Style)FindResource("DarkSlider");
                SaveButton.Style = (Style)FindResource("BottomButtonDark");
                CancelButton.Style = (Style)FindResource("BottomButtonDark");
                CreateArchiveDebugButton.Style = (Style)FindResource("BottomButtonDark");

                OngletParametresGeneraux.Style = (Style)FindResource("DarkOnglet");
                OngletDebug.Style = (Style)FindResource("DarkOnglet");
                OngletInformations.Style = (Style)FindResource("DarkOnglet");
                IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked! ? 
                    MainWindow.ConvertStringColor(textColor) : new SolidColorBrush(Colors.DimGray);

            }
            
            // Définition des brush pour les divers éléments
            var textColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textColor));
            
            // Arrière plan de la fenêtre
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor));
            
            // En-tête de la fenêtre
            SettingsIconPath1.Brush = textColorBrush;
            SettingsIconPath2.Brush = textColorBrush;
            SettingsWindowTopTitle.Foreground = textColorBrush;
            HeaderPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pathColor));
                
            // Corps de la fenêtre
            MainContentBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pathColor));
            MainContentPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
            TranslationTitle.Foreground = textColorBrush;
            EnableTranslationCheckBox.Foreground = textColorBrush;
            DeeplApiKeyText.Foreground = textColorBrush;
            EnableAutomaticTranslationLangDetectionCheckbox.Foreground = textColorBrush;
            TranslationSourceLanguageComboBoxText.Foreground = textColorBrush;
            TranslationDestinationLanguageComboBoxText.Foreground = textColorBrush;
            TranslationLanguageDestinationComboBox.Foreground = textColorBrush;
            GroupAddressManagementTitle.Foreground = textColorBrush;
            RemoveUnusedAddressesCheckBox.Foreground = textColorBrush;
            AppSettingsTitle.Foreground = textColorBrush;
            ThemeTextBox.Foreground = textColorBrush;
            AppLanguageTextBlock.Foreground = textColorBrush;

            EnableTranslationCheckBox.Style = checkboxStyle;
            EnableAutomaticTranslationLangDetectionCheckbox.Style = checkboxStyle;
            RemoveUnusedAddressesCheckBox.Style = checkboxStyle;

            DeeplApiKeyTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textboxBackgroundColor));
            DeeplApiKeyTextBox.BorderBrush = borderBrush;
            DeeplApiKeyTextBox.Foreground = textColorBrush;
                
            // Pied de page avec les boutons save et cancel
            SettingsWindowFooter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
            FooterPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pathColor));
            CancelButtonDrawing.Brush = textColorBrush;
            CancelButtonText.Foreground = textColorBrush;
            SaveButtonDrawing.Brush = textColorBrush;
            SaveButtonText.Foreground = textColorBrush;
            CreateArchiveDebugText.Foreground = textColorBrush;
            
            
            // Menu debug
            ControlOnglet.BorderBrush = borderBrush;
            DebugPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
            InformationsPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
            AddInfosOsCheckBox.Style = checkboxStyle;
            AddInfosHardCheckBox.Style = checkboxStyle;
            AddImportedFilesCheckBox.Style = checkboxStyle;
            IncludeAddressListCheckBox.Style = checkboxStyle;
            AddInfosOsCheckBox.Foreground = textColorBrush;
            AddInfosHardCheckBox.Foreground = textColorBrush;
            AddImportedFilesCheckBox.Foreground = textColorBrush;
            IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked ? textColorBrush : new SolidColorBrush(Colors.DimGray);
            
            OngletParametresGeneraux.Foreground = textColorBrush;
            OngletDebug.Foreground = textColorBrush;
            DebugBrush1.Brush = textColorBrush;
            DebugBrush2.Brush = textColorBrush;
            OngletInformations.Foreground = textColorBrush;
            InformationsText.Foreground = EnableLightTheme ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.DimGray);

            IncludeAddressListCheckBox.IsEnabled = (bool)AddImportedFilesCheckBox.IsChecked!;
            
            
            foreach (ComboBoxItem item in TranslationLanguageDestinationComboBox.Items)
            {
                item.Foreground = item.IsSelected ? new SolidColorBrush(Colors.White) : textColorBrush;
                item.Background = EnableLightTheme ? new SolidColorBrush(Colors.White) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor));
            }
            
            foreach (ComboBoxItem item in ThemeComboBox.Items)
            {
                item.Foreground = item.IsSelected ? new SolidColorBrush(Colors.White) : textColorBrush;
                item.Background = EnableLightTheme ? new SolidColorBrush(Colors.White) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor)); 
            }
            
            foreach (ComboBoxItem item in AppLanguageComboBox.Items)
            {
                item.Foreground = item.IsSelected ? new SolidColorBrush(Colors.White) : textColorBrush;
                item.Background = EnableLightTheme ? new SolidColorBrush(Colors.White) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor)); 
            }
            
            foreach (ComboBoxItem item in TranslationSourceLanguageComboBox.Items)
            {
                item.Foreground = item.IsSelected ? new SolidColorBrush(Colors.White) : textColorBrush;
                item.Background = EnableLightTheme ? new SolidColorBrush(Colors.White) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkBackgroundColor)); 
            }
            
              
            if (!EnableDeeplTranslation)
            {
                DeeplApiKeyTextBox.IsEnabled = false;
                EnableAutomaticTranslationLangDetectionCheckbox.IsEnabled = false;
                TranslationSourceLanguageComboBox.IsEnabled = false;
                TranslationLanguageDestinationComboBox.IsEnabled = false;
                Hyperlink.IsEnabled = false;

                if (EnableLightTheme)
                {
                    Hyperlink.Foreground = new SolidColorBrush(Colors.Gray);
                    DeeplApiKeyText.Foreground = new SolidColorBrush(Colors.Gray);
                    DeeplApiKeyTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
                    EnableAutomaticTranslationLangDetectionCheckbox.Foreground = new SolidColorBrush(Colors.Gray);
                    TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Gray);
                    TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Gray);
                    TranslationLanguageDestinationComboBox.Foreground = new SolidColorBrush(Colors.Gray);
                    TranslationDestinationLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    Hyperlink.Foreground = new SolidColorBrush(Colors.DimGray);
                    DeeplApiKeyText.Foreground = new SolidColorBrush(Colors.DimGray);
                    DeeplApiKeyTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
                    DeeplApiKeyTextBox.Foreground = new SolidColorBrush(Colors.DarkGray);
                    EnableAutomaticTranslationLangDetectionCheckbox.Foreground = new SolidColorBrush(Colors.DimGray);
                    TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.DimGray);
                    TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.DimGray);
                    TranslationLanguageDestinationComboBox.Foreground = new SolidColorBrush(Colors.DimGray);
                    TranslationDestinationLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.DimGray);
                }
                
            }
            
        }
        
        
        // Fonction permettant de détecter le thème de windows (clair/sombre)
        /// <summary>
        /// Detects the current Windows theme (light or dark).
        /// Attempts to read the theme setting from the Windows registry.
        /// Returns true if the theme is light, false if it is dark.
        /// If an error occurs or the registry value is not found, defaults to true (light theme).
        /// </summary>
        /// <returns>
        /// A boolean value indicating whether the Windows theme is light (true) or dark (false).
        /// </returns>
        private bool DetectWindowsTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var registryValue = key?.GetValue("AppsUseLightTheme");
                    
                    if (registryValue is int value)
                    {
                        return value == 1; 
                    }
                }
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"Error: An error occured while trying to retrieve the windows theme : {ex.Message}. Thème par défaut : clair.");
                return true; // Default to dark theme in case of error
            }

            return true;
        }


        // Fonction permettant de détecter la langue de Windows. Si elle est supportée par l'application,
        // on retourne le code de la langue correspondante.
        /// <summary>
        /// Detects the current Windows language.
        /// If the language is supported by the application, it returns the corresponding language code.
        /// Otherwise, it returns an empty string.
        /// </summary>
        /// <returns>
        /// A string representing the Windows language code if supported; otherwise, an empty string.
        /// </returns>
        /// <remarks>
        /// This method reads the "LocaleName" value from the Windows registry under "Control Panel\International".
        /// It extracts the language code from this value and checks if it is in the set of valid language codes.
        /// If an error occurs during the registry access or if the language code is not supported, an empty string is returned.
        /// </remarks>
        private string DetectWindowsLanguage()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International"))
                {
                    var registryValue = key?.GetValue("LocaleName");
                    
                    if (registryValue != null)
                    {
                        // Créer un HashSet avec tous les codes de langue pris en charge par la traduction de l'application
                        var validLanguageCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {
                            "AR", "BG", "CS", "DA", "DE", "EL", "EN", "ES", "ET", "FI",
                            "HU", "ID", "IT", "JA", "KO", "LT", "LV", "NB", "NL", "PL",
                            "PT", "RO", "RU", "SK", "SL", "SV", "TR", "UK", "ZH", "FR"
                        };
                    
                        var localeName = registryValue.ToString();

                        // Extraire les deux premières lettres de localeName pour obtenir le code de langue
                        var languageCode = localeName?.Split('-')[0].ToUpper();

                        // Vérifier si le code de langue extrait est dans le HashSet
                        if (languageCode != null && validLanguageCodes.Contains(languageCode))
                        {
                            App.ConsoleAndLogWriteLine($"Langue windows détectée : {languageCode}");
                            return languageCode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"Error: An error occured while reading the windows language from registry : {ex.Message}");
                return "";
            }

            return "";
        }


        // ----- GESTION DES BOUTONS -----
        // Fonction s'exécutant lors du clic sur le bouton sauvegarder
        /// <summary>
        /// Handles the save button click event by retrieving and validating settings from the settings window,
        /// saving them, and updating relevant UI elements.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
            EnableDeeplTranslation = (bool) EnableTranslationCheckBox.IsChecked!;
            DeeplKey = EncryptStringToBytes(DeeplApiKeyTextBox.Text);
            TranslationDestinationLang = TranslationLanguageDestinationComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            TranslationSourceLang = TranslationSourceLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            EnableAutomaticSourceLangDetection = (bool)EnableAutomaticTranslationLangDetectionCheckbox.IsChecked!;
            RemoveUnusedGroupAddresses = (bool) RemoveUnusedAddressesCheckBox.IsChecked!;
            EnableLightTheme = LightThemeComboBoxItem.IsSelected;
            AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            AppScaleFactor = (int) ScaleSlider.Value;
            
            // Mise à jour éventuellement du contenu pour update la langue du menu
            UpdateWindowContents();
            
            // Mise à jour de l'échelle de toutes les fenêtres
            var scaleFactor = AppScaleFactor / 100f;
            if (scaleFactor <= 1f)
            {
                ApplyScaling(scaleFactor-0.1f);
            }
            else
            {
                ApplyScaling(scaleFactor-0.2f);
            }
            App.DisplayElements!.MainWindow.ApplyScaling(scaleFactor);
            App.DisplayElements.ConsoleWindow.ApplyScaling(scaleFactor);
            App.DisplayElements.GroupAddressRenameWindow.ApplyScaling(scaleFactor-0.2f);
            
            // Mise à jour de la fenêtre de renommage des adresses de groupe
            App.DisplayElements?.GroupAddressRenameWindow.UpdateWindowContents();
            
            // Mise à jour de la fenêtre principale
            App.DisplayElements?.MainWindow.UpdateWindowContents();
            
            // Si on a activé la traduction deepl
            if (EnableDeeplTranslation)
            {
                // On vérifie la validité de la clé API
                var (isValid, errorMessage) = GroupAddressNameCorrector.CheckDeeplKey();
                GroupAddressNameCorrector.ValidDeeplKey = isValid;
                
                // Si la clé est incorrecte
                if (!GroupAddressNameCorrector.ValidDeeplKey)
                {
                    // Traduction de l'en-tête de la fenêtre d'avertissement
                    var warningMessage = AppLang switch
                    {
                        // Arabe
                        "AR" => "تحذير",
                        // Bulgare
                        "BG" => "Предупреждение",
                        // Tchèque
                        "CS" => "Varování",
                        // Danois
                        "DA" => "Advarsel",
                        // Allemand
                        "DE" => "Warnung",
                        // Grec
                        "EL" => "Προειδοποίηση",
                        // Anglais
                        "EN" => "Warning",
                        // Espagnol
                        "ES" => "Advertencia",
                        // Estonien
                        "ET" => "Hoiatus",
                        // Finnois
                        "FI" => "Varoitus",
                        // Hongrois
                        "HU" => "Figyelmeztetés",
                        // Indonésien
                        "ID" => "Peringatan",
                        // Italien
                        "IT" => "Avvertimento",
                        // Japonais
                        "JA" => "警告",
                        // Coréen
                        "KO" => "경고",
                        // Letton
                        "LV" => "Brīdinājums",
                        // Lituanien
                        "LT" => "Įspėjimas",
                        // Norvégien
                        "NB" => "Advarsel",
                        // Néerlandais
                        "NL" => "Waarschuwing",
                        // Polonais
                        "PL" => "Ostrzeżenie",
                        // Portugais
                        "PT" => "Aviso",
                        // Roumain
                        "RO" => "Avertizare",
                        // Russe
                        "RU" => "Предупреждение",
                        // Slovaque
                        "SK" => "Upozornenie",
                        // Slovène
                        "SL" => "Opozorilo",
                        // Suédois
                        "SV" => "Varning",
                        // Turc
                        "TR" => "Uyarı",
                        // Ukrainien
                        "UK" => "Попередження",
                        // Chinois simplifié
                        "ZH" => "警告",
                        // Cas par défaut (français)
                        _ => "Avertissement"
                    };

                    // Message d'erreur
                    MessageBox.Show($"{errorMessage}", warningMessage, MessageBoxButton.OK, MessageBoxImage.Warning);
                    
                    // Décochage de la traduction deepL dans la fenêtre
                    EnableDeeplTranslation = false;
                    
                    // Mise à jour de la case cochable
                    UpdateWindowContents();
                }
            }
            
            // Sauvegarde des paramètres dans le fichier appSettings
            App.ConsoleAndLogWriteLine($"Saving application settings at {Path.GetFullPath("./appSettings")}");
            SaveSettings();
            App.ConsoleAndLogWriteLine("Settings saved successfully");
            
            // Masquage de la fenêtre de paramètres
            Hide();
        }

        
        // Fonction s'exécutant lors du clic sur le bouton annuler
        /// <summary>
        /// Handles the cancel button click event by restoring previous settings and hiding the settings window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateWindowContents(); // Restauration des paramètres précédents dans la fenêtre de paramétrage
            Hide(); // Masquage de la fenêtre de paramétrage
        }

        
        // ----- GESTION DE DES CASES A COCHER -----
        // Fonction s'activant quand on coche l'activation de la traduction DeepL
        /// <summary>
        /// Handles the event triggered when the DeepL translation feature is enabled by showing related UI elements and adjusting the window size.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void EnableTranslation(object sender, RoutedEventArgs e)
        {
            // Activation du lien hypertexte
            Hyperlink.IsEnabled = true;
            // On affiche la textbox qui permet à l'utilisateur d'entrer la clé API DeepL
            DeeplApiKeyTextBox.IsEnabled = true;
            // On affiche le menu déroulant de sélection de la langue de destination de la traduction
            TranslationLanguageDestinationComboBox.IsEnabled = true;
            // On affiche le checkmark de la détection automatique de la langue source de la traduction
            EnableAutomaticTranslationLangDetectionCheckbox.IsEnabled = true;
            
            TranslationSourceLanguageComboBox.IsEnabled = (!EnableAutomaticSourceLangDetection)||(bool)(!EnableAutomaticTranslationLangDetectionCheckbox.IsChecked!);

            if (EnableLightTheme)
            {
                Hyperlink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4071B4"));

                DeeplApiKeyText.Foreground = new SolidColorBrush(Colors.Black);
                DeeplApiKeyTextBox.Background = new SolidColorBrush(Colors.White);

                TranslationLanguageDestinationComboBox.Foreground = new SolidColorBrush(Colors.Black);
                TranslationDestinationLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Black);

                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Black);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Black);

                EnableAutomaticTranslationLangDetectionCheckbox.Foreground = new SolidColorBrush(Colors.Black);
                
                if (TranslationSourceLanguageComboBox.IsEnabled)
                {
                    //TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Black);
                    TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    //TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                    TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Gray);
                    TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
            else
            {
                Hyperlink.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4071B4"));

                DeeplApiKeyText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4")); 
                DeeplApiKeyTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));
                DeeplApiKeyTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
                
                TranslationLanguageDestinationComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));
                TranslationDestinationLanguageComboBoxText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));

                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));

                EnableAutomaticTranslationLangDetectionCheckbox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));

                if (TranslationSourceLanguageComboBox.IsEnabled)
                {
                    //TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                    TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));
                    TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3DED4"));
                }
                else
                {
                    //TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                    TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.DimGray);
                    TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.DimGray);
                }
            }

        }

        
        // Fonction s'activant quand on décoche l'activation de la traduction DeepL
        /// <summary>
        /// Handles the event triggered when the DeepL translation feature is disabled by hiding related UI elements and adjusting the window size.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void DisableTranslation(object sender, RoutedEventArgs e)
        {
            // Désactivation du lien hypertexte
            Hyperlink.IsEnabled = false;
            // On masque la textbox qui permet à l'utilisateur d'entrer la clé API DeepL
            DeeplApiKeyTextBox.IsEnabled = false;
            // On masque le menu déroulant de sélection de la langue de destination de la traduction
            TranslationLanguageDestinationComboBox.IsEnabled = false;
            // On masque le checkmark de la détection automatique de la langue source de la traduction
            EnableAutomaticTranslationLangDetectionCheckbox.IsEnabled = false;
            TranslationSourceLanguageComboBox.IsEnabled = false;

            if (EnableLightTheme)
            {
                Hyperlink.Foreground = new SolidColorBrush(Colors.Gray);

                DeeplApiKeyText.Foreground = new SolidColorBrush(Colors.Gray);
                DeeplApiKeyTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));

                TranslationLanguageDestinationComboBox.Foreground = new SolidColorBrush(Colors.Gray);
                TranslationDestinationLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Gray);

                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Gray);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Gray);

                EnableAutomaticTranslationLangDetectionCheckbox.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                Hyperlink.Foreground = new SolidColorBrush(Colors.DimGray);

                DeeplApiKeyText.Foreground = new SolidColorBrush(Colors.DimGray);
                DeeplApiKeyTextBox.Foreground = new SolidColorBrush(Colors.DarkGray);

                TranslationLanguageDestinationComboBox.Foreground = new SolidColorBrush(Colors.DimGray);
                TranslationDestinationLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.DimGray);

                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.DimGray);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.DimGray);

                EnableAutomaticTranslationLangDetectionCheckbox.Foreground = new SolidColorBrush(Colors.DimGray);
            }
        }
        
        
        // Fonction s'activant quand on coche l'activation de la traduction DeepL
        /// <summary>
        /// Handles the event triggered when automatic translation language detection is enabled by hiding the source language selection UI elements and adjusting the window size.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void EnableAutomaticTranslationLangDetection(object sender, RoutedEventArgs e)
        {
            // On masque le menu déroulant de sélection de la langue de traduction
            TranslationSourceLanguageComboBox.IsEnabled = false;

            if (EnableLightTheme)
            {
                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Gray);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.DimGray);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.DimGray);
            }
        }

        
        /// <summary>
        /// Enables the <see cref="IncludeAddressListCheckBox"/> control and sets its foreground color based on the theme.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void EnableIncludeAddress(object sender, RoutedEventArgs e)
        {
            IncludeAddressListCheckBox.IsEnabled = true;

            IncludeAddressListCheckBox.Foreground = EnableLightTheme ? 
                new SolidColorBrush(Colors.Black) : MainWindow.ConvertStringColor("#E3DED4");
        }

        
        /// <summary>
        /// Disables the <see cref="IncludeAddressListCheckBox"/> control, unchecks it, and sets its foreground color based on the theme.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void DisableIncludeAddress(object sender, RoutedEventArgs e)
        {
            IncludeAddressListCheckBox.IsEnabled = false;
            IncludeAddressListCheckBox.IsChecked = false;

            IncludeAddressListCheckBox.Foreground = EnableLightTheme ? 
                new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.DimGray);
        }

        
        /// <summary>
        /// Handles the <see cref="TabControl.SelectionChanged"/> event to adjust the visibility of buttons based on the selected tab.
        /// </summary>
        /// <param name="sender">The source of the event, expected to be a <see cref="TabControl"/>.</param>
        /// <param name="e">The event data.</param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is not TabControl) return;
            
            var selectedTab = (sender as TabControl)?.SelectedItem as TabItem;
            switch (selectedTab)
            {
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletParametresGeneraux.Header:
                    SaveButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    CreateArchiveDebugButton.Visibility = Visibility.Collapsed;
                    break;
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletDebug.Header:
                    SaveButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    CreateArchiveDebugButton.Visibility = Visibility.Visible;
                    break;
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletInformations.Header:
                    SaveButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Hidden;
                    CreateArchiveDebugButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        
        
        /// <summary>
        /// Creates a debug report based on the state of various checkboxes and the include address list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CreateDebugReport(object sender, RoutedEventArgs e)
        {
            var includeOsInfo = AddInfosOsCheckBox.IsChecked;
            var includeHardwareInfo = AddInfosHardCheckBox.IsChecked;
            var includeImportedProjects = AddImportedFilesCheckBox.IsChecked;
            var includeRemovedGroupAddressList = (bool)IncludeAddressListCheckBox.IsChecked! && (bool)AddImportedFilesCheckBox.IsChecked!;
            
            ProjectFileManager.CreateDebugArchive((bool)includeOsInfo!, (bool)includeHardwareInfo!, (bool)includeImportedProjects!, includeRemovedGroupAddressList!);
        }

        
        // Fonction s'activant quand on décoche l'activation de la traduction DeepL
        /// <summary>
        /// Handles the event triggered when automatic translation language detection is disabled by showing the source language selection UI elements and adjusting the window size.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void DisableAutomaticTranslationLangDetection(object sender, RoutedEventArgs e)
        {
            // On affiche le menu déroulant de sélection de la langue de traduction
            TranslationSourceLanguageComboBox.IsEnabled = true;
            
            if (EnableLightTheme)
            {
                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.Black);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                TranslationSourceLanguageComboBox.Foreground = new SolidColorBrush(Colors.White);
                TranslationSourceLanguageComboBoxText.Foreground = new SolidColorBrush(Colors.White);
            }
            
        }
        
        
        // ----- GESTION DE L'ENCRYPTION -----
        // Fonction permettant d'encrypter un string donné avec une clé et un iv créés dynamiquement
        // Attention : cette fonction permet de stocker UN SEUL string (ici la clé deepL) à la fois. Si vous encryptez un autre string,
        // le combo clé/iv pour le premier string sera perdu.
        /// <summary>
        /// Encrypts the provided plain text string using dynamically generated AES key and IV, and stores the encrypted key and IV in files.
        /// Note: This function is designed to store only one string at a time (e.g., the DeepL key). Encrypting another string will overwrite the key/IV used for the first string.
        /// </summary>
        /// <param name="plainText">The plain text string to be encrypted.</param>
        /// <returns>A byte array containing the encrypted data.</returns>
        private static byte[] EncryptStringToBytes(string plainText)
        {
            // Générer une nouvelle clé et IV pour le cryptage
            using (var aesAlg = Aes.Create())
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
        /// <summary>
        /// Decrypts the provided byte array using encrypted AES key and IV stored in the application files, and returns the decrypted string.
        /// If the key or IV files are missing or cannot be read, an empty string is returned.
        /// </summary>
        /// <param name="encryptedString">The byte array containing the encrypted data to be decrypted.</param>
        /// <returns>The decrypted string, or an empty string if decryption fails.</returns>
        public string DecryptStringFromBytes(byte[] encryptedString)
        {
            // Si les fichiers des clés n'existent pas, on retourne un string vide
            if (!File.Exists("./ek") || !File.Exists("./ei"))
            {
                App.ConsoleAndLogWriteLine("Error: encryption keys could not be retrieved to decrypt the DeepL API Key. Restoring default value.");
                return "";
            }

            var encryptedKey = "";
            var encryptedIv = "";

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
                App.ConsoleAndLogWriteLine(
                    "Error: decryption stream is null or the keys are null. Data could not be decrypted.");
                return "";
            }
            // Si le format des clés est invalide
            catch (FormatException)
            {
                App.ConsoleAndLogWriteLine(
                    "Error: the encryption keys are not in the right format. Data could not be decrypted.");
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
                App.ConsoleAndLogWriteLine(
                    "Error: I/O error occured while attempting to decrypt the data. Data could not be decrypted.");
                return "";
            }
            catch (CryptographicException)
            {
                App.ConsoleAndLogWriteLine("Error: error while attempting to decrypt the DeepL API Key. Data could not be decrypted.");
                return "";
            }
        }
        
        
        // Fonction permettant d'encrypter et de stocker dans un fichier la clé principale de chiffrement
        /// <summary>
        /// Encrypts the provided main encryption key using DPAPI and stores it in a file.
        /// The encryption is performed for the current user and session. If any errors occur during encryption or file operations, 
        /// appropriate error messages are logged, and the operation is aborted.
        /// </summary>
        /// <param name="mainkey">The byte array containing the main encryption key to be encrypted and stored.</param>
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
            // Si le cryptage échoue
            catch (CryptographicException)
            {
                App.ConsoleAndLogWriteLine("Error: Could not encrypt main key.");
                return;
            }
            // Si l'OS ne supporte pas cet algorithme de cryptage
            catch (NotSupportedException)
            {
                App.ConsoleAndLogWriteLine($"Error: The system does not support the methods used to encrypt the main key.");
                return;
            }
            // Si le process n'a plus assez de RAM disponible pour le cryptage
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
        /// <summary>
        /// Retrieves the encrypted main encryption key from a file, decrypts it using DPAPI, and returns it as a base64-encoded string.
        /// If any errors occur during file reading or decryption, appropriate error messages are logged, and an empty string is returned.
        /// </summary>
        /// <returns>A base64-encoded string representation of the decrypted main encryption key, or an empty string if an error occurs.</returns>
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
        /// <summary>
        /// Encrypts a given key or IV using the main encryption key retrieved from the file. 
        /// The encryption is performed using AES with a zeroed IV. The encrypted result is then returned as a base64-encoded string.
        /// If any errors occur during encryption, appropriate error messages are logged, and an empty string is returned.
        /// </summary>
        /// <param name="keyOrIv">The key or IV to be encrypted, provided as a base64-encoded string.</param>
        /// <returns>A base64-encoded string representing the encrypted key or IV, or an empty string if an error occurs.</returns>
        private static string EncryptKeyOrIv(string keyOrIv)
        {
            using (var aesAlg = Aes.Create())
            {
                if (RetrieveAndDecryptMainKey() == "")
                {
                    EncryptAndStoreMainKey(Convert.FromBase64String(GenerateRandomKey(32)));
                }
                
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
        /// <summary>
        /// Decrypts a given base64-encoded encrypted key or IV using the main encryption key retrieved from the file.
        /// The decryption is performed using AES with a zeroed IV. If any errors occur during decryption, appropriate error messages are logged,
        /// and an empty string is returned.
        /// </summary>
        /// <param name="cipherText">The base64-encoded encrypted key or IV to be decrypted.</param>
        /// <returns>A string representing the decrypted key or IV, or an empty string if an error occurs.</returns>
        private static string DecryptKeyOrIv(string cipherText)
        {
            using (var aesAlg = Aes.Create())
            {
                try
                {
                    aesAlg.Key = Convert.FromBase64String(RetrieveAndDecryptMainKey());
                    aesAlg.IV = new byte[16]; // IV de 16 octets rempli de zéros
                }
                catch (ArgumentNullException)
                {
                    App.ConsoleAndLogWriteLine(
                        "Error: The decrypted main key is incorrect. The decryption keys for the DeepL API key could not be retrieved.");
                    return "";
                }
                catch (CryptographicException)
                {
                    App.ConsoleAndLogWriteLine(
                        "Error: The decrypted main key is incorrect. The decryption keys for the DeepL API key could not be retrieved.");
                    try
                    {
                        File.Delete("./emk");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Traductions pour le texte du MessageBox
                        var messageBoxText = App.DisplayElements?.SettingsWindow!.AppLang switch
                        {
                            // Arabe
                            "AR" => "خطأ: لا يمكن الوصول إلى ملف 'ei' أو 'ek' أو 'appSettings'. يرجى التحقق من عدم كونها للقراءة فقط وإعادة المحاولة، أو تشغيل البرنامج كمسؤول.\nرمز الخطأ: 1",
                            // Bulgare
                            "BG" => "Грешка: не може да се достъпи файл 'ei', 'ek' или 'appSettings'. Моля, уверете се, че не са само за четене и опитайте отново или стартирайте програмата като администратор.\nКод на грешка: 1",
                            // Tchèque
                            "CS" => "Chyba: Nelze přistoupit k souboru 'ei', 'ek' nebo 'appSettings'. Zkontrolujte, zda nejsou pouze pro čtení, a zkuste to znovu, nebo spusťte program jako správce.\nChybový kód: 1",
                            // Danois
                            "DA" => "Fejl: Kan ikke få adgang til fil 'ei', 'ek' eller 'appSettings'. Kontroller, at de ikke er skrivebeskyttede, og prøv igen, eller kør programmet som administrator.\nFejlkode: 1",
                            // Allemand
                            "DE" => "Fehler: Zugriff auf die Datei 'ei', 'ek' oder 'appSettings' nicht möglich. Bitte überprüfen Sie, ob die Dateien schreibgeschützt sind, und versuchen Sie es erneut oder starten Sie das Programm als Administrator.\nFehlercode: 1",
                            // Grec
                            "EL" => "Σφάλμα: Δεν είναι δυνατή η πρόσβαση στο αρχείο 'ei', 'ek' ή 'appSettings'. Ελέγξτε αν είναι μόνο για ανάγνωση και προσπαθήστε ξανά, ή εκκινήστε το πρόγραμμα ως διαχειριστής.\nΚωδικός σφάλματος: 1",
                            // Anglais
                            "EN" => "Error: Unable to access file 'ei', 'ek', or 'appSettings'. Please check that they are not read-only and try again, or run the program as an administrator.\nError code: 1",
                            // Espagnol
                            "ES" => "Error: No se puede acceder al archivo 'ei', 'ek' o 'appSettings'. Verifique que no sean de solo lectura e intente de nuevo, o ejecute el programa como administrador.\nCódigo de error: 1",
                            // Estonien
                            "ET" => "Viga: ei saa juurde pääseda failile 'ei', 'ek' või 'appSettings'. Kontrollige, et need ei oleks ainult lugemiseks ja proovige uuesti või käivitage programm administraatorina.\nViga kood: 1",
                            // Finnois
                            "FI" => "Virhe: Ei pääse käsiksi tiedostoon 'ei', 'ek' tai 'appSettings'. Tarkista, etteivät ne ole vain luku-, ja yritä uudelleen tai käynnistä ohjelma järjestelmänvalvojana.\nVirhekoodi: 1",
                            // Hongrois
                            "HU" => "Hiba: Nem lehet hozzáférni az 'ei', 'ek' vagy 'appSettings' fájlhoz. Kérjük, ellenőrizze, hogy nem csak olvashatóak-e, és próbálja újra, vagy indítsa el a programot rendszergazdaként.\nHibakód: 1",
                            // Indonésien
                            "ID" => "Kesalahan: Tidak dapat mengakses file 'ei', 'ek', atau 'appSettings'. Harap periksa agar file-file tersebut tidak hanya dapat dibaca dan coba lagi, atau jalankan program sebagai administrator.\nKode kesalahan: 1",
                            // Italien
                            "IT" => "Errore: Impossibile accedere ai file 'ei', 'ek' o 'appSettings'. Controlla che non siano di sola lettura e riprova, oppure esegui il programma come amministratore.\nCodice errore: 1",
                            // Japonais
                            "JA" => "エラー: 'ei', 'ek' または 'appSettings' にアクセスできません。読み取り専用でないことを確認し、再試行するか、管理者としてプログラムを実行してください。\nエラーコード: 1",
                            // Coréen
                            "KO" => "오류: 파일 'ei', 'ek' 또는 'appSettings'에 접근할 수 없습니다. 읽기 전용이 아닌지 확인하고 다시 시도하거나 관리자로 프로그램을 실행하십시오.\n오류 코드: 1",
                            // Letton
                            "LV" => "Kļūda: nevar piekļūt failam 'ei', 'ek' vai 'appSettings'. Pārbaudiet, vai tie nav tikai lasāmi, un mēģiniet vēlreiz, vai arī palaidiet programmu kā administratoru.\nKļūdas kods: 1",
                            // Lituanien
                            "LT" => "Klaida: negalima pasiekti failo 'ei', 'ek' arba 'appSettings'. Patikrinkite, ar jie nėra tik skaitymo režimu, ir bandykite dar kartą, arba paleiskite programą kaip administratorius.\nKlaidos kodas: 1",
                            // Norvégien
                            "NB" => "Feil: Kan ikke få tilgang til filen 'ei', 'ek' eller 'appSettings'. Kontroller at de ikke er skrivebeskyttet, og prøv igjen, eller kjør programmet som administrator.\nFeilkode: 1",
                            // Néerlandais
                            "NL" => "Fout: Kan geen toegang krijgen tot bestand 'ei', 'ek' of 'appSettings'. Controleer of ze niet alleen-lezen zijn en probeer het opnieuw, of start het programma als administrator.\nFoutcode: 1",
                            // Polonais
                            "PL" => "Błąd: Nie można uzyskać dostępu do pliku 'ei', 'ek' lub 'appSettings'. Sprawdź, czy nie są tylko do odczytu, a następnie spróbuj ponownie, lub uruchom program jako administrator.\nKod błędu: 1",
                            // Portugais
                            "PT" => "Erro: Não é possível acessar o arquivo 'ei', 'ek' ou 'appSettings'. Verifique se eles não são somente leitura e tente novamente ou execute o programa como administrador.\nCódigo de erro: 1",
                            // Roumain
                            "RO" => "Eroare: Nu se poate accesa fișierul 'ei', 'ek' sau 'appSettings'. Verificați dacă nu sunt doar pentru citire și încercați din nou, sau rulați programul ca administrator.\nCod eroare: 1",
                            // Russe
                            "RU" => "Ошибка: невозможно получить доступ к файлу 'ei', 'ek' или 'appSettings'. Пожалуйста, проверьте, чтобы они не были доступными только для чтения, и попробуйте снова, или запустите программу от имени администратора.\nКод ошибки: 1",
                            // Slovaque
                            "SK" => "Chyba: Nie je možné pristupovať k súboru 'ei', 'ek' alebo 'appSettings'. Skontrolujte, či nie sú len na čítanie, a skúste to znova alebo spustite program ako správca.\nChybový kód: 1",
                            // Slovène
                            "SL" => "Napaka: Ni mogoče dostopati do datoteke 'ei', 'ek' ali 'appSettings'. Preverite, ali niso samo za branje, in poskusite znova, ali pa zaženite program kot skrbnik.\nKoda napake: 1",
                            // Suédois
                            "SV" => "Fel: Kan inte komma åt filen 'ei', 'ek' eller 'appSettings'. Kontrollera att de inte är skrivskyddade och försök igen, eller kör programmet som administratör.\nFelkod: 1",
                            // Turc
                            "TR" => "Hata: 'ei', 'ek' veya 'appSettings' dosyasına erişilemiyor. Dosyaların yalnızca okunabilir olmadığını kontrol edin ve tekrar deneyin veya programı yönetici olarak çalıştırın.\nHata kodu: 1",
                            // Ukrainien
                            "UK" => "Помилка: неможливо отримати доступ до файлів 'ei', 'ek' або 'appSettings'. Перевірте, чи не є вони тільки для читання, і спробуйте ще раз або запустіть програму від імені адміністратора.\nКод помилки: 1",
                            // Chinois simplifié
                            "ZH" => "错误：无法访问文件 'ei'、'ek' 或 'appSettings'。请检查它们是否为只读状态并重试，或以管理员身份运行程序。\n错误代码：1",
                            // Cas par défaut (français)
                            _ => "Erreur : impossible d'accéder au fichier 'ei', 'ek' ou 'appSettings'. Veuillez vérifier qu'ils ne sont pas en lecture seule et réessayer, ou démarrez le programme en tant qu'administrateur.\nCode erreur: 1"
                        };

                        // Traductions pour le titre de la MessageBox
                        var messageBoxCaption = App.DisplayElements?.SettingsWindow!.AppLang switch
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

                        // Affichage de la MessageBox avec les traductions
                        MessageBox.Show(messageBoxText, messageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);

                        Application.Current.Shutdown(1);
                    }
                    return "";
                }

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
        /// <summary>
        /// Generates a random key consisting of alphanumeric characters based on the specified length.
        /// If the length is zero or less, an empty string is returned.
        /// </summary>
        /// <param name="length">The length of the random key to be generated.</param>
        /// <returns>A string representing the generated random key. If the length is zero or less, an empty string is returned.</returns>
        private static string GenerateRandomKey(int length)
        {
            // Tableau contenant tous les caractères admissibles dans une clé de cryptage
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
        /// <summary>
        /// Handles the click event on a hyperlink by attempting to open the URL in the default web browser.
        /// If an error occurs during the process, an error message is logged.
        /// </summary>
        /// <param name="sender">The source of the event, typically the hyperlink control.</param>
        /// <param name="e">Event data containing the URI to navigate to.</param>
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

        
        // ----- GESTION DES INPUTS CLAVIER/SOURIS -----
        // Fonction permettant d'effectuer des actions quand une touche spécifique du clavier est appuyée
        /// <summary>
        /// Handles the key down events in the settings window. Depending on the key pressed, 
        /// either restores previous settings and hides the window, or saves new settings and then hides the window.
        /// </summary>
        /// <param name="sender">The source of the event, typically the settings window.</param>
        /// <param name="e">Event data containing information about the key pressed.</param>
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
                    TranslationDestinationLang = TranslationLanguageDestinationComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
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
        
        
        // Fonction gérant le clic sur l'en-tête de la fenêtre de paramètres, de manière que l'on puisse
        // déplacer la fenêtre avec la souris.
        /// <summary>
        /// Initiates a drag operation when the left mouse button is pressed on the header, allowing the window to be moved.
        /// </summary>
        /// <param name="sender">The source of the event, typically the header control.</param>
        /// <param name="e">Event data containing information about the mouse button event.</param>
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        private void ApplyScaling(double scale)
        {
            SettingsWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
            Height = 725 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 725 * scale;
            Width = 500 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 500 * scale;
        }
    }
}