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
    /// <summary>
    ///  Window used to set the application settings.
    /// </summary>
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


        /// <summary>
        /// Strings to preserve during the correction of group addresses. If a string follows the format 'test*', all variations such as 'test1', 'test2', etc., will be retained.
        /// </summary>
        public List<string> StringsToAdd { get; private set; } // Chaines de caractères à conserver lors de la correction des adresses de groupe




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
            StringsToAdd = new List<string>();

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
                        
                        case "strings to keep in corrected addresses":
                            // Récupération de chaque string et ajout à la liste
                            foreach (var st in value.Split(','))
                            {
                                if (!st.Trim().Equals("", StringComparison.OrdinalIgnoreCase)) StringsToAdd.Add(st.Trim());
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
            
            UpdateWindowContents(false, true, true); // Affichage des paramètres dans la fenêtre
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
                
                writer.Write("strings to keep in corrected addresses : ");
                if (StringsToAdd.Count == 0)
                {
                    writer.WriteLine();
                }
                else
                {
                    for (var i=0; i<StringsToAdd.Count; i++)
                    {
                        // Si c'est le dernier élément, on ne met pas de virgule et on saute une ligne
                        if (i == StringsToAdd.Count - 1)
                        {
                            writer.WriteLine($"{StringsToAdd[i]}");
                        }
                        // Sinon, on écrit les uns après les autres chaque élément séparé d'une virgule
                        else
                        {
                            writer.Write($"{StringsToAdd[i]}, ");
                        }
                    }
                }

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
        private void UpdateWindowContents(bool isClosing = false, bool langChanged = false, bool themeChanged = false)
        {
            EnableTranslationCheckBox.IsChecked = EnableDeeplTranslation; // Cochage/décochage

            if (File.Exists("./emk") && File.Exists("./ei") && File.Exists("./ek") && !isClosing) DeeplApiKeyTextBox.Text = DecryptStringFromBytes(DeeplKey); // Décryptage de la clé DeepL

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
            
            // Mise à jour de la liste des strings à conserver
            AddressKeepingTextbox.Text = ""; // On commence par réinitialiser
            for (var i=0; i<StringsToAdd.Count; i++)
            {
                AddressKeepingTextbox.Text += i == StringsToAdd.Count-1 ? $"{StringsToAdd[i]}" : $"{StringsToAdd[i]}, ";
            }

            // Traduction du menu settings
            if (langChanged) TranslateWindowContents();
            
            // Application du thème
            if (themeChanged) ApplyThemeToWindow();
        }


        // Fonction traduisant tous les textes de la fenêtre paramètres
        /// <summary>
        /// This function translates all the texts contained in the setting window to the application language
        /// </summary>
        private void TranslateWindowContents()
        {
            switch (AppLang)
            {
                // Arabe
                case "AR":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ar/pro-api");
                    
                    SettingsWindowTopTitle.Text = "الإعدادات";
                    TranslationTitle.Text = "ترجمة";
                    EnableTranslationCheckBox.Content = "تمكين الترجمة";
                    DeeplApiKeyText.Text = "مفتاح API DeepL:";
                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(انقر هنا للحصول على مفتاح مجاني)");
                    EnableAutomaticTranslationLangDetectionCheckbox.Content = "تمكين الكشف التلقائي عن اللغة للترجمة";
                    TranslationSourceLanguageComboBoxText.Text = "لغة المصدر للترجمة:";
                    TranslationDestinationLanguageComboBoxText.Text = "لغة الوجهة للترجمة:";
                    GroupAddressManagementTitle.Text = "إدارة عناوين المجموعة";
                    RemoveUnusedAddressesCheckBox.Content = "إزالة العناوين غير المستخدمة";
                    AppSettingsTitle.Text = "إعدادات التطبيق";
                    ThemeTextBox.Text = "الموضوع:";
                    LightThemeComboBoxItem.Content = "فاتح (افتراضي)";
                    DarkThemeComboBoxItem.Content = "داكن";
                    AppLanguageTextBlock.Text = "لغة التطبيق:";
                    MenuDebug.Text = "قائمة التصحيح";
                    AddInfosOsCheckBox.Content = "تضمين معلومات نظام التشغيل";
                    AddInfosHardCheckBox.Content = "تضمين معلومات الأجهزة";
                    AddImportedFilesCheckBox.Content = "تضمين الملفات المستوردة منذ بدء التشغيل";
                    IncludeAddressListCheckBox.Content = "تضمين قائمة العناوين المحذوفة من المشاريع";
                    CreateArchiveDebugText.Text = "إنشاء ملف التصحيح";
                    OngletDebug.Header = "تصحيح";
                    OngletInformations.Header = "معلومات";
                    InformationsText.Text = $"{App.AppName}" +
                                            $"\nالإصدار {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                            $"\nالبناء {App.AppBuild}" +
                                            $"\n" +
                                            $"\nبرنامج تم إنشاؤه كجزء من تدريب هندسي بواسطة طلاب INSA Toulouse:" +
                                            $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE و Maxime OLIVEIRA LOPES" +
                                            $"\n" +
                                            $"\nبإشراف:" +
                                            $"\nDidier BESSE (UCRM)" +
                                            $"\nThierry COPPOLA (UCRM)" +
                                            $"\nJean-François KLOTZ (LECS)" +
                                            $"\n" +
                                            $"\nشراكة بين المعهد الوطني للعلوم التطبيقية (INSA) في تولوز واتحاد Cepière Robert Monnier (UCRM)." +
                                            $"\n" +
                                            $"\nإنشاء: 06/2024 - 07/2024\n";
                    SaveButtonText.Text = "حفظ";
                    CancelButtonText.Text = "إلغاء";
                    
                    ScalingText.Text = "التحجيم:";
                    OngletParametresApplication.Header = "عام";
                    OngletCorrection.Header = "تصحيح";
                    AddressKeepingText.Text = "السلاسل المراد الاحتفاظ بها في العناوين أثناء التصحيح";
                    
                    NoteImportante.Text = "\nملاحظة هامة:";
                    NoteImportanteContenu.Text = "الاسم والشعارات وأي صورة مرتبطة بـ KNX هي ملك لا يتجزأ لجمعية KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("موقع جمعية KNX");
                    AddressKeepingTitle.Text = "الشموليات";
                    break;

                // Bulgare
                case "BG":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Настройки";
                    TranslationTitle.Text = "Превод";
                    EnableTranslationCheckBox.Content = "Активиране на превод";
                    DeeplApiKeyText.Text = "DeepL API ключ:";
                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Щракнете тук, за да получите безплатен ключ)");
                    EnableAutomaticTranslationLangDetectionCheckbox.Content = "Активиране на автоматичното откриване на езика за превод";
                    TranslationSourceLanguageComboBoxText.Text = "Изходен език за превод:";
                    TranslationDestinationLanguageComboBoxText.Text = "Език на превода:";
                    GroupAddressManagementTitle.Text = "Управление на груповите адреси";
                    RemoveUnusedAddressesCheckBox.Content = "Премахване на неизползваните адреси";
                    AppSettingsTitle.Text = "Настройки на приложението";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Светло (по подразбиране)";
                    DarkThemeComboBoxItem.Content = "Тъмно";
                    AppLanguageTextBlock.Text = "Език на приложението:";
                    MenuDebug.Text = "Меню за отстраняване на грешки";
                    AddInfosOsCheckBox.Content = "Включване на информация за операционната система";
                    AddInfosHardCheckBox.Content = "Включване на информация за хардуера на компютъра";
                    AddImportedFilesCheckBox.Content = "Включване на файлове, импортирани след стартиране";
                    IncludeAddressListCheckBox.Content = "Включване на списък с адреси на групи, премахнати от проекти";
                    CreateArchiveDebugText.Text = "Създаване на файл за отстраняване на грешки";
                    OngletDebug.Header = "Отстраняване на грешки";
                    OngletInformations.Header = "Информация";
                    InformationsText.Text = $"{App.AppName}" +
                                            $"\nВерсия {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                            $"\nИзграждане {App.AppBuild}" +
                                            $"\n" +
                                            $"\nСофтуер, създаден като част от инженерно стаж от студенти на INSA Toulouse:" +
                                            $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE и Maxime OLIVEIRA LOPES" +
                                            $"\n" +
                                            $"\nПод наблюдението на:" +
                                            $"\nDidier BESSE (UCRM)" +
                                            $"\nThierry COPPOLA (UCRM)" +
                                            $"\nJean-François KLOTZ (LECS)" +
                                            $"\n" +
                                            $"\nПартньорство между Националния институт по приложни науки (INSA) в Тулуза и Съюза Cepière Robert Monnier (UCRM)." +
                                            $"\n" +
                                            $"\nСъздаване: 06/2024 - 07/2024\n";
                    SaveButtonText.Text = "Запази";
                    CancelButtonText.Text = "Отказ";
                    
                    ScalingText.Text = "Мащабиране:";
                    OngletParametresApplication.Header = "Общ";
                    OngletCorrection.Header = "Корекция";
                    AddressKeepingText.Text = "Вериги за запазване в адресите по време на корекция";
                    
                    NoteImportante.Text = "\nВажна бележка:";
                    NoteImportanteContenu.Text = "името, логото и всички изображения, свързани с KNX, са неотменима собственост на асоциацията KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Уебсайт на асоциация KNX");
                    AddressKeepingTitle.Text = "Включвания";
                    break;

                // Tchèque
                case "CS":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/cs/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Nastavení";
                    TranslationTitle.Text = "Překlad";
                    EnableTranslationCheckBox.Content = "Povolit překlad";
                    DeeplApiKeyText.Text = "Klíč API DeepL:";
                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikněte sem pro získání bezplatného klíče)");
                    EnableAutomaticTranslationLangDetectionCheckbox.Content = "Povolit automatické rozpoznání jazyka pro překlad";
                    TranslationSourceLanguageComboBoxText.Text = "Výchozí jazyk překladu:";
                    TranslationDestinationLanguageComboBoxText.Text = "Cílový jazyk překladu:";
                    GroupAddressManagementTitle.Text = "Správa skupinových adres";
                    RemoveUnusedAddressesCheckBox.Content = "Odstranit nepoužívané adresy";
                    AppSettingsTitle.Text = "Nastavení aplikace";
                    ThemeTextBox.Text = "Motiv:";
                    LightThemeComboBoxItem.Content = "Světlý (výchozí)";
                    DarkThemeComboBoxItem.Content = "Tmavý";
                    AppLanguageTextBlock.Text = "Jazyk aplikace:";
                    MenuDebug.Text = "Nabídka ladění";
                    AddInfosOsCheckBox.Content = "Zahrnout informace o operačním systému";
                    AddInfosHardCheckBox.Content = "Zahrnout informace o hardwaru počítače";
                    AddImportedFilesCheckBox.Content = "Zahrnout soubory importované od spuštění";
                    IncludeAddressListCheckBox.Content = "Zahrnout seznam odstraněných skupinových adres v projektech";
                    CreateArchiveDebugText.Text = "Vytvořit soubor pro ladění";
                    OngletDebug.Header = "Ladění";
                    OngletInformations.Header = "Informace";
                    InformationsText.Text = $"{App.AppName}" +
                                            $"\nVerze {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                            $"\nBuild {App.AppBuild}" +
                                            $"\n" +
                                            $"\nSoftware vytvořený jako součást inženýrského stáže studenty INSA Toulouse:" +
                                            $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE a Maxime OLIVEIRA LOPES" +
                                            $"\n" +
                                            $"\nPod dohledem:" +
                                            $"\nDidier BESSE (UCRM)" +
                                            $"\nThierry COPPOLA (UCRM)" +
                                            $"\nJean-François KLOTZ (LECS)" +
                                            $"\n" +
                                            $"\nPartnerství mezi Národním institutem aplikovaných věd (INSA) v Toulouse a Union Cépière Robert Monnier (UCRM)." +
                                            $"\n" +
                                            $"\nVytvořeno: 06/2024 - 07/2024\n";
                    SaveButtonText.Text = "Uložit";
                    CancelButtonText.Text = "Zrušit";
                    
                    ScalingText.Text = "Měřítko:";
                    OngletParametresApplication.Header = "Obecné";
                    OngletCorrection.Header = "Korekce";
                    AddressKeepingText.Text = "Řetězce k uchování v adresách během korekce";
                    
                    NoteImportante.Text = "\nDůležitá poznámka:";
                    NoteImportanteContenu.Text = "název, loga a jakékoli obrázky související s KNX jsou neoddělitelným vlastnictvím asociace KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Webová stránka asociace KNX");
                    AddressKeepingTitle.Text = "Zahrnutí";
                    break;

                // Danois
                case "DA":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Indstillinger";
                    TranslationTitle.Text = "Oversættelse";
                    EnableTranslationCheckBox.Content = "Aktiver oversættelse";
                    DeeplApiKeyText.Text = "DeepL API-nøgle:";
                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik her for at få en gratis nøgle)");
                    EnableAutomaticTranslationLangDetectionCheckbox.Content = "Aktiver automatisk sprogregistrering til oversættelse";
                    TranslationSourceLanguageComboBoxText.Text = "Kildesprog til oversættelse:";
                    TranslationDestinationLanguageComboBoxText.Text = "Måloversættelsessprog:";
                    GroupAddressManagementTitle.Text = "Administration af gruppeadresser";
                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrugte adresser";
                    AppSettingsTitle.Text = "App-indstillinger";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Lys (standard)";
                    DarkThemeComboBoxItem.Content = "Mørk";
                    AppLanguageTextBlock.Text = "App-sprog:";
                    MenuDebug.Text = "Fejlfindingsmenu";
                    AddInfosOsCheckBox.Content = "Inkluder oplysninger om operativsystemet";
                    AddInfosHardCheckBox.Content = "Inkluder oplysninger om computerhardware";
                    AddImportedFilesCheckBox.Content = "Inkluder filer importeret siden opstart";
                    IncludeAddressListCheckBox.Content = "Inkluder liste over gruppeadresser slettet fra projekter";
                    CreateArchiveDebugText.Text = "Opret fejlfindingsfil";
                    OngletDebug.Header = "Fejlfindings";
                    OngletInformations.Header = "Information";
                    InformationsText.Text = $"{App.AppName}" +
                                            $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                            $"\nBuild {App.AppBuild}" +
                                            $"\n" +
                                            $"\nSoftware skabt som en del af en ingeniørpraktik af studerende fra INSA Toulouse:" +
                                            $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE og Maxime OLIVEIRA LOPES" +
                                            $"\n" +
                                            $"\nUnder vejledning af:" +
                                            $"\nDidier BESSE (UCRM)" +
                                            $"\nThierry COPPOLA (UCRM)" +
                                            $"\nJean-François KLOTZ (LECS)" +
                                            $"\n" +
                                            $"\nPartnerskab mellem National Institute of Applied Sciences (INSA) i Toulouse og Union Cépière Robert Monnier (UCRM)." +
                                            $"\n" +
                                            $"\nOprettelse: 06/2024 - 07/2024\n";
                    SaveButtonText.Text = "Gem";
                    CancelButtonText.Text = "Annuller";
                    
                    ScalingText.Text = "Skalering:";
                    OngletParametresApplication.Header = "Generel";
                    OngletCorrection.Header = "Korrektion";
                    AddressKeepingText.Text = "Kæder, der skal bevares i adresser under korrektion";
                    
                    NoteImportante.Text = "\nVigtig bemærkning:";
                    NoteImportanteContenu.Text = "navnet, logoerne og alle billeder relateret til KNX er uadskillelig ejendom tilhørende KNX-foreningen. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX-foreningens hjemmeside");
                    AddressKeepingTitle.Text = "Inklusioner";
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
                        "Automatische Spracherkennung für die Übersetzung aktivieren";
                    TranslationSourceLanguageComboBoxText.Text = "Quellsprache der Übersetzung:";

                    TranslationDestinationLanguageComboBoxText.Text = "Zielsprache der Übersetzung:";

                    GroupAddressManagementTitle.Text = "Gruppenadressverwaltung";
                    RemoveUnusedAddressesCheckBox.Content = "Nicht verwendete Adressen entfernen";

                    AppSettingsTitle.Text = "Anwendungseinstellungen";
                    ThemeTextBox.Text = "Thema:";
                    LightThemeComboBoxItem.Content = "Hell (Standard)";
                    DarkThemeComboBoxItem.Content = "Dunkel";

                    AppLanguageTextBlock.Text = "Anwendungssprache:";

                    MenuDebug.Text = "Debug-Menü";
                    AddInfosOsCheckBox.Content = "Betriebssysteminformationen hinzufügen";
                    AddInfosHardCheckBox.Content = "Hardwareinformationen hinzufügen";
                    AddImportedFilesCheckBox.Content = "Importierte Projektdateien seit dem Start hinzufügen";
                    IncludeAddressListCheckBox.Content = "Liste der gelöschten Gruppenadressen in Projekten hinzufügen";

                    CreateArchiveDebugText.Text = "Debug-Datei erstellen";

                    OngletDebug.Header = "Debuggen";
                    
                    OngletInformations.Header = "Informationen";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware im Rahmen eines Ingenieurpraktikums von Studenten der INSA Toulouse entwickelt:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE und Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nUnter der Aufsicht von:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerschaft zwischen dem Institut National des Sciences Appliquées (INSA) de Toulouse und der Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nUmsetzung: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Speichern";
                    CancelButtonText.Text = "Abbrechen";
                    
                    ScalingText.Text = "Skalierung:";
                    OngletParametresApplication.Header = "Allgemein";
                    OngletCorrection.Header = "Korrektur";
                    AddressKeepingText.Text = "Ketten, die in Adressen während der Korrektur beibehalten werden sollen";
                    
                    NoteImportante.Text = "\nWichtiger Hinweis:";
                    NoteImportanteContenu.Text = "der Name, die Logos und alle Bilder im Zusammenhang mit KNX sind unveräußerliches Eigentum der KNX-Vereinigung. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Website der KNX-Vereinigung");
                    AddressKeepingTitle.Text = "Inklusionen";
                    break;

                // Grec
                case "EL":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Ρυθμίσεις";
                    TranslationTitle.Text = "Μετάφραση";
                    EnableTranslationCheckBox.Content = "Ενεργοποίηση μετάφρασης";
                    DeeplApiKeyText.Text = "Κλειδί API DeepL:";
                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Κάντε κλικ εδώ για να αποκτήσετε ένα δωρεάν κλειδί)");
                    EnableAutomaticTranslationLangDetectionCheckbox.Content = "Ενεργοποίηση αυτόματης ανίχνευσης γλώσσας για μετάφραση";
                    TranslationSourceLanguageComboBoxText.Text = "Γλώσσα προέλευσης για μετάφραση:";
                    TranslationDestinationLanguageComboBoxText.Text = "Γλώσσα προορισμού για μετάφραση:";
                    GroupAddressManagementTitle.Text = "Διαχείριση ομαδικών διευθύνσεων";
                    RemoveUnusedAddressesCheckBox.Content = "Κατάργηση μη χρησιμοποιούμενων διευθύνσεων";
                    AppSettingsTitle.Text = "Ρυθμίσεις εφαρμογής";
                    ThemeTextBox.Text = "Θέμα:";
                    LightThemeComboBoxItem.Content = "Φωτεινό (προεπιλογή)";
                    DarkThemeComboBoxItem.Content = "Σκοτεινό";
                    AppLanguageTextBlock.Text = "Γλώσσα εφαρμογής:";
                    MenuDebug.Text = "Μενού εντοπισμού σφαλμάτων";
                    AddInfosOsCheckBox.Content = "Συμπερίληψη πληροφοριών λειτουργικού συστήματος";
                    AddInfosHardCheckBox.Content = "Συμπερίληψη πληροφοριών υλικού υπολογιστή";
                    AddImportedFilesCheckBox.Content = "Συμπερίληψη αρχείων που εισάγονται από την εκκίνηση";
                    IncludeAddressListCheckBox.Content = "Συμπερίληψη λίστας διαγραμμένων ομαδικών διευθύνσεων στα έργα";
                    CreateArchiveDebugText.Text = "Δημιουργία αρχείου εντοπισμού σφαλμάτων";
                    OngletDebug.Header = "Εντοπισμός σφαλμάτων";
                    OngletInformations.Header = "Πληροφορίες";
                    InformationsText.Text = $"{App.AppName}" +
                                            $"\nΈκδοση {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                            $"\nΚατασκευή {App.AppBuild}" +
                                            $"\n" +
                                            $"\nΛογισμικό που δημιουργήθηκε ως μέρος της μηχανικής πρακτικής από φοιτητές της INSA Toulouse:" +
                                            $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE και Maxime OLIVEIRA LOPES" +
                                            $"\n" +
                                            $"\nΥπό την επίβλεψη:" +
                                            $"\nDidier BESSE (UCRM)" +
                                            $"\nThierry COPPOLA (UCRM)" +
                                            $"\nJean-François KLOTZ (LECS)" +
                                            $"\n" +
                                            $"\nΣυνεργασία μεταξύ του Εθνικού Ινστιτούτου Εφαρμοσμένων Επιστημών (INSA) της Τουλούζης και της Ένωσης Cépière Robert Monnier (UCRM)." +
                                            $"\n" +
                                            $"\nΔημιουργία: 06/2024 - 07/2024\n";
                    SaveButtonText.Text = "Αποθήκευση";
                    CancelButtonText.Text = "Άκυρο";
                    
                    ScalingText.Text = "Κλιμάκωση:";
                    OngletParametresApplication.Header = "Γενικός";
                    OngletCorrection.Header = "Διόρθωση";
                    AddressKeepingText.Text = "Αλυσίδες προς διατήρηση στις διευθύνσεις κατά τη διόρθωση";
                    
                    NoteImportante.Text = "\nΣημαντική σημείωση:";
                    NoteImportanteContenu.Text = "το όνομα, τα λογότυπα και οποιαδήποτε εικόνα που σχετίζεται με το KNX είναι αδιαίρετη ιδιοκτησία του συλλόγου KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Ιστότοπος του συλλόγου KNX");
                    AddressKeepingTitle.Text = "Συμπερίληψη";
                    break;

                // Anglais
                case "EN":
                    SettingsWindowTopTitle.Text = "Settings";
                    TranslationTitle.Text = "Translation";
                    EnableTranslationCheckBox.Content = "Enable translation";
                    DeeplApiKeyText.Text = "DeepL API key:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Click here to get a free key)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Enable automatic language detection for translation";
                    TranslationSourceLanguageComboBoxText.Text = "Translation source language:";

                    TranslationDestinationLanguageComboBoxText.Text = "Translation destination language:";

                    GroupAddressManagementTitle.Text = "Group address management";
                    RemoveUnusedAddressesCheckBox.Content = "Remove unused addresses";

                    AppSettingsTitle.Text = "Application settings";
                    ThemeTextBox.Text = "Theme:";
                    LightThemeComboBoxItem.Content = "Light (default)";
                    DarkThemeComboBoxItem.Content = "Dark";

                    AppLanguageTextBlock.Text = "Application language:";

                    MenuDebug.Text = "Debug menu";
                    AddInfosOsCheckBox.Content = "Include operating system information";
                    AddInfosHardCheckBox.Content = "Include computer hardware information";
                    AddImportedFilesCheckBox.Content = "Include project files imported since launch";
                    IncludeAddressListCheckBox.Content = "Include list of deleted group addresses in projects";

                    CreateArchiveDebugText.Text = "Create debug file";

                    OngletDebug.Header = "Debugging";
                    
                    OngletInformations.Header = "Information";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware developed as part of an engineering internship by students of INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE and Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nUnder the supervision of:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnership between the Institut National des Sciences Appliquées (INSA) de Toulouse and the Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nImplementation: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Save";
                    CancelButtonText.Text = "Cancel";
                    
                    ScalingText.Text = "Scaling:";
                    OngletParametresApplication.Header = "General";
                    OngletCorrection.Header = "Correction";
                    AddressKeepingText.Text = "Strings to keep in addresses during correction";
                    
                    NoteImportante.Text = "\nImportant note:";
                    NoteImportanteContenu.Text = "the name, logos, and any images related to KNX are the inalienable property of the KNX association. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX association website");
                    AddressKeepingTitle.Text = "Inclusions";
                    break;

                // Espagnol
                case "ES":
                    SettingsWindowTopTitle.Text = "Configuraciones";
                    TranslationTitle.Text = "Traducción";
                    EnableTranslationCheckBox.Content = "Activar traducción";
                    DeeplApiKeyText.Text = "Clave API de DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Haga clic aquí para obtener una clave gratuita)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/es/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Activar detección automática de idioma para la traducción";
                    TranslationSourceLanguageComboBoxText.Text = "Idioma fuente de la traducción:";

                    TranslationDestinationLanguageComboBoxText.Text = "Idioma de destino de la traducción:";

                    GroupAddressManagementTitle.Text = "Gestión de direcciones de grupo";
                    RemoveUnusedAddressesCheckBox.Content = "Eliminar direcciones no utilizadas";

                    AppSettingsTitle.Text = "Configuraciones de la aplicación";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Claro (predeterminado)";
                    DarkThemeComboBoxItem.Content = "Oscuro";

                    AppLanguageTextBlock.Text = "Idioma de la aplicación:";

                    MenuDebug.Text = "Menú de depuración";
                    AddInfosOsCheckBox.Content = "Incluir información del sistema operativo";
                    AddInfosHardCheckBox.Content = "Incluir información de hardware del ordenador";
                    AddImportedFilesCheckBox.Content = "Incluir archivos de proyectos importados desde el inicio";
                    IncludeAddressListCheckBox.Content = "Incluir lista de direcciones de grupo eliminadas en los proyectos";

                    CreateArchiveDebugText.Text = "Crear archivo de depuración";

                    OngletDebug.Header = "Depuración";
                    
                    OngletInformations.Header = "Información";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersión {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nCompilación {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware desarrollado como parte de una pasantía de ingeniería por estudiantes de INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE y Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nBajo la supervisión de:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nAsociación entre el Instituto Nacional de Ciencias Aplicadas (INSA) de Toulouse y la Unión Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nImplementación: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Guardar";
                    CancelButtonText.Text = "Cancelar";
                    
                    ScalingText.Text = "Escalado:";
                    OngletParametresApplication.Header = "General";
                    OngletCorrection.Header = "Corrección";
                    AddressKeepingText.Text = "Cadenas para mantener en las direcciones durante la corrección";
                    
                    NoteImportante.Text = "\nNota importante:";
                    NoteImportanteContenu.Text = "el nombre, los logotipos y cualquier imagen relacionada con KNX son propiedad inalienable de la asociación KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Sitio web de la asociación KNX");
                    AddressKeepingTitle.Text = "Inclusiones";
                    break;

                // Estonien
                case "ET":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Seaded";
                    TranslationTitle.Text = "Tõlge";
                    EnableTranslationCheckBox.Content = "Luba tõlge";
                    DeeplApiKeyText.Text = "DeepL API võti:";
                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klõpsake siin, et saada tasuta võti)");
                    EnableAutomaticTranslationLangDetectionCheckbox.Content = "Luba automaatne keele tuvastamine tõlkimiseks";
                    TranslationSourceLanguageComboBoxText.Text = "Tõlke allikakeel:";
                    TranslationDestinationLanguageComboBoxText.Text = "Sihtkeel tõlkimiseks:";
                    GroupAddressManagementTitle.Text = "Rühma aadresside haldamine";
                    RemoveUnusedAddressesCheckBox.Content = "Eemalda kasutamata aadressid";
                    AppSettingsTitle.Text = "Rakenduse seaded";
                    ThemeTextBox.Text = "Teema:";
                    LightThemeComboBoxItem.Content = "Hele (vaikimisi)";
                    DarkThemeComboBoxItem.Content = "Tume";
                    AppLanguageTextBlock.Text = "Rakenduse keel:";
                    MenuDebug.Text = "Silumise menüü";
                    AddInfosOsCheckBox.Content = "Lisage teave operatsioonisüsteemi kohta";
                    AddInfosHardCheckBox.Content = "Lisage teave arvuti riistvara kohta";
                    AddImportedFilesCheckBox.Content = "Lisage käivitamisest imporditud failid";
                    IncludeAddressListCheckBox.Content = "Lisage projektidest eemaldatud rühma aadresside loend";
                    CreateArchiveDebugText.Text = "Loo silumisfail";
                    OngletDebug.Header = "Silumine";
                    OngletInformations.Header = "Teave";
                    InformationsText.Text = $"{App.AppName}" +
                                            $"\nVersioon {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                                            $"\nKoostamine {App.AppBuild}" +
                                            $"\n" +
                                            $"\nTarkvara, mille lõid osana inseneripraktikast INSA Toulouse üliõpilased:" +
                                            $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ja Maxime OLIVEIRA LOPES" +
                                            $"\n" +
                                            $"\nJärelevalve all:" +
                                            $"\nDidier BESSE (UCRM)" +
                                            $"\nThierry COPPOLA (UCRM)" +
                                            $"\nJean-François KLOTZ (LECS)" +
                                            $"\n" +
                                            $"\nPartnerlus Toulouse'i Rakendusteaduste Riikliku Instituudi (INSA) ja Union Cépière Robert Monnier (UCRM) vahel." +
                                            $"\n" +
                                            $"\nLoomine: 06/2024 - 07/2024\n";
                    SaveButtonText.Text = "Salvesta";
                    CancelButtonText.Text = "Tühista";
                    
                    ScalingText.Text = "Skaala:";
                    OngletParametresApplication.Header = "Üldine";
                    OngletCorrection.Header = "Korrektuur";
                    AddressKeepingText.Text = "Aadresside säilitamise stringid korrektuuri ajal";
                    
                    NoteImportante.Text = "\nOluline märkus:";
                    NoteImportanteContenu.Text = "KNX-i nimi, logod ja kõik pildid, mis on seotud KNX-iga, on KNX-i ühenduse võõrandamatu omand. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX ühenduse veebisait");
                    AddressKeepingTitle.Text = "Kaasamised";
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
                        "Ota käyttöön automaattinen kielen tunnistus käännöstä varten";
                    TranslationSourceLanguageComboBoxText.Text = "Käännöksen lähdekieli:";

                    TranslationDestinationLanguageComboBoxText.Text = "Käännöksen kohdekieli:";

                    GroupAddressManagementTitle.Text = "Ryhmäosoitteiden hallinta";
                    RemoveUnusedAddressesCheckBox.Content = "Poista käyttämättömät osoitteet";

                    AppSettingsTitle.Text = "Sovelluksen asetukset";
                    ThemeTextBox.Text = "Teema:";
                    LightThemeComboBoxItem.Content = "Vaalea (oletus)";
                    DarkThemeComboBoxItem.Content = "Tumma";

                    AppLanguageTextBlock.Text = "Sovelluksen kieli:";

                    MenuDebug.Text = "Virheenkorjausvalikko";
                    AddInfosOsCheckBox.Content = "Sisällytä käyttöjärjestelmän tiedot";
                    AddInfosHardCheckBox.Content = "Sisällytä tietokoneen laitteistotiedot";
                    AddImportedFilesCheckBox.Content = "Sisällytä aloituksen jälkeen tuodut projektitiedostot";
                    IncludeAddressListCheckBox.Content = "Sisällytä poistettujen ryhmäosoitteiden luettelo projekteihin";

                    CreateArchiveDebugText.Text = "Luo virheenkorjaustiedosto";
                    
                    OngletDebug.Header = "Virheenkorjaus";
                    
                    OngletInformations.Header = "Tiedot";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersio {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nKokoelma {App.AppBuild}" +
                        $"\n" +
                        $"\nOhjelmisto kehitetty osana INSAn Toulousen insinööriharjoittelua:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ja Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nValvonnassa:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nYhteistyö Instituutti National des Sciences Appliquées (INSA) de Toulousen ja Union Cépière Robert Monnier (UCRM) välillä." +
                        $"\n" +
                        $"\nToteutus: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Tallenna";
                    CancelButtonText.Text = "Peruuta";
                    
                    ScalingText.Text = "Skaalaus:";
                    OngletParametresApplication.Header = "Yleinen";
                    OngletCorrection.Header = "Korjaus";
                    AddressKeepingText.Text = "Osoitteisiin säilytettävät merkkijonot korjauksen aikana";
                    
                    NoteImportante.Text = "\nTärkeä huomautus:";
                    NoteImportanteContenu.Text = "KNX:n nimi, logot ja kaikki kuvat, jotka liittyvät KNX:ään, ovat KNX-yhdistyksen luovuttamatonta omaisuutta. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX-yhdistyksen verkkosivusto");
                    AddressKeepingTitle.Text = "Sisällytykset";
                    break;

                // Hongrois
                case "HU":
                    SettingsWindowTopTitle.Text = "Beállítások";
                    TranslationTitle.Text = "Fordítás";
                    EnableTranslationCheckBox.Content = "Fordítás engedélyezése";
                    DeeplApiKeyText.Text = "DeepL API-kulcs:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kattintson ide egy ingyenes kulcs megszerzéséhez)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Automatikus nyelvfelismerés engedélyezése a fordításhoz";
                    TranslationSourceLanguageComboBoxText.Text = "Fordítás forrásnyelve:";

                    TranslationDestinationLanguageComboBoxText.Text = "Fordítás célnyelve:";

                    GroupAddressManagementTitle.Text = "Csoport címkezelés";
                    RemoveUnusedAddressesCheckBox.Content = "Nem használt címek eltávolítása";

                    AppSettingsTitle.Text = "Alkalmazás beállításai";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Világos (alapértelmezett)";
                    DarkThemeComboBoxItem.Content = "Sötét";

                    AppLanguageTextBlock.Text = "Alkalmazás nyelve:";

                    MenuDebug.Text = "Hibakeresési menü";
                    AddInfosOsCheckBox.Content = "Tartalmazza az operációs rendszer információit";
                    AddInfosHardCheckBox.Content = "Tartalmazza a számítógép hardverinformációit";
                    AddImportedFilesCheckBox.Content = "Tartalmazza az indítás óta importált projektek fájljait";
                    IncludeAddressListCheckBox.Content = "Tartalmazza a projektekben törölt csoport címek listáját";

                    CreateArchiveDebugText.Text = "Hibakeresési fájl létrehozása";

                    OngletDebug.Header = "Hibakeresés";
                    
                    OngletInformations.Header = "Információk";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVerzió {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSzoftver az INSA Toulouse mérnöki szakmai gyakorlat keretében fejlesztett:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE és Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nFelügyelete alatt:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerség az Institut National des Sciences Appliquées (INSA) de Toulouse és az Union Cépière Robert Monnier (UCRM) között." +
                        $"\n" +
                        $"\nMegvalósítás: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Mentés";
                    CancelButtonText.Text = "Mégse";
                    
                    ScalingText.Text = "Méretezés:";
                    OngletParametresApplication.Header = "Általános";
                    OngletCorrection.Header = "Javítás";
                    AddressKeepingText.Text = "A címekben megőrzendő karakterláncok javítás közben";
                    
                    NoteImportante.Text = "\nFontos megjegyzés:";
                    NoteImportanteContenu.Text = "a KNX név, logók és bármilyen kép, amely a KNX-hez kapcsolódik, a KNX egyesület elidegeníthetetlen tulajdona. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX egyesület weboldala");
                    AddressKeepingTitle.Text = "Beillesztések";
                    break;

                // Indonésien
                case "ID":
                    SettingsWindowTopTitle.Text = "Pengaturan";
                    TranslationTitle.Text = "Terjemahan";
                    EnableTranslationCheckBox.Content = "Aktifkan terjemahan";
                    DeeplApiKeyText.Text = "Kunci API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klik di sini untuk mendapatkan kunci gratis)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/id/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktifkan deteksi bahasa otomatis untuk terjemahan";
                    TranslationSourceLanguageComboBoxText.Text = "Bahasa sumber terjemahan:";

                    TranslationDestinationLanguageComboBoxText.Text = "Bahasa tujuan terjemahan:";

                    GroupAddressManagementTitle.Text = "Manajemen alamat grup";
                    RemoveUnusedAddressesCheckBox.Content = "Hapus alamat yang tidak digunakan";

                    AppSettingsTitle.Text = "Pengaturan aplikasi";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Terang (default)";
                    DarkThemeComboBoxItem.Content = "Gelap";

                    AppLanguageTextBlock.Text = "Bahasa aplikasi:";

                    MenuDebug.Text = "Menu debug";
                    AddInfosOsCheckBox.Content = "Sertakan informasi sistem operasi";
                    AddInfosHardCheckBox.Content = "Sertakan informasi perangkat keras komputer";
                    AddImportedFilesCheckBox.Content = "Sertakan file proyek yang diimpor sejak diluncurkan";
                    IncludeAddressListCheckBox.Content = "Sertakan daftar alamat grup yang dihapus dalam proyek";

                    CreateArchiveDebugText.Text = "Buat file debug";

                    OngletDebug.Header = "Debug";
                    
                    OngletInformations.Header = "Informasi";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersi {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nPerangkat lunak dikembangkan sebagai bagian dari magang teknik oleh siswa INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE dan Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nDi bawah pengawasan:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nKemitraan antara Institut National des Sciences Appliquées (INSA) de Toulouse dan Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nImplementasi: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Simpan";
                    CancelButtonText.Text = "Batal";
                    
                    ScalingText.Text = "Skalasi:";
                    OngletParametresApplication.Header = "Umum";
                    OngletCorrection.Header = "Koreksi";
                    AddressKeepingText.Text = "String yang harus disimpan dalam alamat selama koreksi";
                    
                    NoteImportante.Text = "\nCatatan penting:";
                    NoteImportanteContenu.Text = "nama, logo, dan gambar apapun yang terkait dengan KNX adalah milik tidak terpisahkan dari asosiasi KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Situs web asosiasi KNX");
                    AddressKeepingTitle.Text = "Penyertaan";
                    break;

                // Italien
                case "IT":
                    SettingsWindowTopTitle.Text = "Impostazioni";
                    TranslationTitle.Text = "Traduzione";
                    EnableTranslationCheckBox.Content = "Abilita traduzione";
                    DeeplApiKeyText.Text = "Chiave API DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Clicca qui per ottenere una chiave gratuita)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/it/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Abilita rilevamento automatico della lingua per la traduzione";
                    TranslationSourceLanguageComboBoxText.Text = "Lingua di origine della traduzione:";

                    TranslationDestinationLanguageComboBoxText.Text = "Lingua di destinazione della traduzione:";

                    GroupAddressManagementTitle.Text = "Gestione indirizzi di gruppo";
                    RemoveUnusedAddressesCheckBox.Content = "Rimuovi indirizzi inutilizzati";

                    AppSettingsTitle.Text = "Impostazioni dell'app";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Chiaro (predefinito)";
                    DarkThemeComboBoxItem.Content = "Scuro";

                    AppLanguageTextBlock.Text = "Lingua dell'applicazione:";

                    MenuDebug.Text = "Menu di debug";
                    AddInfosOsCheckBox.Content = "Includi informazioni sul sistema operativo";
                    AddInfosHardCheckBox.Content = "Includi informazioni sull'hardware del computer";
                    AddImportedFilesCheckBox.Content = "Includi i file dei progetti importati dall'avvio";
                    IncludeAddressListCheckBox.Content = "Includi l'elenco degli indirizzi di gruppo eliminati nei progetti";

                    CreateArchiveDebugText.Text = "Crea file di debug";

                    OngletDebug.Header = "Debug";
                    
                    OngletInformations.Header = "Informazioni";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersione {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware sviluppato nell'ambito di uno stage di ingegneria da studenti dell'INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE e Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nSotto la supervisione di:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartenariato tra l'Istituto Nazionale delle Scienze Applicate (INSA) di Tolosa e l'Unione Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealizzazione: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Salva";
                    CancelButtonText.Text = "Annulla";
                    
                    ScalingText.Text = "Ridimensionamento:";
                    OngletParametresApplication.Header = "Generale";
                    OngletCorrection.Header = "Correzione";
                    AddressKeepingText.Text = "Stringhe da conservare negli indirizzi durante la correzione";
                    
                    NoteImportante.Text = "\nNota importante:";
                    NoteImportanteContenu.Text = "il nome, i loghi e tutte le immagini relative a KNX sono proprietà inalienabile dell'associazione KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Sito web dell'associazione KNX");
                    AddressKeepingTitle.Text = "Inclusioni";
                    break;

                // Japonais
                case "JA":
                    SettingsWindowTopTitle.Text = "設定";
                    TranslationTitle.Text = "翻訳";
                    EnableTranslationCheckBox.Content = "翻訳を有効にする";
                    DeeplApiKeyText.Text = "DeepL APIキー:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(無料のキーを取得するにはここをクリックしてください)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ja/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "翻訳のための自動言語検出を有効にする";
                    TranslationSourceLanguageComboBoxText.Text = "翻訳のソース言語:";

                    TranslationDestinationLanguageComboBoxText.Text = "翻訳のターゲット言語:";

                    GroupAddressManagementTitle.Text = "グループアドレス管理";
                    RemoveUnusedAddressesCheckBox.Content = "未使用のアドレスを削除する";

                    AppSettingsTitle.Text = "アプリ設定";
                    ThemeTextBox.Text = "テーマ:";
                    LightThemeComboBoxItem.Content = "ライト（デフォルト）";
                    DarkThemeComboBoxItem.Content = "ダーク";

                    AppLanguageTextBlock.Text = "アプリの言語:";

                    MenuDebug.Text = "デバッグメニュー";
                    AddInfosOsCheckBox.Content = "オペレーティングシステム情報を含める";
                    AddInfosHardCheckBox.Content = "コンピュータのハードウェア情報を含める";
                    AddImportedFilesCheckBox.Content = "起動以来インポートされたプロジェクトファイルを含める";
                    IncludeAddressListCheckBox.Content = "プロジェクトに削除されたグループアドレスのリストを含める";

                    CreateArchiveDebugText.Text = "デバッグファイルを作成";

                    OngletDebug.Header = "デバッグ";
                    
                    OngletInformations.Header = "情報";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nバージョン {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nビルド {App.AppBuild}" +
                        $"\n" +
                        $"\nINSAトゥールーズの学生によるエンジニアリングインターンシップの一環として開発されたソフトウェア:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\n監督の下:" +
                        $"\nDidier BESSE（UCRM）" +
                        $"\nThierry COPPOLA（UCRM）" +
                        $"\nJean-François KLOTZ（LECS）" +
                        $"\n" +
                        $"\nトゥールーズ国立応用科学研究所（INSA）とシェピエールロバートモニエ連合（UCRM）のパートナーシップ。" +
                        $"\n" +
                        $"\n実装: 2024年06月 - 2024年07月\n";
                        
                    SaveButtonText.Text = "保存";
                    CancelButtonText.Text = "キャンセル";
                    
                    ScalingText.Text = "スケーリング:";
                    OngletParametresApplication.Header = "一般";
                    OngletCorrection.Header = "修正";
                    AddressKeepingText.Text = "修正中に住所に保持する文字列";
                    
                    NoteImportante.Text = "\n重要な注意:";
                    NoteImportanteContenu.Text = "KNXの名前、ロゴ、およびKNXに関連するすべての画像は、KNX協会の不可分の財産です。 \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX協会のウェブサイト");
                    AddressKeepingTitle.Text = "インクルージョン";
                    break;

                // Coréen
                case "KO":
                    SettingsWindowTopTitle.Text = "설정";
                    TranslationTitle.Text = "번역";
                    EnableTranslationCheckBox.Content = "번역 활성화";
                    DeeplApiKeyText.Text = "DeepL API 키:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(무료 키를 얻으려면 여기를 클릭하세요)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ko/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "번역을 위한 자동 언어 감지 활성화";
                    TranslationSourceLanguageComboBoxText.Text = "번역 소스 언어:";

                    TranslationDestinationLanguageComboBoxText.Text = "번역 대상 언어:";

                    GroupAddressManagementTitle.Text = "그룹 주소 관리";
                    RemoveUnusedAddressesCheckBox.Content = "사용하지 않는 주소 삭제";

                    AppSettingsTitle.Text = "앱 설정";
                    ThemeTextBox.Text = "테마:";
                    LightThemeComboBoxItem.Content = "라이트 (기본)";
                    DarkThemeComboBoxItem.Content = "다크";

                    AppLanguageTextBlock.Text = "앱 언어:";

                    MenuDebug.Text = "디버그 메뉴";
                    AddInfosOsCheckBox.Content = "운영 체제 정보 포함";
                    AddInfosHardCheckBox.Content = "컴퓨터 하드웨어 정보 포함";
                    AddImportedFilesCheckBox.Content = "시작 후 가져온 프로젝트 파일 포함";
                    IncludeAddressListCheckBox.Content = "프로젝트에서 삭제된 그룹 주소 목록 포함";

                    CreateArchiveDebugText.Text = "디버그 파일 생성";

                    OngletDebug.Header = "디버그";
                    
                    OngletInformations.Header = "정보";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\n버전 {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\n빌드 {App.AppBuild}" +
                        $"\n" +
                        $"\nINSA 툴루즈 학생들이 엔지니어링 인턴십의 일환으로 개발한 소프트웨어:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE, Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\n감독 하에:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\n툴루즈 국립 응용 과학 연구소 (INSA)와 Union Cépière Robert Monnier (UCRM) 간의 파트너십." +
                        $"\n" +
                        $"\n실행: 2024년 6월 - 2024년 7월\n";
                        
                    SaveButtonText.Text = "저장";
                    CancelButtonText.Text = "취소";
                    
                    ScalingText.Text = "확대/축소:";
                    OngletParametresApplication.Header = "일반";
                    OngletCorrection.Header = "교정";
                    AddressKeepingText.Text = "교정 중 주소에 보관할 문자열";
                    
                    NoteImportante.Text = "\n중요한 참고 사항:";
                    NoteImportanteContenu.Text = "KNX의 이름, 로고 및 KNX와 관련된 모든 이미지는 KNX 협회의 양도할 수 없는 자산입니다. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX 협회 웹사이트");
                    AddressKeepingTitle.Text = "포함";
                    break;

                // Letton
                case "LV":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Iestatījumi";
                    TranslationTitle.Text = "Tulkošana";
                    EnableTranslationCheckBox.Content = "Aktivizēt tulkošanu";
                    DeeplApiKeyText.Text = "DeepL API atslēga:";

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktivizēt automātisko valodas noteikšanu tulkojumam";
                    TranslationSourceLanguageComboBoxText.Text = "Avota valoda tulkojumam:";
                    TranslationDestinationLanguageComboBoxText.Text = "Mērķa valoda tulkojumam:";

                    GroupAddressManagementTitle.Text = "Grupu adresu pārvaldība";
                    RemoveUnusedAddressesCheckBox.Content = "Noņemt neizmantotās adreses";

                    AppSettingsTitle.Text = "Lietotnes iestatījumi";
                    ThemeTextBox.Text = "Tēma:";
                    LightThemeComboBoxItem.Content = "Gaišs (noklusējums)";
                    DarkThemeComboBoxItem.Content = "Tumšs";

                    AppLanguageTextBlock.Text = "Lietotnes valoda:";

                    MenuDebug.Text = "Problēmu novēršana";
                    AddInfosOsCheckBox.Content = "Iekļaut operētājsistēmas informāciju";
                    AddInfosHardCheckBox.Content = "Iekļaut datora aparatūras informāciju";
                    AddImportedFilesCheckBox.Content = "Iekļaut projektos importēto failu informāciju";
                    IncludeAddressListCheckBox.Content = "Iekļaut grupu adreses, kas dzēstas no projektiem";

                    CreateArchiveDebugText.Text = "Izveidot problēmu novēršanas failu";

                    OngletDebug.Header = "Problēmu novēršana";
                                    
                    OngletInformations.Header = "Informācija";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersija {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBūvēt {App.AppBuild}" +
                        $"\n" +
                        $"\nProgrammatūra izstrādāta INSA Toulouse inženierijas prakses ietvaros:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE un Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nPārraudzīja:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerība starp National Institute of Applied Sciences (INSA) Toulouse un Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nIzstrāde: 06/2024 - 07/2024\n";
                                        
                    SaveButtonText.Text = "Saglabāt";
                    CancelButtonText.Text = "Atcelt";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Noklikšķiniet šeit, lai bez maksas saņemtu atslēgu)");
                    
                    ScalingText.Text = "Mērogošana:";
                    OngletParametresApplication.Header = "Vispārīgs";
                    OngletCorrection.Header = "Korekcija";
                    AddressKeepingText.Text = "Virknes, kuras saglabāt adresēs korekcijas laikā";
                    
                    NoteImportante.Text = "\nSvarīga piezīme:";
                    NoteImportanteContenu.Text = "KNX nosaukums, logotipi un jebkādi attēli, kas saistīti ar KNX, ir KNX asociācijas neatņemams īpašums. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX asociācijas tīmekļa vietne");
                    AddressKeepingTitle.Text = "Iekļaušana";
                    break;

                // Lituanien
                case "LT":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    SettingsWindowTopTitle.Text = "Nustatymai";
                    TranslationTitle.Text = "Vertimas";
                    EnableTranslationCheckBox.Content = "Įjungti vertimą";
                    DeeplApiKeyText.Text = "DeepL API raktas:";

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

                    MenuDebug.Text = "Derinimo meniu";
                    AddInfosOsCheckBox.Content = "Įtraukti operacinės sistemos informaciją";
                    AddInfosHardCheckBox.Content = "Įtraukti kompiuterio aparatūros informaciją";
                    AddImportedFilesCheckBox.Content = "Įtraukti importuotų projektų failus";
                    IncludeAddressListCheckBox.Content = "Įtraukti iš projektų ištrintų grupių adresų sąrašą";

                    CreateArchiveDebugText.Text = "Sukurti derinimo failą";

                    OngletDebug.Header = "Derinimas";
                            
                    OngletInformations.Header = "Informacija";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersija {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nKūrimas {App.AppBuild}" +
                        $"\n" +
                        $"\nPrograminė įranga sukurta INSA Toulouse inžinerijos praktikos metu:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ir Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nPrižiūrėtojai:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerystė tarp Nacionalinio taikomųjų mokslų instituto (INSA) Tulūzoje ir Sąjungos Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealizacija: 06/2024 - 07/2024\n";
                                
                    SaveButtonText.Text = "Išsaugoti";
                    CancelButtonText.Text = "Atšaukti";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Spustelėkite čia, kad nemokamai gautumėte raktą)");
                    
                    ScalingText.Text = "Mastelio keitimas:";
                    OngletParametresApplication.Header = "Bendras";
                    OngletCorrection.Header = "Korekcija";
                    AddressKeepingText.Text = "Eilutės, kurias reikia išsaugoti adresuose korekcijos metu";
                    
                    NoteImportante.Text = "\nSvarbi pastaba:";
                    NoteImportanteContenu.Text = "KNX pavadinimas, logotipai ir bet kokie su KNX susiję vaizdai yra neatsiejama KNX asociacijos nuosavybė. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX asociacijos svetainė");
                    AddressKeepingTitle.Text = "Įtraukimas";
                    break;

                // Norvégien
                case "NB":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Innstillinger";
                    TranslationTitle.Text = "Oversettelse";
                    EnableTranslationCheckBox.Content = "Aktiver oversettelse";
                    DeeplApiKeyText.Text = "DeepL API-nøkkel:";

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Aktiver automatisk språkgjenkjenning for oversettelse";
                    TranslationSourceLanguageComboBoxText.Text = "Kildespråk for oversettelse:";
                    TranslationDestinationLanguageComboBoxText.Text = "Målspråk for oversettelse:";

                    GroupAddressManagementTitle.Text = "Gruppeadressestyring";
                    RemoveUnusedAddressesCheckBox.Content = "Fjern ubrukte adresser";

                    AppSettingsTitle.Text = "Appinnstillinger";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Lys (standard)";
                    DarkThemeComboBoxItem.Content = "Mørk";

                    AppLanguageTextBlock.Text = "Appspråk:";

                    MenuDebug.Text = "Feilsøkingsmeny";
                    AddInfosOsCheckBox.Content = "Inkluder informasjon om operativsystemet";
                    AddInfosHardCheckBox.Content = "Inkluder informasjon om datamaskinens maskinvare";
                    AddImportedFilesCheckBox.Content = "Inkluder filer importert til prosjekter siden oppstart";
                    IncludeAddressListCheckBox.Content = "Inkluder listen over fjernede gruppeadresser fra prosjekter";

                    CreateArchiveDebugText.Text = "Opprett feilsøkingsfil";

                    OngletDebug.Header = "Feilsøking";
                            
                    OngletInformations.Header = "Informasjon";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersjon {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBygg {App.AppBuild}" +
                        $"\n" +
                        $"\nProgramvare laget som en del av et ingeniørpraksis ved INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE og Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nUnder veiledning av:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerskap mellom National Institute of Applied Sciences (INSA) i Toulouse og Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nUtførelse: 06/2024 - 07/2024\n";
                                
                    SaveButtonText.Text = "Lagre";
                    CancelButtonText.Text = "Avbryt";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Klikk her for å få en gratis nøkkel)");
                    
                    ScalingText.Text = "Skalering:";
                    OngletParametresApplication.Header = "Generell";
                    OngletCorrection.Header = "Korrigering";
                    AddressKeepingText.Text = "Strenger å beholde i adresser under korrigering";
                    
                    NoteImportante.Text = "\nViktig merknad:";
                    NoteImportanteContenu.Text = "navnet, logoene og alle bilder knyttet til KNX er udelelig eiendom tilhørende KNX-foreningen. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX-foreningens nettsted");
                    AddressKeepingTitle.Text = "Inkluderinger";
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
                    TranslationSourceLanguageComboBoxText.Text = "Bron taal van de vertaling:";

                    TranslationDestinationLanguageComboBoxText.Text = "Doeltaal van de vertaling:";

                    GroupAddressManagementTitle.Text = "Groepsadresbeheer";
                    RemoveUnusedAddressesCheckBox.Content = "Ongebruikte adressen verwijderen";

                    AppSettingsTitle.Text = "Applicatie instellingen";
                    ThemeTextBox.Text = "Thema:";
                    LightThemeComboBoxItem.Content = "Licht (standaard)";
                    DarkThemeComboBoxItem.Content = "Donker";

                    AppLanguageTextBlock.Text = "Applicatietaal:";

                    MenuDebug.Text = "Debug-menu";
                    AddInfosOsCheckBox.Content = "Inclusief OS-informatie";
                    AddInfosHardCheckBox.Content = "Inclusief hardware-informatie";
                    AddImportedFilesCheckBox.Content = "Inclusief geïmporteerde projectbestanden sinds de start";
                    IncludeAddressListCheckBox.Content = "Inclusief verwijderde groepsadreslijst in projecten";

                    CreateArchiveDebugText.Text = "Maak debugbestand aan";

                    OngletDebug.Header = "Debug";
                    
                    OngletInformations.Header = "Informatie";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersie {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware gemaakt in het kader van een ingenieursstage door studenten van INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE en Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nOnder supervisie van:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerschap tussen het Institut National des Sciences Appliquées (INSA) van Toulouse en de Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealisatie: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Opslaan";
                    CancelButtonText.Text = "Annuleren";
                    
                    ScalingText.Text = "Schaal:";
                    OngletParametresApplication.Header = "Algemeen";
                    OngletCorrection.Header = "Correctie";
                    AddressKeepingText.Text = "Strings om te behouden in adressen tijdens correctie";
                    
                    NoteImportante.Text = "\nBelangrijke opmerking:";
                    NoteImportanteContenu.Text = "de naam, logo's en alle afbeeldingen die verband houden met KNX zijn het onvervreemdbaar eigendom van de KNX-vereniging. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Website van de KNX-vereniging");
                    AddressKeepingTitle.Text = "Opnamen";
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
                    TranslationSourceLanguageComboBoxText.Text = "Język źródłowy tłumaczenia:";

                    TranslationDestinationLanguageComboBoxText.Text = "Język docelowy tłumaczenia:";

                    GroupAddressManagementTitle.Text = "Zarządzanie adresami grup";
                    RemoveUnusedAddressesCheckBox.Content = "Usuń nieużywane adresy";

                    AppSettingsTitle.Text = "Ustawienia aplikacji";
                    ThemeTextBox.Text = "Temat:";
                    LightThemeComboBoxItem.Content = "Jasny (domyślnie)";
                    DarkThemeComboBoxItem.Content = "Ciemny";

                    AppLanguageTextBlock.Text = "Język aplikacji:";

                    MenuDebug.Text = "Menu debugowania";
                    AddInfosOsCheckBox.Content = "Dołącz informacje o systemie operacyjnym";
                    AddInfosHardCheckBox.Content = "Dołącz informacje o sprzęcie";
                    AddImportedFilesCheckBox.Content = "Dołącz pliki projektów zaimportowane od uruchomienia";
                    IncludeAddressListCheckBox.Content = "Dołącz listę usuniętych adresów grup w projektach";

                    CreateArchiveDebugText.Text = "Utwórz plik debugowania";

                    OngletDebug.Header = "Debugowanie";
                    
                    OngletInformations.Header = "Informacje";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nWersja {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nOprogramowanie stworzone w ramach praktyk inżynierskich przez studentów INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE i Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nPod nadzorem:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerstwo między Institut National des Sciences Appliquées (INSA) w Tuluzie a Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealizacja: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Zapisz";
                    CancelButtonText.Text = "Anuluj";
                    
                    ScalingText.Text = "Skalowanie:";
                    OngletParametresApplication.Header = "Ogólne";
                    OngletCorrection.Header = "Korekta";
                    AddressKeepingText.Text = "Ciągi do zachowania w adresach podczas korekty";
                    
                    NoteImportante.Text = "\nWażna uwaga:";
                    NoteImportanteContenu.Text = "nazwa, logo i wszystkie obrazy związane z KNX są niezbywalną własnością stowarzyszenia KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Strona internetowa stowarzyszenia KNX");
                    AddressKeepingTitle.Text = "Włączenia";
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
                    TranslationSourceLanguageComboBoxText.Text = "Idioma de origem da tradução:";

                    TranslationDestinationLanguageComboBoxText.Text = "Idioma de destino da tradução:";

                    GroupAddressManagementTitle.Text = "Gestão de endereços de grupo";
                    RemoveUnusedAddressesCheckBox.Content = "Remover endereços não utilizados";

                    AppSettingsTitle.Text = "Configurações do aplicativo";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Claro (padrão)";
                    DarkThemeComboBoxItem.Content = "Escuro";

                    AppLanguageTextBlock.Text = "Idioma do aplicativo:";

                    MenuDebug.Text = "Menu de depuração";
                    AddInfosOsCheckBox.Content = "Incluir informações do sistema operacional";
                    AddInfosHardCheckBox.Content = "Incluir informações de hardware";
                    AddImportedFilesCheckBox.Content = "Incluir arquivos de projetos importados desde o início";
                    IncludeAddressListCheckBox.Content = "Incluir lista de endereços de grupo removidos nos projetos";

                    CreateArchiveDebugText.Text = "Criar arquivo de depuração";

                    OngletDebug.Header = "Depuração";
                    
                    OngletInformations.Header = "Informações";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersão {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware realizado no âmbito de um estágio de engenharia por estudantes da INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE e Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nSob a supervisão de:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nParceria entre o Institut National des Sciences Appliquées (INSA) de Toulouse e a Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealização: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Salvar";
                    CancelButtonText.Text = "Cancelar";
                    
                    ScalingText.Text = "Dimensionamento:";
                    OngletParametresApplication.Header = "Geral";
                    OngletCorrection.Header = "Correção";
                    AddressKeepingText.Text = "Cadeias para manter nos endereços durante a correção";
                    
                    NoteImportante.Text = "\nNota importante:";
                    NoteImportanteContenu.Text = "o nome, logotipos e quaisquer imagens relacionadas com KNX são propriedade inalienável da associação KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Website da associação KNX");
                    AddressKeepingTitle.Text = "Inclusões";
                    break;

                // Roumain
                case "RO":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Setări";
                    TranslationTitle.Text = "Traducere";
                    EnableTranslationCheckBox.Content = "Activează traducerea";
                    DeeplApiKeyText.Text = "Cheie API DeepL:";

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Activează detectarea automată a limbii pentru traducere";
                    TranslationSourceLanguageComboBoxText.Text = "Limbă sursă pentru traducere:";
                    TranslationDestinationLanguageComboBoxText.Text = "Limbă țintă pentru traducere:";

                    GroupAddressManagementTitle.Text = "Gestionarea adreselor de grup";
                    RemoveUnusedAddressesCheckBox.Content = "Elimină adresele neutilizate";

                    AppSettingsTitle.Text = "Setările aplicației";
                    ThemeTextBox.Text = "Temă:";
                    LightThemeComboBoxItem.Content = "Deschis (implicit)";
                    DarkThemeComboBoxItem.Content = "Întunecat";

                    AppLanguageTextBlock.Text = "Limba aplicației:";

                    MenuDebug.Text = "Meniu depanare";
                    AddInfosOsCheckBox.Content = "Includeți informațiile despre sistemul de operare";
                    AddInfosHardCheckBox.Content = "Includeți informațiile despre hardware-ul computerului";
                    AddImportedFilesCheckBox.Content = "Includeți fișierele importate în proiecte de la pornire";
                    IncludeAddressListCheckBox.Content = "Includeți lista adreselor de grup șterse din proiecte";

                    CreateArchiveDebugText.Text = "Creați fișierul de depanare";

                    OngletDebug.Header = "Depanare";
                            
                    OngletInformations.Header = "Informații";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersiune {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftware creat în cadrul unui stagiu de inginerie la INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE și Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nSub supravegherea lui:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nParteneriat între Institutul Național de Științe Aplicate (INSA) din Toulouse și Uniunea Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealizare: 06/2024 - 07/2024\n";
                                
                    SaveButtonText.Text = "Salvați";
                    CancelButtonText.Text = "Anulați";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Faceți clic aici pentru a obține o cheie gratuit)");
                    
                    ScalingText.Text = "Scalare:";
                    OngletParametresApplication.Header = "General";
                    OngletCorrection.Header = "Corecție";
                    AddressKeepingText.Text = "Șiruri de păstrat în adrese în timpul corectării";
                    
                    NoteImportante.Text = "\nNotă importantă:";
                    NoteImportanteContenu.Text = "numele, siglele și orice imagine legată de KNX sunt proprietatea inalienabilă a asociației KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Site-ul asociației KNX");
                    AddressKeepingTitle.Text = "Incluziuni";
                    break;

                // Slovaque
                case "SK":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    SettingsWindowTopTitle.Text = "Nastavenia";
                    TranslationTitle.Text = "Preklad";
                    EnableTranslationCheckBox.Content = "Povoliť preklad";
                    DeeplApiKeyText.Text = "DeepL API kľúč:";

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Povoliť automatické rozpoznávanie jazyka pre preklad";
                    TranslationSourceLanguageComboBoxText.Text = "Zdrojový jazyk pre preklad:";
                    TranslationDestinationLanguageComboBoxText.Text = "Cieľový jazyk pre preklad:";

                    GroupAddressManagementTitle.Text = "Správa skupinových adries";
                    RemoveUnusedAddressesCheckBox.Content = "Odstrániť nepoužité adresy";

                    AppSettingsTitle.Text = "Nastavenia aplikácie";
                    ThemeTextBox.Text = "Téma:";
                    LightThemeComboBoxItem.Content = "Svetlá (predvolená)";
                    DarkThemeComboBoxItem.Content = "Tmavá";

                    AppLanguageTextBlock.Text = "Jazyk aplikácie:";

                    MenuDebug.Text = "Ladiace menu";
                    AddInfosOsCheckBox.Content = "Zahrnúť informácie o operačnom systéme";
                    AddInfosHardCheckBox.Content = "Zahrnúť informácie o hardvéri počítača";
                    AddImportedFilesCheckBox.Content = "Zahrnúť súbory importované do projektov od spustenia";
                    IncludeAddressListCheckBox.Content = "Zahrnúť zoznam odstránených skupinových adries z projektov";

                    CreateArchiveDebugText.Text = "Vytvoriť ladiaci súbor";

                    OngletDebug.Header = "Ladenie";
                            
                    OngletInformations.Header = "Informácie";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVerzia {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nSoftvér vytvorený v rámci inžinierskej stáže na INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE a Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nPod dohľadom:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerstvo medzi Národným inštitútom aplikovaných vied (INSA) v Toulouse a Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRealizácia: 06/2024 - 07/2024\n";
                                
                    SaveButtonText.Text = "Uložiť";
                    CancelButtonText.Text = "Zrušiť";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite sem pre získanie kľúča zadarmo)");
                    
                    ScalingText.Text = "Mierka:";
                    OngletParametresApplication.Header = "Všeobecné";
                    OngletCorrection.Header = "Korekcia";
                    AddressKeepingText.Text = "Reťazce na uchovanie v adresách počas korekcie";
                    
                    NoteImportante.Text = "\nDôležitá poznámka:";
                    NoteImportanteContenu.Text = "názov, logá a akékoľvek obrázky týkajúce sa KNX sú neoddeliteľným majetkom združenia KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Webová stránka združenia KNX");
                    AddressKeepingTitle.Text = "Zahrnutia";
                    break;

                // Slovène
                case "SL":
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/en/pro-api");
                    
                    SettingsWindowTopTitle.Text = "Nastavitve";
                    TranslationTitle.Text = "Prevod";
                    EnableTranslationCheckBox.Content = "Omogoči prevod";
                    DeeplApiKeyText.Text = "DeepL API ključ:";

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Omogoči samodejno zaznavanje jezika za prevod";
                    TranslationSourceLanguageComboBoxText.Text = "Izvorni jezik za prevod:";
                    TranslationDestinationLanguageComboBoxText.Text = "Ciljni jezik za prevod:";

                    GroupAddressManagementTitle.Text = "Upravljanje skupinskih naslovov";
                    RemoveUnusedAddressesCheckBox.Content = "Odstrani neuporabljene naslove";

                    AppSettingsTitle.Text = "Nastavitve aplikacije";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Svetla (privzeta)";
                    DarkThemeComboBoxItem.Content = "Temna";

                    AppLanguageTextBlock.Text = "Jezik aplikacije:";

                    MenuDebug.Text = "Meni za odpravljanje napak";
                    AddInfosOsCheckBox.Content = "Vključi informacije o operacijskem sistemu";
                    AddInfosHardCheckBox.Content = "Vključi informacije o strojni opremi računalnika";
                    AddImportedFilesCheckBox.Content = "Vključi datoteke, uvožene v projekte od zagona";
                    IncludeAddressListCheckBox.Content = "Vključi seznam izbrisanih skupinskih naslovov iz projektov";

                    CreateArchiveDebugText.Text = "Ustvari datoteko za odpravljanje napak";

                    OngletDebug.Header = "Odpravljanje napak";
                            
                    OngletInformations.Header = "Informacije";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nRazličica {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nIzgradnja {App.AppBuild}" +
                        $"\n" +
                        $"\nProgramska oprema, izdelana v okviru inženirskega pripravništva na INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE in Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nPod nadzorom:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartnerstvo med Nacionalnim inštitutom za uporabne znanosti (INSA) v Toulouseu in Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nIzvedba: 06/2024 - 07/2024\n";
                                
                    SaveButtonText.Text = "Shrani";
                    CancelButtonText.Text = "Prekliči";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Kliknite tukaj za brezplačni ključ)");
                    
                    ScalingText.Text = "Spreminjanje velikosti:";
                    OngletParametresApplication.Header = "Splošno";
                    OngletCorrection.Header = "Popravek";
                    AddressKeepingText.Text = "Nizi za ohranitev v naslovih med popravljanjem";
                    
                    NoteImportante.Text = "\nPomembna opomba:";
                    NoteImportanteContenu.Text = "ime, logotipi in vse slike, povezane s KNX, so neodtujljiva last združenja KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Spletna stran združenja KNX");
                    AddressKeepingTitle.Text = "Vključki";
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
                        "Aktivera automatisk språkdetection för översättning";
                    TranslationSourceLanguageComboBoxText.Text = "Källspråk för översättning:";

                    TranslationDestinationLanguageComboBoxText.Text = "Målspråk för översättning:";

                    GroupAddressManagementTitle.Text = "Gruppadresshantering";
                    RemoveUnusedAddressesCheckBox.Content = "Ta bort oanvända adresser";

                    AppSettingsTitle.Text = "Appinställningar";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Ljus (standard)";
                    DarkThemeComboBoxItem.Content = "Mörk";

                    AppLanguageTextBlock.Text = "Appens språk:";

                    MenuDebug.Text = "Felsökningsmeny";
                    AddInfosOsCheckBox.Content = "Inkludera information om operativsystemet";
                    AddInfosHardCheckBox.Content = "Inkludera information om hårdvara";
                    AddImportedFilesCheckBox.Content = "Inkludera importerade projektfiler sedan start";
                    IncludeAddressListCheckBox.Content = "Inkludera lista över borttagna gruppadresser i projekt";

                    CreateArchiveDebugText.Text = "Skapa felsökningsfil";

                    OngletDebug.Header = "Felsökning";
                    
                    OngletInformations.Header = "Information";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nProgramvara utvecklad inom ramen för en ingenjörspraktik av studenter från INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE och Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nUnder överinseende av:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nSamarbete mellan Institut National des Sciences Appliquées (INSA) i Toulouse och Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nGenomförande: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Spara";
                    CancelButtonText.Text = "Avbryt";
                    
                    ScalingText.Text = "Skalning:";
                    OngletParametresApplication.Header = "Allmänt";
                    OngletCorrection.Header = "Korrigering";
                    AddressKeepingText.Text = "Strängar att behålla i adresser under korrigering";
                    
                    NoteImportante.Text = "\nViktig anmärkning:";
                    NoteImportanteContenu.Text = "namnet, logotyperna och alla bilder relaterade till KNX är KNX-föreningens omistliga egendom. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX-föreningens webbplats");
                    AddressKeepingTitle.Text = "Inkluderingar";
                    break;

                // Turc
                case "TR":
                    SettingsWindowTopTitle.Text = "Ayarlar";
                    TranslationTitle.Text = "Çeviri";
                    EnableTranslationCheckBox.Content = "Çeviriyi etkinleştir";
                    DeeplApiKeyText.Text = "DeepL API anahtarı:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Ücretsiz bir anahtar almak için buraya tıklayın)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/tr/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Çeviri için otomatik dil algılamayı etkinleştir";
                    TranslationSourceLanguageComboBoxText.Text = "Çeviri kaynak dili:";

                    TranslationDestinationLanguageComboBoxText.Text = "Çeviri hedef dili:";

                    GroupAddressManagementTitle.Text = "Grup adresi yönetimi";
                    RemoveUnusedAddressesCheckBox.Content = "Kullanılmayan adresleri kaldır";

                    AppSettingsTitle.Text = "Uygulama ayarları";
                    ThemeTextBox.Text = "Tema:";
                    LightThemeComboBoxItem.Content = "Açık (varsayılan)";
                    DarkThemeComboBoxItem.Content = "Koyu";

                    AppLanguageTextBlock.Text = "Uygulama dili:";

                    MenuDebug.Text = "Hata ayıklama menüsü";
                    AddInfosOsCheckBox.Content = "İşletim sistemi bilgilerini dahil et";
                    AddInfosHardCheckBox.Content = "Donanım bilgilerini dahil et";
                    AddImportedFilesCheckBox.Content = "Başlatmadan bu yana ithal edilen proje dosyalarını dahil et";
                    IncludeAddressListCheckBox.Content = "Projelerde silinen grup adresi listesini dahil et";

                    CreateArchiveDebugText.Text = "Hata ayıklama dosyası oluştur";

                    OngletDebug.Header = "Hata ayıklama";
                    
                    OngletInformations.Header = "Bilgiler";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nSürüm {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nYapı {App.AppBuild}" +
                        $"\n" +
                        $"\nINSA Toulouse öğrencileri tarafından bir mühendislik stajı kapsamında yapılan yazılım:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE ve Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nGözetim altında:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nInstitut National des Sciences Appliquées (INSA) Toulouse ve Union Cépière Robert Monnier (UCRM) arasındaki ortaklık." +
                        $"\n" +
                        $"\nGerçekleştirme: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Kaydet";
                    CancelButtonText.Text = "İptal";
                    
                    ScalingText.Text = "Ölçeklendirme:";
                    OngletParametresApplication.Header = "Genel";
                    OngletCorrection.Header = "Düzeltme";
                    AddressKeepingText.Text = "Düzeltme sırasında adreslerde saklanacak dizeler";
                    
                    NoteImportante.Text = "\nÖnemli not:";
                    NoteImportanteContenu.Text = "KNX'in adı, logoları ve KNX ile ilgili tüm resimler, KNX derneğinin devredilemez mülküdür. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX derneği web sitesi");
                    AddressKeepingTitle.Text = "Dâhil olanlar";
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
                        "Увімкнути автоматичне виявлення мови для перекладу";
                    TranslationSourceLanguageComboBoxText.Text = "Мова джерела перекладу:";

                    TranslationDestinationLanguageComboBoxText.Text = "Мова призначення перекладу:";

                    GroupAddressManagementTitle.Text = "Управління адресами груп";
                    RemoveUnusedAddressesCheckBox.Content = "Видалити невикористані адреси";

                    AppSettingsTitle.Text = "Налаштування додатку";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Світла (за замовчуванням)";
                    DarkThemeComboBoxItem.Content = "Темна";

                    AppLanguageTextBlock.Text = "Мова додатку:";

                    MenuDebug.Text = "Меню налагодження";
                    AddInfosOsCheckBox.Content = "Включити інформацію про операційну систему";
                    AddInfosHardCheckBox.Content = "Включити інформацію про апаратне забезпечення";
                    AddImportedFilesCheckBox.Content = "Включити файли проектів, імпортовані з моменту запуску";
                    IncludeAddressListCheckBox.Content = "Включити список видалених адрес груп у проектах";

                    CreateArchiveDebugText.Text = "Створити файл налагодження";

                    OngletDebug.Header = "Налагодження";
                    
                    OngletInformations.Header = "Інформація";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nВерсія {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nЗбірка {App.AppBuild}" +
                        $"\n" +
                        $"\nПрограмне забезпечення розроблене в рамках інженерного стажування студентами INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE та Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nПід наглядом:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nПартнерство між Institut National des Sciences Appliquées (INSA) в Тулузі та Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nРеалізація: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Зберегти";
                    CancelButtonText.Text = "Скасувати";
                    
                    ScalingText.Text = "Масштабування:";
                    OngletParametresApplication.Header = "Загальний";
                    OngletCorrection.Header = "Корекція";
                    AddressKeepingText.Text = "Рядки для збереження в адресах під час корекції";
                    
                    NoteImportante.Text = "\nВажлива примітка:";
                    NoteImportanteContenu.Text = "назва, логотипи та будь-які зображення, пов'язані з KNX, є невід'ємною власністю асоціації KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Вебсайт асоціації KNX");
                    AddressKeepingTitle.Text = "Включення";
                    break;

                // Russe
                case "RU":
                    SettingsWindowTopTitle.Text = "Настройки";
                    TranslationTitle.Text = "Перевод";
                    EnableTranslationCheckBox.Content = "Включить перевод";
                    DeeplApiKeyText.Text = "API-ключ DeepL:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("(Нажмите здесь, чтобы получить бесплатный ключ)");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/ru/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "Включить автоматическое определение языка для перевода";
                    TranslationSourceLanguageComboBoxText.Text = "Язык источника перевода:";

                    TranslationDestinationLanguageComboBoxText.Text = "Целевой язык перевода:";

                    GroupAddressManagementTitle.Text = "Управление адресами группы";
                    RemoveUnusedAddressesCheckBox.Content = "Удалить неиспользуемые адреса";

                    AppSettingsTitle.Text = "Настройки приложения";
                    ThemeTextBox.Text = "Тема:";
                    LightThemeComboBoxItem.Content = "Светлая (по умолчанию)";
                    DarkThemeComboBoxItem.Content = "Темная";

                    AppLanguageTextBlock.Text = "Язык приложения:";

                    MenuDebug.Text = "Меню отладки";
                    AddInfosOsCheckBox.Content = "Включить информацию о ОС";
                    AddInfosHardCheckBox.Content = "Включить информацию о оборудовании";
                    AddImportedFilesCheckBox.Content = "Включить файлы проектов, импортированные с момента запуска";
                    IncludeAddressListCheckBox.Content = "Включить список удаленных адресов групп в проектах";

                    CreateArchiveDebugText.Text = "Создать файл отладки";

                    OngletDebug.Header = "Отладка";
                    
                    OngletInformations.Header = "Информация";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nВерсия {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nСборка {App.AppBuild}" +
                        $"\n" +
                        $"\nПрограммное обеспечение, разработанное в рамках инженерной стажировки студентами INSA Toulouse:" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE и Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nПод руководством:" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nПартнерство между Institut National des Sciences Appliquées (INSA) в Тулузе и Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nРеализация: 06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "Сохранить";
                    CancelButtonText.Text = "Отмена";
                    
                    ScalingText.Text = "Масштабирование:";
                    OngletParametresApplication.Header = "Общий";
                    OngletCorrection.Header = "Коррекция";
                    AddressKeepingText.Text = "Строки для сохранения в адресах при корректировке";
                    
                    NoteImportante.Text = "\nВажное примечание:";
                    NoteImportanteContenu.Text = "название, логотипы и любые изображения, связанные с KNX, являются неотъемлемой собственностью ассоциации KNX. \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Веб-сайт ассоциации KNX");
                    AddressKeepingTitle.Text = "Включения";
                    break;

                // Chinois simplifié
                case "ZH":
                    SettingsWindowTopTitle.Text = "设置";
                    TranslationTitle.Text = "翻译";
                    EnableTranslationCheckBox.Content = "启用翻译";
                    DeeplApiKeyText.Text = "DeepL API 密钥:";

                    Hyperlink.Inlines.Clear();
                    Hyperlink.Inlines.Add("（点击此处获取免费密钥）");
                    Hyperlink.NavigateUri = new Uri("https://www.deepl.com/zh/pro-api");

                    EnableAutomaticTranslationLangDetectionCheckbox.Content =
                        "启用自动语言检测进行翻译";
                    TranslationSourceLanguageComboBoxText.Text = "翻译源语言:";

                    TranslationDestinationLanguageComboBoxText.Text = "翻译目标语言:";

                    GroupAddressManagementTitle.Text = "组地址管理";
                    RemoveUnusedAddressesCheckBox.Content = "删除未使用的地址";

                    AppSettingsTitle.Text = "应用设置";
                    ThemeTextBox.Text = "主题:";
                    LightThemeComboBoxItem.Content = "浅色（默认）";
                    DarkThemeComboBoxItem.Content = "深色";

                    AppLanguageTextBlock.Text = "应用语言:";

                    MenuDebug.Text = "调试菜单";
                    AddInfosOsCheckBox.Content = "包括操作系统信息";
                    AddInfosHardCheckBox.Content = "包括硬件信息";
                    AddImportedFilesCheckBox.Content = "包括启动以来导入的项目文件";
                    IncludeAddressListCheckBox.Content = "包括项目中删除的组地址列表";

                    CreateArchiveDebugText.Text = "创建调试文件";

                    OngletDebug.Header = "调试";
                    
                    OngletInformations.Header = "信息";
                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\n版本 {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\n构建 {App.AppBuild}" +
                        $"\n" +
                        $"\n由INSA Toulouse的学生在工程实习中开发的软件：" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE 和 Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\n在以下人员的指导下：" +
                        $"\nDidier BESSE（UCRM）" +
                        $"\nThierry COPPOLA（UCRM）" +
                        $"\nJean-François KLOTZ（LECS）" +
                        $"\n" +
                        $"\nToulouse的Institut National des Sciences Appliquées（INSA）与Union Cépière Robert Monnier（UCRM）之间的合作。" +
                        $"\n" +
                        $"\n实施：06/2024 - 07/2024\n";
                        
                    SaveButtonText.Text = "保存";
                    CancelButtonText.Text = "取消";
                    
                    ScalingText.Text = "缩放：";
                    OngletParametresApplication.Header = "常规";
                    OngletCorrection.Header = "修正";
                    AddressKeepingText.Text = "修正期间要在地址中保留的字符串";
                    
                    NoteImportante.Text = "\n重要提示:";
                    NoteImportanteContenu.Text = "KNX的名称、标识和任何与KNX相关的图片都是KNX协会不可分割的财产。 \u279e";
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("KNX协会网站");
                    AddressKeepingTitle.Text = "包含";
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
                    
                    OngletDebug.Header = "Débogage";
                    OngletInformations.Header = "Informations";

                    InformationsText.Text =
                        $"{App.AppName}" +
                        $"\nVersion {App.AppVersion.ToString(CultureInfo.InvariantCulture)}" +
                        $"\nBuild {App.AppBuild}" +
                        $"\n" +
                        $"\nLogiciel réalisé dans le cadre d'un stage d'ingénierie par des étudiants de l'INSA Toulouse :" +
                        $"\nNathan BRUGIÈRE, Emma COUSTON, Hugo MICHEL, Daichi MALBRANCHE et Maxime OLIVEIRA LOPES" +
                        $"\n" +
                        $"\nSous la supervision de :" +
                        $"\nDidier BESSE (UCRM)" +
                        $"\nThierry COPPOLA (UCRM)" +
                        $"\nJean-François KLOTZ (LECS)" +
                        $"\n" +
                        $"\nPartenariat entre l'Institut National des Sciences Appliquées (INSA) de Toulouse et l'Union Cépière Robert Monnier (UCRM)." +
                        $"\n" +
                        $"\nRéalisation: 06/2024 - 07/2024\n";

                    NoteImportante.Text = "\nNote importante:";
                    NoteImportanteContenu.Text =
                        " le nom, les logos et toute image liée à KNX sont la propriété inaliénable de l'association KNX. \u279e";
                    
                    HyperlinkInfo.Inlines.Clear();
                    HyperlinkInfo.Inlines.Add("Site web de l'association KNX");
                    AddressKeepingTitle.Text = "Inclusions";
                        
                    SaveButtonText.Text = "Enregistrer";
                    CancelButtonText.Text = "Annuler";
                    
                    ScalingText.Text = "Mise à l'échelle :";
                    OngletParametresApplication.Header = "Général";
                    OngletCorrection.Header = "Correction";
                    AddressKeepingText.Text = "Chaînes à conserver dans les adresses durant la correction";
                    break;
            }
        }


        // Fonction qui applique le thème au contenu de la fenêtre
        /// <summary>
        /// This functions applies the light/dark theme to the settings window
        /// </summary>
        private void ApplyThemeToWindow()
        {
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

                OngletCorrection.Style = (Style)FindResource("LightOnglet");
                OngletDebug.Style = (Style)FindResource("LightOnglet");
                OngletInformations.Style = (Style)FindResource("LightOnglet");
                OngletParametresApplication.Style = (Style)FindResource("LightOnglet");
                IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked! ? 
                    MainWindow.ConvertStringColor(textColor) : new SolidColorBrush(Colors.Gray);
                HyperlinkInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4071B4"));
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

                OngletCorrection.Style = (Style)FindResource("DarkOnglet");
                OngletDebug.Style = (Style)FindResource("DarkOnglet");
                OngletInformations.Style = (Style)FindResource("DarkOnglet");
                OngletParametresApplication.Style = (Style)FindResource("DarkOnglet");
                IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked! ? 
                    MainWindow.ConvertStringColor(textColor) : new SolidColorBrush(Colors.DimGray);
                HyperlinkInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4071B4"));


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
            GeneralSettingsTab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
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
            
            AddressKeepingTextbox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textboxBackgroundColor));
            AddressKeepingTextbox.BorderBrush = borderBrush;
            AddressKeepingTextbox.Foreground = textColorBrush;

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
            InformationsGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(deepDarkBackgroundColor));
            AddInfosOsCheckBox.Style = checkboxStyle;
            AddInfosHardCheckBox.Style = checkboxStyle;
            AddImportedFilesCheckBox.Style = checkboxStyle;
            IncludeAddressListCheckBox.Style = checkboxStyle;
            AddInfosOsCheckBox.Foreground = textColorBrush;
            AddInfosHardCheckBox.Foreground = textColorBrush;
            AddImportedFilesCheckBox.Foreground = textColorBrush;
            IncludeAddressListCheckBox.Foreground = (bool)AddImportedFilesCheckBox.IsChecked ? textColorBrush : new SolidColorBrush(Colors.DimGray);

            OngletCorrection.BorderBrush = borderBrush;
            OngletParametresApplication.BorderBrush = borderBrush;
            OngletCorrection.Foreground = textColorBrush;
            OngletDebug.Foreground = textColorBrush;
            OngletParametresApplication.Foreground = textColorBrush;
            DebugBrush1.Brush = textColorBrush;
            DebugBrush2.Brush = textColorBrush;
            OngletInformations.Foreground = textColorBrush;
            InformationsText.Foreground = textColorBrush;

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
            // Sauvegarde des anciens paramètres
            var previousEnableDeeplTranslation = EnableDeeplTranslation;
            var previousTranslationDestinationLang = TranslationDestinationLang;
            var previousTranslationSourceLang = TranslationSourceLang;
            var previousEnableAutomaticSourceLangDetection = EnableAutomaticSourceLangDetection;
            var previousRemoveUnusedGroupAddresses = RemoveUnusedGroupAddresses;
            var previousEnableLightTheme = EnableLightTheme;
            var previousAppLang = AppLang;
            var previousAppScaleFactor = AppScaleFactor;
            var previousDeepLKey = DeeplKey;

            // Récupération de tous les paramètres entrés dans la fenêtre de paramétrage
            EnableDeeplTranslation = (bool)EnableTranslationCheckBox.IsChecked!;
            TranslationDestinationLang = TranslationLanguageDestinationComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            TranslationSourceLang = TranslationSourceLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            EnableAutomaticSourceLangDetection = (bool)EnableAutomaticTranslationLangDetectionCheckbox.IsChecked!;
            RemoveUnusedGroupAddresses = (bool)RemoveUnusedAddressesCheckBox.IsChecked!;
            EnableLightTheme = LightThemeComboBoxItem.IsSelected;
            AppLang = AppLanguageComboBox.Text.Split([" - "], StringSplitOptions.None)[0];
            AppScaleFactor = (int)ScaleSlider.Value;

            List<string> newStringsToKeep = [];

            // On remplit la liste en récupérant les différents strings. Ils peuvent être séparés par des virgules ou des points virgules
            foreach (var st in AddressKeepingTextbox.Text.Split(','))
            {
                // Note: la fonction Trim() permet de supprimer s'il y en a les espaces au début
                // ou à la fin du string avant de l'ajouter à la liste tout en conservant ceux
                // à l'intérieur du string
                // Note 2 : On ne prend pas en compte les chaines vides ou les suites d'espaces
                if (!st.Trim().Equals("", StringComparison.OrdinalIgnoreCase))
                {
                    newStringsToKeep.AddRange(from st2 in st.Split(';') where !st2.Trim().Equals("", StringComparison.OrdinalIgnoreCase) select st2.Trim());
                }
            }

            // Vérification de si la liste de strings a changé :
            // Si la longueur des listes est différente ou qu'un élément d'une liste n'est pas dans l'autre et vice-versa, alors la liste a changé
            var listChanged = newStringsToKeep.Count != StringsToAdd.Count ||
                              newStringsToKeep.Except(StringsToAdd).Any() ||
                              StringsToAdd.Except(newStringsToKeep).Any();
            if (listChanged)
            {
                StringsToAdd = newStringsToKeep;
            }

            // Par défaut, si les fichiers de décryptage n'existent pas dans l'arborescence des fichiers,
            // on considèrera que la clé deepl a changé si la textbox n'est pas vide
            var deeplKeyChanged = !string.IsNullOrWhiteSpace(DeeplApiKeyTextBox.Text);

            // Si les clés de décryptage existent, on compare le contenu de la clé deepl entré dans la fenêtre avec celle que l'on peut décrypter
            if (File.Exists("./emk") && File.Exists("./ei") && File.Exists("./ek")) deeplKeyChanged = DecryptStringFromBytes(previousDeepLKey) != DeeplApiKeyTextBox.Text;

            // Si on a activé la traduction deepl et que la clé a changé où est vide
            if (EnableDeeplTranslation && (deeplKeyChanged || string.IsNullOrWhiteSpace(DeeplApiKeyTextBox.Text)))
            {
                // On récupère la nouvelle clé et on l'encrypte
                DeeplKey = EncryptStringToBytes(DeeplApiKeyTextBox.Text);

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

            // Si on a changé un des paramètres, on les sauvegarde. Sinon, inutile de réécrire le fichier.
            if (previousEnableDeeplTranslation != EnableDeeplTranslation ||
                previousTranslationDestinationLang != TranslationDestinationLang ||
                previousTranslationSourceLang != TranslationSourceLang ||
                previousEnableAutomaticSourceLangDetection != EnableAutomaticSourceLangDetection ||
                previousRemoveUnusedGroupAddresses != RemoveUnusedGroupAddresses ||
                previousEnableLightTheme != EnableLightTheme || previousAppLang != AppLang ||
                previousAppScaleFactor != AppScaleFactor ||
                deeplKeyChanged || listChanged)
            {
                // Sauvegarde des paramètres dans le fichier appSettings
                App.ConsoleAndLogWriteLine($"Settings changed. Saving application settings at {Path.GetFullPath("./appSettings")}");
                SaveSettings();
                App.ConsoleAndLogWriteLine("Settings saved successfully");
            }
            else
            {
                App.ConsoleAndLogWriteLine("Settings are unchanged. No need to save them.");
            }

            // Mise à jour éventuellement du contenu pour update la langue du menu
            UpdateWindowContents(false, previousAppLang != AppLang, previousEnableLightTheme != EnableLightTheme);

            // Si on a modifié l'échelle dans les paramètres
            if (AppScaleFactor != previousAppScaleFactor)
            {
                // Mise à jour de l'échelle de toutes les fenêtres
                var scaleFactor = AppScaleFactor / 100f;
                if (scaleFactor <= 1f)
                {
                    ApplyScaling(scaleFactor - 0.1f);
                }
                else
                {
                    ApplyScaling(scaleFactor - 0.2f);
                }
                App.DisplayElements!.MainWindow.ApplyScaling(scaleFactor);
                App.DisplayElements.GroupAddressRenameWindow.ApplyScaling(scaleFactor - 0.2f);
            }

            // Mise à jour de la fenêtre de renommage des adresses de groupe
            App.DisplayElements?.GroupAddressRenameWindow.UpdateWindowContents(previousAppLang != AppLang, previousEnableLightTheme != EnableLightTheme, previousAppScaleFactor == AppScaleFactor);

            // Mise à jour de la fenêtre principale
            App.DisplayElements?.MainWindow.UpdateWindowContents(previousAppLang != AppLang, previousEnableLightTheme != EnableLightTheme, previousAppScaleFactor == AppScaleFactor);

            //Faire apparaitre le bouton Reload 
            if ((previousRemoveUnusedGroupAddresses != RemoveUnusedGroupAddresses || previousEnableDeeplTranslation != EnableDeeplTranslation || listChanged
                || previousEnableAutomaticSourceLangDetection == EnableAutomaticSourceLangDetection || previousTranslationSourceLang == TranslationSourceLang || previousTranslationDestinationLang == TranslationDestinationLang) 
                && (App.Fm?.ProjectFolderPath != ""))
            {
                if (Application.Current.MainWindow is MainWindow mainWindow) mainWindow.ButtonReload.Visibility = Visibility.Visible;
            }

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
            UpdateWindowContents(false, true, true); // Restauration des paramètres précédents dans la fenêtre de paramétrage
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

            TranslationSourceLanguageComboBox.IsEnabled = (!EnableAutomaticSourceLangDetection) || (bool)(!EnableAutomaticTranslationLangDetectionCheckbox.IsChecked!);

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
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletCorrection.Header:
                    CancelButton.Visibility = Visibility.Visible;
                    SaveButton.Visibility = Visibility.Visible;
                    CreateArchiveDebugButton.Visibility = Visibility.Collapsed;
                    break;
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletDebug.Header:

                    CancelButton.Visibility = Visibility.Collapsed;
                    SaveButton.Visibility = Visibility.Collapsed;

                    CreateArchiveDebugButton.Visibility = Visibility.Visible;
                    break;
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletInformations.Header:
                    SaveButton.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Hidden;
                    CreateArchiveDebugButton.Visibility = Visibility.Collapsed;
                    break;
                case { Header: not null } when selectedTab.Header.ToString() == (string?)OngletParametresApplication.Header:
                    SaveButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
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
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

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
                    UpdateWindowContents(false, true, true); // Restauration des paramètres précédents dans la fenêtre de paramétrage
                    Hide(); // Masquage de la fenêtre de paramétrage
                    break;

                // Si on appuie sur entrée, on sauvegarde les modifications et on ferme
                case Key.Enter:
                    SaveButtonClick(null!, null!);
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


        /// <summary>
        /// Applies scaling to the window by adjusting the layout transform and resizing the window based on the specified scale factor.
        /// </summary>
        /// <param name="scale">The scale factor to apply.</param>
        private void ApplyScaling(double scale)
        {
            SettingsWindowBorder.LayoutTransform = new ScaleTransform(scale, scale);
            
            Height = 605 * scale > 0.9*SystemParameters.PrimaryScreenHeight ? 0.9*SystemParameters.PrimaryScreenHeight : 605 * scale;
            Width = 500 * scale > 0.9*SystemParameters.PrimaryScreenWidth ? 0.9*SystemParameters.PrimaryScreenWidth : 500 * scale;
        }
    }
}
