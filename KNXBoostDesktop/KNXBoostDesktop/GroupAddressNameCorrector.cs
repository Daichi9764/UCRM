using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;
using DeepL;

namespace KNXBoostDesktop;


public class GroupAddressNameCorrector
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
    private static Formatter _formatter;
    
    /// <summary>
    /// Collection to memorize translations of group names already done.
    /// </summary>
    private static readonly HashSet<string> TranslationCache = new ();
    
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 9159MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Xml.Linq.XAttribute; size: 7650MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Xml.Linq.XElement; size: 4051MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 2513MB")]
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    public static async Task CorrectName()
    {
        try
        {
            string loadXml, extractingInfos, infosExtracted, extractingDeviceReferences, extractingDeviceInfo, infoAndReferencesExtracted, constructingNewAddresses, savingUpdatedXml;
            
            // Traduction des textes de la fenêtre de chargement
            switch (App.DisplayElements?.SettingsWindow?.AppLang)
            {
                // Arabe
                case "AR":
                    loadXml = "جارٍ تحميل ملف XML...";
                    extractingInfos = "استخراج المعلومات...";
                    infosExtracted = "تم استخراج المعلومات من الملف.";
                    extractingDeviceReferences = "استخراج مراجع الأجهزة...";
                    extractingDeviceInfo = "استخراج معلومات الأجهزة... (قد تستغرق هذه العملية وقتًا)";
                    infoAndReferencesExtracted = "تم استخراج المراجع والمعلومات الخاصة بالأجهزة.";
                    constructingNewAddresses = "بناء العناوين الجديدة للمجموعة...";
                    savingUpdatedXml = "جارٍ حفظ ملف XML المحدث...";
                    break;

                // Bulgare
                case "BG":
                    loadXml = "Зареждане на XML файл...";
                    extractingInfos = "Извличане на информация...";
                    infosExtracted = "Информацията е извлечена от файла.";
                    extractingDeviceReferences = "Извличане на референции на устройства...";
                    extractingDeviceInfo = "Извличане на информация за устройствата... (Тази стъпка може да отнеме време)";
                    infoAndReferencesExtracted = "Референциите и информацията за устройствата са извлечени.";
                    constructingNewAddresses = "Конструиране на нови групови адреси...";
                    savingUpdatedXml = "Запазване на актуализирания XML файл...";
                    break;

                // Tchèque
                case "CS":
                    loadXml = "Načítání souboru XML...";
                    extractingInfos = "Extrahování informací...";
                    infosExtracted = "Informace byly extrahovány ze souboru.";
                    extractingDeviceReferences = "Extrahování odkazů na zařízení...";
                    extractingDeviceInfo = "Extrahování informací o zařízeních... (Tato fáze může chvíli trvat)";
                    infoAndReferencesExtracted = "Reference a informace o zařízeních byly extrahovány.";
                    constructingNewAddresses = "Vytváření nových skupinových adres...";
                    savingUpdatedXml = "Ukládání aktualizovaného souboru XML...";
                    break;

                // Danois
                case "DA":
                    loadXml = "Indlæser XML-fil...";
                    extractingInfos = "Udvinder informationer...";
                    infosExtracted = "Oplysningerne er udvundet fra filen.";
                    extractingDeviceReferences = "Udvinder enhedsreferencer...";
                    extractingDeviceInfo = "Udvinder enhedsoplysninger... (Denne proces kan tage noget tid)";
                    infoAndReferencesExtracted = "Referencer og oplysninger om enheder er udvundet.";
                    constructingNewAddresses = "Opretter nye gruppeadresser...";
                    savingUpdatedXml = "Gemmer opdateret XML-fil...";
                    break;

                // Allemand
                case "DE":
                    loadXml = "XML-Datei wird geladen...";
                    extractingInfos = "Informationen werden extrahiert...";
                    infosExtracted = "Informationen wurden aus der Datei extrahiert.";
                    extractingDeviceReferences = "Geräteverweise werden extrahiert...";
                    extractingDeviceInfo = "Geräteinformationen werden extrahiert... (Dieser Schritt kann einige Zeit dauern)";
                    infoAndReferencesExtracted = "Geräteverweise und -informationen wurden extrahiert.";
                    constructingNewAddresses = "Neue Gruppenadressen werden erstellt...";
                    savingUpdatedXml = "Aktualisierte XML-Datei wird gespeichert...";
                    break;

                // Grec
                case "EL":
                    loadXml = "Φόρτωση αρχείου XML...";
                    extractingInfos = "Εξαγωγή πληροφοριών...";
                    infosExtracted = "Οι πληροφορίες εξήχθησαν από το αρχείο.";
                    extractingDeviceReferences = "Εξαγωγή αναφορών συσκευών...";
                    extractingDeviceInfo = "Εξαγωγή πληροφοριών συσκευών... (Αυτή η διαδικασία μπορεί να πάρει χρόνο)";
                    infoAndReferencesExtracted = "Οι αναφορές και οι πληροφορίες για τις συσκευές εξήχθησαν.";
                    constructingNewAddresses = "Κατασκευή νέων διευθύνσεων ομάδας...";
                    savingUpdatedXml = "Αποθήκευση ενημερωμένου αρχείου XML...";
                    break;

                // Anglais
                case "EN":
                    loadXml = "Loading XML file...";
                    extractingInfos = "Extracting information...";
                    infosExtracted = "Information extracted from the file.";
                    extractingDeviceReferences = "Extracting device references...";
                    extractingDeviceInfo = "Extracting device information... (This step may take some time)";
                    infoAndReferencesExtracted = "Device references and information extracted.";
                    constructingNewAddresses = "Constructing new group addresses...";
                    savingUpdatedXml = "Saving updated XML file...";
                    break;

                // Espagnol
                case "ES":
                    loadXml = "Cargando archivo XML...";
                    extractingInfos = "Extrayendo información...";
                    infosExtracted = "Información extraída del archivo.";
                    extractingDeviceReferences = "Extrayendo referencias de dispositivos...";
                    extractingDeviceInfo = "Extrayendo información de los dispositivos... (Este paso puede tardar)";
                    infoAndReferencesExtracted = "Referencias e información de los dispositivos extraídas.";
                    constructingNewAddresses = "Construyendo nuevas direcciones de grupo...";
                    savingUpdatedXml = "Guardando archivo XML actualizado...";
                    break;

                // Estonien
                case "ET":
                    loadXml = "Laadib XML-faili...";
                    extractingInfos = "Teabe ekstraheerimine...";
                    infosExtracted = "Teave on failist ekstraheeritud.";
                    extractingDeviceReferences = "Seadme viidete ekstraheerimine...";
                    extractingDeviceInfo = "Seadme teabe ekstraheerimine... (See etapp võib aega võtta)";
                    infoAndReferencesExtracted = "Seadme viited ja teave on ekstraheeritud.";
                    constructingNewAddresses = "Uute rühma aadresside koostamine...";
                    savingUpdatedXml = "Uuendatud XML-faili salvestamine...";
                    break;

                // Finnois
                case "FI":
                    loadXml = "Ladataan XML-tiedostoa...";
                    extractingInfos = "Tietojen purkaminen...";
                    infosExtracted = "Tiedot on purettu tiedostosta.";
                    extractingDeviceReferences = "Laitteiden viitteiden purkaminen...";
                    extractingDeviceInfo = "Laitetietojen purkaminen... (Tämä vaihe voi kestää jonkin aikaa)";
                    infoAndReferencesExtracted = "Laitteiden viitteet ja tiedot on purettu.";
                    constructingNewAddresses = "Luodaan uusia ryhmäosoitteita...";
                    savingUpdatedXml = "Tallennetaan päivitettyä XML-tiedostoa...";
                    break;

                // Hongrois
                case "HU":
                    loadXml = "XML-fájl betöltése...";
                    extractingInfos = "Információk kinyerése...";
                    infosExtracted = "Az információk kinyerése megtörtént.";
                    extractingDeviceReferences = "Eszköz hivatkozások kinyerése...";
                    extractingDeviceInfo = "Eszköz információk kinyerése... (Ez a lépés eltarthat egy ideig)";
                    infoAndReferencesExtracted = "Az eszköz hivatkozások és információk kinyerése megtörtént.";
                    constructingNewAddresses = "Új csoport címek létrehozása...";
                    savingUpdatedXml = "A frissített XML-fájl mentése...";
                    break;

                // Indonésien
                case "ID":
                    loadXml = "Memuat file XML...";
                    extractingInfos = "Ekstraksi informasi...";
                    infosExtracted = "Informasi diekstraksi dari file.";
                    extractingDeviceReferences = "Ekstraksi referensi perangkat...";
                    extractingDeviceInfo = "Ekstraksi informasi perangkat... (Langkah ini mungkin memakan waktu)";
                    infoAndReferencesExtracted = "Referensi dan informasi perangkat diekstraksi.";
                    constructingNewAddresses = "Membangun alamat grup baru...";
                    savingUpdatedXml = "Menyimpan file XML yang diperbarui...";
                    break;

                // Italien
                case "IT":
                    loadXml = "Caricamento del file XML...";
                    extractingInfos = "Estrazione delle informazioni...";
                    infosExtracted = "Informazioni estratte dal file.";
                    extractingDeviceReferences = "Estrazione dei riferimenti dei dispositivi...";
                    extractingDeviceInfo = "Estrazione delle informazioni sui dispositivi... (Questa fase può richiedere tempo)";
                    infoAndReferencesExtracted = "Riferimenti e informazioni sui dispositivi estratti.";
                    constructingNewAddresses = "Costruzione dei nuovi indirizzi di gruppo...";
                    savingUpdatedXml = "Salvataggio del file XML aggiornato...";
                    break;

                // Japonais
                case "JA":
                    loadXml = "XMLファイルを読み込んでいます...";
                    extractingInfos = "情報を抽出しています...";
                    infosExtracted = "ファイルから情報が抽出されました。";
                    extractingDeviceReferences = "デバイスの参照を抽出しています...";
                    extractingDeviceInfo = "デバイス情報を抽出しています... (このステップには時間がかかる場合があります)";
                    infoAndReferencesExtracted = "デバイスの参照および情報が抽出されました。";
                    constructingNewAddresses = "新しいグループアドレスを構築しています...";
                    savingUpdatedXml = "更新されたXMLファイルを保存しています...";
                    break;

                // Coréen
                case "KO":
                    loadXml = "XML 파일을 로드 중...";
                    extractingInfos = "정보를 추출 중...";
                    infosExtracted = "파일에서 정보가 추출되었습니다.";
                    extractingDeviceReferences = "디바이스 참조를 추출 중...";
                    extractingDeviceInfo = "디바이스 정보를 추출 중... (이 단계는 시간이 걸릴 수 있습니다)";
                    infoAndReferencesExtracted = "디바이스 참조와 정보가 추출되었습니다.";
                    constructingNewAddresses = "새 그룹 주소를 구성 중...";
                    savingUpdatedXml = "업데이트된 XML 파일을 저장 중...";
                    break;

                // Letton
                case "LV":
                    loadXml = "Ielādē XML failu...";
                    extractingInfos = "Informācijas iegūšana...";
                    infosExtracted = "Informācija iegūta no faila.";
                    extractingDeviceReferences = "Iegūst ierīču atsauces...";
                    extractingDeviceInfo = "Ierīču informācijas iegūšana... (Šis solis var aizņemt laiku)";
                    infoAndReferencesExtracted = "Ierīču atsauces un informācija iegūta.";
                    constructingNewAddresses = "Veido jaunās grupas adreses...";
                    savingUpdatedXml = "Saglabā atjaunināto XML failu...";
                    break;

                // Lituanien
                case "LT":
                    loadXml = "Įkeliama XML byla...";
                    extractingInfos = "Ištraukiama informacija...";
                    infosExtracted = "Informacija ištraukta iš bylos.";
                    extractingDeviceReferences = "Ištraukiamos įrenginio nuorodos...";
                    extractingDeviceInfo = "Ištraukiama informacija apie įrenginius... (Šis veiksmas gali užtrukti)";
                    infoAndReferencesExtracted = "Ištrauktos įrenginių nuorodos ir informacija.";
                    constructingNewAddresses = "Kuriami nauji grupės adresai...";
                    savingUpdatedXml = "Įrašoma atnaujinta XML byla...";
                    break;

                // Norvégien
                case "NB":
                    loadXml = "Laster inn XML-fil...";
                    extractingInfos = "Henter informasjon...";
                    infosExtracted = "Informasjonen er hentet fra filen.";
                    extractingDeviceReferences = "Henter enhetsreferanser...";
                    extractingDeviceInfo = "Henter enhetsinformasjon... (Dette trinnet kan ta tid)";
                    infoAndReferencesExtracted = "Enhetsreferanser og informasjon er hentet.";
                    constructingNewAddresses = "Bygger nye gruppeadresser...";
                    savingUpdatedXml = "Lagrer oppdatert XML-fil...";
                    break;

                // Néerlandais
                case "NL":
                    loadXml = "XML-bestand laden...";
                    extractingInfos = "Informatie wordt geëxtraheerd...";
                    infosExtracted = "Informatie uit het bestand geëxtraheerd.";
                    extractingDeviceReferences = "Apparaatverwijzingen worden geëxtraheerd...";
                    extractingDeviceInfo = "Apparaatinformatie wordt geëxtraheerd... (Deze stap kan enige tijd duren)";
                    infoAndReferencesExtracted = "Apparaatverwijzingen en -informatie geëxtraheerd.";
                    constructingNewAddresses = "Nieuwe groepsadressen worden aangemaakt...";
                    savingUpdatedXml = "Bijgewerkt XML-bestand opslaan...";
                    break;

                // Polonais
                case "PL":
                    loadXml = "Ładowanie pliku XML...";
                    extractingInfos = "Wyodrębnianie informacji...";
                    infosExtracted = "Informacje wyodrębnione z pliku.";
                    extractingDeviceReferences = "Wyodrębnianie odniesień do urządzeń...";
                    extractingDeviceInfo = "Wyodrębnianie informacji o urządzeniach... (Ten krok może potrwać)";
                    infoAndReferencesExtracted = "Odniesienia do urządzeń i informacje wyodrębnione.";
                    constructingNewAddresses = "Tworzenie nowych adresów grupowych...";
                    savingUpdatedXml = "Zapisywanie zaktualizowanego pliku XML...";
                    break;

                // Portugais
                case "PT":
                    loadXml = "Carregando arquivo XML...";
                    extractingInfos = "Extraindo informações...";
                    infosExtracted = "Informações extraídas do arquivo.";
                    extractingDeviceReferences = "Extraindo referências de dispositivos...";
                    extractingDeviceInfo = "Extraindo informações dos dispositivos... (Este passo pode demorar)";
                    infoAndReferencesExtracted = "Referências e informações dos dispositivos extraídas.";
                    constructingNewAddresses = "Construindo novos endereços de grupo...";
                    savingUpdatedXml = "Salvando arquivo XML atualizado...";
                    break;

                // Roumain
                case "RO":
                    loadXml = "Se încarcă fișierul XML...";
                    extractingInfos = "Extragerea informațiilor...";
                    infosExtracted = "Informațiile au fost extrase din fișier.";
                    extractingDeviceReferences = "Se extrag referințele dispozitivelor...";
                    extractingDeviceInfo = "Se extrag informațiile despre dispozitive... (Acest pas poate dura ceva timp)";
                    infoAndReferencesExtracted = "Referințele și informațiile despre dispozitive au fost extrase.";
                    constructingNewAddresses = "Se construiesc noile adrese de grup...";
                    savingUpdatedXml = "Se salvează fișierul XML actualizat...";
                    break;

                // Russe
                case "RU":
                    loadXml = "Загрузка файла XML...";
                    extractingInfos = "Извлечение информации...";
                    infosExtracted = "Информация извлечена из файла.";
                    extractingDeviceReferences = "Извлечение ссылок на устройства...";
                    extractingDeviceInfo = "Извлечение информации об устройствах... (Этот шаг может занять некоторое время)";
                    infoAndReferencesExtracted = "Ссылки и информация об устройствах извлечены.";
                    constructingNewAddresses = "Построение новых групповых адресов...";
                    savingUpdatedXml = "Сохранение обновленного XML-файла...";
                    break;

                // Slovaque
                case "SK":
                    loadXml = "Načítava sa súbor XML...";
                    extractingInfos = "Extrakcia informácií...";
                    infosExtracted = "Informácie boli extrahované zo súboru.";
                    extractingDeviceReferences = "Extrakcia referencií zariadení...";
                    extractingDeviceInfo = "Extrakcia informácií o zariadeniach... (Tento krok môže chvíľu trvať)";
                    infoAndReferencesExtracted = "Referencie a informácie o zariadeniach boli extrahované.";
                    constructingNewAddresses = "Vytváranie nových skupinových adries...";
                    savingUpdatedXml = "Ukladanie aktualizovaného súboru XML...";
                    break;

                // Slovène
                case "SL":
                    loadXml = "Nalaganje XML datoteke...";
                    extractingInfos = "Izvlečenje informacij...";
                    infosExtracted = "Informacije iz datoteke so izvlečene.";
                    extractingDeviceReferences = "Izvlečenje sklicev naprav...";
                    extractingDeviceInfo = "Izvlečenje informacij o napravah... (Ta korak lahko traja nekaj časa)";
                    infoAndReferencesExtracted = "Sklici in informacije o napravah so izvlečeni.";
                    constructingNewAddresses = "Ustvarjanje novih skupinskih naslovov...";
                    savingUpdatedXml = "Shranjevanje posodobljene XML datoteke...";
                    break;

                // Suédois
                case "SV":
                    loadXml = "Laddar XML-fil...";
                    extractingInfos = "Extraherar information...";
                    infosExtracted = "Information extraherad från filen.";
                    extractingDeviceReferences = "Extraherar enhetsreferenser...";
                    extractingDeviceInfo = "Extraherar enhetsinformation... (Detta steg kan ta tid)";
                    infoAndReferencesExtracted = "Enhetsreferenser och information extraherad.";
                    constructingNewAddresses = "Bygger nya gruppadresser...";
                    savingUpdatedXml = "Sparar uppdaterad XML-fil...";
                    break;

                // Turc
                case "TR":
                    loadXml = "XML dosyası yükleniyor...";
                    extractingInfos = "Bilgiler çıkarılıyor...";
                    infosExtracted = "Bilgiler dosyadan çıkarıldı.";
                    extractingDeviceReferences = "Cihaz referansları çıkarılıyor...";
                    extractingDeviceInfo = "Cihaz bilgileri çıkarılıyor... (Bu adım biraz zaman alabilir)";
                    infoAndReferencesExtracted = "Cihaz referansları ve bilgileri çıkarıldı.";
                    constructingNewAddresses = "Yeni grup adresleri oluşturuluyor...";
                    savingUpdatedXml = "Güncellenmiş XML dosyası kaydediliyor...";
                    break;

                // Ukrainien
                case "UK":
                    loadXml = "Завантаження файлу XML...";
                    extractingInfos = "Вилучення інформації...";
                    infosExtracted = "Інформація вилучена з файлу.";
                    extractingDeviceReferences = "Вилучення посилань на пристрої...";
                    extractingDeviceInfo = "Вилучення інформації про пристрої... (Цей етап може зайняти деякий час)";
                    infoAndReferencesExtracted = "Посилання та інформація про пристрої вилучені.";
                    constructingNewAddresses = "Створення нових групових адрес...";
                    savingUpdatedXml = "Збереження оновленого файлу XML...";
                    break;

                // Chinois simplifié
                case "ZH":
                    loadXml = "正在加载 XML 文件...";
                    extractingInfos = "提取信息...";
                    infosExtracted = "信息已从文件中提取。";
                    extractingDeviceReferences = "提取设备参考...";
                    extractingDeviceInfo = "提取设备信息... (此步骤可能需要一些时间)";
                    infoAndReferencesExtracted = "设备参考和信息已提取。";
                    constructingNewAddresses = "正在构建新的组地址...";
                    savingUpdatedXml = "保存更新后的 XML 文件...";
                    break;

                // Langue par défaut (français)
                default:
                    loadXml = "Chargement du fichier XML...";
                    extractingInfos = "Extraction des informations...";
                    infosExtracted = "Informations extraites du fichier.";
                    extractingDeviceReferences = "Extraction des références des appareils...";
                    extractingDeviceInfo = "Extraction des informations sur les appareils... (Cette étape peut prendre du temps)";
                    infoAndReferencesExtracted = "Références et informations sur les appareils extraites.";
                    constructingNewAddresses = "Construction des nouvelles adresses de groupe...";
                    savingUpdatedXml = "Sauvegarde du fichier XML mis à jour...";
                    break;
            }
            
            
            //Define the project path
            _projectFilesDirectory = Path.Combine(App.Fm?.ProjectFolderPath ?? string.Empty, @"knxproj_exported");
            
            // Define the XML namespace used in the KNX project file
            SetNamespaceFromXml(App.Fm?.ZeroXmlPath ?? string.Empty);
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(loadXml);
            
            // Load the XML file from the specified path
            XDocument? knxDoc = App.Fm?.LoadKnxDocument(App.Fm.ZeroXmlPath);
            if (knxDoc == null) return;
            
            // Create a formatter object for normalizing names
            _formatter = GetFormatter();
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(extractingInfos);

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

                    return new
                    {
                        RoomName = room.Attribute("Name")?.Value,
                        FloorName = getAncestorName("Floor"),
                        BuildingPartName = getAncestorName("BuildingPart"),
                        BuildingName = getAncestorName("Building"),
                        DistributionBoardName = getDescendantName("DistributionBoard"),
                        DeviceRefs = room.Descendants(_globalKnxNamespace + "DeviceInstanceRef")
                            .Select(dir => dir.Attribute("RefId")?.Value)
                            .ToList()
                    };
                })
                .ToList();

            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(infosExtracted);

            // Display extracted location information
            App.ConsoleAndLogWriteLine("Extracted Location Information:");
            foreach (var loc in locationInfo)
            {
                string message = string.Empty;
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
            }
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(extractingDeviceReferences);


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
                            var links = cir.Attribute("Links")?.Value.Split(' ') ?? new string[0];
                            var comObjectInstanceRefId = cir.Attribute("RefId")?.Value;

                            return links.Select((link, index) => new
                            {
                                GroupAddressRef = link,
                                DeviceInstanceId = id,
                                ComObjectInstanceRefId = comObjectInstanceRefId,
                                IsFirstLink = index == 0
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
                    ObjectType = hardwareFileName != null && g.ComObjectInstanceRefId != null && mxxxxDirectory != null && g.IsFirstLink ?
                        GetObjectType(hardwareFileName, mxxxxDirectory, g.ComObjectInstanceRefId) : 
                        null
                });
            }).ToList();

            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(infoAndReferencesExtracted);
            
            // Display extracted device instance references
            App.ConsoleAndLogWriteLine("Extracted Device Instance References:");
            foreach (var dr in deviceRefs)
            {
                App.ConsoleAndLogWriteLine($"Device Instance ID: {dr.DeviceInstanceId}, Product Ref ID: {dr.ProductRefId}, Is Device Rail Mounted ? : {dr.IsDeviceRailMounted}, Group Address Ref: {dr.GroupAddressRef}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
            }

            // Group deviceRefs by GroupAddressRef
            var groupedDeviceRefs = deviceRefs.GroupBy(dr => dr.GroupAddressRef)
                .Select(g => new
                {
                    GroupAddressRef = g.Key,
                    Devices = g.ToList()
                })
                .ToList();

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
            }
            
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(constructingNewAddresses);
            
            // Collection to track the IDs of renamed GroupAddresses
            HashSet<string> renamedGroupAddressIds = new HashSet<string>();
            
            // Construct the new name of the group address by iterating through each group of device references
            foreach (var gdr in groupedDeviceRefs)
            {
                // Get the first rail-mounted device reference, if any
                var deviceRailMounted = gdr.Devices.FirstOrDefault(dr => dr.IsDeviceRailMounted);
                // Get the first device reference with a non-empty ObjectType, if any
                var deviceRefObjectType = gdr.Devices.FirstOrDefault(dr => !string.IsNullOrEmpty(dr.ObjectType));
                // Get the first non-rail-mounted device reference, if any
                var deviceNotRailMounted = gdr.Devices.FirstOrDefault(dr => dr.IsDeviceRailMounted == false);
                
                if (deviceNotRailMounted != null)
                {
                    // Find the GroupAddress element that matches the device's GroupAddressRef
                    var groupAddressElement = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                        .FirstOrDefault(ga => ga.Attribute("Id")?.Value.EndsWith(deviceNotRailMounted.GroupAddressRef) == true);

                    if (groupAddressElement == null)
                    {
                        App.ConsoleAndLogWriteLine($"No GroupAddress element found for GroupAddressRef: {deviceNotRailMounted.GroupAddressRef}");
                        continue; 
                    }

                    App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                    var nameAttr = groupAddressElement.Attribute("Name");

                    if (nameAttr == null)
                    {
                        App.ConsoleAndLogWriteLine($"No group address name found for  {groupAddressElement}");
                        continue; 
                    }
                        
                    // Get the location information for the device reference
                    var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceNotRailMounted.DeviceInstanceId));

                    // Si aucune localisation n'est trouvée, itérer sur les autres dispositifs du groupe
                    if (location == null)
                    {
                        // Utilisation d'un indicateur pour savoir si une localisation a été trouvée
                        bool locationFound = false;

                        foreach (var device in gdr.Devices)
                        {
                            // Vérifier si nous avons déjà trouvé une localisation
                            if (locationFound)
                            {
                                break; // Sortir de la boucle si une localisation a été trouvée
                            }

                            // Éviter de rechercher à nouveau dans deviceNotRailMounted
                            if (device == deviceNotRailMounted)
                            {
                                continue; // Passer à l'itération suivante si c'est deviceNotRailMounted
                            }

                            location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(device.DeviceInstanceId));
                            if (location != null)
                            {
                                locationFound = true; // Mettre à jour l'indicateur
                            }
                        }
                    }
                    
                    string nameLocation = GetLocationName(location, nameAttr.Value);
                    
                    // Determine the nameObjectType based on the available device references
                    string nameObjectType =
                        DetermineNameObjectType(deviceRailMounted, deviceRefObjectType, nameAttr.Value);

                    string nameFunction = GetGroupRangeFunctionName(groupAddressElement);
                    
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
                    
                   
                }
                else if (deviceRailMounted != null)
                {
                    // Find the GroupAddress element that matches the device's GroupAddressRef
                    var groupAddressElement = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                        .FirstOrDefault(ga => ga.Attribute("Id")?.Value.EndsWith(deviceRailMounted.GroupAddressRef) == true);

                    if (groupAddressElement == null)
                    {
                        App.ConsoleAndLogWriteLine($"No GroupAddress element found for GroupAddressRef: {deviceNotRailMounted?.GroupAddressRef}");
                        continue; 
                    }
                    
                    App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                    var nameAttr = groupAddressElement.Attribute("Name");
                    
                    if (nameAttr == null)
                    {
                        App.ConsoleAndLogWriteLine($"No group address name found for  {groupAddressElement}");
                        continue; 
                    }
                    
                    // Get the location information for the device reference
                    var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceRailMounted.DeviceInstanceId));
                    
                    // Si aucune localisation n'est trouvée, itérer sur les autres dispositifs du groupe
                    if (location == null)
                    {
                        // Utilisation d'un indicateur pour savoir si une localisation a été trouvée
                        bool locationFound = false;

                        foreach (var device in gdr.Devices)
                        {
                            // Vérifier si nous avons déjà trouvé une localisation
                            if (locationFound)
                            {
                                break; // Sortir de la boucle si une localisation a été trouvée
                            }

                            // Éviter de rechercher à nouveau dans deviceNotRailMounted
                            if (device == deviceRailMounted)
                            {
                                continue; // Passer à l'itération suivante si c'est deviceNotRailMounted
                            }

                            location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(device.DeviceInstanceId));
                            if (location != null)
                            {
                                locationFound = true; // Mettre à jour l'indicateur
                            }
                        }
                    }

                    string nameLocation = GetLocationName(location, nameAttr.Value);
                                                
                    // Determine the nameObjectType based on the available device references
                    string nameObjectType = DetermineNameObjectType(deviceRailMounted, deviceRefObjectType, nameAttr.Value);
                            
                    string nameFunction = GetGroupRangeFunctionName(groupAddressElement);

                    // Construct the new name by combining the object type, function, and location
                    var newName = nameObjectType + nameFunction + nameLocation;
                    App.ConsoleAndLogWriteLine($"Original Name: {nameAttr.Value}");
                    App.ConsoleAndLogWriteLine($"New Name: {newName}");
                    nameAttr.Value = newName; // Update the GroupAddress element's name

                    if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses)
                    {
                        // Mark the address as renamed
                        renamedGroupAddressIds.Add(groupAddressElement.Attribute("Id")?.Value ?? string.Empty); 
                    }
                    
                    
                }
            }
            
            // Load the original XML file without any additional modifications
            XDocument? originalKnxDoc = App.Fm?.LoadKnxDocument(App.Fm.ZeroXmlPath);
            
            
            // Deletes unused (not renamed) GroupAddresses if requested
            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses && originalKnxDoc != null)
            {
                await using StreamWriter writer = new StreamWriter(App.Fm?.ProjectFolderPath + "/deleted_group_addresses.txt", append: true); 
                await writer.WriteLineAsync("Deleted addresses :");
                var allGroupAddresses = originalKnxDoc.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
                foreach (var groupAddress in allGroupAddresses)
                {
                    var groupId = groupAddress.Attribute("Id")?.Value;
                    if (groupId != null && !renamedGroupAddressIds.Contains(groupId))
                    {
                        var groupElement = groupAddress.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
                        string msg = $"- " + groupAddress.Attribute("Name")?.Value + " (" ;
                        var ancestorgroupElement = groupElement?.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
                        if (ancestorgroupElement != null)
                        {
                            msg += ancestorgroupElement.Attribute("Name")?.Value + " -> ";
                        }

                        msg += groupElement?.Attribute("Name")?.Value + ") with Id : " + groupId;
                        await writer.WriteLineAsync(msg); // Write message in the log file named deleted_group_addresses
                        
                        // Delete it in originalKnxDoc
                        groupAddress.Remove();

                        // Delete it in knxDoc
                        var correspondingGroupAddressInKnxDoc = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                            .FirstOrDefault(ga => ga.Attribute("Id")?.Value == groupId);

                        if (correspondingGroupAddressInKnxDoc != null)
                        {
                            correspondingGroupAddressInKnxDoc.Remove();

                            App.ConsoleAndLogWriteLine($"Removed unrenamed GroupAddress ID: {groupId}");
                        }
                           
                    } 
                }
            }

            // Save the updated XML files
            App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements?.LoadingWindow?.LogActivity(savingUpdatedXml);
            
            App.Fm?.SaveXml(knxDoc, $"{App.Fm.ProjectFolderPath}/0_updated.xml");

            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses && originalKnxDoc != null)
            {
                App.Fm?.SaveXml(originalKnxDoc, $"{App.Fm.ProjectFolderPath}/0_original.xml");
            }
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during CorrectName(): {ex.Message}");
        }
    }

    
    // Method that retrieves the ReadFlag and WriteFlag associated with a participant to determine its ObjectType (Cmd/Ie)
    /// <summary>
    /// Retrieves the object type of a participant based on its ReadFlag and WriteFlag values from a hardware XML file.
    ///
    /// This method constructs the path to the hardware XML file located in the specified <paramref name="mxxxxDirectory"/> 
    /// and attempts to locate the <c>ComObjectRef</c> element with a matching ID based on <paramref name="comObjectInstanceRefId"/>.
    /// If the <c>ReadFlag</c> or <c>WriteFlag</c> attributes are not found in <c>ComObjectRef</c>, it checks the <c>ComObject</c> element.
    /// The object type is determined based on the combination of <c>ReadFlag</c> and <c>WriteFlag</c> values and is returned as "Cmd" or "Ie".
    /// If errors occur during file or directory access, XML parsing, or if the expected elements or attributes are not found, 
    /// the method logs an error and returns an empty string.
    ///
    /// <param name="hardwareFileName">The name of the hardware XML file to be loaded.</param>
    /// <param name="mxxxxDirectory">The directory containing the hardware XML file.</param>
    /// <param name="comObjectInstanceRefId">The reference ID of the ComObject instance to locate.</param>
    /// <returns>
    /// Returns the object type ("Cmd" or "Ie") based on the <c>ReadFlag</c> and <c>WriteFlag</c> attributes, or an empty string if
    /// the file, directory, or expected XML elements/attributes are not found or if an error occurs.
    /// </returns>
    /// </summary>
    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    private static string GetObjectType(string hardwareFileName, string mxxxxDirectory, string comObjectInstanceRefId)
    {

        // Construct the full path to the Mxxxx directory
        string mxxxxDirectoryPath = Path.Combine(_projectFilesDirectory, mxxxxDirectory);

        try
        {
            // Check if the Mxxxx directory exists
            if (!Directory.Exists(mxxxxDirectoryPath))
            {
                App.ConsoleAndLogWriteLine($"Directory not found: {mxxxxDirectoryPath}");
                return string.Empty;
            }

            // Construct the full path to the hardware file
            string filePath = Path.Combine(mxxxxDirectoryPath, hardwareFileName);

            // Check if the hardware file exists
            if (!File.Exists(filePath))
            {
                App.ConsoleAndLogWriteLine($"File not found: {filePath}");
                return string.Empty;
            }

            App.ConsoleAndLogWriteLine($"Opening file: {filePath}");

            // Load the XML file
            XDocument hardwareDoc = XDocument.Load(filePath);
                
            // Find the ComObject element with the matching ID
            var comObjectRefElement = hardwareDoc.Descendants(_globalKnxNamespace + "ComObjectRef")
                .FirstOrDefault(co => co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefId) == true);
                
            if (comObjectRefElement == null)
            {
                App.ConsoleAndLogWriteLine($"ComObjectRef with Id ending in: {comObjectInstanceRefId} not found in file: {filePath}");
                return string.Empty;
            }

            App.ConsoleAndLogWriteLine($"Found ComObjectRef with Id ending in: {comObjectInstanceRefId}");
            var readFlag = comObjectRefElement.Attribute("ReadFlag")?.Value;
            var writeFlag = comObjectRefElement.Attribute("WriteFlag")?.Value;

            // If ReadFlag or WriteFlag are not found in ComObjectRef, check in ComObject
            if (readFlag == null || writeFlag == null)
            {
                var comObjectInstanceRefIdCut = comObjectInstanceRefId.IndexOf('_') >= 0 ? 
                    comObjectInstanceRefId.Substring(0,comObjectInstanceRefId.IndexOf('_')) : null;
                        
                var comObjectElement = hardwareDoc.Descendants(_globalKnxNamespace + "ComObject")
                    .FirstOrDefault(co => comObjectInstanceRefIdCut != null && co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefIdCut) == true);
                if (comObjectElement == null)
                {
                    App.ConsoleAndLogWriteLine($"ComObject with Id ending in: {comObjectInstanceRefIdCut} not found in file: {filePath}");
                    return string.Empty;
                }

                App.ConsoleAndLogWriteLine($"Found ComObject with Id ending in: {comObjectInstanceRefIdCut}");
                            
                // ??= is used to assert the expression if the variable is null
                readFlag ??= comObjectElement.Attribute("ReadFlag")?.Value;
                writeFlag ??= comObjectElement.Attribute("WriteFlag")?.Value;
            }
                    
            App.ConsoleAndLogWriteLine($"ReadFlag: {readFlag}, WriteFlag: {writeFlag}");
               
            // Determine the ObjectType based on the ReadFlag and WriteFlag values
            if (readFlag == "Enabled" && writeFlag == "Disabled")
            {
                return "Ie";
            }
            else if (writeFlag == "Enabled" && readFlag == "Disabled")
            {
                return "Cmd";
            }
            else if (writeFlag == "Enabled" && readFlag == "Enabled")
            {
                return "Cmd";
            }
            else
            {
                return string.Empty;
            }
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
            return (hardwareFileName, mxxxxDirectory);
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
        // Construct the full path to the Mxxxx directory
        string mxxxxDirectoryPath = Path.Combine(_projectFilesDirectory, mxxxxDirectory);
        
        // Construct the full path to the Hardware.xml file
        string hardwareFilePath = Path.Combine(mxxxxDirectoryPath, "Hardware.xml");
        
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
        
        try
        {
            // Load the Hardware.xml file
            XDocument hardwareDoc = XDocument.Load(hardwareFilePath);

            // Find the Product element with the matching ID
            var productElement = hardwareDoc.Descendants(_globalKnxNamespace + "Product")
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
            string isRailMountedValue = isRailMountedAttr.Value.ToLower();
            if (isRailMountedValue == "true" || isRailMountedValue == "1")
            { 
                return true;
            }
            else if (isRailMountedValue == "false" || isRailMountedValue == "0") 
            { 
                return false;
            }
            else 
            { 
                App.ConsoleAndLogWriteLine($"Unexpected IsRailMounted attribute value: {isRailMountedAttr.Value} for Product with Id: {productRefId}");
                return false; // Default to false for unexpected attribute values
            }
        }
        catch (XmlException ex)
        {
            App.ConsoleAndLogWriteLine($"Error reading Hardware.xml (XML exception): {ex.Message}");
            return false; // Default to false in case of an XML error
        }
        catch (Exception ex)
        { 
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during GetIsDeviceRailMounted(): {ex.Message}");
            return false; // Default to false in case of an error
        }
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
            XmlDocument doc = new XmlDocument();

            // Load XML file
            doc.Load(zeroXmlFilePath);

            // Check the existence of the namespace in the root element
            XmlElement? root = doc.DocumentElement;
            if (root != null)
            {
                // Get the namespace
                string xmlns = root.GetAttribute("xmlns");
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
                // Arabe
                "AR" => "حقل مفتاح API لـ DeepL فارغ. لن تعمل وظيفة الترجمة.",
                // Bulgare
                "BG" => "Полето за API ключа на DeepL е празно. Функцията за превод няма да работи.",
                // Tchèque
                "CS" => "Pole klíče API DeepL je prázdné. Překladová funkce nebude fungovat.",
                // Danois
                "DA" => "DeepL API-nøglen er tom. Oversættelsesfunktionen vil ikke virke.",
                // Allemand
                "DE" => "Das DeepL API-Schlüsselfeld ist leer. Die Übersetzungsfunktion wird nicht funktionieren.",
                // Grec
                "EL" => "Το πεδίο κλειδιού API του DeepL είναι κενό. Η λειτουργία μετάφρασης δεν θα λειτουργήσει.",
                // Anglais
                "EN" => "The DeepL API key field is empty. The translation function will not work.",
                // Espagnol
                "ES" => "El campo de la clave API de DeepL está vacío. La función de traducción no funcionará.",
                // Estonien
                "ET" => "DeepL API võtmeväli on tühi. Tõlkefunktsioon ei tööta.",
                // Finnois
                "FI" => "DeepL API-avainkenttä on tyhjä. Käännöstoiminto ei toimi.",
                // Hongrois
                "HU" => "A DeepL API kulcsmező üres. A fordítási funkció nem fog működni.",
                // Indonésien
                "ID" => "Kolom kunci API DeepL kosong. Fungsi terjemahan tidak akan berfungsi.",
                // Italien
                "IT" => "Il campo della chiave API di DeepL è vuoto. La funzione di traduzione non funzionerà.",
                // Japonais
                "JA" => "DeepL APIキーのフィールドが空です。翻訳機能は動作しません。",
                // Coréen
                "KO" => "DeepL API 키 필드가 비어 있습니다. 번역 기능이 작동하지 않습니다.",
                // Letton
                "LV" => "DeepL API atslēgas lauks ir tukšs. Tulkotāja funkcija nedarbosies.",
                // Lituanien
                "LT" => "DeepL API rakto laukas tuščias. Vertimo funkcija neveiks.",
                // Norvégien
                "NB" => "DeepL API-nøkkelfeltet er tomt. Oversettelsesfunksjonen vil ikke fungere.",
                // Néerlandais
                "NL" => "Het DeepL API-sleutelveld is leeg. De vertaalfunctie zal niet werken.",
                // Polonais
                "PL" => "Pole klucza API DeepL jest puste. Funkcja tłumaczenia nie będzie działać.",
                // Portugais
                "PT" => "O campo da chave API do DeepL está vazio. A função de tradução não funcionará.",
                // Roumain
                "RO" => "Câmpul cheii API DeepL este gol. Funcția de traducere nu va funcționa.",
                // Russe
                "RU" => "Поле для API-ключа DeepL пусто. Функция перевода не будет работать.",
                // Slovaque
                "SK" => "Pole API kľúča DeepL je prázdne. Prekladová funkcia nebude fungovať.",
                // Slovène
                "SL" => "Polje za API ključ DeepL je prazno. Prevajalska funkcija ne bo delovala.",
                // Suédois
                "SV" => "DeepL API-nyckelfältet är tomt. Översättningsfunktionen kommer inte att fungera.",
                // Turc
                "TR" => "DeepL API anahtar alanı boş. Çeviri işlevi çalışmayacak.",
                // Ukrainien
                "UK" => "Поле для ключа API DeepL порожнє. Функція перекладу не працюватиме.",
                // Chinois simplifié
                "ZH" => "DeepL API 密钥字段为空。翻译功能将无法工作。",
                // Cas par défaut (français)
                _ => "Le champ de la clé API DeepL est vide. La fonction de traduction des adresses de groupe a été désactivée."
            };
        }
        catch (AuthorizationException ex)
        {
            App.ConsoleAndLogWriteLine($"DeepL API key error: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "مفتاح API DeepL المدخل غير صحيح. تم تعطيل وظيفة ترجمة العناوين الجماعية.",
                // Bulgare
                "BG" => "Въведеният DeepL API ключ е невалиден. Функцията за превод на групови адреси е деактивирана.",
                // Tchèque
                "CS" => "Zadaný klíč API DeepL je neplatný. Funkce překladu skupinových adres byla deaktivována.",
                // Danois
                "DA" => "Den indtastede DeepL API-nøgle er ugyldig. Funktionen til oversættelse af gruppeadresser er deaktiveret.",
                // Allemand
                "DE" => "Der eingegebene DeepL API-Schlüssel ist ungültig. Die Übersetzungsfunktion für Gruppenadressen wurde deaktiviert.",
                // Grec
                "EL" => "Το κλειδί API του DeepL που εισάγατε δεν είναι έγκυρο. Η λειτουργία μετάφρασης διευθύνσεων ομάδας έχει απενεργοποιηθεί.",
                // Anglais
                "EN" => "The entered DeepL API key is incorrect. The group address translation function has been disabled.",
                // Espagnol
                "ES" => "La clave de API de DeepL ingresada es incorrecta. La función de traducción de direcciones de grupo ha sido desactivada.",
                // Estonien
                "ET" => "Sisestatud DeepL API võti on vale. Rühma aadresside tõlkimise funktsioon on keelatud.",
                // Finnois
                "FI" => "Syötetty DeepL API-avain on virheellinen. Ryhmäosoitteiden käännöstoiminto on poistettu käytöstä.",
                // Hongrois
                "HU" => "A megadott DeepL API kulcs érvénytelen. A csoportcímek fordítási funkciója le van tiltva.",
                // Indonésien
                "ID" => "Kunci API DeepL yang dimasukkan tidak valid. Fungsi terjemahan alamat grup telah dinonaktifkan.",
                // Italien
                "IT" => "La chiave API DeepL inserita non è valida. La funzione di traduzione degli indirizzi di gruppo è stata disattivata.",
                // Japonais
                "JA" => "入力されたDeepL APIキーが無効です。グループアドレス翻訳機能が無効になっています。",
                // Coréen
                "KO" => "입력한 DeepL API 키가 잘못되었습니다. 그룹 주소 번역 기능이 비활성화되었습니다.",
                // Letton
                "LV" => "Ievadītā DeepL API atslēga ir nepareiza. Grupas adreses tulkošanas funkcija ir atspējota.",
                // Lituanien
                "LT" => "Įvestas neteisingas DeepL API raktas. Grupės adresų vertimo funkcija išjungta.",
                // Norvégien
                "NB" => "Den angitte DeepL API-nøkkelen er ugyldig. Funksjonen for oversettelse av gruppeadresser er deaktivert.",
                // Néerlandais
                "NL" => "De ingevoerde DeepL API-sleutel is ongeldig. De functie voor het vertalen van groepsadressen is uitgeschakeld.",
                // Polonais
                "PL" => "Wprowadzony klucz API DeepL jest nieprawidłowy. Funkcja tłumaczenia adresów grupowych została wyłączona.",
                // Portugais
                "PT" => "A chave API do DeepL inserida está incorreta. A função de tradução de endereços de grupo foi desativada.",
                // Roumain
                "RO" => "Cheia API DeepL introdusă este incorectă. Funcția de traducere a adreselor de grup a fost dezactivată.",
                // Russe
                "RU" => "Введенный ключ API DeepL неверен. Функция перевода групповых адресов отключена.",
                // Slovaque
                "SK" => "Zadaný kľúč API DeepL je neplatný. Funkcia prekladu skupinových adries bola deaktivovaná.",
                // Slovène
                "SL" => "Vneseni DeepL API ključ je neveljaven. Funkcija prevajanja skupinskih naslovov je onemogočena.",
                // Suédois
                "SV" => "Den angivna DeepL API-nyckeln är ogiltig. Funktionen för översättning av gruppadresser har inaktiverats.",
                // Turc
                "TR" => "Girilen DeepL API anahtarı geçersiz. Grup adresi çeviri fonksiyonu devre dışı bırakıldı.",
                // Ukrainien
                "UK" => "Введений ключ API DeepL неправильний. Функцію перекладу групових адрес вимкнено.",
                // Chinois simplifié
                "ZH" => "输入的 DeepL API 密钥不正确。已禁用群组地址翻译功能。",
                // Cas par défaut (français)
                _ => "La clé API DeepL entrée est incorrecte. La fonction de traduction des adresses de groupe a été désactivée."
            };

        }
        catch (HttpRequestException ex)
        {
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => $"خطأ في الاتصال بالشبكة عند التحقق من مفتاح API DeepL لتفعيل ترجمة عناوين المجموعة: {ex.Message}. يرجى التحقق من اتصال الشبكة وإعادة المحاولة.",
                // Bulgare
                "BG" => $"Грешка в мрежовата връзка при проверка на API ключа на DeepL за активиране на превода на групови адреси: {ex.Message}. Моля, проверете мрежовата си връзка и опитайте отново.",
                // Tchèque
                "CS" => $"Chyba síťového připojení při ověřování klíče API DeepL pro aktivaci překladu skupinových adres: {ex.Message}. Zkontrolujte prosím své síťové připojení a zkuste to znovu.",
                // Danois
                "DA" => $"Netværksforbindelsesfejl ved verificering af DeepL API-nøglen for at aktivere oversættelse af gruppeadresser: {ex.Message}. Kontroller venligst din netværksforbindelse og prøv igen.",
                // Allemand
                "DE" => $"Netzwerkverbindungsfehler bei der Überprüfung des DeepL API-Schlüssels zur Aktivierung der Übersetzung von Gruppenadressen: {ex.Message}. Bitte überprüfen Sie Ihre Netzwerkverbindung und versuchen Sie es erneut.",
                // Grec
                "EL" => $"Σφάλμα σύνδεσης δικτύου κατά την επαλήθευση του κλειδιού API DeepL για την ενεργοποίηση της μετάφρασης διευθύνσεων ομάδας: {ex.Message}. Ελέγξτε τη σύνδεση δικτύου σας και δοκιμάστε ξανά.",
                // Anglais
                "EN" => $"Network connection error when verifying the DeepL API key to enable group address translation: {ex.Message}. Please check your network connection and try again.",
                // Espagnol
                "ES" => $"Error de conexión de red al verificar la clave API de DeepL para habilitar la traducción de direcciones de grupo: {ex.Message}. Por favor, verifique su conexión de red y vuelva a intentarlo.",
                // Estonien
                "ET" => $"Võrguühenduse viga DeepL API võtme kontrollimisel rühma aadressi tõlke lubamiseks: {ex.Message}. Kontrollige oma võrguühendust ja proovige uuesti.",
                // Finnois
                "FI" => $"Verkkoyhteysvirhe tarkistettaessa DeepL API-avainta ryhmäosoitteiden kääntämisen aktivoimiseksi: {ex.Message}. Tarkista verkkoyhteytesi ja yritä uudelleen.",
                // Hongrois
                "HU" => $"Hálózati kapcsolat hiba a DeepL API kulcs ellenőrzésekor a csoportcím fordításának engedélyezéséhez: {ex.Message}. Kérjük, ellenőrizze a hálózati kapcsolatát, és próbálja újra.",
                // Indonésien
                "ID" => $"Kesalahan koneksi jaringan saat memverifikasi kunci API DeepL untuk mengaktifkan terjemahan alamat grup: {ex.Message}. Silakan periksa koneksi jaringan Anda dan coba lagi.",
                // Italien
                "IT" => $"Errore di connessione di rete durante la verifica della chiave API DeepL per abilitare la traduzione degli indirizzi di gruppo: {ex.Message}. Si prega di controllare la connessione di rete e riprovare.",
                // Japonais
                "JA" => $"グループアドレスの翻訳を有効にするためにDeepL APIキーを検証する際のネットワーク接続エラー: {ex.Message}. ネットワーク接続を確認して、もう一度やり直してください。",
                // Coréen
                "KO" => $"그룹 주소 번역을 활성화하기 위해 DeepL API 키를 확인하는 동안 네트워크 연결 오류: {ex.Message}. 네트워크 연결을 확인하고 다시 시도하십시오.",
                // Letton
                "LV" => $"Tīkla savienojuma kļūda, pārbaudot DeepL API atslēgu, lai aktivizētu grupas adrešu tulkošanu: {ex.Message}. Lūdzu, pārbaudiet savu tīkla savienojumu un mēģiniet vēlreiz.",
                // Lituanien
                "LT" => $"Tinklo ryšio klaida tikrinant DeepL API raktą grupės adresų vertimui įjungti: {ex.Message}. Patikrinkite savo tinklo ryšį ir bandykite dar kartą.",
                // Norvégien
                "NB" => $"Nettverksforbindelsesfeil ved verifisering av DeepL API-nøkkelen for å aktivere oversettelse av gruppeadresser: {ex.Message}. Vennligst sjekk nettverksforbindelsen din og prøv igjen.",
                // Néerlandais
                "NL" => $"Netwerkverbindingsfout bij het verifiëren van de DeepL API-sleutel om groepsadresvertaling in te schakelen: {ex.Message}. Controleer uw netwerkverbinding en probeer het opnieuw.",
                // Polonais
                "PL" => $"Błąd połączenia sieciowego podczas weryfikacji klucza API DeepL w celu włączenia tłumaczenia adresów grupowych: {ex.Message}. Sprawdź swoje połączenie sieciowe i spróbuj ponownie.",
                // Portugais
                "PT" => $"Erro de conexão de rede ao verificar a chave API do DeepL para ativar a tradução de endereços de grupo: {ex.Message}. Verifique sua conexão de rede e tente novamente.",
                // Roumain
                "RO" => $"Eroare de conexiune la rețea la verificarea cheii API DeepL pentru a activa traducerea adreselor de grup: {ex.Message}. Vă rugăm să verificați conexiunea la rețea și să încercați din nou.",
                // Russe
                "RU" => $"Ошибка сетевого подключения при проверке ключа API DeepL для включения перевода групповых адресов: {ex.Message}. Пожалуйста, проверьте сетевое подключение и попробуйте еще раз.",
                // Slovaque
                "SK" => $"Chyba sieťového pripojenia pri overovaní kľúča API DeepL na aktiváciu prekladu skupinových adries: {ex.Message}. Skontrolujte svoje sieťové pripojenie a skúste to znova.",
                // Slovène
                "SL" => $"Napaka omrežne povezave pri preverjanju DeepL API ključa za omogočanje prevajanja skupinskih naslovov: {ex.Message}. Preverite svojo omrežno povezavo in poskusite znova.",
                // Suédois
                "SV" => $"Nätverksanslutningsfel vid verifiering av DeepL API-nyckeln för att aktivera översättning av gruppadresser: {ex.Message}. Kontrollera din nätverksanslutning och försök igen.",
                // Turc
                "TR" => $"Grup adresi çevirisini etkinleştirmek için DeepL API anahtarı doğrulanırken ağ bağlantısı hatası: {ex.Message}. Lütfen ağ bağlantınızı kontrol edin ve tekrar deneyin.",
                // Ukrainien
                "UK" => $"Помилка мережевого підключення під час перевірки ключа API DeepL для активації перекладу групових адрес: {ex.Message}. Будь ласка, перевірте своє мережеве підключення та спробуйте ще раз.",
                // Chinois simplifié
                "ZH" => $"在验证 DeepL API 密钥以启用组地址翻译时出现网络连接错误: {ex.Message}. 请检查您的网络连接，然后重试。",
                // Cas par défaut (français)
                _ => $"Erreur de connexion réseau lors de la vérification de la clé API DeepL pour activer la traduction des adresses de groupe: {ex.Message}. Veuillez vérifier votre connexion réseau et réessayer."
            };
            
        }
        catch (DeepLException ex)
        {
            App.ConsoleAndLogWriteLine($"DeepL API error: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => "خطأ أثناء تنشيط واجهة برمجة تطبيقات الترجمة DeepL: {ex.Message}. تم تعطيل ترجمة العناوين الجماعية.",
                // Bulgare
                "BG" => "Грешка при активиране на API за превод DeepL: {ex.Message}. Преводът на групови адреси е деактивиран.",
                // Tchèque
                "CS" => "Chyba při aktivaci překladového API DeepL: {ex.Message}. Překlad skupinových adres byl deaktivován.",
                // Danois
                "DA" => "Fejl ved aktivering af DeepL-oversættelses-API: {ex.Message}. Oversættelse af gruppeadresser er deaktiveret.",
                // Allemand
                "DE" => "Fehler bei der Aktivierung der DeepL-Übersetzungs-API: {ex.Message}. Die Übersetzung von Gruppenadressen wurde deaktiviert.",
                // Grec
                "EL" => "Σφάλμα κατά την ενεργοποίηση του API μετάφρασης DeepL: {ex.Message}. Η μετάφραση των διευθύνσεων ομάδας έχει απενεργοποιηθεί.",
                // Anglais
                "EN" => "Error activating the DeepL translation API: {ex.Message}. Group address translation has been disabled.",
                // Espagnol
                "ES" => "Error al activar la API de traducción de DeepL: {ex.Message}. La traducción de direcciones de grupo ha sido desactivada.",
                // Estonien
                "ET" => "Tõlke-API DeepL aktiveerimisel ilmnes viga: {ex.Message}. Grupi aadresside tõlkimine on keelatud.",
                // Finnois
                "FI" => "Virhe DeepL-käännös-API:n aktivoinnissa: {ex.Message}. Ryhmäosoitteiden kääntäminen on poistettu käytöstä.",
                // Hongrois
                "HU" => "Hiba történt a DeepL fordító-API aktiválása során: {ex.Message}. A csoportcímek fordítása le van tiltva.",
                // Indonésien
                "ID" => "Kesalahan saat mengaktifkan API terjemahan DeepL: {ex.Message}. Terjemahan alamat grup telah dinonaktifkan.",
                // Italien
                "IT" => "Errore durante l'attivazione dell'API di traduzione DeepL: {ex.Message}. La traduzione degli indirizzi di gruppo è stata disattivata.",
                // Japonais
                "JA" => "DeepL翻訳APIの有効化中にエラーが発生しました: {ex.Message}。グループアドレスの翻訳が無効になっています。",
                // Coréen
                "KO" => "DeepL 번역 API 활성화 중 오류 발생: {ex.Message}. 그룹 주소 번역이 비활성화되었습니다.",
                // Letton
                "LV" => "Kļūda, aktivizējot DeepL tulkošanas API: {ex.Message}. Grupas adrešu tulkošana ir atspējota.",
                // Lituanien
                "LT" => "Klaida aktyvinant DeepL vertimo API: {ex.Message}. Grupės adresų vertimas išjungtas.",
                // Norvégien
                "NB" => "Feil ved aktivering av DeepL-oversettelses-API: {ex.Message}. Oversettelse av gruppeadresser er deaktivert.",
                // Néerlandais
                "NL" => "Fout bij het activeren van de DeepL-vertaal-API: {ex.Message}. Vertaling van groepsadressen is uitgeschakeld.",
                // Polonais
                "PL" => "Błąd podczas aktywacji API tłumaczenia DeepL: {ex.Message}. Tłumaczenie adresów grupowych zostało wyłączone.",
                // Portugais
                "PT" => "Erro ao ativar a API de tradução DeepL: {ex.Message}. A tradução de endereços de grupo foi desativada.",
                // Roumain
                "RO" => "Eroare la activarea API-ului de traducere DeepL: {ex.Message}. Traducerea adreselor de grup a fost dezactivată.",
                // Russe
                "RU" => "Ошибка при активации API перевода DeepL: {ex.Message}. Перевод групповых адресов отключен.",
                // Slovaque
                "SK" => "Chyba pri aktivácii prekladového API DeepL: {ex.Message}. Preklad skupinových adries bol deaktivovaný.",
                // Slovène
                "SL" => "Napaka pri aktivaciji prevajalskega API-ja DeepL: {ex.Message}. Prevajanje skupinskih naslovov je onemogočeno.",
                // Suédois
                "SV" => "Fel vid aktivering av DeepL-översättnings-API: {ex.Message}. Översättning av gruppadresser har inaktiverats.",
                // Turc
                "TR" => "DeepL çeviri API'si etkinleştirilirken hata oluştu: {ex.Message}. Grup adresi çevirisi devre dışı bırakıldı.",
                // Ukrainien
                "UK" => "Помилка під час активації API перекладу DeepL: {ex.Message}. Переклад групових адрес вимкнено.",
                // Chinois simplifié
                "ZH" => "激活DeepL翻译API时出错: {ex.Message}。组地址翻译已禁用。",
                // Cas par défaut (français)
                _ => $"Erreur lors de l'activation de l'API de traduction DeepL: {ex.Message}. La traduction des adresses de groupe a été désactivée."
            };

        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
            keyValid = false;
            
            errMessage = App.DisplayElements?.SettingsWindow!.AppLang switch
            {
                // Arabe
                "AR" => $"حدث خطأ غير متوقع أثناء تفعيل واجهة برمجة تطبيقات الترجمة DeepL: {ex.Message}. تم تعطيل ترجمة عناوين المجموعة.",
                // Bulgare
                "BG" => $"Възникна неочаквана грешка при активиране на API за превод на DeepL: {ex.Message}. Преводът на груповите адреси е деактивиран.",
                // Tchèque
                "CS" => $"Při aktivaci překládacího API DeepL došlo k neočekávané chybě: {ex.Message}. Překlad skupinových adres byl deaktivován.",
                // Danois
                "DA" => $"Der opstod en uventet fejl under aktivering af DeepL-oversættelses-API: {ex.Message}. Oversættelse af gruppeadresser er deaktiveret.",
                // Allemand
                "DE" => $"Ein unerwarteter Fehler ist beim Aktivieren der DeepL-Übersetzungs-API aufgetreten: {ex.Message}. Die Übersetzung von Gruppenadressen wurde deaktiviert.",
                // Grec
                "EL" => $"Παρουσιάστηκε ένα απρόσμενο σφάλμα κατά την ενεργοποίηση του API μετάφρασης DeepL: {ex.Message}. Η μετάφραση των διευθύνσεων ομάδας έχει απενεργοποιηθεί.",
                // Anglais
                "EN" => $"An unexpected error occurred while activating the DeepL translation API: {ex.Message}. Group address translation has been disabled.",
                // Espagnol
                "ES" => $"Ocurrió un error inesperado al activar la API de traducción de DeepL: {ex.Message}. La traducción de direcciones de grupo ha sido desactivada.",
                // Estonien
                "ET" => $"DeepL tõlke API aktiveerimisel ilmnes ootamatu viga: {ex.Message}. Grupi aadresside tõlkimine on keelatud.",
                // Finnois
                "FI" => $"DeepL-käännös-API:ta aktivoitaessa tapahtui odottamaton virhe: {ex.Message}. Ryhmäosoitteiden kääntäminen on poistettu käytöstä.",
                // Hongrois
                "HU" => $"A DeepL fordító-API aktiválása során váratlan hiba történt: {ex.Message}. A csoportcímek fordítása letiltásra került.",
                // Indonésien
                "ID" => $"Terjadi kesalahan tak terduga saat mengaktifkan API terjemahan DeepL: {ex.Message}. Terjemahan alamat grup telah dinonaktifkan.",
                // Italien
                "IT" => $"Si è verificato un errore imprevisto durante l'attivazione dell'API di traduzione DeepL: {ex.Message}. La traduzione degli indirizzi di gruppo è stata disabilitata.",
                // Japonais
                "JA" => $"DeepL翻訳APIの有効化中に予期しないエラーが発生しました: {ex.Message}。グループアドレスの翻訳が無効になりました。",
                // Coréen
                "KO" => $"DeepL 번역 API를 활성화하는 동안 예상치 못한 오류가 발생했습니다: {ex.Message}. 그룹 주소 번역이 비활성화되었습니다.",
                // Letton
                "LV" => $"Aktivējot DeepL tulkošanas API, radās neparedzēta kļūda: {ex.Message}. Grupas adreses tulkošana ir atspējota.",
                // Lituanien
                "LT" => $"Įjungiant „DeepL“ vertimo API įvyko nenumatyta klaida: {ex.Message}. Grupės adresų vertimas buvo išjungtas.",
                // Norvégien
                "NB" => $"En uventet feil oppsto under aktivering av DeepL oversettelses-API: {ex.Message}. Oversettelse av gruppeadresser er deaktivert.",
                // Néerlandais
                "NL" => $"Er is een onverwachte fout opgetreden bij het activeren van de DeepL-vertaal-API: {ex.Message}. Vertaling van groepsadressen is uitgeschakeld.",
                // Polonais
                "PL" => $"Wystąpił nieoczekiwany błąd podczas aktywacji interfejsu API tłumaczenia DeepL: {ex.Message}. Tłumaczenie adresów grupowych zostało wyłączone.",
                // Portugais
                "PT" => $"Ocorreu um erro inesperado ao ativar a API de tradução DeepL: {ex.Message}. A tradução de endereços de grupo foi desativada.",
                // Roumain
                "RO" => $"A apărut o eroare neașteptată în timpul activării API-ului de traducere DeepL: {ex.Message}. Traducerea adreselor de grup a fost dezactivată.",
                // Russe
                "RU" => $"Произошла непредвиденная ошибка при активации API перевода DeepL: {ex.Message}. Перевод групповых адресов был отключен.",
                // Slovaque
                "SK" => $"Pri aktivácii prekladacieho API DeepL došlo k neočakávanej chybe: {ex.Message}. Preklad skupinových adries bol deaktivovaný.",
                // Slovène
                "SL" => $"Pri aktivaciji prevajalskega API-ja DeepL je prišlo do nepričakovane napake: {ex.Message}. Prevajanje naslovov skupin je onemogočeno.",
                // Suédois
                "SV" => $"Ett oväntat fel uppstod vid aktivering av DeepL-översättnings-API: {ex.Message}. Översättning av gruppadresser har inaktiverats.",
                // Turc
                "TR" => $"DeepL çeviri API'si etkinleştirilirken beklenmedik bir hata oluştu: {ex.Message}. Grup adresi çevirisi devre dışı bırakıldı.",
                // Ukrainien
                "UK" => $"Виникла непередбачена помилка під час активації API перекладу DeepL: {ex.Message}. Переклад групових адрес відключено.",
                // Chinois simplifié
                "ZH" => $"激活DeepL翻译API时发生意外错误: {ex.Message}。已禁用群组地址翻译。",
                // Cas par défaut (français)
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
    /// Generates a formatted location name based on the provided location object and name attribute value.
    /// If the location object is null, returns a default formatted location name.
    /// Otherwise, constructs the location name using the available attributes of the location object,
    /// with fallback values for missing attributes. Appends a formatted distribution board name if present,
    /// and adds the name attribute value if it matches the specified pattern.
    ///
    /// <param name="location">A dynamic object representing the location with attributes such as BuildingName, BuildingPartName, FloorName, RoomName, and DistributionBoardName.</param>
    /// <param name="nameAttrValue">A string containing the name attribute value that is appended to the location name if it matches a specific pattern.</param>
    /// <returns>
    /// A formatted string representing the location name, constructed from the location attributes and the name attribute value.
    /// </returns>
    /// </summary>
    static string GetLocationName(dynamic location, string nameAttrValue)
    {
        string nameLocation;
        Match match;
        if (location == null)
        {
            // Default location details if no location information is found
            App.ConsoleAndLogWriteLine("No location found");
            
            nameLocation = $"_{_formatter.Format("Bâtiment")}_{_formatter.Format("Facade XX")}_{_formatter.Format("Etage")}_{_formatter.Format("Piece")}";
            
            //Add circuit part to the name if it exist
            match = Regex.Match(nameAttrValue, @"(?=.*[a-zA-Z])(?=.*\d)[a-zA-Z0-9/+]+$");
            if (match.Success)
            {
                nameLocation += "_" + match.Value;
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
        if (!string.IsNullOrEmpty(distributionBoardName))
        {
            nameLocation += $"_{_formatter.Format(distributionBoardName)}";
        }

        //Add circuit part to the name if it exist
        match = Regex.Match(nameAttrValue, @"(?=.*[a-zA-Z])(?=.*\d)[a-zA-Z0-9/+]+$");
        if (match.Success)
        {
            nameLocation += "_" + match.Value;
        }

        return nameLocation;
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
    static string DetermineNameObjectType(dynamic deviceRailMounted, dynamic deviceRefObjectType, string nameAttrValue)
    {
        if (Regex.IsMatch(nameAttrValue, @"^(?!.*\bie\b).*?\b(cmd)\b(?!.*\bie\b).*$", RegexOptions.IgnoreCase))
        {
            return $"{_formatter.Format("Cmd")}";
        }
        else if (Regex.IsMatch(nameAttrValue, @"\bie\b", RegexOptions.IgnoreCase))
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
    /// Retrieves and formats the function name for a group range based on the provided group address element.
    /// It traverses the ancestors of the element to find the relevant group range elements,
    /// formats their names, and combines them into a single function name.
    /// If no group range element is found, returns an empty string.
    ///
    /// <param name="groupAddressElement">An XElement representing the group address.</param>
    /// <returns>
    /// A formatted string representing the function name for the group range, constructed from the names of the ancestor group range elements.
    /// </returns>
    /// </summary>
    static string GetGroupRangeFunctionName(XElement groupAddressElement)
    {
        // Get the GroupRange ancestor element, if any
        var groupRangeElement = groupAddressElement.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
        if (groupRangeElement == null) return string.Empty;

        string nameFunction = string.Empty;
        
        // Check for a higher-level GroupRange ancestor
        var ancestorGroupRange = groupRangeElement.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
        if (ancestorGroupRange != null)
        {
            // Format the name of the ancestor GroupRange
            nameFunction = $"_{_formatter.Format(ancestorGroupRange.Attribute("Name")?.Value ?? string.Empty)}";
            // Translate the group name
            TranslateGroupRangeName(ancestorGroupRange);
        }
        
        // Format the name of the current GroupRange
        nameFunction += $"_{_formatter.Format(groupRangeElement.Attribute("Name")?.Value ?? string.Empty)}";
        // Translate the group name
        TranslateGroupRangeName(groupRangeElement);

        return nameFunction;
    }

    /// <summary>
    /// Translates the name attribute of the provided group range element using DeepL translation if enabled and valid.
    /// Checks if translation is enabled, the DeepL API key is valid, and the name attribute is not already translated.
    /// If these conditions are met, translates the name attribute and adds it to the translation cache to avoid redundant translations.
    ///
    /// <param name="groupRangeElement">An XElement representing the group range with a "Name" attribute to be translated.</param>
    /// </summary>
    static void TranslateGroupRangeName(XElement groupRangeElement)
    {
        //Check if the translation is needed
        if (App.DisplayElements?.SettingsWindow == null || !App.DisplayElements.SettingsWindow.EnableDeeplTranslation || !ValidDeeplKey)
            return;

        var nameAttr = groupRangeElement.Attribute("Name");
        if (nameAttr == null) return;

        string nameValue = nameAttr.Value;
        if (string.IsNullOrEmpty(nameValue) || TranslationCache.Contains(nameValue))
            return;

        // Translated only if not already translated
        nameAttr.Value = _formatter.Translate(nameValue);
        TranslationCache.Add(nameAttr.Value);
    }
    
}




