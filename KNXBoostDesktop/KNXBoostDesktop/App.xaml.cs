/***************************************************************************
 * Nom du Projet : KNX Boost Desktop
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime
 * Date          : 12/06/2024
 * Version       : 2.2
 *
 * Description :
 * Fichier principal contenant la structure de l'application et toutes les
 * fonctions necessaires a son utilisation.
 *
 * Remarques :
 * Repo GitHub --> https://github.com/Daichi9764/UCRM
 *
 * **************************************************************************/

// ReSharper disable GrammarMistakeInComment

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace KNXBoostDesktop
{
    public partial class App
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Donnees de l'application

        /// <summary>
        /// Represents the name of the application.
        /// </summary>
        public const string AppName = "KNX Boost Desktop"; // Nom de l'application

        /// <summary>
        /// Represents the version of the application.
        /// </summary>
        public const float AppVersion = 2.2f; // Version de l'application

        /// <summary>
        /// Represents the build of the application. Updated each time portions of code are merged on github.
        /// </summary>
        public static readonly int AppBuild = 379;
        
        
        // Gestion des logs
        /// <summary>
        /// Stores the file path for the log file. This path is used to determine where the log entries will be written.
        /// </summary>
        public static string? LogPath { get; private set; } // Chemin du fichier logs
        
        /// <summary>
        /// Provides a <see cref="StreamWriter"/> instance for writing log entries to the log file.
        /// </summary>
        /// <remarks>
        /// This writer is used for appending log messages to the file specified by <see cref="LogPath"/>.
        /// </remarks>
        private static StreamWriter? _writer; // Permet l'ecriture du fichier de logging
        
        
        
        // Composants de l'application
        
        /// <summary>
        /// Manages project files, providing functionality to handle project-related file operations.
        /// </summary>
        public static ProjectFileManager? Fm { get; private set; } // Gestionnaire de fichiers du projet
        
        /// <summary>
        /// Manages the application's display elements, including windows, buttons, and other UI components.
        /// </summary>
        public static DisplayElements? DisplayElements { get; private set; } // Gestionnaire de l'affichage (contient les fenetres, boutons, ...)
        
        
        
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Fonction s'executant e l'ouverture de l'application
        /// <summary>
        /// Executes when the application starts up.
        /// <para>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///     <item>
        ///         Creates a directory for log files if it does not already exist.
        ///     </item>
        ///     <item>
        ///         Initializes the log file path and sets up the <see cref="_writer"/> for logging.
        ///     </item>
        ///     <item>
        ///         Logs the start-up process of the application.
        ///     </item>
        ///     <item>
        ///         Initializes and displays the main window and updates related UI components.
        ///     </item>
        ///     <item>
        ///         Opens the project file manager.
        ///     </item>
        ///     <item>
        ///         Attempts to archive old log files and cleans up folders from the last session.
        ///     </item>
        ///     <item>
        ///         Logs a message indicating that the application has started successfully and performs garbage collection.
        ///     </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="e">An instance of <see cref="StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }

            LogPath = $"./logs/logs-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            _writer = new StreamWriter(LogPath);

            base.OnStartup(e);
            
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

            // Activation de l'auto-vidage du buffer du stream d'ecriture
            _writer.AutoFlush = true;


            ConsoleAndLogWriteLine(
                $"STARTING {AppName.ToUpper()} V{AppVersion.ToString(CultureInfo.InvariantCulture)} BUILD {AppBuild}...");


            // Ouverture la fenetre principale
            ConsoleAndLogWriteLine("Opening main window");
            DisplayElements = new DisplayElements();

            // Mise a jour de la fenetre de renommage des adresses de groupe
            DisplayElements.GroupAddressRenameWindow.UpdateWindowContents(true, true, true);

            // Mise a jour de la fenetre principale (titre, langue, thème, ...)
            DisplayElements.MainWindow.UpdateWindowContents(true, true, true);

            // Affichage de la fenêtre principale
            DisplayElements.ShowMainWindow();


            // Ouverture du gestionnaire de fichiers de projet
            ConsoleAndLogWriteLine("Opening project file manager");
            Fm = new ProjectFileManager();


            // Tentative d'archivage des fichiers de log
            ConsoleAndLogWriteLine("Trying to archive log files");
            ArchiveLogs();


            // Nettoyage des dossiers restants de la derniere session
            ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
            DeleteAllExceptLogsAndResources();

            // CheckForUpdatesAsync();

            ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
            ConsoleAndLogWriteLine("-----------------------------------------------------------");

            if ((Directory.Exists("./logs")) && (Directory.GetFiles("./logs", "*.txt").Length == 1) &&
                (Directory.GetFiles("./logs", "*.zip").Length == 0))
            {
                ShowInitialInformation();
            }
            
            // Appel au garbage collector pour nettoyer les variables issues 
            GC.Collect();
        }

        
        
        // Fonction s'executant lorsque l'on ferme l'application
        /// <summary>
        /// Executes when the application is closing.
        /// <para>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///     <item>
        ///         Logs the start of the application closing process.
        ///     </item>
        ///     <item>
        ///         Calls the base class implementation of <see cref="OnExit"/> to ensure proper shutdown behavior.
        ///     </item>
        ///     <item>
        ///         Logs the successful closure of the application.
        ///     </item>
        ///     <item>
        ///         Closes the log file stream if it is open, to ensure all log entries are properly written.
        ///     </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="e">An instance of <see cref="ExitEventArgs"/> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            ConsoleAndLogWriteLine("-----------------------------------------------------------");
            ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP...");
            
            base.OnExit(e);
            
            ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
            _writer?.Close(); // Fermeture du stream d'ecriture des logs
        }



        // Fonction pour afficher le message d'avertissement au premier lancement de l'application
        /// <summary>
        /// Displays an informational message box to the user regarding the importance of 
        /// well-structured project data when importing a KNX project into KNX Boost Desktop.
        /// The message and caption are localized based on the application's language settings.
        /// </summary>
        private static void ShowInitialInformation()
        {
            var messageBoxText = DisplayElements?.SettingsWindow!.AppLang switch
                {
                    // Arabe
                    "AR" => "عند استيراد مشروع KNX إلى KNX Boost Desktop، يرجى ملاحظة أن الدقة في تصحيح عناوين المجموعة تعتمد بشكل كبير على جودة وهيكلية بيانات المشروع. إذا كان مشروع KNX غير منظم بشكل جيد أو يفتقر إلى المعلومات الأساسية، فقد لا يتمكن البرنامج من تصحيح عناوين المجموعة بطريقة مناسبة وموثوقة تمامًا. تأكد من أن مشروعك منظم جيدًا للحصول على أفضل النتائج.",
                    // Bulgare
                    "BG" => "При импортиране на проект KNX в KNX Boost Desktop, моля, имайте предвид, че точността при коригиране на груповите адреси силно зависи от качеството и структурата на данните на проекта. Ако проектът KNX не е добре структуриран или липсва съществена информация, софтуерът може да не успее да коригира груповите адреси по напълно подходящ и надежден начин. Уверете се, че проектът ви е добре структуриран, за да получите най-добри резултати.",
                    // Tchèque
                    "CS" => "Při importu projektu KNX do KNX Boost Desktop mějte na paměti, že přesnost při opravě skupinových adres silně závisí na kvalitě a struktuře dat projektu. Pokud projekt KNX není dobře strukturován nebo postrádá zásadní informace, software nemusí být schopen opravit skupinové adresy zcela relevantním a spolehlivým způsobem. Ujistěte se, že váš projekt je dobře strukturován, abyste dosáhli nejlepších výsledků.",
                    // Danois
                    "DA" => "Når du importerer et KNX-projekt til KNX Boost Desktop, skal du være opmærksom på, at nøjagtigheden af korrektionen af gruppeadresser afhænger meget af kvaliteten og strukturen af projektdataene. Hvis KNX-projektet er dårligt struktureret eller mangler væsentlige oplysninger, kan softwaren muligvis ikke rette gruppeadresserne på en helt relevant og pålidelig måde. Sørg for, at dit projekt er godt struktureret for at opnå de bedste resultater.",
                    // Allemand
                    "DE" => "Beim Import eines KNX-Projekts in KNX Boost Desktop ist zu beachten, dass die Genauigkeit bei der Korrektur von Gruppenadressen stark von der Qualität und Strukturierung der Projektdaten abhängt. Wenn das KNX-Projekt schlecht strukturiert ist oder wesentliche Informationen fehlen, kann die Software möglicherweise nicht in der Lage sein, die Gruppenadressen auf eine vollständig relevante und zuverlässige Weise zu korrigieren. Stellen Sie sicher, dass Ihr Projekt gut strukturiert ist, um die besten Ergebnisse zu erzielen.",
                    // Grec
                    "EL" => "Κατά την εισαγωγή ενός έργου KNX στο KNX Boost Desktop, λάβετε υπόψη ότι η ακρίβεια στη διόρθωση των διευθύνσεων ομάδων εξαρτάται σε μεγάλο βαθμό από την ποιότητα και τη δομή των δεδομένων του έργου. Εάν το έργο KNX είναι κακώς δομημένο ή λείπουν βασικές πληροφορίες, το λογισμικό ενδέχεται να μην είναι σε θέση να διορθώσει τις διευθύνσεις ομάδων με έναν πλήρως σχετικό και αξιόπιστο τρόπο. Βεβαιωθείτε ότι το έργο σας είναι καλά δομημένο για να έχετε τα καλύτερα αποτελέσματα.",
                    // Anglais
                    "EN" => "When importing a KNX project into KNX Boost Desktop, please note that the accuracy in correcting group addresses highly depends on the quality and structuring of the project data. If the KNX project is poorly structured or lacks essential information, the software might not be able to correct the group addresses in a fully relevant and reliable manner. Ensure your project is well-structured to achieve the best results.",
                    // Espagnol
                    "ES" => "Al importar un proyecto KNX en KNX Boost Desktop, tenga en cuenta que la precisión en la corrección de las direcciones de grupo depende en gran medida de la calidad y la estructuración de los datos del proyecto. Si el proyecto KNX está mal estructurado o carece de información esencial, el software podría no ser capaz de corregir las direcciones de grupo de manera completamente relevante y confiable. Asegúrese de que su proyecto esté bien estructurado para obtener los mejores resultados.",
                    // Estonien
                    "ET" => "KNX projekti importimisel KNX Boost Desktop-i, pidage meeles, et grupiaadresside parandamise täpsus sõltub suuresti projektiandmete kvaliteedist ja struktuurist. Kui KNX projekt on halvasti struktureeritud või puuduvad olulised andmed, ei pruugi tarkvara olla võimeline grupiaadresse täiesti asjakohaselt ja usaldusväärselt parandama. Parimate tulemuste saavutamiseks veenduge, et teie projekt on hästi struktureeritud.",
                    // Finnois
                    "FI" => "Kun tuot KNX-projektia KNX Boost Desktopiin, huomaa, että ryhmäosoitteiden korjaamisen tarkkuus riippuu suuresti projektin tietojen laadusta ja rakenteesta. Jos KNX-projekti on huonosti jäsennelty tai siitä puuttuu olennaisia tietoja, ohjelmisto ei välttämättä pysty korjaamaan ryhmäosoitteita täysin merkityksellisellä ja luotettavalla tavalla. Varmista, että projektisi on hyvin jäsennelty parhaan tuloksen saavuttamiseksi.",
                    // Hongrois
                    "HU" => "A KNX projekt KNX Boost Desktopba történő importálásakor vegye figyelembe, hogy a csoportcímek korrigálásának pontossága nagymértékben függ a projektadatok minőségétől és strukturáltságától. Ha a KNX projekt rosszul strukturált vagy hiányzik a lényeges információk, a szoftver nem biztos, hogy teljesen megfelelő és megbízható módon tudja korrigálni a csoportcímeket. Győződjön meg róla, hogy a projekt jól strukturált a legjobb eredmények elérése érdekében.",
                    // Indonésien
                    "ID" => "Saat mengimpor proyek KNX ke KNX Boost Desktop, harap perhatikan bahwa akurasi dalam memperbaiki alamat grup sangat bergantung pada kualitas dan penataan data proyek. Jika proyek KNX tidak terstruktur dengan baik atau kekurangan informasi penting, perangkat lunak mungkin tidak dapat memperbaiki alamat grup dengan cara yang sepenuhnya relevan dan andal. Pastikan proyek Anda terstruktur dengan baik untuk mendapatkan hasil terbaik.",
                    // Italien
                    "IT" => "Quando si importa un progetto KNX in KNX Boost Desktop, si prega di notare che la precisione nella correzione degli indirizzi di gruppo dipende fortemente dalla qualità e dalla strutturazione dei dati del progetto. Se il progetto KNX è mal strutturato o manca di informazioni essenziali, il software potrebbe non essere in grado di correggere gli indirizzi di gruppo in modo completamente pertinente e affidabile. Assicurarsi che il progetto sia ben strutturato per ottenere i migliori risultati.",
                    // Japonais
                    "JA" => "KNXプロジェクトをKNX Boost Desktopにインポートする際、グループアドレスの修正の精度はプロジェクトデータの質と構造に大きく依存することに注意してください。KNXプロジェクトが構造化されていないか、重要な情報が不足している場合、ソフトウェアはグループアドレスを完全に適切で信頼できる方法で修正できない可能性があります。最良の結果を得るには、プロジェクトが適切に構造化されていることを確認してください。",
                    // Coréen
                    "KO" => "KNX 프로젝트를 KNX Boost Desktop에 가져올 때, 그룹 주소 수정의 정확도는 프로젝트 데이터의 품질과 구조에 크게 좌우됨을 유의하십시오. KNX 프로젝트가 잘 구조화되지 않았거나 중요한 정보가 부족한 경우 소프트웨어가 그룹 주소를 완전히 적절하고 신뢰할 수 있는 방식으로 수정할 수 없을 수 있습니다. 최상의 결과를 얻으려면 프로젝트가 잘 구조화되어 있는지 확인하십시오.",
                    // Letton
                    "LV" => "Importējot KNX projektu KNX Boost Desktop, lūdzu, ņemiet vērā, ka precizitāte grupu adrešu labošanā lielā mērā ir atkarīga no projekta datu kvalitātes un struktūras. Ja KNX projekts ir slikti strukturēts vai trūkst būtiskas informācijas, programmatūra var nebūt spējīga labot grupu adreses pilnībā atbilstošā un uzticamā veidā. Pārliecinieties, ka jūsu projekts ir labi strukturēts, lai iegūtu vislabākos rezultātus.",
                    // Lituanien
                    "LT" => "Importuojant KNX projektą į KNX Boost Desktop, atkreipkite dėmesį, kad grupių adresų taisymo tikslumas labai priklauso nuo projekto duomenų kokybės ir struktūros. Jei KNX projektas yra blogai struktūruotas arba trūksta esminės informacijos, programinė įranga gali nesugebėti taisyti grupių adresų visiškai tinkamu ir patikimu būdu. Įsitikinkite, kad jūsų projektas yra gerai struktūrizuotas, kad gautumėte geriausius rezultatus.",
                    // Norvégien
                    "NB" => "Når du importerer et KNX-prosjekt til KNX Boost Desktop, vennligst merk at nøyaktigheten i korrigering av gruppeadresser avhenger sterkt av kvaliteten og strukturen på prosjektdataene. Hvis KNX-prosjektet er dårlig strukturert eller mangler essensiell informasjon, kan det hende at programvaren ikke kan korrigere gruppeadressene på en helt relevant og pålitelig måte. Sørg for at prosjektet ditt er godt strukturert for å oppnå de beste resultatene.",
                    // Néerlandais
                    "NL" => "Bij het importeren van een KNX-project in KNX Boost Desktop, houd er rekening mee dat de nauwkeurigheid bij het corrigeren van groepsadressen sterk afhankelijk is van de kwaliteit en structuur van de projectgegevens. Als het KNX-project slecht gestructureerd is of essentiële informatie mist, kan de software mogelijk niet in staat zijn om de groepsadressen op een volledig relevante en betrouwbare manier te corrigeren. Zorg ervoor dat uw project goed gestructureerd is om de beste resultaten te behalen.",
                    // Polonais
                    "PL" => "Podczas importowania projektu KNX do KNX Boost Desktop, należy pamiętać, że dokładność korekty adresów grupowych w dużej mierze zależy od jakości i struktury danych projektu. Jeśli projekt KNX jest źle zorganizowany lub brakuje mu istotnych informacji, oprogramowanie może nie być w stanie poprawić adresów grupowych w sposób całkowicie odpowiedni i niezawodny. Upewnij się, że Twój projekt jest dobrze zorganizowany, aby uzyskać najlepsze wyniki.",
                    // Portugais
                    "PT" => "Ao importar um projeto KNX para o KNX Boost Desktop, observe que a precisão na correção dos endereços de grupo depende muito da qualidade e estruturação dos dados do projeto. Se o projeto KNX estiver mal estruturado ou faltar informações essenciais, o software pode não conseguir corrigir os endereços de grupo de maneira totalmente relevante e confiável. Certifique-se de que seu projeto está bem estruturado para obter os melhores resultados.",
                    // Roumain
                    "RO" => "La importul unui proiect KNX în KNX Boost Desktop, vă rugăm să rețineți că precizia în corectarea adreselor de grup depinde foarte mult de calitatea și structura datelor proiectului. Dacă proiectul KNX este slab structurat sau lipsesc informații esențiale, software-ul ar putea să nu fie capabil să corecteze adresele de grup într-un mod complet relevant și fiabil. Asigurați-vă că proiectul dvs. este bine structurat pentru a obține cele mai bune rezultate.",
                    // Russe
                    "RU" => "При импорте проекта KNX в KNX Boost Desktop, обратите внимание, что точность исправления групповых адресов во многом зависит от качества и структуры данных проекта. Если проект KNX плохо структурирован или в нем отсутствуют важные данные, программа может быть не в состоянии скорректировать групповые адреса полностью релевантным и надежным образом. Убедитесь, что ваш проект хорошо структурирован, чтобы добиться наилучших результатов.",
                    // Slovaque
                    "SK" => "Pri importovaní projektu KNX do KNX Boost Desktop, vezmite na vedomie, že presnosť pri opravovaní skupinových adries veľmi závisí od kvality a štruktúry údajov projektu. Ak je projekt KNX zle štruktúrovaný alebo mu chýbajú základné informácie, softvér nemusí byť schopný opraviť skupinové adresy úplne relevantným a spoľahlivým spôsobom. Uistite sa, že váš projekt je dobre štruktúrovaný, aby ste dosiahli najlepšie výsledky.",
                    // Slovène
                    "SL" => "Pri uvažanju KNX projekta v KNX Boost Desktop, upoštevajte, da je natančnost pri popravku skupinskih naslovov močno odvisna od kakovosti in strukture podatkov projekta. Če je KNX projekt slabo strukturiran ali manjkajo bistvene informacije, programska oprema morda ne bo mogla popraviti skupinskih naslovov na popolnoma ustrezen in zanesljiv način. Poskrbite, da bo vaš projekt dobro strukturiran za dosego najboljših rezultatov.",
                    // Suédois
                    "SV" => "Vid import av ett KNX-projekt till KNX Boost Desktop, observera att noggrannheten vid korrigering av gruppadresser beror mycket på kvaliteten och strukturen på projektdata. Om KNX-projektet är dåligt strukturerat eller saknar väsentlig information, kanske programvaran inte kan korrigera gruppadresserna på ett helt relevant och tillförlitligt sätt. Se till att ditt projekt är välstrukturerat för att uppnå bästa resultat.",
                    // Turc
                    "TR" => "Bir KNX projesini KNX Boost Desktop'a aktarırken, grup adreslerini düzeltmedeki doğruluğun büyük ölçüde proje verilerinin kalitesine ve yapısına bağlı olduğunu unutmayın. KNX projesi kötü yapılandırılmışsa veya temel bilgileri eksikse, yazılım grup adreslerini tamamen alakalı ve güvenilir bir şekilde düzeltemeyebilir. En iyi sonuçları almak için projenizin iyi yapılandırılmış olduğundan emin olun.",
                    // Ukrainien
                    "UK" => "Під час імпорту проекту KNX до KNX Boost Desktop зверніть увагу, що точність у виправленні групових адрес значною мірою залежить від якості та структури даних проекту. Якщо проект KNX погано структурований або бракує основної інформації, програмне забезпечення може не мати змоги виправити групові адреси у повністю доречний та надійний спосіб. Переконайтеся, що ваш проект добре структурований, щоб отримати найкращі результати.",
                    // Chinois simplifié
                    "ZH" => "在将 KNX 项目导入 KNX Boost Desktop 时，请注意，纠正组地址的准确性在很大程度上取决于项目数据的质量和结构。如果 KNX 项目结构不佳或缺乏必要的信息，软件可能无法以完全相关和可靠的方式纠正组地址。确保您的项目结构良好，以获得最佳结果。",
                    // Cas par défaut (français)
                    _ => "Lors de l'importation d'un projet KNX dans KNX Boost Desktop, veuillez noter que la précision dans la correction des adresses de groupe dépend fortement de la qualité et de la structuration des données du projet. Si le projet KNX est mal structuré ou manque d'informations essentielles, le logiciel pourrait ne pas être en mesure de corriger les adresses de groupe de manière entièrement pertinente et fiable. Assurez-vous que votre projet est bien structuré pour obtenir les meilleurs résultats."
                };

                var caption = DisplayElements?.SettingsWindow!.AppLang switch
                {
                    // Arabe
                    "AR" => "معلومة هامة",
                    // Bulgare
                    "BG" => "Важна информация",
                    // Tchèque
                    "CS" => "Důležitá informace",
                    // Danois
                    "DA" => "Vigtig information",
                    // Allemand
                    "DE" => "Wichtige Information",
                    // Grec
                    "EL" => "Σημαντική πληροφορία",
                    // Anglais
                    "EN" => "Important Information",
                    // Espagnol
                    "ES" => "Información importante",
                    // Estonien
                    "ET" => "Tähtis teave",
                    // Finnois
                    "FI" => "Tärkeää tietoa",
                    // Hongrois
                    "HU" => "Fontos információ",
                    // Indonésien
                    "ID" => "Informasi Penting",
                    // Italien
                    "IT" => "Informazione importante",
                    // Japonais
                    "JA" => "重要な情報",
                    // Coréen
                    "KO" => "중요 정보",
                    // Letton
                    "LV" => "Svarīga informācija",
                    // Lituanien
                    "LT" => "Svarbi informacija",
                    // Norvégien
                    "NB" => "Viktig informasjon",
                    // Néerlandais
                    "NL" => "Belangrijke informatie",
                    // Polonais
                    "PL" => "Ważna informacja",
                    // Portugais
                    "PT" => "Informação Importante",
                    // Roumain
                    "RO" => "Informație importantă",
                    // Russe
                    "RU" => "Важная информация",
                    // Slovaque
                    "SK" => "Dôležitá informácia",
                    // Slovène
                    "SL" => "Pomembna informacija",
                    // Suédois
                    "SV" => "Viktig information",
                    // Turc
                    "TR" => "Önemli Bilgi",
                    // Ukrainien
                    "UK" => "Важлива інформація",
                    // Chinois simplifié
                    "ZH" => "重要信息",
                    // Cas par défaut (français)
                    _ => "Information importante"
                };

                MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        
        
        // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'ecrivant dans les
        // logs sans sauter de ligne apres le message.
        /// <summary>
        /// Writes a message to the application console and log file without appending a newline after the message.
        /// <para>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///     <item>
        ///         Writes the provided message to the console without adding a newline character.
        ///     </item>
        ///     <item>
        ///         If the console window is visible, scrolls to the end of the console text to ensure the latest message is visible.
        ///     </item>
        ///     <item>
        ///         Writes the same message to the log file without appending a newline character.
        ///     </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="msg">The message to be written to the console and log file.</param>
        public static void ConsoleAndLogWrite(string msg)
        {
            Console.Write(msg); // Ecriture du message dans la console
            _writer?.Write(msg); // Ecriture du message dans le fichier logs
        }

        
        
        // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'ecrivant dans les
        // logs. Ajoute la date et l'heure avant affichage. Saut d'une ligne en fin de message.
        /// <summary>
        /// Writes a message to the application console and log file, including the current date and time, and appends a newline after the message.
        /// <para>
        /// This method performs the following tasks:
        /// <list type="bullet">
        ///     <item>
        ///         Writes the provided message to the console with a timestamp (date and time) at the beginning, followed by a newline.
        ///     </item>
        ///     <item>
        ///         If the console window is visible, scrolls to the end of the console text to ensure that the latest message is displayed.
        ///     </item>
        ///     <item>
        ///         Writes the same message to the log file with a timestamp (date and time) at the beginning, followed by a newline.
        ///     </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="msg">The message to be written to the console and log file.</param>
        public static void ConsoleAndLogWriteLine(string msg)
        {
            Console.WriteLine($@"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
            _writer?.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
        }

        
        
        // Fonction d'archivage des logs
        // Fonctionnement : S'il y a plus de 50 fichiers logs.txt, ces fichiers sont rassembles et compresses dans une archive zip
        // S'il y a plus de 10 archives, ces dernieres sont supprimees avant la creation de la nouvelle archive
        // Conséquence : on ne stocke les logs que des 50 derniers lancements de l'application
        /// <summary>
        /// Archives the log files in the log directory by compressing them into a ZIP archive when the number of log files exceeds 50.
        /// <para>
        /// If there are more than 50 log files, the method will create a new ZIP archive containing all log files, excluding the current log file.
        /// If there are already 10 or more existing archives, it will delete the oldest ones before creating a new archive.
        /// This ensures that only the log files from the last 50 application runs are retained.
        /// </para>
        /// <para>
        /// If there are fewer than 50 log files, no archiving will be performed.
        /// </para>
        /// <para>
        /// If an error occurs during the process, it logs the error message to the console and log file.
        /// </para>
        /// </summary>
        private static void ArchiveLogs()
        {
            var logDirectory = @"./logs/"; // Chemin du dossier de logs
            
            try
            {
                // Verifier si le repertoire existe
                if (!Directory.Exists(logDirectory))
                {
                    ConsoleAndLogWriteLine($"--> The specified directory does not exist : {logDirectory}");
                    return;
                }

                // Obtenir tous les fichiers log dans le repertoire
                var logFiles = Directory.GetFiles(logDirectory, "*.txt");

                // Verifier s'il y a plus de 50 fichiers log
                if (logFiles.Length > 50)
                {
                    // Obtenir tous les fichiers d'archive dans le repertoire
                    var archiveFiles = Directory.GetFiles(logDirectory, "LogsArchive-*.zip");

                    // Supprimer les archives existantes si elles sont plus de 10
                    if (archiveFiles.Length >= 10)
                    {
                        foreach (var archiveFile in archiveFiles)
                        {
                            File.Delete(archiveFile);
                        }
                        ConsoleAndLogWriteLine("--> Deleted all existing archive files as they exceeded the limit of 10.");
                    }

                    // Creer le nom du fichier zip avec la date actuelle
                    var zipFileName = Path.Combine(logDirectory, $"LogsArchive-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");

                    // Creer l'archive zip et y ajouter les fichiers log
                    using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                    {
                        foreach (var logFile in logFiles)
                        {
                            if (logFile != LogPath) // Si le fichier logs n'est pas celui que l'on vient de creer pour le lancement actuel
                            {
                                zip.CreateEntryFromFile(logFile, Path.GetFileName(logFile)); // On l'ajoute e l'archive
                                File.Delete(logFile); // Puis, on le supprime
                            }
                        }
                    }

                    ConsoleAndLogWriteLine($"--> Successfully archived log files to {zipFileName}");
                }
                else
                {
                    ConsoleAndLogWriteLine("--> Not enough log files to archive.");
                }
            }
            catch (Exception ex)
            {
                ConsoleAndLogWriteLine($"--> An error occured while creating the log archive : {ex.Message}");
            }
        }
        
        
        
        // Fonction permettant de supprimer tous les dossiers presents dans le dossier courant
        // Sauf le fichier logs. Cela permet de supprimer tous les projets exportes a la session precedente.
        // Fonction pour supprimer tous les dossiers sauf le dossier 'logs'
        /// <summary>
        /// Deletes all directories in the application directory except for those named 'logs' and 'resources'.
        /// <para>
        /// This method iterates through all subdirectories in the base directory and deletes them, excluding the directories 'logs' and 'resources'.
        /// This helps in cleaning up directories from previous sessions, retaining only the specified directories for future use.
        /// </para>
        /// <para>
        /// In case of an error during the deletion, such as unauthorized access or I/O errors, the method logs the error message to the console and continues processing other directories.
        /// </para>
        /// <para>
        /// The method logs the path of each successfully deleted directory to the application log for tracking purposes.
        /// </para>
        /// </summary>
        private static void DeleteAllExceptLogsAndResources()
        {
            if (Directory.GetDirectories("./").Length <= 3 && Directory.GetFiles("./", "*.zip").Length == 0)
            {
                ConsoleAndLogWriteLine("--> No folder or zip file to delete");
            }
            
            // Itération sur tous les répertoires dans le répertoire de base
            foreach (var directory in Directory.GetDirectories("./"))
            {
                // Exclure le dossier 'logs', 'de' et 'runtimes'
                if ((Path.GetFileName(directory).Equals("logs", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("runtimes", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("de", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Supprimer le dossier et son contenu
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {directory}: {ex.Message}");
                    continue;
                }
                catch (IOException ex)
                {
                    ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {directory}: {ex.Message}");
                    continue;
                }
                ConsoleAndLogWriteLine($"--> Deleted directory: {directory}");
            }

            foreach (var zipFile in Directory.GetFiles("./", "*.zip"))
            {
                try
                {
                    File.Delete(zipFile);
                }
                catch (UnauthorizedAccessException ex)
                {
                    ConsoleAndLogWriteLine($@"--> Access denied while attempting to delete {zipFile}: {ex.Message}");
                    continue;
                }
                catch (IOException ex)
                {
                    ConsoleAndLogWriteLine($@"--> I/O error while attempting to delete {zipFile}: {ex.Message}");
                    continue;
                }
                ConsoleAndLogWriteLine($"--> Deleted file: {zipFile}");
            }
            
        }
        
        
        // Mise à jour automatique de l'application
        // private const string UpdateInfoFilePath = "./update-info.txt";
        //
        // public async Task CheckForUpdatesAsync()
        // {
        //     string repoOwner = "Daichi9764";
        //     string repoName = "UCRM";
        //     string branch = "main"; // Ou la branche que vous utilisez
        //     string folderPath = "KNXBoostDesktop/latest-build";
        //     string currentCommitSha = GetCurrentCommitSha();
        //
        //     string latestCommitSha = await GetLatestCommitShaAsync(repoOwner, repoName);
        //
        //     App.ConsoleAndLogWriteLine($"{latestCommitSha} {currentCommitSha}");
        //     
        //     if (latestCommitSha != currentCommitSha)
        //     {
        //         App.ConsoleAndLogWriteLine("BONJOUR");
        //         
        //         string zipFilePath = Path.Combine(Path.GetTempPath(), "update.zip");
        //
        //         await DownloadAndZipFilesAsync(repoOwner, repoName, branch, folderPath, zipFilePath);
        //
        //         // Sauvegarder le nouveau SHA du commit
        //         SaveCurrentCommitSha(latestCommitSha);
        //
        //         await ApplyUpdateAsync(zipFilePath);
        //     }
        // }
        //
        // private string GetCurrentCommitSha()
        // {
        //     if (File.Exists(UpdateInfoFilePath))
        //     {
        //         return File.ReadAllText(UpdateInfoFilePath).Trim();
        //     }
        //     return string.Empty;
        // }
        //
        // private void SaveCurrentCommitSha(string sha)
        // {
        //     File.WriteAllText(UpdateInfoFilePath, sha);
        // }
        //
        // private async Task<string> GetLatestCommitShaAsync(string repoOwner, string repoName)
        // {
        //     string apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/commits";
        //     using (HttpClient client = new HttpClient())
        //     {
        //         client.DefaultRequestHeaders.UserAgent.ParseAdd("request"); // GitHub API requiert un en-tête User-Agent
        //         HttpResponseMessage response = await client.GetAsync(apiUrl);
        //         response.EnsureSuccessStatusCode();
        //         string responseBody = await response.Content.ReadAsStringAsync();
        //         JArray commits = JArray.Parse(responseBody);
        //         return (string)commits[0]["sha"];
        //     }
        // }
        //
        // private async Task DownloadAndZipFilesAsync(string repoOwner, string repoName, string branch, string folderPath, string zipFilePath)
        // {
        //     ConsoleAndLogWriteLine("On commence à dl");
        //     
        //     string apiUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/contents/{folderPath}?ref={branch}";
        //     using (HttpClient client = new HttpClient())
        //     {
        //         client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
        //         HttpResponseMessage response = await client.GetAsync(apiUrl);
        //         response.EnsureSuccessStatusCode();
        //         string responseBody = await response.Content.ReadAsStringAsync();
        //         JArray files = JArray.Parse(responseBody);
        //
        //         using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        //         {
        //             foreach (var file in files)
        //             {
        //                 string fileName = (string)file["name"];
        //                 ConsoleAndLogWriteLine($"{fileName}");
        //                 string downloadUrl = (string)file["download_url"];
        //                 using (var fileStream = new MemoryStream(await client.GetByteArrayAsync(downloadUrl)))
        //                 {
        //                     var zipEntry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
        //                     using (var entryStream = zipEntry.Open())
        //                     {
        //                         fileStream.CopyTo(entryStream);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
        //
        // private async Task ApplyUpdateAsync(string zipFilePath)
        // {
        //     string updaterPath = "update.exe";
        //     string tempFolderPath = Path.Combine(Path.GetTempPath(), "update");
        //     if (Directory.Exists(tempFolderPath))
        //     {
        //         Directory.Delete(tempFolderPath, true);
        //     }
        //     Directory.CreateDirectory(tempFolderPath);
        //     ZipFile.ExtractToDirectory(zipFilePath, tempFolderPath);
        //     Process.Start(new ProcessStartInfo
        //     {
        //         FileName = Path.Combine(tempFolderPath, updaterPath),
        //         UseShellExecute = true
        //     });
        //     Application.Current.Shutdown();
        // }
        
        // Destructeur de App
        /// <summary>
        /// Finalizer for the <see cref="App"/> class.
        /// Closes the log writer stream if it is still open when the application is being finalized.
        /// </summary>
        ~App()
        {
            // Si le stream d'écriture dans les logs est toujours ouvert, on le ferme
            _writer?.Close();
        }
    }
}































































