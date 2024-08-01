using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using System.Xml;
using System.IO;
using System.Text;
using System.Globalization;
using System.Xml.Linq;
using System.Net.Http;
using DeepL;

namespace KNXBoostDesktop;


public static class GroupAddressNameCorrector
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Represents the global XML namespace for KNX projects.
    /// </summary>
    private static XNamespace _globalKnxNamespace = string.Empty;
    
    /// <summary>
    /// Holds the path to the directory containing the project files.
    /// </summary>
    private static string _projectFilesDirectory = string.Empty;
    
    /// <summary>
    /// Provides the translation services used for localizing text within the application.
    /// </summary>
    public static Translator? Translator { get; private set; }
    
    /// <summary>
    /// Indicates whether the DeepL API key is valid and can be used for translations.
    /// </summary>
    public static bool ValidDeeplKey;

    /// <summary>
    /// Formatter to use when calling CorrectName()
    /// </summary>
    private static Formatter _formatter = null!;
   
    /// <summary>
    /// Collection to memorize translations of group names already done.
    /// </summary>
    private static readonly HashSet<string> TranslationCache = new();
    
    /// <summary>
    /// Collection to memorize format of group names already done.
    /// </summary>
    private static readonly HashSet<string> FormatCache = new();

    /// <summary>
    /// Collection to memorize object types already computed based on hardware file, Mxxxx directory,
    /// ComObject instance reference ID, ReadFlag found, WriteFlag found, and TransmitFlag Found.
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> ObjectTypeCache = new();
    
    /// <summary>
    /// Collection to memorize the rail-mounted status of devices based on product reference ID and Mxxxx directory.
    /// </summary>
    private static readonly ConcurrentDictionary<string, bool> IsDeviceRailMountedCache = new();
    
    /// <summary>
    /// Collection to memorize formatted hardware file names and Mxxxx directories based on hardware to program reference IDs.
    /// </summary>
    private static readonly ConcurrentDictionary<string, (string HardwareFileName, string MxxxxDirectory)> Hardware2ProgramRefIdCache = new();
    
    /// <summary>
    /// Collection to memorize if the method FormatNewFileName as been called with the parameter hardwareFileName and its result
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> NewFileNameCache = new();

    /// <summary>
    /// A predefined set of group names used for matching phrases in the group range elements. 
    /// </summary>
    private static readonly HashSet<string> GroupName =
    [
        "Date", "Heure", "Vent", "Luminosite Nord", "Luminosite Sud", "Luminosite Est", "Luminosite Ouest",
        "Luminosite Globale", "Radiation Solaire Nord", "Radiation Solaire Est", "Radiation Solaire Sud", 
        "Radiation Solaire Ouest", "Radiation Solaire Globale", "T Ambiante", "Consigne En Cours", "Sortie Chauffage",
        "Contact De Feuille", "Taux De Fonctionnement", "Presence", "Modification Valeur Consigne En Cours", 
        "Valeur Consigne En Cours", "On Off", "Variation", "Eau Chaude Sanitaire", "Ventilation", "Compteur", 
        "Montee Descente", "Inclinaison Stop", "Automatismes", "Pourcentage Hauteur", "Pourcentage Lamelles",
        "Pourcentage", "Haut Bas"
    ];

    /// <summary>
    /// A predefined set of group names used for matching phrases in the ancestor of the group range elements. 
    /// </summary>
    private static readonly HashSet<string> AncestorGroupName =
    [
        "Systeme", "Chauffage", "Programmation", "Delestage", "Eclairage", "Alarmes Techniques",
        "Eau Chaude Sanitaire Ventilation", "Comptage", "Supervision", "Ouvrants Motorises"
    ];

    /// <summary>
    /// Represents the total number of devices currently in the project.
    /// </summary>
    public static int TotalDevices;

    /// <summary>
    /// Represents the total number of addresses of the project.
    /// </summary>
    public static int TotalAddresses;

    /// <summary>
    /// Represents the total number of addresses that have been deleted from the project.
    /// </summary>
    public static int TotalDeletedAddresses;

    /// <summary>
    /// An array of characters used as separators for parsing strings, including spaces, commas, periods, semicolons, colons, exclamation marks, question marks, and underscores.
    /// </summary>
    private static readonly char[] Separator = [' ', ',', '.', ';', ':', '!', '?', '_', '/'];

    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 9159MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Xml.Linq.XAttribute; size: 7650MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Xml.Linq.XElement; size: 4051MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 2513MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // ReSharper disable once InvalidXmlDocComment
        /// <summary>
        /// Main task that retrieves information about devices and locations to format the new name for the group addresses.
        /// Translates names if necessary and removes unused addresses.
        /// </summary>
        /// <param name="cancellationToken"> Indicates if the task needs to be cancelled.</param>
    public static async Task CorrectName(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            string loadXml, 
                extractingInfos, 
                infosExtracted, 
                extractingDeviceReferences, 
                extractingDeviceInfo, 
                infoAndReferencesExtracted, 
                constructingNewAddresses, 
                suppressedAddresses,
                savingUpdatedXml,
                task;
            
                // Text translation of loading window
                switch (App.DisplayElements?.SettingsWindow?.AppLang)
                {
                    // Arabic
                    case "AR":
                        loadXml = "جارٍ تحميل ملف XML...";
                        extractingInfos = "جارٍ استخراج المعلومات...";
                        infosExtracted = "تم استخراج المعلومات من الملف.";
                        extractingDeviceReferences = "جارٍ استخراج مراجع الأجهزة...";
                        extractingDeviceInfo = "جارٍ استخراج المعلومات عن الأجهزة...\n(قد تستغرق هذه العملية وقتًا)";
                        infoAndReferencesExtracted = "جارٍ تحديث ملفات التصحيح...";
                        constructingNewAddresses = "جارٍ إنشاء عناوين المجموعة الجديدة...";
                        suppressedAddresses = "جارٍ حذف العناوين غير المستخدمة...";
                        savingUpdatedXml = "جارٍ حفظ ملف XML المحدث...";
                        task = "مهمة";
                        break;

                    // Bulgarian
                    case "BG":
                        loadXml = "Зареждане на XML файл...";
                        extractingInfos = "Извличане на информация...";
                        infosExtracted = "Информацията е извлечена от файла.";
                        extractingDeviceReferences = "Извличане на референции на устройствата...";
                        extractingDeviceInfo = "Извличане на информация за устройствата...\n(Тази стъпка може да отнеме време)";
                        infoAndReferencesExtracted = "Актуализиране на отладъчните файлове...";
                        constructingNewAddresses = "Създаване на нови групови адреси...";
                        suppressedAddresses = "Премахване на неизползваните адреси...";
                        savingUpdatedXml = "Запазване на актуализирания XML файл...";
                        task = "Задача";
                        break;

                    // Czech
                    case "CS":
                        loadXml = "Načítání souboru XML...";
                        extractingInfos = "Extrahování informací...";
                        infosExtracted = "Informace byly extrahovány ze souboru.";
                        extractingDeviceReferences = "Extrahování referencí zařízení...";
                        extractingDeviceInfo = "Extrahování informací o zařízeních...\n(Tato fáze může trvat delší dobu)";
                        infoAndReferencesExtracted = "Aktualizace ladicích souborů...";
                        constructingNewAddresses = "Vytváření nových skupinových adres...";
                        suppressedAddresses = "Odstranění nepoužitých adres...";
                        savingUpdatedXml = "Ukládání aktualizovaného souboru XML...";
                        task = "Úkol";
                        break;

                    // Danish
                    case "DA":
                        loadXml = "Indlæser XML-fil...";
                        extractingInfos = "Udpakning af oplysninger...";
                        infosExtracted = "Oplysningerne er udtrukket fra filen.";
                        extractingDeviceReferences = "Udpakning af enhedsreferencer...";
                        extractingDeviceInfo = "Udpakning af oplysninger om enheder...\n(Denne fase kan tage tid)";
                        infoAndReferencesExtracted = "Opdatering af fejlfinding filer...";
                        constructingNewAddresses = "Oprettelse af nye gruppeadresser...";
                        suppressedAddresses = "Fjernelse af ubrugte adresser...";
                        savingUpdatedXml = "Gemmer den opdaterede XML-fil...";
                        task = "Opgave";
                        break;

                    // German
                    case "DE":
                        loadXml = "Laden der XML-Datei...";
                        extractingInfos = "Extrahieren von Informationen...";
                        infosExtracted = "Informationen aus der Datei extrahiert.";
                        extractingDeviceReferences = "Extrahieren der Geräteverweise...";
                        extractingDeviceInfo = "Extrahieren von Geräteinformationen...\n(Dieser Vorgang kann einige Zeit dauern)";
                        infoAndReferencesExtracted = "Aktualisieren der Debug-Dateien...";
                        constructingNewAddresses = "Erstellen neuer Gruppenadressen...";
                        suppressedAddresses = "Entfernen nicht verwendeter Adressen...";
                        savingUpdatedXml = "Speichern der aktualisierten XML-Datei...";
                        task = "Aufgabe";
                        break;

                    // Greek
                    case "EL":
                        loadXml = "Φόρτωση αρχείου XML...";
                        extractingInfos = "Εξαγωγή πληροφοριών...";
                        infosExtracted = "Οι πληροφορίες εξήχθησαν από το αρχείο.";
                        extractingDeviceReferences = "Εξαγωγή αναφορών συσκευών...";
                        extractingDeviceInfo = "Εξαγωγή πληροφοριών για τις συσκευές...\n(Αυτή η φάση μπορεί να διαρκέσει αρκετή ώρα)";
                        infoAndReferencesExtracted = "Ενημέρωση αρχείων αποσφαλμάτωσης...";
                        constructingNewAddresses = "Δημιουργία νέων ομαδικών διευθύνσεων...";
                        suppressedAddresses = "Κατάργηση μη χρησιμοποιούμενων διευθύνσεων...";
                        savingUpdatedXml = "Αποθήκευση ενημερωμένου αρχείου XML...";
                        task = "Εργασία";
                        break;

                    // English
                    case "EN":
                        loadXml = "Loading XML file...";
                        extractingInfos = "Extracting information...";
                        infosExtracted = "Information extracted from the file.";
                        extractingDeviceReferences = "Extracting device references...";
                        extractingDeviceInfo = "Extracting device information...\n(This step may take some time)";
                        infoAndReferencesExtracted = "Updating debug files...";
                        constructingNewAddresses = "Constructing new group addresses...";
                        suppressedAddresses = "Removing unused addresses...";
                        savingUpdatedXml = "Saving updated XML file...";
                        task = "Task";
                        break;

                    // Spanish
                    case "ES":
                        loadXml = "Cargando archivo XML...";
                        extractingInfos = "Extrayendo información...";
                        infosExtracted = "Información extraída del archivo.";
                        extractingDeviceReferences = "Extrayendo referencias de dispositivos...";
                        extractingDeviceInfo = "Extrayendo información de los dispositivos...\n(Este paso puede tardar un tiempo)";
                        infoAndReferencesExtracted = "Actualizando archivos de depuración...";
                        constructingNewAddresses = "Construyendo nuevas direcciones de grupo...";
                        suppressedAddresses = "Eliminando direcciones no utilizadas...";
                        savingUpdatedXml = "Guardando el archivo XML actualizado...";
                        task = "Tarea";
                        break;

                    // Estonian
                    case "ET":
                        loadXml = "XML-faili laadimine...";
                        extractingInfos = "Teabe ekstraheerimine...";
                        infosExtracted = "Teave on failist ekstraheeritud.";
                        extractingDeviceReferences = "Seadme viidete ekstraheerimine...";
                        extractingDeviceInfo = "Seadme teabe ekstraheerimine...\n(See etapp võib võtta aega)";
                        infoAndReferencesExtracted = "Silumisfailide värskendamine...";
                        constructingNewAddresses = "Uute rühma aadresside koostamine...";
                        suppressedAddresses = "Kasutamata aadresside eemaldamine...";
                        savingUpdatedXml = "Värskendatud XML-faili salvestamine...";
                        task = "Ülesanne";
                        break;

                    // Finnish
                    case "FI":
                        loadXml = "Ladataan XML-tiedostoa...";
                        extractingInfos = "Tietojen purkaminen...";
                        infosExtracted = "Tiedot on purettu tiedostosta.";
                        extractingDeviceReferences = "Laitteiden viitteiden purkaminen...";
                        extractingDeviceInfo = "Laitteiden tietojen purkaminen...\n(Tämä vaihe voi kestää jonkin aikaa)";
                        infoAndReferencesExtracted = "Vianmääritystiedostojen päivittäminen...";
                        constructingNewAddresses = "Uusien ryhmäosoitteiden luominen...";
                        suppressedAddresses = "Käyttämättömien osoitteiden poistaminen...";
                        savingUpdatedXml = "Päivitetyn XML-tiedoston tallentaminen...";
                        task = "Tehtävä";
                        break;

                    // Hungarian
                    case "HU":
                        loadXml = "XML fájl betöltése...";
                        extractingInfos = "Információk kinyerése...";
                        infosExtracted = "Az információk ki lettek nyerve a fájlból.";
                        extractingDeviceReferences = "Eszköz hivatkozások kinyerése...";
                        extractingDeviceInfo = "Eszköz információk kinyerése...\n(Ez a lépés eltarthat egy ideig)";
                        infoAndReferencesExtracted = "Hibakeresési fájlok frissítése...";
                        constructingNewAddresses = "Új csoportcímek létrehozása...";
                        suppressedAddresses = "Nem használt címek eltávolítása...";
                        savingUpdatedXml = "Frissített XML fájl mentése...";
                        task = "Feladat";
                        break;

                    // Indonesian
                    case "ID":
                        loadXml = "Memuat file XML...";
                        extractingInfos = "Mengekstrak informasi...";
                        infosExtracted = "Informasi telah diekstrak dari file.";
                        extractingDeviceReferences = "Mengekstrak referensi perangkat...";
                        extractingDeviceInfo = "Mengekstrak informasi perangkat...\n(Langkah ini mungkin memakan waktu)";
                        infoAndReferencesExtracted = "Memperbarui file debug...";
                        constructingNewAddresses = "Membangun alamat grup baru...";
                        suppressedAddresses = "Menghapus alamat yang tidak digunakan...";
                        savingUpdatedXml = "Menyimpan file XML yang diperbarui...";
                        task = "Tugas";
                        break;

                    // Italian
                    case "IT":
                        loadXml = "Caricamento del file XML...";
                        extractingInfos = "Estrazione delle informazioni...";
                        infosExtracted = "Informazioni estratte dal file.";
                        extractingDeviceReferences = "Estrazione dei riferimenti dei dispositivi...";
                        extractingDeviceInfo = "Estrazione delle informazioni sui dispositivi...\n(Questa fase può richiedere del tempo)";
                        infoAndReferencesExtracted = "Aggiornamento dei file di debug...";
                        constructingNewAddresses = "Costruzione di nuovi indirizzi di gruppo...";
                        suppressedAddresses = "Rimozione degli indirizzi non utilizzati...";
                        savingUpdatedXml = "Salvataggio del file XML aggiornato...";
                        task = "Compito";
                        break;

                    // Japanese
                    case "JA":
                        loadXml = "XMLファイルを読み込んでいます...";
                        extractingInfos = "情報を抽出しています...";
                        infosExtracted = "ファイルから情報を抽出しました。";
                        extractingDeviceReferences = "デバイスの参照を抽出しています...";
                        extractingDeviceInfo = "デバイス情報を抽出しています...\n(このステップには時間がかかる場合があります)";
                        infoAndReferencesExtracted = "デバッグファイルを更新しています...";
                        constructingNewAddresses = "新しいグループアドレスを構築しています...";
                        suppressedAddresses = "未使用のアドレスを削除しています...";
                        savingUpdatedXml = "更新されたXMLファイルを保存しています...";
                        task = "タスク";
                        break;

                    // Korean
                    case "KO":
                        loadXml = "XML 파일 로드 중...";
                        extractingInfos = "정보 추출 중...";
                        infosExtracted = "파일에서 정보가 추출되었습니다.";
                        extractingDeviceReferences = "장치 참조 추출 중...";
                        extractingDeviceInfo = "장치 정보 추출 중...\n(이 단계는 시간이 걸릴 수 있습니다)";
                        infoAndReferencesExtracted = "디버그 파일 업데이트 중...";
                        constructingNewAddresses = "새 그룹 주소 생성 중...";
                        suppressedAddresses = "사용되지 않는 주소 제거 중...";
                        savingUpdatedXml = "업데이트된 XML 파일 저장 중...";
                        task = "작업";
                        break;

                    // Latvian
                    case "LV":
                        loadXml = "Ielādē XML failu...";
                        extractingInfos = "Izvelk informāciju...";
                        infosExtracted = "Informācija ir izvilkta no faila.";
                        extractingDeviceReferences = "Izvelk ierīču atsauces...";
                        extractingDeviceInfo = "Izvelk informāciju par ierīcēm...\n(Šis posms var aizņemt kādu laiku)";
                        infoAndReferencesExtracted = "Atjaunina atkļūdošanas failus...";
                        constructingNewAddresses = "Izveido jaunas grupas adreses...";
                        suppressedAddresses = "Noņem neizmantotās adreses...";
                        savingUpdatedXml = "Saglabā atjaunināto XML failu...";
                        task = "Uzdevums";
                        break;

                    // Lithuanian
                    case "LT":
                        loadXml = "Įkeliama XML byla...";
                        extractingInfos = "Išgaunama informacija...";
                        infosExtracted = "Informacija išgauta iš bylos.";
                        extractingDeviceReferences = "Išgaunamos įrenginių nuorodos...";
                        extractingDeviceInfo = "Išgaunama įrenginių informacija...\n(Šis etapas gali užtrukti)";
                        infoAndReferencesExtracted = "Atnaujinami derinimo failai...";
                        constructingNewAddresses = "Kuriami nauji grupės adresai...";
                        suppressedAddresses = "Šalinami nenaudojami adresai...";
                        savingUpdatedXml = "Išsaugoma atnaujinta XML byla...";
                        task = "Užduotis";
                        break;

                    // Norwegian
                    case "NB":
                        loadXml = "Laster inn XML-fil...";
                        extractingInfos = "Uthenter informasjon...";
                        infosExtracted = "Informasjonen er hentet fra filen.";
                        extractingDeviceReferences = "Uthenter enhetsreferanser...";
                        extractingDeviceInfo = "Uthenter informasjon om enheter...\n(Denne prosessen kan ta tid)";
                        infoAndReferencesExtracted = "Oppdaterer feilsøkingsfiler...";
                        constructingNewAddresses = "Oppretter nye gruppeadresser...";
                        suppressedAddresses = "Fjerner ubrukte adresser...";
                        savingUpdatedXml = "Lagrer oppdatert XML-fil...";
                        task = "Oppgave";
                        break;

                    // Dutch
                    case "NL":
                        loadXml = "XML-bestand laden...";
                        extractingInfos = "Informatie extraheren...";
                        infosExtracted = "Informatie is uit het bestand gehaald.";
                        extractingDeviceReferences = "Apparaatverwijzingen extraheren...";
                        extractingDeviceInfo = "Apparaatinformatie extraheren...\n(Dit kan enige tijd duren)";
                        infoAndReferencesExtracted = "Debug-bestanden bijwerken...";
                        constructingNewAddresses = "Nieuwe groepsadressen construeren...";
                        suppressedAddresses = "Niet-gebruikte adressen verwijderen...";
                        savingUpdatedXml = "Bijgewerkt XML-bestand opslaan...";
                        task = "Taak";
                        break;

                    // Polish
                    case "PL":
                        loadXml = "Ładowanie pliku XML...";
                        extractingInfos = "Wydobywanie informacji...";
                        infosExtracted = "Informacje wyodrębnione z pliku.";
                        extractingDeviceReferences = "Wydobywanie referencji urządzeń...";
                        extractingDeviceInfo = "Wydobywanie informacji o urządzeniach...\n(Ten krok może zająć trochę czasu)";
                        infoAndReferencesExtracted = "Aktualizowanie plików debugowania...";
                        constructingNewAddresses = "Tworzenie nowych adresów grupowych...";
                        suppressedAddresses = "Usuwanie nieużywanych adresów...";
                        savingUpdatedXml = "Zapisywanie zaktualizowanego pliku XML...";
                        task = "Zadanie";
                        break;

                    // Portuguese
                    case "PT":
                        loadXml = "Carregando arquivo XML...";
                        extractingInfos = "Extraindo informações...";
                        infosExtracted = "Informações extraídas do arquivo.";
                        extractingDeviceReferences = "Extraindo referências de dispositivos...";
                        extractingDeviceInfo = "Extraindo informações dos dispositivos...\n(Esta etapa pode demorar)";
                        infoAndReferencesExtracted = "Atualizando arquivos de depuração...";
                        constructingNewAddresses = "Construindo novos endereços de grupo...";
                        suppressedAddresses = "Removendo endereços não utilizados...";
                        savingUpdatedXml = "Salvando arquivo XML atualizado...";
                        task = "Tarefa";
                        break;

                    // Romanian
                    case "RO":
                        loadXml = "Se încarcă fișierul XML...";
                        extractingInfos = "Se extrag informațiile...";
                        infosExtracted = "Informațiile au fost extrase din fișier.";
                        extractingDeviceReferences = "Se extrag referințele dispozitivelor...";
                        extractingDeviceInfo = "Se extrag informațiile dispozitivelor...\n(Această etapă poate dura ceva timp)";
                        infoAndReferencesExtracted = "Se actualizează fișierele de depanare...";
                        constructingNewAddresses = "Se construiesc noile adrese de grup...";
                        suppressedAddresses = "Se elimină adresele neutilizate...";
                        savingUpdatedXml = "Se salvează fișierul XML actualizat...";
                        task = "Sarcină";
                        break;

                    // Russian
                    case "RU":
                        loadXml = "Загрузка XML файла...";
                        extractingInfos = "Извлечение информации...";
                        infosExtracted = "Информация извлечена из файла.";
                        extractingDeviceReferences = "Извлечение ссылок на устройства...";
                        extractingDeviceInfo = "Извлечение информации об устройствах...\n(Этот процесс может занять некоторое время)";
                        infoAndReferencesExtracted = "Обновление отладочных файлов...";
                        constructingNewAddresses = "Создание новых групповых адресов...";
                        suppressedAddresses = "Удаление неиспользуемых адресов...";
                        savingUpdatedXml = "Сохранение обновленного XML файла...";
                        task = "Задача";
                        break;

                    // Slovak
                    case "SK":
                        loadXml = "Načítava sa XML súbor...";
                        extractingInfos = "Extrahujú sa informácie...";
                        infosExtracted = "Informácie boli extrahované zo súboru.";
                        extractingDeviceReferences = "Extrahujú sa referencie zariadení...";
                        extractingDeviceInfo = "Extrahujú sa informácie o zariadeniach...\n(Tento krok môže trvať dlhšie)";
                        infoAndReferencesExtracted = "Aktualizujú sa súbory na ladenie...";
                        constructingNewAddresses = "Vytvárajú sa nové skupinové adresy...";
                        suppressedAddresses = "Odstraňujú sa nepoužívané adresy...";
                        savingUpdatedXml = "Ukladá sa aktualizovaný XML súbor...";
                        task = "Úloha";
                        break;

                    // Slovenian
                    case "SL":
                        loadXml = "Nalaganje XML datoteke...";
                        extractingInfos = "Izvlečenje informacij...";
                        infosExtracted = "Informacije so izvlečene iz datoteke.";
                        extractingDeviceReferences = "Izvlečenje referenc naprav...";
                        extractingDeviceInfo = "Izvlečenje informacij o napravah...\n(Ta postopek lahko traja nekaj časa)";
                        infoAndReferencesExtracted = "Posodabljanje datotek za odpravljanje napak...";
                        constructingNewAddresses = "Ustvarjanje novih skupinskih naslovov...";
                        suppressedAddresses = "Odstranjevanje neuporabljenih naslovov...";
                        savingUpdatedXml = "Shranjevanje posodobljene XML datoteke...";
                        task = "Naloga";
                        break;

                    // Swedish
                    case "SV":
                        loadXml = "Laddar XML-fil...";
                        extractingInfos = "Extraherar information...";
                        infosExtracted = "Informationen har extraherats från filen.";
                        extractingDeviceReferences = "Extraherar enhetsreferenser...";
                        extractingDeviceInfo = "Extraherar enhetsinformation...\n(Detta steg kan ta tid)";
                        infoAndReferencesExtracted = "Uppdaterar felsökningsfiler...";
                        constructingNewAddresses = "Konstruerar nya gruppadresser...";
                        suppressedAddresses = "Tar bort oanvända adresser...";
                        savingUpdatedXml = "Sparar uppdaterad XML-fil...";
                        task = "Uppgift";
                        break;

                    // Turkish
                    case "TR":
                        loadXml = "XML dosyası yükleniyor...";
                        extractingInfos = "Bilgiler çıkarılıyor...";
                        infosExtracted = "Bilgiler dosyadan çıkarıldı.";
                        extractingDeviceReferences = "Cihaz referansları çıkarılıyor...";
                        extractingDeviceInfo = "Cihaz bilgileri çıkarılıyor...\n(Bu adım zaman alabilir)";
                        infoAndReferencesExtracted = "Hata ayıklama dosyaları güncelleniyor...";
                        constructingNewAddresses = "Yeni grup adresleri oluşturuluyor...";
                        suppressedAddresses = "Kullanılmayan adresler kaldırılıyor...";
                        savingUpdatedXml = "Güncellenmiş XML dosyası kaydediliyor...";
                        task = "Görev";
                        break;

                    // Ukrainian
                    case "UK":
                        loadXml = "Завантаження файлу XML...";
                        extractingInfos = "Витяг інформації...";
                        infosExtracted = "Інформація витягнута з файлу.";
                        extractingDeviceReferences = "Витяг посилань на пристрої...";
                        extractingDeviceInfo = "Витяг інформації про пристрої...\n(Цей крок може зайняти деякий час)";
                        infoAndReferencesExtracted = "Оновлення файлів налагодження...";
                        constructingNewAddresses = "Створення нових групових адрес...";
                        suppressedAddresses = "Видалення невикористаних адрес...";
                        savingUpdatedXml = "Збереження оновленого файлу XML...";
                        task = "Завдання";
                        break;

                    // Chinese (simplified)
                    case "ZH":
                        loadXml = "加载 XML 文件...";
                        extractingInfos = "提取信息...";
                        infosExtracted = "信息已从文件中提取。";
                        extractingDeviceReferences = "提取设备参考...";
                        extractingDeviceInfo = "提取设备信息...\n（此步骤可能需要一些时间）";
                        infoAndReferencesExtracted = "更新调试文件...";
                        constructingNewAddresses = "构建新的组地址...";
                        suppressedAddresses = "删除未使用的地址...";
                        savingUpdatedXml = "保存更新的 XML 文件...";
                        task = "任务";
                        break;

                    // Default language (french)
                    default:
                        loadXml = "Chargement du fichier XML...";
                        extractingInfos = "Extraction des informations...";
                        infosExtracted = "Informations extraites du fichier.";
                        extractingDeviceReferences = "Extraction des références des appareils...";
                        extractingDeviceInfo = "Extraction des informations sur les appareils...\n(Cette étape peut prendre du temps)";
                        infoAndReferencesExtracted = "Mise à jour des fichiers de débogage...";
                        constructingNewAddresses = "Construction des nouvelles adresses de groupe...";
                        suppressedAddresses = "Suppression des adresses non utilisées...";
                        savingUpdatedXml = "Sauvegarde du fichier XML mis à jour...";
                        task = "Tâche";
                        break;
                }
            
            //Define the project path
            _projectFilesDirectory = Path.Combine(App.Fm?.ProjectFolderPath ?? string.Empty, @"knxproj_exported");
            
            // Define the XML namespace used in the KNX project file
            SetNamespaceFromXml(App.Fm?.ZeroXmlPath ?? string.Empty);
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(loadXml);
            
            // Load the XML file from the specified path
            var knxDoc = App.Fm?.LoadXmlDocument(App.Fm.ZeroXmlPath);
            if (knxDoc == null) return;
            
            // Create a formatter object for normalizing names
            _formatter = GetFormatter();
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(extractingInfos);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            // Extract location information from the KNX file
            var locationInfo = knxDoc.Descendants(_globalKnxNamespace + "Space")
                .Where(s => 
                {
                    var type = s.Attribute("Type")?.Value;
                    return type == "Room" || type == "Corridor";
                })
                .Select(room =>
                {
                    var getAncestorName = new Func<string, string>(type =>
                        room.Ancestors(_globalKnxNamespace + "Space")
                            .FirstOrDefault(s => s.Attribute("Type")?.Value == type)
                            ?.Attribute("Name")?.Value ?? string.Empty
                    );

                    var getDescendantName = new Func<string, string>(type =>
                        room.Descendants(_globalKnxNamespace + "Space")
                            .FirstOrDefault(s => s.Attribute("Type")?.Value == type)
                            ?.Attribute("Name")?.Value ?? string.Empty
                    );
                    
                    var allDeviceRefs = room.Descendants(_globalKnxNamespace + "DeviceInstanceRef")
                        .Select(dir => dir.Attribute("RefId")?.Value)
                        .ToList();
                    
                    var deviceRefsInDistributionBoards = room.Elements(_globalKnxNamespace + "Space")
                        .Where(s => s.Attribute("Type")?.Value == "DistributionBoard")
                        .SelectMany(db => db.Elements(_globalKnxNamespace + "DeviceInstanceRef")
                            .Select(dir => dir.Attribute("RefId")?.Value))
                        .ToList();

                    return new
                    {
                        RoomName = room.Attribute("Name")?.Value,
                        FloorName = getAncestorName("Floor"),
                        BuildingPartName = getAncestorName("BuildingPart"),
                        BuildingName = getAncestorName("Building"),
                        DistributionBoardName = getDescendantName("DistributionBoard"),
                        DeviceRefs = allDeviceRefs, 
                        DeviceRefsInDistributionBoards = deviceRefsInDistributionBoards 
                    };
                })
                .ToList();

            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(infosExtracted);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            // Display extracted location information
            TotalDevices = 0;
            App.ConsoleAndLogWriteLine("Extracted Location Information:");
            foreach (var loc in locationInfo)
            {
                TotalDevices += loc.DeviceRefs.Count;
                var message = string.Empty;
                if (loc.DistributionBoardName != string.Empty)
                {
                    message = $"Distribution Board Name : {loc.DistributionBoardName} ";
                }

                message += $"Room Name: {loc.RoomName}, Floor: {loc.FloorName}, Building Part: {loc.BuildingPartName}, Building: {loc.BuildingName}";
                App.ConsoleAndLogWriteLine(message);
                foreach (var deviceRef in loc.DeviceRefs)
                {
                    App.ConsoleAndLogWriteLine($"  DeviceRef: {deviceRef}");
                }
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(extractingDeviceReferences);

            // Check if user as close mainWindow
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
            // Extract device instance references and  their group object instance references from the KNX file
            var deviceRefsTemp1 = knxDoc.Descendants(_globalKnxNamespace + "DeviceInstance")
                .Select(di =>
                {
                    var id = di.Attribute("Id")?.Value;
                    var hardware2ProgramRefId = di.Attribute("Hardware2ProgramRefId")?.Value;
                    var productRefId = di.Attribute("ProductRefId")?.Value;

                    var groupObjectInstanceRefs = di.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
                        .Where(cir => cir.Attribute("Links") != null)
                        .SelectMany(cir =>
                        {
                            var links = cir.Attribute("Links")?.Value.Split(' ') ?? [];
                            var comObjectInstanceRefId = cir.Attribute("RefId")?.Value;
                            if (comObjectInstanceRefId != null && !comObjectInstanceRefId.StartsWith("O-") && comObjectInstanceRefId.Contains("O-"))
                            {
                                var index = comObjectInstanceRefId.IndexOf("O-", StringComparison.Ordinal);
                                comObjectInstanceRefId = comObjectInstanceRefId.Substring(index);
                            }
                            var readFlag = cir.Attribute("ReadFlag")?.Value;
                            var writeFlag = cir.Attribute("WriteFlag")?.Value;
                            var transmitFlag = cir.Attribute("TransmitFlag")?.Value;

                            return links.Select((link, index) => new
                            {
                                GroupAddressRef = link,
                                DeviceInstanceId = id,
                                ComObjectInstanceRefId = comObjectInstanceRefId,
                                IsFirstLink = index == 0,
                                ReadFlag = readFlag,
                                WriteFlag = writeFlag,
                                TransmitFlag = transmitFlag 
                            });
                        });

                    return new
                    {
                        Id = id,
                        Hardware2ProgramRefId = hardware2ProgramRefId,
                        ProductRefId = productRefId,
                        GroupObjectInstanceRefs = groupObjectInstanceRefs
                    };
                });
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(extractingDeviceInfo);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
            var deviceRefs = deviceRefsTemp1.SelectMany(di =>
            {
                var formattedHardware = di.Hardware2ProgramRefId != null ? 
                    (FormatHardware2ProgramRefId(di.Hardware2ProgramRefId)) : 
                    (null, null);

                var hardwareFileName = formattedHardware.Item1;
                var mxxxxDirectory = formattedHardware.Item2;
                var isDeviceRailMounted = mxxxxDirectory != null && di.ProductRefId != null && 
                                          GetIsDeviceRailMounted(di.ProductRefId, mxxxxDirectory);

                return di.GroupObjectInstanceRefs.Select(g => new
                {
                    di.Id,
                    di.ProductRefId,
                    HardwareFileName = hardwareFileName,
                    MxxxxDirectory = mxxxxDirectory,
                    IsDeviceRailMounted = isDeviceRailMounted,
                    g.GroupAddressRef,
                    g.DeviceInstanceId,
                    g.ComObjectInstanceRefId,
                    g.ReadFlag,
                    g.WriteFlag,
                    g.TransmitFlag,
                    ObjectType = GetObjectType(hardwareFileName ?? string.Empty, mxxxxDirectory ?? string.Empty, g.ComObjectInstanceRefId ?? string.Empty, g.ReadFlag ?? string.Empty, g.WriteFlag ?? string.Empty, g.TransmitFlag ?? string.Empty)
                });
            }).ToList();

            App.DisplayElements?.LoadingWindow?.UpdateTaskName($"{task} 2/3");
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(infoAndReferencesExtracted);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
            // Display extracted device instance references
            App.ConsoleAndLogWriteLine("Extracted Device Instance References:");
            foreach (var dr in deviceRefs)
            {
                App.ConsoleAndLogWriteLine($"Device Instance ID: {dr.DeviceInstanceId}, Product Ref ID: {dr.ProductRefId}, Is Device Rail Mounted ? : {dr.IsDeviceRailMounted}, Group Address Ref: {dr.GroupAddressRef}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            
            var groupingAdresses = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabic
                "AR" => "تجميع المعلومات حول عناوين المجموعة...",
                // Bulgarian
                "BG" => "Групиране на информация за груповите адреси...",
                // Czech
                "CS" => "Skupinové informace o adresách...",
                // Danish
                "DA" => "Gruppering af oplysninger om gruppeadresser...",
                // German
                "DE" => "Zusammenfassung von Informationen zu Gruppenadressen...",
                // Greek
                "EL" => "Ομαδοποίηση πληροφοριών σχετικά με τις διευθύνσεις ομάδας...",
                // English
                "EN" => "Grouping information about group addresses...",
                // Spanish
                "ES" => "Agrupando información sobre direcciones de grupo...",
                // Estonian
                "ET" => "Grupiaadresside teabe koondamine...",
                // Finnish
                "FI" => "Ryhmäosoitteiden tietojen ryhmittely...",
                // Hungarian
                "HU" => "Csoportcímekre vonatkozó információk csoportosítása...",
                // Indonesian
                "ID" => "Mengelompokkan informasi tentang alamat grup...",
                // Italian
                "IT" => "Raggruppamento delle informazioni sugli indirizzi di gruppo...",
                // Japanese
                "JA" => "グループアドレスに関する情報をグループ化しています...",
                // Korean
                "KO" => "그룹 주소에 대한 정보 그룹화 중...",
                // Latvian
                "LV" => "Grupas adrešu informācijas grupēšana...",
                // Lithuanian
                "LT" => "Grupės adresų informacijos grupavimas...",
                // Norwegian
                "NB" => "Gruppering av informasjon om gruppeadresser...",
                // Dutch
                "NL" => "Groeperen van informatie over groepsadressen...",
                // Polish
                "PL" => "Grupowanie informacji o adresach grupowych...",
                // Portuguese
                "PT" => "Agrupando informações sobre endereços de grupo...",
                // Romanian
                "RO" => "Gruparea informațiilor despre adresele de grup...",
                // Russian
                "RU" => "Группировка информации о групповых адресах...",
                // Slovak
                "SK" => "Zoskupovanie informácií o skupinových adresách...",
                // Slovenian
                "SL" => "Združevanje informacij o skupinskih naslovih...",
                // Swedish
                "SV" => "Grupperar information om gruppadresser...",
                // Turkish
                "TR" => "Grup adresleri hakkında bilgi gruplandırma...",
                // Ukrainian
                "UK" => "Групування інформації про групові адреси...",
                // Chinese (simplified)
                "ZH" => "正在整理组地址信息...",
                // Default case (french)
                _ => "Regroupement des informations sur les adresses de groupe..."
            };
            
            App.DisplayElements?.LoadingWindow?.LogActivity(groupingAdresses);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            // Group deviceRefs by GroupAddressRef
            var groupedDeviceRefs = deviceRefs.GroupBy(dr => dr.GroupAddressRef)
                .Select(g => new
                {
                    GroupAddressRef = g.Key,
                    Devices = g.ToList()
                })
                .ToList();
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            // Display grouped device instance references
            App.ConsoleAndLogWriteLine("Grouped Device Instance References:");
            foreach (var group in groupedDeviceRefs)
            {
                App.ConsoleAndLogWriteLine($"Group Address Ref: {group.GroupAddressRef}");
                foreach (var dr in group.Devices)
                {
                    App.ConsoleAndLogWriteLine(
                        $"  Device Instance ID: {dr.DeviceInstanceId}, Product Ref ID: {dr.ProductRefId}, Is Device Rail Mounted ? : {dr.IsDeviceRailMounted}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
                }
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(constructingNewAddresses);
            // Collection to track the IDs of renamed GroupAddresses
            HashSet<string> renamedGroupAddressIds = new HashSet<string>();

            TotalAddresses = groupedDeviceRefs.Count;
            var totalGroup = groupedDeviceRefs.Count;
            var countGroup = 1;

            // Construct the new name of the group address by iterating through each group of device references
            
            foreach (var gdr in groupedDeviceRefs)
            {
                App.DisplayElements?.LoadingWindow?.UpdateLogActivity(9, constructingNewAddresses + $" ({countGroup++}/{totalGroup})");
                // Get the first rail-mounted device reference, if any
                var deviceRailMounted = gdr.Devices.FirstOrDefault(dr => dr.IsDeviceRailMounted);
                // Get the first device reference with a non-empty ObjectType, if any
                var deviceRefObjectType = gdr.Devices.FirstOrDefault(dr => !string.IsNullOrEmpty(dr.ObjectType));

                var deviceUsed = deviceRailMounted;
                if (deviceUsed == null && deviceRefObjectType != null)
                {
                    deviceUsed = deviceRefObjectType;
                }
                else if (deviceUsed == null)
                {
                    deviceUsed = gdr.Devices.FirstOrDefault(dr => dr.IsDeviceRailMounted == false);
                }
                
                // Find the GroupAddress element that matches the device's GroupAddressRef
                var groupAddressElement = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                    .FirstOrDefault(ga => ga.Attribute("Id")?.Value.EndsWith(deviceUsed?.GroupAddressRef!) == true);

                if (groupAddressElement == null)
                {
                    App.ConsoleAndLogWriteLine($"No GroupAddress element found for GroupAddressRef: {deviceUsed?.GroupAddressRef}");
                    continue; 
                }

                App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                var nameAttr = groupAddressElement.Attribute("Name");

                if (nameAttr == null)
                {
                    App.ConsoleAndLogWriteLine($"No group address name found for  {groupAddressElement}");
                    continue; 
                }

                var nameAttrWords = nameAttr.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                var location = locationInfo
                    .FirstOrDefault(loc => loc.DeviceRefs
                        .Any(deviceRef => gdr.Devices
                            .Any(dr => dr.IsDeviceRailMounted == false && dr.DeviceInstanceId == deviceRef)));
                var deviceLocated = gdr.Devices.FirstOrDefault(d => location != null && location.DeviceRefs.Any(dr => dr == d.DeviceInstanceId))?.DeviceInstanceId ?? string.Empty;
                
                // Browse all the locations linked to a device to find a match with nameAttr
                foreach (var device in gdr.Devices)
                {
                    var tempLocation = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(device.DeviceInstanceId));
                    if (tempLocation?.RoomName != null)
                    {
                        var normalizedRoomName = RemoveDiacritics(tempLocation.RoomName);
                        var normalizedRoomWords = normalizedRoomName.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

                        var allWordsMatch = normalizedRoomWords.All(roomWord => 
                            nameAttrWords.Any(word => RemoveDiacritics(word).Equals(roomWord, StringComparison.OrdinalIgnoreCase))
                        );

                        if (allWordsMatch)
                        {
                            location = tempLocation;
                            deviceLocated = device.DeviceInstanceId ?? string.Empty;
                            break;
                        }
                    }
                }

                if (location == null)
                {
                    location = locationInfo
                        .FirstOrDefault(loc => loc.DeviceRefs
                            .Any(deviceRef => gdr.Devices
                                .Any(dr => dr.IsDeviceRailMounted && dr.DeviceInstanceId == deviceRef)));
                    deviceLocated = gdr.Devices.FirstOrDefault(d => location != null && location.DeviceRefs.Any(dr => dr == d.DeviceInstanceId))?.DeviceInstanceId ?? string.Empty;

                }
                    
                var nameLocation = GetLocationName(location!, nameAttr.Value, deviceLocated);
                    
                // Determine the nameObjectType based on the available device references
                var nameObjectType =
                    DetermineNameObjectType(deviceRailMounted!, deviceRefObjectType!, nameAttr.Value);

                var nameFunction = GetGroupRangeFunctionName(groupAddressElement);
                    
                // Construct the new name by combining the object type, function, and location
                var newName = nameObjectType + nameFunction + nameLocation;
                App.ConsoleAndLogWriteLine($"Original Name: {nameAttr.Value}");
                App.ConsoleAndLogWriteLine($"New Name: {newName}");
                nameAttr.Value = newName;  // Update the GroupAddress element's name

                if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses)
                {
                    // Mark the address as renamed
                    renamedGroupAddressIds.Add(groupAddressElement.Attribute("Id")?.Value ?? string.Empty); 
                }
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
            
            // Load the original XML file without any additional modifications
            var originalKnxDoc = App.Fm?.LoadXmlDocument(App.Fm.ZeroXmlPath);
            
            //Duplicate the knxDoc for the unused addresses
            var baseKnxDoc = new XDocument(knxDoc);

            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(suppressedAddresses);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
            // Deletes unused (not renamed) GroupAddresses if requested
            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses && originalKnxDoc != null)
            {
                await using var writer = new StreamWriter(App.Fm?.ProjectFolderPath + "/deleted_group_addresses.txt", append: true); 
                
                var title = "DELETED ADDRESSES";
                var border = new string('-', 68);
                var formattedTitle = $"|{title.PadLeft((66 + title.Length) / 2),-66}|";
                await writer.WriteLineAsync(border);
                await writer.WriteLineAsync(formattedTitle);
                await writer.WriteLineAsync(border);
                
                var allGroupAddresses = originalKnxDoc.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
                
                TotalDeletedAddresses = allGroupAddresses.Count - groupedDeviceRefs.Count();
                var totalAddressesUnused = allGroupAddresses.Count - groupedDeviceRefs.Count();
                var countAddressesUnused = 1;
                
                foreach (var groupAddress in allGroupAddresses)
                {
                    var groupId = groupAddress.Attribute("Id")?.Value;
                    if (groupId != null && !renamedGroupAddressIds.Contains(groupId))
                    {
                        App.DisplayElements?.LoadingWindow?.UpdateLogActivity(10, suppressedAddresses + $" ({countAddressesUnused++}/{totalAddressesUnused})");

                        var groupElement = groupAddress.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
                        var msg = new StringBuilder();
                        msg.AppendLine("--------------------------------------------------------------------");
                        msg.AppendLine($"Group Address ID: {groupId}");
                        msg.AppendLine($"Name: {groupAddress.Attribute("Name")?.Value}");
                        msg.AppendLine("Hierarchy:");


                        var ancestorGroupElement = groupElement?.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
                        if (ancestorGroupElement != null)
                        {
                            msg.AppendLine($"  -> {ancestorGroupElement.Attribute("Name")?.Value}");
                        }

                        if (groupElement != null)
                        {
                            msg.AppendLine($"    -> {groupElement.Attribute("Name")?.Value}");
                        }

                        msg.AppendLine("--------------------------------------------------------------------");

                        await writer.WriteLineAsync(msg.ToString()); // Write message in the log file named deleted_group_addresses

                        // Delete it in originalKnxDoc
                        groupAddress.Remove();

                        // Add an "*" to addresses to identify them in ETS
                        var correspondingGroupAddressInBaseKnxDoc = baseKnxDoc?.Descendants(_globalKnxNamespace + "GroupAddress")
                            .FirstOrDefault(ga => ga.Attribute("Id")?.Value == groupId);
                        if (correspondingGroupAddressInBaseKnxDoc != null)
                        {
                            correspondingGroupAddressInBaseKnxDoc.Attribute("Name")!.Value = "*" + correspondingGroupAddressInBaseKnxDoc.Attribute("Name")?.Value;
                        }

                        // Delete it in knxDoc
                        var correspondingGroupAddressInKnxDoc = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                            .FirstOrDefault(ga => ga.Attribute("Id")?.Value == groupId);

                        if (correspondingGroupAddressInKnxDoc != null)
                        {
                            correspondingGroupAddressInKnxDoc.Remove();
                            App.ConsoleAndLogWriteLine($"Removed unrenamed GroupAddress ID: {groupId}");
                        }
                    }
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }

            }

            // Save the updated XML files
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(savingUpdatedXml);
            App.Fm?.SaveXml(knxDoc, $"{App.Fm.ProjectFolderPath}0_updated.xml");

            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses && originalKnxDoc != null)
            {
                App.Fm?.SaveXml(originalKnxDoc, $"{App.Fm.ProjectFolderPath}0_original.xml");
                App.Fm?.SaveXml(baseKnxDoc!, $"{App.Fm.ProjectFolderPath}0_updatedUnusedAddresses.xml");
            }
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during CorrectName(): {ex.Message}");
        }
    }

    
    // Method that retrieves the ReadFlag,  WriteFlag and TransmitFlag associated with a participant to determine its ObjectType (Cmd/Ie)
    /// <summary>
    /// Retrieves the object type based on the ReadFlag,  WriteFlag and TransmitFlag values from a hardware XML file.
    /// This method constructs the path to the hardware XML file located in the specified directory and attempts
    /// to locate the ComObjectRef element with a matching ID based on the provided ComObjectInstanceRefId.
    /// If the ReReadFlag,  WriteFlag or TransmitFlag attributes are not found in ComObjectRef, it checks the ComObject element.
    /// The object type ("Cmd" or "Ie") is determined based on the combination of ReadFlag,  WriteFlag and TransmitFlag values.
    /// If errors occur during file or directory access, XML parsing, or if the expected elements or attributes
    /// are not found, the method logs an error and returns an empty string.
    /// 
    /// <param name="hardwareFileName">The name of the hardware XML file to be loaded.</param>
    /// <param name="mxxxxDirectory">The directory containing the Mxxxx files.</param>
    /// <param name="comObjectInstanceRefId">The reference ID of the ComObjectInstance to locate in the XML file.</param>
    /// <param name="readFlagFound">The initial value of the read flag if available.</param>
    /// <param name="writeFlagFound">The initial value of the write flag if available.</param>
    /// <param name="transmitFlagFound">The initial value of the transmit flag if available.</param>
    /// <returns>
    /// Returns the object type ("Cmd" or "Ie") based on the ReadFlag,  WriteFlag and TransmitFlag attributes, or an empty string if
    /// the file, directory, or expected XML elements/attributes are not found or if an error occurs.
    /// </returns>
    /// </summary>
    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    private static string GetObjectType(string hardwareFileName, string mxxxxDirectory, string comObjectInstanceRefId, string readFlagFound, string writeFlagFound, string transmitFlagFound)
    {
        // Construct a unique key for the cache based on the function parameters
        var cacheKey = $"{hardwareFileName}_{mxxxxDirectory}_{comObjectInstanceRefId}_{readFlagFound}_{writeFlagFound}_{transmitFlagFound}";

        // Check if the result is already in the cache
        if (ObjectTypeCache.TryGetValue(cacheKey, out var cacheResult))
        {
            return cacheResult;
        }
        
        // Construct the full path to the Mxxxx directory
        var mxxxxDirectoryPath = Path.Combine(_projectFilesDirectory, mxxxxDirectory);

        try
        {
            var readFlag = readFlagFound;
            var writeFlag = writeFlagFound;
            var transmitFlag = transmitFlagFound;
            if (string.IsNullOrEmpty(readFlag) || string.IsNullOrEmpty(writeFlag)||string.IsNullOrEmpty(transmitFlag))
            {
                // Check if the Mxxxx directory exists
                if (!Directory.Exists(mxxxxDirectoryPath))
                {
                    App.ConsoleAndLogWriteLine($"Directory not found: {mxxxxDirectoryPath}");
                    return string.Empty;
                }

                // Construct the full path to the hardware file
                var filePath = Path.Combine(mxxxxDirectoryPath, hardwareFileName);

                // Check if the hardware file exists
                if (!File.Exists(filePath))
                {
                    App.ConsoleAndLogWriteLine($"File not found: {filePath}");
                    var newFilePath = Path.Combine(mxxxxDirectoryPath,FormatNewFileName(comObjectInstanceRefId, hardwareFileName));

                    if (!File.Exists(newFilePath))
                    {
                        App.ConsoleAndLogWriteLine($"File not found: {newFilePath}");
                        return string.Empty;  
                    }

                    filePath = newFilePath ;
                }

                App.ConsoleAndLogWriteLine($"Opening file: {filePath}");

                // Load the XML file
                var hardwareDoc = App.Fm?.LoadXmlDocument(filePath);
                    
                // Find the ComObject element with the matching ID
                var comObjectRefElement = hardwareDoc?.Descendants(_globalKnxNamespace + "ComObjectRef")
                    .FirstOrDefault(co => co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefId) == true);
                    
                if (comObjectRefElement == null)
                {
                    App.ConsoleAndLogWriteLine($"ComObjectRef with Id ending in: {comObjectInstanceRefId} not found in file: {filePath}");
                    return string.Empty;
                }

                App.ConsoleAndLogWriteLine($"Found ComObjectRef with Id ending in: {comObjectInstanceRefId}");
                if (string.IsNullOrEmpty(readFlag)) readFlag = comObjectRefElement.Attribute("ReadFlag")?.Value;
                if (string.IsNullOrEmpty(writeFlag)) writeFlag = comObjectRefElement.Attribute("WriteFlag")?.Value;
                if (string.IsNullOrEmpty(transmitFlag)) transmitFlag = comObjectRefElement.Attribute("TransmitFlag")?.Value;

                // If ReadFlag, WriteFlag or TransmitFlag are not found in ComObjectRef, check in ComObject
                if (readFlag == null || writeFlag == null || transmitFlag == null)
                {
                    var comObjectInstanceRefIdCut = comObjectInstanceRefId.IndexOf('_') >= 0 ? 
                        comObjectInstanceRefId.Substring(0,comObjectInstanceRefId.IndexOf('_')) : null;

                    if (hardwareDoc != null)
                    {
                        var comObjectElement = hardwareDoc.Descendants(_globalKnxNamespace + "ComObject")
                            .FirstOrDefault(co => comObjectInstanceRefIdCut != null && co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefIdCut) == true);
                        if (comObjectElement == null)
                        {
                            App.ConsoleAndLogWriteLine($"ComObject with Id ending in: {comObjectInstanceRefIdCut} not found in file: {filePath}");
                            return string.Empty;
                        }

                        App.ConsoleAndLogWriteLine($"Found ComObject with Id ending in: {comObjectInstanceRefIdCut}");
                                
                        // ??= is used to assert the expression if the variable is null
                        if (string.IsNullOrEmpty(readFlag)) readFlag = comObjectElement.Attribute("ReadFlag")?.Value;
                        if (string.IsNullOrEmpty(writeFlag)) writeFlag = comObjectElement.Attribute("WriteFlag")?.Value;
                        if (string.IsNullOrEmpty(transmitFlag)) transmitFlag = comObjectElement.Attribute("TransmitFlag")?.Value;
                    }
                }
            }        
            App.ConsoleAndLogWriteLine($"ReadFlag: {readFlag}, WriteFlag: {writeFlag}, TransmitFlag : {transmitFlag}");
               
            // Determine the ObjectType based on the ReadFlag and WriteFlag values
            string result;
            if (readFlag == "Enabled" && writeFlag == "Disabled")
            {
                result = "Ie";
            }
            else if (writeFlag == "Enabled" && readFlag == "Disabled")
            {
                result = "Cmd";
            }
            else if (writeFlag == "Enabled" && readFlag == "Enabled")
            {
                result = "Cmd";
            }
            else if (transmitFlag == "Enabled" && writeFlag == "Disabled")
            {
                result = "Ie";
            }
            else
            {
                result = string.Empty;
            }

            // Store the result in the cache before returning
            ObjectTypeCache[cacheKey] = result;
            return result;
        }
        catch (FileNotFoundException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: File not found. {ex.Message}");
            return string.Empty;
        }
        catch (DirectoryNotFoundException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: Directory not found. {ex.Message}");
            return string.Empty;
        }
        catch (XmlException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: Invalid XML. {ex.Message}");
            return string.Empty;
        }
        catch (IOException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: IO exception occurred. {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred in GetObjectType(): {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Formats a new file name based on the provided comObjectInstanceRefId and hardwareFileName.
    /// 
    /// This method retrieves an XML document from App.Fm.ZeroXmlPath and uses it to find a parameter reference ID 
    /// based on the comObjectInstanceRefId and hardwareFileName. It removes a specific prefix ("M-XXXX_A") and ".xml" 
    /// extension from hardwareFileName to clean it up. The cleaned file name is used to query the XML document and 
    /// construct a new file name based on the retrieved parameter reference ID. The function caches results based on 
    /// hardwareFileName to optimize subsequent calls.
    /// 
    /// If the XML document is not available or if the required attributes are not found, the method returns an empty string.
    /// </summary>
    /// <param name="comObjectInstanceRefId">The reference ID used to identify the com object instance.</param>
    /// <param name="hardwareFileName">The original hardware file name.</param>
    /// <returns>The formatted new file name.</returns>
    private static string FormatNewFileName(string comObjectInstanceRefId, string hardwareFileName)
    {
        // Check if the filename has already been formatted and cached
        if (NewFileNameCache.TryGetValue(hardwareFileName, out var cachedResult))
        {
            return cachedResult;
        }

        var knxDoc = App.Fm?.LoadXmlDocument(App.Fm.ZeroXmlPath);
        if (knxDoc == null) return string.Empty;
        
        // Determine the part to remove (starts with "M-" and ends with "_A")
        var prefixToRemove = "M-"; // Fixed part before the number
        var suffixToRemove = "_A"; // Fixed part after the number

        // Find the index where the number starts (after "M-")
        var startIndex = hardwareFileName.IndexOf(prefixToRemove, StringComparison.Ordinal) + prefixToRemove.Length;

        // Find the index where the number ends (just before "_A")
        var endIndex = hardwareFileName.IndexOf(suffixToRemove, startIndex, StringComparison.Ordinal);

        // Extract the number part
        var numberPart = hardwareFileName.Substring(startIndex, endIndex - startIndex);

        // Remove the prefix and suffix
        var cleanedFileName = hardwareFileName.Replace(prefixToRemove + numberPart + suffixToRemove, "");

        // Remove the .xml extension
        cleanedFileName = cleanedFileName.Replace(".xml", "");

        // Query XML document to find ParameterInstanceRef based on cleanedFileName
        IEnumerable<XElement> comObject = knxDoc.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
            .Where(cir => cir.Attribute("RefId")?.Value == comObjectInstanceRefId);

        var parameterRefId = comObject.Ancestors(_globalKnxNamespace + "DeviceInstance").FirstOrDefault(co => co.Attribute("Hardware2ProgramRefId") != null &&
                co.Attribute("Hardware2ProgramRefId")!.Value.EndsWith(cleanedFileName))
            ?.Descendants(_globalKnxNamespace + "ParameterInstanceRef").Attributes("RefId").FirstOrDefault()?.Value ?? string.Empty;
        
        // Split parameterRefId based on "_P" and get the first part
        string[] parts = parameterRefId.Split(new[] { "_P" }, StringSplitOptions.None);
                    
        var formattedFileName = $"{parts[0]}.xml";

        // Cache the result for future use with this hardwareFileName
        NewFileNameCache[hardwareFileName] = formattedFileName;

        return formattedFileName;
    }
    
    
    // Method that reconstructs the name of the hardware file and its directory from the hardware2ProgramRefId of a device
    /// <summary>
    /// Reconstructs the hardware file name and directory based on the given hardware2ProgramRefId.
    ///
    /// This method extracts the hardware file name and directory from the provided <paramref name="hardware2ProgramRefId"/> 
    /// by splitting the string around the "HP" substring. It identifies the "M-XXXX" directory prefix from the portion before "HP" 
    /// and constructs the hardware file name using the extracted directory and the portion after "HP". If "HP" or the "M-XXXX" 
    /// prefix is not found, or if any errors occur, the method returns two empty strings.
    ///
    /// <param name="hardware2ProgramRefId">The reference ID used to reconstruct the hardware file name and directory.</param>
    /// <returns>
    /// A tuple containing the reconstructed hardware file name and the directory. Both values are empty strings if the
    /// reconstruction fails or an error occurs.
    /// </returns>
    /// </summary>
    private static (string HardwareFileName, string MxxxxDirectory) FormatHardware2ProgramRefId(string hardware2ProgramRefId)
    {
        // Check if the result is already in the cache
        if (Hardware2ProgramRefIdCache.TryGetValue(hardware2ProgramRefId, out var cachedResult))
        {
            return cachedResult;
        }
        
        try
        {
            // Extract the part before "HP" and the part after "HP"
            var parts = hardware2ProgramRefId.Split(new[] { "HP" }, StringSplitOptions.None);
            if (parts.Length < 2) return (string.Empty, string.Empty); // If "HP" is not found, return two empty strings

            var beforeHp = parts[0].TrimEnd('-');
            var afterHp = parts[1].TrimStart('-'); // Remove any leading hyphen after "HP"

            // Extract "M-XXXX" from the part before "HP"
            var mxxxxDirectory = beforeHp.Split('_').FirstOrDefault(part => part.StartsWith("M-"));

            if (string.IsNullOrEmpty(mxxxxDirectory)) return (string.Empty, string.Empty); // If "M-XXXX" is not found, return two empty strings

            var hardwareFileName = $"{mxxxxDirectory}_A-{afterHp}.xml";
            var result = (hardwareFileName, mxxxxDirectory);

            // Store the result in the cache before returning
            Hardware2ProgramRefIdCache[hardware2ProgramRefId] = result;
            return result;
        }
        catch (ArgumentNullException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: Argument null exception occurred. {ex.Message}");
            return (string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during FormatHardware2ProgramRefId(): {ex.Message}");
            return (string.Empty, string.Empty);
        }
    }
    
    
    // Method that retrieves and returns the value of IsRailMounted from the Hardware.xml file in the mxxxxDirectory of the device based on productRefId
    /// <summary>
    /// Retrieves the value of the <c>IsRailMounted</c> attribute for a specific device from the Hardware.xml file.
    ///
    /// This method constructs the path to the Hardware.xml file located in the specified <paramref name="mxxxxDirectory"/>
    /// and extracts the <c>IsRailMounted</c> attribute for the device with the specified <paramref name="productRefId"/>.
    /// If the directory or file does not exist, or if the required attribute cannot be found, the method logs an error and defaults
    /// to <c>false</c>. The method handles XML parsing errors and other exceptions gracefully.
    ///
    /// <param name="productRefId">The product reference ID of the device for which to retrieve the <c>IsRailMounted</c> attribute.</param>
    /// <param name="mxxxxDirectory">The name of the directory containing the Hardware.xml file.</param>
    /// <returns>
    /// <c>true</c> if the <c>IsRailMounted</c> attribute is present and set to true or 1; otherwise, <c>false</c> if the attribute is
    /// not found or set to false or 0, or if any error occurs.
    /// </returns>
    /// </summary>
    private static bool GetIsDeviceRailMounted(string productRefId, string mxxxxDirectory)
    {
        // Construct a unique key for the cache based on the function parameters
        var cacheKey = $"{productRefId}_{mxxxxDirectory}";

        // Check if the result is already in the cache
        if (IsDeviceRailMountedCache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }
        
        // Construct the full path to the Mxxxx directory
        var mxxxxDirectoryPath = Path.Combine(_projectFilesDirectory, mxxxxDirectory);
        
        // Construct the full path to the Hardware.xml file
        var hardwareFilePath = Path.Combine(mxxxxDirectoryPath, "Hardware.xml");
        
        //Check if the Directory exists
        if (!Directory.Exists(mxxxxDirectoryPath))
        { 
            App.ConsoleAndLogWriteLine($"{mxxxxDirectory} not found in directory: {mxxxxDirectoryPath}");
            return false; // Default to false if the directory does not exist
        } 
        
        // Check if the Hardware.xml file exists
        if (!File.Exists(hardwareFilePath)) 
        { 
            App.ConsoleAndLogWriteLine($"Hardware.xml not found in directory: {mxxxxDirectoryPath}"); 
            return false; // Default to false if the file does not exist
        }
        
        
        // Load the Hardware.xml file
        var hardwareDoc = App.Fm?.LoadXmlDocument(hardwareFilePath);

        // Find the Product element with the matching ID
        var productElement = hardwareDoc?.Descendants(_globalKnxNamespace + "Product")
                .FirstOrDefault(pe => pe.Attribute("Id")?.Value == productRefId);

        if (productElement == null)
        { 
            App.ConsoleAndLogWriteLine($"Product with Id: {productRefId} not found in file: {hardwareFilePath}");
            return false; // Default to false if the product is not found
        }

        // Get the IsRailMounted attribute value
        var isRailMountedAttr = productElement.Attribute("IsRailMounted");
        if (isRailMountedAttr == null) 
        { 
            App.ConsoleAndLogWriteLine($"IsRailMounted attribute not found for Product with Id: {productRefId}");
            return false; // Default to false if the attribute is not found
        }

        // Convert the attribute value to boolean
        var isRailMountedValue = isRailMountedAttr.Value.ToLower();
        bool result;
        if (isRailMountedValue == "true" || isRailMountedValue == "1")
        { 
            result = true;
        }
        else if (isRailMountedValue == "false" || isRailMountedValue == "0") 
        { 
            result = false;
        }
        else 
        { 
            App.ConsoleAndLogWriteLine($"Unexpected IsRailMounted attribute value: {isRailMountedAttr.Value} for Product with Id: {productRefId}");
            result = false; // Default to false for unexpected attribute values
        }

        // Store the result in the cache before returning
        IsDeviceRailMountedCache[cacheKey] = result;
        return result;
        
    }

    
    // Method that retrieves the namespace to use for searching in .xml files from the zeroFilePath (since the namespace varies depending on the ETS version)
    /// <summary>
    /// Sets the global KNX XML namespace from the specified XML file.
    ///
    /// This method loads the XML file located at <paramref name="zeroXmlFilePath"/> and retrieves
    /// the namespace declaration from the root element. If a namespace is found, it updates the
    /// static field <c>_globalKnxNamespace</c> with the retrieved namespace. If the XML file cannot
    /// be loaded or an error occurs during processing, appropriate error messages are logged.
    ///
    /// <param name="zeroXmlFilePath">The path to the XML file from which to extract the namespace.</param>
    /// </summary>
    private static void SetNamespaceFromXml(string zeroXmlFilePath)
    {
        try
        {
            var doc = new XmlDocument();

            // Load XML file
            doc.Load(zeroXmlFilePath);

            // Check the existence of the namespace in the root element
            var root = doc.DocumentElement;
            if (root != null)
            {
                // Get the namespace
                var xmlns = root.GetAttribute("xmlns");
                if (!string.IsNullOrEmpty(xmlns))
                {
                    _globalKnxNamespace = XNamespace.Get(xmlns);
                }
            }
        }
        catch (XmlException ex)
        {
            App.ConsoleAndLogWriteLine($"Error loading XML file (XML exception): {ex.Message}");
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during SetNamespaceFromXml(): {ex.Message}");
        }
    }

    
    /// <summary>
    /// Checks the validity of the DeepL API key by attempting a test translation.
    /// Retrieves the API key from the application settings, initializes the DeepL Translator,
    /// and performs a test translation to verify that the key is valid and operational.
    ///
    /// <returns>
    /// A tuple containing a boolean indicating success or failure and a string with an error message
    /// if the key is invalid or if an exception occurs during the check.
    /// </returns>
    /// </summary>
    public static (bool, string) CheckDeeplKey()
    {
        bool keyValid;
        string errMessage;
        
        try
        {
            // Check if the key is null
            if (string.IsNullOrEmpty(App.DisplayElements?.SettingsWindow?.DecryptStringFromBytes(App.DisplayElements.SettingsWindow.DeeplKey) ?? string.Empty))
            {
                throw new ArgumentNullException($"DeepL API key is not configured.");
            }
            
            // Initialize the DeepL Translator
            Translator = new Translator(App.DisplayElements?.SettingsWindow?.DecryptStringFromBytes(App.DisplayElements.SettingsWindow.DeeplKey) ?? string.Empty);

            GC.Collect();
            
            // Perform a small test translation to check the key
            Task.Run(async () =>
            {
                await Translator.TranslateTextAsync("test", "EN", "FR");
            }).GetAwaiter().GetResult();

            keyValid = true;
            errMessage = string.Empty;
        }
        catch (ArgumentNullException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabic
                "AR" => "حقل مفتاح API لـ DeepL فارغ. لن تعمل وظيفة الترجمة.",
                // Bulgarian
                "BG" => "Полето за API ключа на DeepL е празно. Функцията за превод няма да работи.",
                // Czech
                "CS" => "Pole klíče API DeepL je prázdné. Překladová funkce nebude fungovat.",
                // Danish
                "DA" => "DeepL API-nøglen er tom. Oversættelsesfunktionen vil ikke virke.",
                // German
                "DE" => "Das DeepL API-Schlüsselfeld ist leer. Die Übersetzungsfunktion wird nicht funktionieren.",
                // Greek
                "EL" => "Το πεδίο κλειδιού API του DeepL είναι κενό. Η λειτουργία μετάφρασης δεν θα λειτουργήσει.",
                // English
                "EN" => "The DeepL API key field is empty. The translation function will not work.",
                // Spanish
                "ES" => "El campo de la clave API de DeepL está vacío. La función de traducción no funcionará.",
                // Estonian
                "ET" => "DeepL API võtmeväli on tühi. Tõlkefunktsioon ei tööta.",
                // Finnish
                "FI" => "DeepL API-avainkenttä on tyhjä. Käännöstoiminto ei toimi.",
                // Hungarian
                "HU" => "A DeepL API kulcsmező üres. A fordítási funkció nem fog működni.",
                // Indonesian
                "ID" => "Kolom kunci API DeepL kosong. Fungsi terjemahan tidak akan berfungsi.",
                // Italian
                "IT" => "Il campo della chiave API di DeepL è vuoto. La funzione di traduzione non funzionerà.",
                // Japanese
                "JA" => "DeepL APIキーのフィールドが空です。翻訳機能は動作しません。",
                // Korean
                "KO" => "DeepL API 키 필드가 비어 있습니다. 번역 기능이 작동하지 않습니다.",
                // Latvian
                "LV" => "DeepL API atslēgas lauks ir tukšs. Tulkotāja funkcija nedarbosies.",
                // Lithuanian
                "LT" => "DeepL API rakto laukas tuščias. Vertimo funkcija neveiks.",
                // Norwegian
                "NB" => "DeepL API-nøkkelfeltet er tomt. Oversettelsesfunksjonen vil ikke fungere.",
                // Dutch
                "NL" => "Het DeepL API-sleutelveld is leeg. De vertaalfunctie zal niet werken.",
                // Polish
                "PL" => "Pole klucza API DeepL jest puste. Funkcja tłumaczenia nie będzie działać.",
                // Portuguese
                "PT" => "O campo da chave API do DeepL está vazio. A função de tradução não funcionará.",
                // Romanian
                "RO" => "Câmpul cheii API DeepL este gol. Funcția de traducere nu va funcționa.",
                // Russian
                "RU" => "Поле для API-ключа DeepL пусто. Функция перевода не будет работать.",
                // Slovak
                "SK" => "Pole API kľúča DeepL je prázdne. Prekladová funkcia nebude fungovať.",
                // Slovenian
                "SL" => "Polje za API ključ DeepL je prazno. Prevajalska funkcija ne bo delovala.",
                // Swedish
                "SV" => "DeepL API-nyckelfältet är tomt. Översättningsfunktionen kommer inte att fungera.",
                // Turkish
                "TR" => "DeepL API anahtar alanı boş. Çeviri işlevi çalışmayacak.",
                // Ukrainian
                "UK" => "Поле для ключа API DeepL порожнє. Функція перекладу не працюватиме.",
                // Chinese (simplified)
                "ZH" => "DeepL API 密钥字段为空。翻译功能将无法工作。",
                // Default case (french)
                _ => "Le champ de la clé API DeepL est vide. La fonction de traduction des adresses de groupe a été désactivée."
            };
        }
        catch (AuthorizationException ex)
        {
            App.ConsoleAndLogWriteLine($"DeepL API key error: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabic
                "AR" => "مفتاح API DeepL المدخل غير صحيح. تم تعطيل وظيفة ترجمة العناوين الجماعية.",
                // Bulgarian
                "BG" => "Въведеният DeepL API ключ е невалиден. Функцията за превод на групови адреси е деактивирана.",
                // Czech
                "CS" => "Zadaný klíč API DeepL je neplatný. Funkce překladu skupinových adres byla deaktivována.",
                // Danish
                "DA" => "Den indtastede DeepL API-nøgle er ugyldig. Funktionen til oversættelse af gruppeadresser er deaktiveret.",
                // German
                "DE" => "Der eingegebene DeepL API-Schlüssel ist ungültig. Die Übersetzungsfunktion für Gruppenadressen wurde deaktiviert.",
                // Greek
                "EL" => "Το κλειδί API του DeepL που εισάγατε δεν είναι έγκυρο. Η λειτουργία μετάφρασης διευθύνσεων ομάδας έχει απενεργοποιηθεί.",
                // English
                "EN" => "The entered DeepL API key is incorrect. The group address translation function has been disabled.",
                // Spanish
                "ES" => "La clave de API de DeepL ingresada es incorrecta. La función de traducción de direcciones de grupo ha sido desactivada.",
                // Estonian
                "ET" => "Sisestatud DeepL API võti on vale. Rühma aadresside tõlkimise funktsioon on keelatud.",
                // Finnish
                "FI" => "Syötetty DeepL API-avain on virheellinen. Ryhmäosoitteiden käännöstoiminto on poistettu käytöstä.",
                // Hungarian
                "HU" => "A megadott DeepL API kulcs érvénytelen. A csoportcímek fordítási funkciója le van tiltva.",
                // Indonesian
                "ID" => "Kunci API DeepL yang dimasukkan tidak valid. Fungsi terjemahan alamat grup telah dinonaktifkan.",
                // Italian
                "IT" => "La chiave API DeepL inserita non è valida. La funzione di traduzione degli indirizzi di gruppo è stata disattivata.",
                // Japanese
                "JA" => "入力されたDeepL APIキーが無効です。グループアドレス翻訳機能が無効になっています。",
                // Korean
                "KO" => "입력한 DeepL API 키가 잘못되었습니다. 그룹 주소 번역 기능이 비활성화되었습니다.",
                // Latvian
                "LV" => "Ievadītā DeepL API atslēga ir nepareiza. Grupas adreses tulkošanas funkcija ir atspējota.",
                // Lithuanian
                "LT" => "Įvestas neteisingas DeepL API raktas. Grupės adresų vertimo funkcija išjungta.",
                // Norwegian
                "NB" => "Den angitte DeepL API-nøkkelen er ugyldig. Funksjonen for oversettelse av gruppeadresser er deaktivert.",
                // Dutch
                "NL" => "De ingevoerde DeepL API-sleutel is ongeldig. De functie voor het vertalen van groepsadressen is uitgeschakeld.",
                // Polish
                "PL" => "Wprowadzony klucz API DeepL jest nieprawidłowy. Funkcja tłumaczenia adresów grupowych została wyłączona.",
                // Portuguese
                "PT" => "A chave API do DeepL inserida está incorreta. A função de tradução de endereços de grupo foi desativada.",
                // Romanian
                "RO" => "Cheia API DeepL introdusă este incorectă. Funcția de traducere a adreselor de grup a fost dezactivată.",
                // Russian
                "RU" => "Введенный ключ API DeepL неверен. Функция перевода групповых адресов отключена.",
                // Slovak
                "SK" => "Zadaný kľúč API DeepL je neplatný. Funkcia prekladu skupinových adries bola deaktivovaná.",
                // Slovenian
                "SL" => "Vneseni DeepL API ključ je neveljaven. Funkcija prevajanja skupinskih naslovov je onemogočena.",
                // Swedish
                "SV" => "Den angivna DeepL API-nyckeln är ogiltig. Funktionen för översättning av gruppadresser har inaktiverats.",
                // Turkish
                "TR" => "Girilen DeepL API anahtarı geçersiz. Grup adresi çeviri fonksiyonu devre dışı bırakıldı.",
                // Ukrainian
                "UK" => "Введений ключ API DeepL неправильний. Функцію перекладу групових адрес вимкнено.",
                // Chinese (simplified)
                "ZH" => "输入的 DeepL API 密钥不正确。已禁用群组地址翻译功能。",
                // Default case (french)
                _ => "La clé API DeepL entrée est incorrecte. La fonction de traduction des adresses de groupe a été désactivée."
            };

        }
        catch (HttpRequestException ex)
        {
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabic
                "AR" => $"خطأ في الاتصال بالشبكة عند التحقق من مفتاح API DeepL لتفعيل ترجمة عناوين المجموعة: {ex.Message}. يرجى التحقق من اتصال الشبكة وإعادة المحاولة.",
                // Bulgarian
                "BG" => $"Грешка в мрежовата връзка при проверка на API ключа на DeepL за активиране на превода на групови адреси: {ex.Message}. Моля, проверете мрежовата си връзка и опитайте отново.",
                // Czech
                "CS" => $"Chyba síťového připojení při ověřování klíče API DeepL pro aktivaci překladu skupinových adres: {ex.Message}. Zkontrolujte prosím své síťové připojení a zkuste to znovu.",
                // Danish
                "DA" => $"Netværksforbindelsesfejl ved verificering af DeepL API-nøglen for at aktivere oversættelse af gruppeadresser: {ex.Message}. Kontroller venligst din netværksforbindelse og prøv igen.",
                // German
                "DE" => $"Netzwerkverbindungsfehler bei der Überprüfung des DeepL API-Schlüssels zur Aktivierung der Übersetzung von Gruppenadressen: {ex.Message}. Bitte überprüfen Sie Ihre Netzwerkverbindung und versuchen Sie es erneut.",
                // Greek
                "EL" => $"Σφάλμα σύνδεσης δικτύου κατά την επαλήθευση του κλειδιού API DeepL για την ενεργοποίηση της μετάφρασης διευθύνσεων ομάδας: {ex.Message}. Ελέγξτε τη σύνδεση δικτύου σας και δοκιμάστε ξανά.",
                // English
                "EN" => $"Network connection error when verifying the DeepL API key to enable group address translation: {ex.Message}. Please check your network connection and try again.",
                // Spanish
                "ES" => $"Error de conexión de red al verificar la clave API de DeepL para habilitar la traducción de direcciones de grupo: {ex.Message}. Por favor, verifique su conexión de red y vuelva a intentarlo.",
                // Estonian
                "ET" => $"Võrguühenduse viga DeepL API võtme kontrollimisel rühma aadressi tõlke lubamiseks: {ex.Message}. Kontrollige oma võrguühendust ja proovige uuesti.",
                // Finnish
                "FI" => $"Verkkoyhteysvirhe tarkistettaessa DeepL API-avainta ryhmäosoitteiden kääntämisen aktivoimiseksi: {ex.Message}. Tarkista verkkoyhteytesi ja yritä uudelleen.",
                // Hungarian
                "HU" => $"Hálózati kapcsolat hiba a DeepL API kulcs ellenőrzésekor a csoportcím fordításának engedélyezéséhez: {ex.Message}. Kérjük, ellenőrizze a hálózati kapcsolatát, és próbálja újra.",
                // Indonesian
                "ID" => $"Kesalahan koneksi jaringan saat memverifikasi kunci API DeepL untuk mengaktifkan terjemahan alamat grup: {ex.Message}. Silakan periksa koneksi jaringan Anda dan coba lagi.",
                // Italian
                "IT" => $"Errore di connessione di rete durante la verifica della chiave API DeepL per abilitare la traduzione degli indirizzi di gruppo: {ex.Message}. Si prega di controllare la connessione di rete e riprovare.",
                // Japanese
                "JA" => $"グループアドレスの翻訳を有効にするためにDeepL APIキーを検証する際のネットワーク接続エラー: {ex.Message}. ネットワーク接続を確認して、もう一度やり直してください。",
                // Korean
                "KO" => $"그룹 주소 번역을 활성화하기 위해 DeepL API 키를 확인하는 동안 네트워크 연결 오류: {ex.Message}. 네트워크 연결을 확인하고 다시 시도하십시오.",
                // Latvian
                "LV" => $"Tīkla savienojuma kļūda, pārbaudot DeepL API atslēgu, lai aktivizētu grupas adrešu tulkošanu: {ex.Message}. Lūdzu, pārbaudiet savu tīkla savienojumu un mēģiniet vēlreiz.",
                // Lithuanian
                "LT" => $"Tinklo ryšio klaida tikrinant DeepL API raktą grupės adresų vertimui įjungti: {ex.Message}. Patikrinkite savo tinklo ryšį ir bandykite dar kartą.",
                // Norwegian
                "NB" => $"Nettverksforbindelsesfeil ved verifisering av DeepL API-nøkkelen for å aktivere oversettelse av gruppeadresser: {ex.Message}. Vennligst sjekk nettverksforbindelsen din og prøv igjen.",
                // Dutch
                "NL" => $"Netwerkverbindingsfout bij het verifiëren van de DeepL API-sleutel om groepsadresvertaling in te schakelen: {ex.Message}. Controleer uw netwerkverbinding en probeer het opnieuw.",
                // Polish
                "PL" => $"Błąd połączenia sieciowego podczas weryfikacji klucza API DeepL w celu włączenia tłumaczenia adresów grupowych: {ex.Message}. Sprawdź swoje połączenie sieciowe i spróbuj ponownie.",
                // Portuguese
                "PT" => $"Erro de conexão de rede ao verificar a chave API do DeepL para ativar a tradução de endereços de grupo: {ex.Message}. Verifique sua conexão de rede e tente novamente.",
                // Romanian
                "RO" => $"Eroare de conexiune la rețea la verificarea cheii API DeepL pentru a activa traducerea adreselor de grup: {ex.Message}. Vă rugăm să verificați conexiunea la rețea și să încercați din nou.",
                // Russian
                "RU" => $"Ошибка сетевого подключения при проверке ключа API DeepL для включения перевода групповых адресов: {ex.Message}. Пожалуйста, проверьте сетевое подключение и попробуйте еще раз.",
                // Slovak
                "SK" => $"Chyba sieťového pripojenia pri overovaní kľúča API DeepL na aktiváciu prekladu skupinových adries: {ex.Message}. Skontrolujte svoje sieťové pripojenie a skúste to znova.",
                // Slovenian
                "SL" => $"Napaka omrežne povezave pri preverjanju DeepL API ključa za omogočanje prevajanja skupinskih naslovov: {ex.Message}. Preverite svojo omrežno povezavo in poskusite znova.",
                // Swedish
                "SV" => $"Nätverksanslutningsfel vid verifiering av DeepL API-nyckeln för att aktivera översättning av gruppadresser: {ex.Message}. Kontrollera din nätverksanslutning och försök igen.",
                // Turkish
                "TR" => $"Grup adresi çevirisini etkinleştirmek için DeepL API anahtarı doğrulanırken ağ bağlantısı hatası: {ex.Message}. Lütfen ağ bağlantınızı kontrol edin ve tekrar deneyin.",
                // Ukrainian
                "UK" => $"Помилка мережевого підключення під час перевірки ключа API DeepL для активації перекладу групових адрес: {ex.Message}. Будь ласка, перевірте своє мережеве підключення та спробуйте ще раз.",
                // Chinese (simplified)
                "ZH" => $"在验证 DeepL API 密钥以启用组地址翻译时出现网络连接错误: {ex.Message}. 请检查您的网络连接，然后重试。",
                // Default case (french)
                _ => $"Erreur de connexion réseau lors de la vérification de la clé API DeepL pour activer la traduction des adresses de groupe: {ex.Message}. Veuillez vérifier votre connexion réseau et réessayer."
            };
            
        }
        catch (DeepLException ex)
        {
            App.ConsoleAndLogWriteLine($"DeepL API error: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabic
                "AR" => "خطأ أثناء تنشيط واجهة برمجة تطبيقات الترجمة DeepL: {ex.Message}. تم تعطيل ترجمة العناوين الجماعية.",
                // Bulgarian
                "BG" => "Грешка при активиране на API за превод DeepL: {ex.Message}. Преводът на групови адреси е деактивиран.",
                // Czech
                "CS" => "Chyba při aktivaci překladového API DeepL: {ex.Message}. Překlad skupinových adres byl deaktivován.",
                // Danish
                "DA" => "Fejl ved aktivering af DeepL-oversættelses-API: {ex.Message}. Oversættelse af gruppeadresser er deaktiveret.",
                // German
                "DE" => "Fehler bei der Aktivierung der DeepL-Übersetzungs-API: {ex.Message}. Die Übersetzung von Gruppenadressen wurde deaktiviert.",
                // Greek
                "EL" => "Σφάλμα κατά την ενεργοποίηση του API μετάφρασης DeepL: {ex.Message}. Η μετάφραση των διευθύνσεων ομάδας έχει απενεργοποιηθεί.",
                // English
                "EN" => "Error activating the DeepL translation API: {ex.Message}. Group address translation has been disabled.",
                // Spanish
                "ES" => "Error al activar la API de traducción de DeepL: {ex.Message}. La traducción de direcciones de grupo ha sido desactivada.",
                // Estonian
                "ET" => "Tõlke-API DeepL aktiveerimisel ilmnes viga: {ex.Message}. Grupi aadresside tõlkimine on keelatud.",
                // Finnish
                "FI" => "Virhe DeepL-käännös-API:n aktivoinnissa: {ex.Message}. Ryhmäosoitteiden kääntäminen on poistettu käytöstä.",
                // Hungarian
                "HU" => "Hiba történt a DeepL fordító-API aktiválása során: {ex.Message}. A csoportcímek fordítása le van tiltva.",
                // Indonesian
                "ID" => "Kesalahan saat mengaktifkan API terjemahan DeepL: {ex.Message}. Terjemahan alamat grup telah dinonaktifkan.",
                // Italian
                "IT" => "Errore durante l'attivazione dell'API di traduzione DeepL: {ex.Message}. La traduzione degli indirizzi di gruppo è stata disattivata.",
                // Japanese
                "JA" => "DeepL翻訳APIの有効化中にエラーが発生しました: {ex.Message}。グループアドレスの翻訳が無効になっています。",
                // Korean
                "KO" => "DeepL 번역 API 활성화 중 오류 발생: {ex.Message}. 그룹 주소 번역이 비활성화되었습니다.",
                // Latvian
                "LV" => "Kļūda, aktivizējot DeepL tulkošanas API: {ex.Message}. Grupas adrešu tulkošana ir atspējota.",
                // Lithuanian
                "LT" => "Klaida aktyvinant DeepL vertimo API: {ex.Message}. Grupės adresų vertimas išjungtas.",
                // Norwegian
                "NB" => "Feil ved aktivering av DeepL-oversettelses-API: {ex.Message}. Oversettelse av gruppeadresser er deaktivert.",
                // Dutch
                "NL" => "Fout bij het activeren van de DeepL-vertaal-API: {ex.Message}. Vertaling van groepsadressen is uitgeschakeld.",
                // Polish
                "PL" => "Błąd podczas aktywacji API tłumaczenia DeepL: {ex.Message}. Tłumaczenie adresów grupowych zostało wyłączone.",
                // Portuguese
                "PT" => "Erro ao ativar a API de tradução DeepL: {ex.Message}. A tradução de endereços de grupo foi desativada.",
                // Romanian
                "RO" => "Eroare la activarea API-ului de traducere DeepL: {ex.Message}. Traducerea adreselor de grup a fost dezactivată.",
                // Russian
                "RU" => "Ошибка при активации API перевода DeepL: {ex.Message}. Перевод групповых адресов отключен.",
                // Slovak
                "SK" => "Chyba pri aktivácii prekladového API DeepL: {ex.Message}. Preklad skupinových adries bol deaktivovaný.",
                // Slovenian
                "SL" => "Napaka pri aktivaciji prevajalskega API-ja DeepL: {ex.Message}. Prevajanje skupinskih naslovov je onemogočeno.",
                // Swedish
                "SV" => "Fel vid aktivering av DeepL-översättnings-API: {ex.Message}. Översättning av gruppadresser har inaktiverats.",
                // Turkish
                "TR" => "DeepL çeviri API'si etkinleştirilirken hata oluştu: {ex.Message}. Grup adresi çevirisi devre dışı bırakıldı.",
                // Ukrainian
                "UK" => "Помилка під час активації API перекладу DeepL: {ex.Message}. Переклад групових адрес вимкнено.",
                // Chinese (simplified)
                "ZH" => "激活DeepL翻译API时出错: {ex.Message}。组地址翻译已禁用。",
                // Default case (french)
                _ => $"Erreur lors de l'activation de l'API de traduction DeepL: {ex.Message}. La traduction des adresses de groupe a été désactivée."
            };

        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabic
                "AR" => $"حدث خطأ غير متوقع أثناء تفعيل واجهة برمجة تطبيقات الترجمة DeepL: {ex.Message}. تم تعطيل ترجمة عناوين المجموعة.",
                // Bulgarian
                "BG" => $"Възникна неочаквана грешка при активиране на API за превод на DeepL: {ex.Message}. Преводът на груповите адреси е деактивиран.",
                // Czech
                "CS" => $"Při aktivaci překládacího API DeepL došlo k neočekávané chybě: {ex.Message}. Překlad skupinových adres byl deaktivován.",
                // Danish
                "DA" => $"Der opstod en uventet fejl under aktivering af DeepL-oversættelses-API: {ex.Message}. Oversættelse af gruppeadresser er deaktiveret.",
                // German
                "DE" => $"Ein unerwarteter Fehler ist beim Aktivieren der DeepL-Übersetzungs-API aufgetreten: {ex.Message}. Die Übersetzung von Gruppenadressen wurde deaktiviert.",
                // Greek
                "EL" => $"Παρουσιάστηκε ένα απρόσμενο σφάλμα κατά την ενεργοποίηση του API μετάφρασης DeepL: {ex.Message}. Η μετάφραση των διευθύνσεων ομάδας έχει απενεργοποιηθεί.",
                // English
                "EN" => $"An unexpected error occurred while activating the DeepL translation API: {ex.Message}. Group address translation has been disabled.",
                // Spanish
                "ES" => $"Ocurrió un error inesperado al activar la API de traducción de DeepL: {ex.Message}. La traducción de direcciones de grupo ha sido desactivada.",
                // Estonian
                "ET" => $"DeepL tõlke API aktiveerimisel ilmnes ootamatu viga: {ex.Message}. Grupi aadresside tõlkimine on keelatud.",
                // Finnish
                "FI" => $"DeepL-käännös-API:ta aktivoitaessa tapahtui odottamaton virhe: {ex.Message}. Ryhmäosoitteiden kääntäminen on poistettu käytöstä.",
                // Hungarian
                "HU" => $"A DeepL fordító-API aktiválása során váratlan hiba történt: {ex.Message}. A csoportcímek fordítása letiltásra került.",
                // Indonesian
                "ID" => $"Terjadi kesalahan tak terduga saat mengaktifkan API terjemahan DeepL: {ex.Message}. Terjemahan alamat grup telah dinonaktifkan.",
                // Italian
                "IT" => $"Si è verificato un errore imprevisto durante l'attivazione dell'API di traduzione DeepL: {ex.Message}. La traduzione degli indirizzi di gruppo è stata disabilitata.",
                // Japanese
                "JA" => $"DeepL翻訳APIの有効化中に予期しないエラーが発生しました: {ex.Message}。グループアドレスの翻訳が無効になりました。",
                // Korean
                "KO" => $"DeepL 번역 API를 활성화하는 동안 예상치 못한 오류가 발생했습니다: {ex.Message}. 그룹 주소 번역이 비활성화되었습니다.",
                // Latvian
                "LV" => $"Aktivējot DeepL tulkošanas API, radās neparedzēta kļūda: {ex.Message}. Grupas adreses tulkošana ir atspējota.",
                // Lithuanian
                "LT" => $"Įjungiant „DeepL“ vertimo API įvyko nenumatyta klaida: {ex.Message}. Grupės adresų vertimas buvo išjungtas.",
                // Norwegian
                "NB" => $"En uventet feil oppsto under aktivering av DeepL oversettelses-API: {ex.Message}. Oversettelse av gruppeadresser er deaktivert.",
                // Dutch
                "NL" => $"Er is een onverwachte fout opgetreden bij het activeren van de DeepL-vertaal-API: {ex.Message}. Vertaling van groepsadressen is uitgeschakeld.",
                // Polish
                "PL" => $"Wystąpił nieoczekiwany błąd podczas aktywacji interfejsu API tłumaczenia DeepL: {ex.Message}. Tłumaczenie adresów grupowych zostało wyłączone.",
                // Portuguese
                "PT" => $"Ocorreu um erro inesperado ao ativar a API de tradução DeepL: {ex.Message}. A tradução de endereços de grupo foi desativada.",
                // Romanian
                "RO" => $"A apărut o eroare neașteptată în timpul activării API-ului de traducere DeepL: {ex.Message}. Traducerea adreselor de grup a fost dezactivată.",
                // Russian
                "RU" => $"Произошла непредвиденная ошибка при активации API перевода DeepL: {ex.Message}. Перевод групповых адресов был отключен.",
                // Slovak
                "SK" => $"Pri aktivácii prekladacieho API DeepL došlo k neočakávanej chybe: {ex.Message}. Preklad skupinových adries bol deaktivovaný.",
                // Slovenian
                "SL" => $"Pri aktivaciji prevajalskega API-ja DeepL je prišlo do nepričakovane napake: {ex.Message}. Prevajanje naslovov skupin je onemogočeno.",
                // Swedish
                "SV" => $"Ett oväntat fel uppstod vid aktivering av DeepL-översättnings-API: {ex.Message}. Översättning av gruppadresser har inaktiverats.",
                // Turkish
                "TR" => $"DeepL çeviri API'si etkinleştirilirken beklenmedik bir hata oluştu: {ex.Message}. Grup adresi çevirisi devre dışı bırakıldı.",
                // Ukrainian
                "UK" => $"Виникла непередбачена помилка під час активації API перекладу DeepL: {ex.Message}. Переклад групових адрес відключено.",
                // Chinese (simplified)
                "ZH" => $"激活DeepL翻译API时发生意外错误: {ex.Message}。已禁用群组地址翻译。",
                // Default case (french)
                _ => $"Une erreur inattendue est survenue lors de l'activation de l'API de traduction DeepL: {ex.Message}. La traduction des adresses de groupe a été désactivée."
            };

        }

        return (keyValid, errMessage);
    }
    
    /// <summary>
    /// Retrieves the appropriate Formatter instance based on the application settings.
    /// Checks if the settings window is accessible and if DeepL translation is enabled.
    /// If enabled, validates the DeepL API key and returns a translation formatter if the key is valid.
    /// Otherwise, returns a normalization formatter.
    ///
    /// <returns>
    /// A Formatter instance, either for translation if the DeepL API key is valid,
    /// or for normalization if the key is invalid or if DeepL translation is not enabled.
    /// </returns>
    /// </summary>
    private static Formatter GetFormatter()
    {
        if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableDeeplTranslation)
        {
            ValidDeeplKey = CheckDeeplKey().Item1;
            if (ValidDeeplKey)
            {
                return new FormatterTranslate();
            }
        }

        return new FormatterNormalize();
    }

    /// <summary>
    /// Generates a formatted location name based on the provided location details and name attribute value. 
    /// 
    /// If the `location` parameter is null, the function processes `nameAttrValue` to remove specific words ("cmd", "cde", "ie") 
    /// and returns the remaining text with extra spaces condensed. If location details are provided, it formats and combines 
    /// the building name, part name, floor name, room name, and optionally the distribution board name if the device reference 
    /// is included in the distribution board. Additional strings and circuit identifiers from `nameAttrValue` are also appended 
    /// if applicable.
    /// 
    /// <param name="location">An object with location details (building, part, floor, room, distribution board).</param>
    /// <param name="nameAttrValue">A string for processing additional names or identifiers.</param>
    /// <param name="deviceRef">A reference to check inclusion in the distribution board.</param>
    /// <returns>A formatted string representing the location name.</returns>
    /// </summary>
    private static string GetLocationName(dynamic location, string nameAttrValue, string deviceRef)
    {
        string nameLocation;
        if (location == null)
        {
            // If no location information is found, add the original name without cmd, cde or ie 
            App.ConsoleAndLogWriteLine("No location found");
            
            string[] splitNameAttr = nameAttrValue.Split(' ');
            var result = "";
            string[] wordsToRemove = ["cmd", "cde", "ie"];

            foreach (var part in splitNameAttr)
            {
                var shouldRemove = false;
                foreach (var word in wordsToRemove)
                {
                    if (part.Equals(word, StringComparison.OrdinalIgnoreCase))
                    {
                        shouldRemove = true;
                        break;
                    }
                }

                if (!shouldRemove)
                {
                    result += part + " ";
                }
            }

            // Remove extra spaces created by removing words
            result = result.Trim();
            nameLocation = "_";
            var lastWasSpace = false;

            foreach (var c in result)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace)
                    {
                        nameLocation += c;
                        lastWasSpace = true;
                    }
                }
                else
                {
                    nameLocation += c;
                    lastWasSpace = false;
                }
            }

            return nameLocation;

        }

        string buildingName = !string.IsNullOrEmpty(location.BuildingName) ? location.BuildingName : "Bâtiment";
        string buildingPartName = !string.IsNullOrEmpty(location.BuildingPartName) ? location.BuildingPartName : "Facade XX";
        string floorName = !string.IsNullOrEmpty(location.FloorName) ? location.FloorName : "Etage";
        string roomName = !string.IsNullOrEmpty(location.RoomName) ? location.RoomName : "Piece";
        string distributionBoardName = !string.IsNullOrEmpty(location.DistributionBoardName) ? location.DistributionBoardName : string.Empty;

        // Format the location details
        nameLocation = $"_{_formatter.Format(buildingName)}_{_formatter.Format(buildingPartName)}_{_formatter.Format(floorName)}_{_formatter.Format(roomName)}";
        if (!string.IsNullOrEmpty(distributionBoardName) && location.DeviceRefsInDistributionBoards.Contains(deviceRef))
        {
            nameLocation += $"_{_formatter.Format(distributionBoardName)}";
        }

        // Add StringsToAdd to the name if it exists
        var (matchStrings, valueStrings) = AsStringsToAddInPreviousName(nameAttrValue);
        if (matchStrings)
        {
            nameLocation = valueStrings.Aggregate(nameLocation, (current, word) => current + ("_" + word));
        }

        // Add circuit part to the name if it exists, and it was not already appended 
        var (matchCircuit, valueCircuit) = AsCircuitInPreviousName(nameAttrValue);
        if (matchCircuit && !valueStrings.Contains(valueCircuit))
        { 
            nameLocation += "_" + valueCircuit;
        }

        return nameLocation;
    }
    
    /// <summary>
    /// Analyzes a given name attribute value to determine if it includes a valid circuit identifier in its last word.
    /// The method splits the name attribute value into individual words and inspects the last word for the presence of both letters and digits.
    /// Special characters other than '/' and '+' will invalidate the last word as a circuit identifier.
    /// 
    /// <param name="nameAttrValue">A string containing the name attribute value to be analyzed.</param>
    /// <returns>
    /// A tuple containing a boolean and a string:
    /// - The boolean indicates whether the last word contains both letters and digits, signifying it as a valid circuit identifier.
    /// - The string returns the last word if it is valid, otherwise an empty string.
    /// </returns>
    /// </summary>
    private static (bool, string) AsCircuitInPreviousName(string nameAttrValue)
    {
        // Replace underscores with spaces
        var modifiedNameAttrValue = nameAttrValue.Replace('_', ' ');

        // Separate the string into words using spaces as delimiters
        string[] words = modifiedNameAttrValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Check for words and get the last word
        if (words.Length == 0)
        {
            return (false, string.Empty);
        }
        
        var lastWord = words[^1]; // words[length - 1]

        var containsLetter = false;
        var containsDigit = false;

        foreach (var c in lastWord)
        {
            if (char.IsLetter(c))
            {
                containsLetter = true;
            }
            if (char.IsDigit(c))
            {
                containsDigit = true;
            }
            if (!char.IsLetterOrDigit(c) && c != '/' && c != '+' && c != '-')
            {
                return (false, lastWord);
            }
        }

        var isValid = containsLetter && containsDigit;
        return (isValid, isValid ? lastWord : string.Empty);
    }
    
    /// <summary>
    /// Analyzes a given name attribute value to identify and return words that match any of the predefined strings to add, considering case-insensitivity and special matching rules.
    /// 
    /// The method first replaces underscores in the input string with spaces to ensure proper word separation. It then splits the modified string into individual words. Each word is compared against a list of predefined strings to add, which are converted to lowercase for case-insensitive comparison. 
    /// The predefined strings may include wildcards ('*') to indicate that a word should start with a specific prefix. The method checks for exact matches as well as prefix matches based on these rules.
    /// 
    /// The function returns a tuple containing:
    /// - A boolean indicating whether at least one word from the input string matches the criteria.
    /// - An array of strings containing all matching words.
    /// 
    /// If no matches are found, the boolean will be false and the array will be empty.
    /// 
    /// <param name="nameAttrValue">A string representing the name attribute value to be analyzed.</param>
    /// <returns>
    /// A tuple where:
    /// - The boolean is true if there are any matching words, false otherwise.
    /// - The string array contains the matching words, or is empty if no matches are found.
    /// </returns>
    /// </summary>
    private static (bool, string[]) AsStringsToAddInPreviousName(string nameAttrValue)
    {
        // Replace underscores with spaces
        var modifiedNameAttrValue = nameAttrValue.Replace('_', ' ');

       // Separate the string into words using spaces as delimiters
        string[] words = modifiedNameAttrValue.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        // Convert all stringsToAdd to lowercase for case-insensitive comparison
        var lowerCaseStringsToAdd = App.DisplayElements?.SettingsWindow?.StringsToAdd.Select(s => s.Text.ToLower()).ToArray();
        
        // List to hold matching words
        var matchingWords = new List<string>();

        // Check if any word matches a word in lowerCaseStringsToAdd (case-insensitive)
        if (lowerCaseStringsToAdd != null)
        {
            foreach (var word in words)
            {
                var lowerCaseWord = RemoveDiacritics(word.ToLower());

                var wordMatchesAnyStringToAdd = false;
                foreach (var stringToAdd in lowerCaseStringsToAdd)
                {
                    var stringToAddWords = stringToAdd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var allWordsMatch = true;
                    foreach (var stringToAddWord in stringToAddWords)
                    {
                        var lowerCaseStringToAddWord = RemoveDiacritics(stringToAddWord.ToLower());

                        if (lowerCaseStringToAddWord.Contains('*'))
                        {
                            // Split the stringToAddWord by '*'
                            var parts = lowerCaseStringToAddWord.Split('*');

                            // Check if the word matches the pattern
                            var currentIndex = 0;
                            var isMatch = true;

                            foreach (var part in parts)
                            {
                                if (string.IsNullOrEmpty(part)) continue;

                                currentIndex = lowerCaseWord.IndexOf(part, currentIndex, StringComparison.Ordinal);

                                if (currentIndex == -1)
                                {
                                    isMatch = false;
                                    break;
                                }

                                currentIndex += part.Length;
                            }

                            // Ensure that the entire word is covered by the pattern
                            if (!(isMatch && parts[0] == lowerCaseWord.Substring(0, parts[0].Length) && lowerCaseWord.EndsWith(parts[^1])))
                            {
                                allWordsMatch = false;
                                break;
                            }
                        }
                        else if (lowerCaseStringToAddWord != lowerCaseWord)
                        {
                            allWordsMatch = false;
                            break;
                        }
                    }

                    if (allWordsMatch)
                    {
                        wordMatchesAnyStringToAdd = true;
                        break;
                    }
                }

                if (wordMatchesAnyStringToAdd)
                {
                    matchingWords.Add(word);
                }
            }
        }
        return (matchingWords.Count > 0, matchingWords.ToArray());
    }

    /// <summary>
    /// Determines the formatted name for a device based on its type and the provided name attribute value.
    /// Checks for specific patterns in the name attribute value to categorize the device as "Cmd" or "Ie".
    /// If no patterns match, it returns the formatted ObjectType of the device if available.
    /// Otherwise, returns a default formatted type name.
    ///
    /// <param name="deviceRailMounted">A dynamic object representing a rail-mounted device with an ObjectType attribute.</param>
    /// <param name="deviceRefObjectType">A dynamic object representing a reference object type with an ObjectType attribute.</param>
    /// <param name="nameAttrValue">A string containing the name attribute value to be checked for specific patterns.</param>
    /// <returns>
    /// A formatted string representing the device type, determined by matching patterns in the name attribute value or by using the ObjectType of the provided device objects.
    /// </returns>
    /// </summary>
    private static string DetermineNameObjectType(dynamic deviceRailMounted, dynamic deviceRefObjectType, string nameAttrValue)
    {
        // Split nameAttrValue into a list of words using spaces and other common separators, including underscores
        var words = nameAttrValue.ToLower().Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        // Check for the presence of "cmd" and "ie" in the list of words
        var containsCmd = words.Contains("cmd") || words.Contains("cde");
        var containsIe = words.Contains("ie");

        if (containsCmd && !containsIe)
        {
            return $"{_formatter.Format("Cmd")}";
        }
        else if (containsIe)
        {
            return $"{_formatter.Format("Ie")}";
        }
        else if (deviceRailMounted != null && !string.IsNullOrEmpty(deviceRailMounted?.ObjectType))
        {
            // Format the ObjectType of the rail-mounted device
            return $"{_formatter.Format(deviceRailMounted?.ObjectType ?? string.Empty)}";
        }
        else if (deviceRefObjectType != null)
        {
            // Format the ObjectType of the device with a non-empty ObjectType
            return $"{_formatter.Format(deviceRefObjectType.ObjectType ?? string.Empty)}";
        }
        
        

        // Default nameObjectType if no valid ObjectType is found
        App.ConsoleAndLogWriteLine($"No Object Type found for {nameAttrValue}");
        return $"{_formatter.Format("Type")}";
    }

    
    /// <summary>
    /// Constructs a function name based on the "Name" attribute of a group range element and its ancestors. 
    /// If DeepL translation is enabled and set to French, it attempts to match predefined phrases 
    /// in both the current and ancestor group names for more accurate formatting.
    /// 
    /// <param name="groupAddressElement">An XElement representing the group address whose group range name is to be processed.</param>
    /// <returns>A formatted string representing the function name derived from the group range name and its ancestors.</returns>
    /// </summary>
    private static string GetGroupRangeFunctionName(XElement groupAddressElement)
    {
        // Get the GroupRange ancestor element, if any
        var groupRangeElement = groupAddressElement.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
        if (groupRangeElement == null) return string.Empty;

        var nameFunction = string.Empty;
        var matchingPhraseGroupName = string.Empty; // Variable to store the matching phrase
        var matchingPhraseAncestorGroupName = string.Empty; // Variable to store the matching phrase

        
        // Check for a higher-level GroupRange ancestor
        var ancestorGroupRange = groupRangeElement.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
        if (ancestorGroupRange != null)
        {
            // Format the group name
            FormatGroupRangeName(ancestorGroupRange);
            
            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.TranslationSourceLang == "FR")
            {
                var ancestorNameWords = ancestorGroupRange.Attribute("Name")?.Value.Split(Separator).Select(word => word.ToLower()).Where(word => !string.IsNullOrEmpty(word)).ToHashSet() ;
                // Iterate through each string in the AncestorGroupName hash set
                if (ancestorNameWords != null)
                {
                    foreach (var phrase in AncestorGroupName)
                    {
                        var phraseWords = phrase.Split(' ').Select(word => word.ToLower())
                            .Where(word => !string.IsNullOrEmpty(word));
                        var allWordsMatch = true;

                        // Check if each word in the phrase exists in the ancestorName
                        foreach (var word in phraseWords)
                        {
                            if (!ancestorNameWords.Contains(word))
                            {
                                allWordsMatch = false;
                                break;
                            }
                        }

                        // If all words match, store the phrase and stop checking
                        if (allWordsMatch)
                        {
                            matchingPhraseAncestorGroupName = phrase;
                            break;
                        }
                    }
                }
            }
            // Format the name of the ancestor GroupRange
            if (matchingPhraseAncestorGroupName != string.Empty)
            {
                nameFunction += $"_{_formatter.Format(matchingPhraseAncestorGroupName)}";
            }
            else
            {
                nameFunction = $"_{_formatter.Format(ancestorGroupRange.Attribute("Name")?.Value ?? string.Empty)}";
            }
        }
        
        // Format the group name
        FormatGroupRangeName(groupRangeElement);
        
        if (ancestorGroupRange == null)
        {
            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.TranslationSourceLang == "FR")
            {
                var ancestorNameWords = groupRangeElement.Attribute("Name")?.Value.Split(Separator).Select(word => word.ToLower()).Where(word => !string.IsNullOrEmpty(word)).ToHashSet() ;
                // Iterate through each string in the AncestorGroupName hash set
                if (ancestorNameWords != null)
                {
                    foreach (var phrase in AncestorGroupName)
                    {
                        var phraseWords = phrase.Split(' ').Select(word => word.ToLower())
                            .Where(word => !string.IsNullOrEmpty(word));
                        var allWordsMatch = true;

                        // Check if each word in the phrase exists in the ancestorName
                        foreach (var word in phraseWords)
                        {
                            if (!ancestorNameWords.Contains(word))
                            {
                                allWordsMatch = false;
                                break;
                            }
                        }

                        // If all words match, store the phrase and stop checking
                        if (allWordsMatch)
                        {
                            matchingPhraseGroupName = phrase;
                            break;
                        }
                    }
                }
            }
        }
        else
        {

            if (App.DisplayElements?.SettingsWindow != null &&
                App.DisplayElements.SettingsWindow.TranslationSourceLang == "FR")
            {
                var groupRangeNameWords = groupRangeElement.Attribute("Name")?.Value.Split(Separator)
                    .Select(word => word.ToLower()).Where(word => !string.IsNullOrEmpty(word)).ToHashSet();
                // Iterate through each string in the GroupName hash set
                if (groupRangeNameWords != null)
                {
                    foreach (var phrase in GroupName)
                    {
                        var phraseWords = phrase.Split(' ').Select(word => word.ToLower())
                            .Where(word => !string.IsNullOrEmpty(word));
                        var allWordsMatch = true;

                        // Check if each word in the phrase exists in the ancestorName
                        foreach (var word in phraseWords)
                        {
                            if (word == "pourcentage" && groupRangeNameWords.Contains("%"))
                            {
                                continue;
                            }

                            if (!groupRangeNameWords.Contains(word))
                            {
                                allWordsMatch = false;
                                break;
                            }
                        }

                        // If all words match, store the phrase and stop checking
                        if (allWordsMatch)
                        {
                            matchingPhraseGroupName = phrase;
                            break;
                        }
                    }
                }
            }
        }

        // Format the name of the current GroupRange
        if (matchingPhraseGroupName != string.Empty)
        {
            nameFunction += $"_{_formatter.Format(matchingPhraseGroupName)}";
        }
        else
        {   
            nameFunction += $"_{_formatter.Format(groupRangeElement.Attribute("Name")?.Value ?? string.Empty)}";
        }
        
        return nameFunction;
    }

    /// <summary>
    /// Formats the "Name" attribute of the provided group range element. If DeepL translation is enabled and a valid API key is present,
    /// it checks if the "Name" attribute needs translation. If it meets the conditions, it translates the name and updates the cache.
    /// Otherwise, it removes diacritics from the name if DeepL translation is disabled.
    ///
    /// <param name="groupRangeElement">An XElement representing the group range with a "Name" attribute to be formatted.</param>
    /// </summary>
    private static void FormatGroupRangeName(XElement groupRangeElement)
    {
        var nameAttr = groupRangeElement.Attribute("Name");
        if (nameAttr == null) return;

        var nameValue = nameAttr.Value;

        if (FormatCache.Contains(nameValue))
            return;
        
        if (App.DisplayElements?.SettingsWindow != null && !App.DisplayElements.SettingsWindow.EnableDeeplTranslation )
        {
            nameAttr.Value = RemoveDiacritics(nameValue);
            FormatCache.Add(nameAttr.Value);
            return;
        }
        
        //Check if the translation is needed
        if (string.IsNullOrEmpty(nameValue) || TranslationCache.Contains(nameValue)|| !ValidDeeplKey)
            return;

        // Translated only if not already translated
        nameAttr.Value = _formatter.Translate(nameValue);
        TranslationCache.Add(nameAttr.Value);
    }

    /// <summary>
    /// Removes diacritics (accents) from the provided text by normalizing the string to its decomposed form
    /// and filtering out non-spacing mark characters.
    /// This method is useful for standardizing text comparisons by ignoring accents.
    ///
    /// <param name="text">The input string from which diacritics should be removed.</param>
    /// <returns>A new string with all diacritics removed.</returns>
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    
}
