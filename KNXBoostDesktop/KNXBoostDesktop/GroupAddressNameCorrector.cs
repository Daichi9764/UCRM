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
    private static XNamespace _globalKnxNamespace = string.Empty;
    private static string _projectFilesDirectory = string.Empty;
    public static string AuthKey { get; private set; }
    public static Translator Translator { get; private set; }
    public static bool ValidDeeplKey; 
    
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 9159MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Xml.Linq.XAttribute; size: 7650MB")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Xml.Linq.XElement; size: 4051MB")]
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
            
            // Load the XML file from the specified path
            XDocument knxDoc;
            
            App.DisplayElements!.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(loadXml);

            try
            {
                knxDoc = XDocument.Load(App.Fm?.ZeroXmlPath ?? string.Empty);
            }
            catch (FileNotFoundException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: File not found. {ex.Message}");
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Directory not found. {ex.Message}");
                return;
            }
            catch (IOException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: IO exception occurred. {ex.Message}");
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Access denied. {ex.Message}");
                return;
            }
            catch (XmlException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Invalid XML. {ex.Message}");
                return;
            }

            // Create a formatter object for normalizing names
            Formatter formatter;
            if (App.DisplayElements.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableDeeplTranslation)
            {
                ValidDeeplKey = CheckDeeplKey().Item1;
                if (ValidDeeplKey)
                {
                    formatter= new FormatterTranslate();
                }
                else
                {
                    formatter= new FormatterNormalize();
                }
            }
            else
            { 
                formatter = new FormatterNormalize();
            }
            
            App.DisplayElements!.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(extractingInfos);

            // Extract location information from the KNX file
            var locationInfo = knxDoc.Descendants(_globalKnxNamespace + "Space")
                .Where(s => s.Attribute("Type")?.Value == "Room" || s.Attribute("Type")?.Value == "Corridor")
                .Select(room => new
                {
                    RoomName = room.Attribute("Name")?.Value,
                    FloorName = room.Ancestors(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Floor")?.Attribute("Name")?.Value,
                    BuildingPartName = room.Ancestors(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "BuildingPart")?.Attribute("Name")?.Value,
                    BuildingName = room.Ancestors(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Building")?.Attribute("Name")?.Value,
                    DistributionBoardName = room.Descendants(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "DistributionBoard")?.Attribute("Name")?.Value,
                    DeviceRefs = room.Descendants(_globalKnxNamespace + "DeviceInstanceRef").Select(dir => dir.Attribute("RefId")?.Value)
                })
                .ToList();
            
            App.DisplayElements.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(infosExtracted);

            // Display extracted location information
            App.ConsoleAndLogWriteLine("Extracted Location Information:");
            foreach (var loc in locationInfo)
            {
                string message = string.Empty;
                if (loc.DistributionBoardName != null)
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
            
            App.DisplayElements.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(extractingDeviceReferences);

            // Extract device instance references and  their group object instance references from the KNX file
            var deviceRefsTemp1 = knxDoc.Descendants(_globalKnxNamespace + "DeviceInstance")
            .Select(di => new
            {
                Id = di.Attribute("Id")?.Value,
                Hardware2ProgramRefId = di.Attribute("Hardware2ProgramRefId")?.Value,
                ProductRefId = di.Attribute("ProductRefId")?.Value,
                GroupObjectInstanceRefs = di.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
                    .Where(cir => cir.Attribute("Links") != null)
                    .SelectMany(cir => (cir.Attribute("Links")?.Value.Split(' ') ?? [])
                        .Select((link, index) => new
                        {
                            GroupAddressRef = link,
                            DeviceInstanceId = di.Attribute("Id")?.Value,
                            ComObjectInstanceRefId = cir.Attribute("RefId")?.Value,
                            IsFirstLink = index == 0  // Mark if it's the first link
                        }))
            });
            
            App.DisplayElements.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(extractingDeviceInfo);
            
            var deviceRefs = deviceRefsTemp1.SelectMany(di => di.GroupObjectInstanceRefs.Select(g => new
            {
                di.Id,
                di.ProductRefId,
                HardwareFileName = di.Hardware2ProgramRefId != null ? 
                    FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).HardwareFileName : 
                    null,
                MxxxxDirectory = di.Hardware2ProgramRefId != null ? 
                    FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory : 
                    null,
                IsDeviceRailMounted = di.ProductRefId != null && di.Hardware2ProgramRefId != null && GetIsDeviceRailMounted(di.ProductRefId, FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory),
                g.GroupAddressRef,
                g.DeviceInstanceId,
                g.ComObjectInstanceRefId,
                ObjectType = di.Hardware2ProgramRefId != null && g.ComObjectInstanceRefId != null && g.IsFirstLink ?
                    GetObjectType(FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).HardwareFileName, FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory, g.ComObjectInstanceRefId) : 
                    null
            }))
            .ToList();

            App.DisplayElements.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(infoAndReferencesExtracted);
            
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
            
            App.DisplayElements.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(constructingNewAddresses);
            
            // Collection to track the IDs of renamed GroupAddresses
            HashSet<string> renamedGroupAddressIds = new HashSet<string>();
            
            // Collection to memorize translations of group names already done
            HashSet<string> translationCache = new HashSet<string>();
            
            // Construct the new name of the group address by iterating through each group of device references
            foreach (var gdr in groupedDeviceRefs)
            {
                string nameObjectType;
                string nameFunction = string.Empty;

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

                    if (groupAddressElement != null)
                    {
                        App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                        var nameAttr = groupAddressElement.Attribute("Name");
                        
                        if (nameAttr != null)
                        {
                            // Get the location information for the device reference
                            var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceNotRailMounted.DeviceInstanceId));

                            string nameLocation;
                            if (location != null)
                            {
                                string buildingName = !string.IsNullOrEmpty(location.BuildingName) ? location.BuildingName : "Batiment";
                                string buildingPartName = !string.IsNullOrEmpty(location.BuildingPartName) ? location.BuildingPartName : "FacadeXx";
                                string floorName = !string.IsNullOrEmpty(location.FloorName) ? location.FloorName : "FacadeXx";
                                string roomName = !string.IsNullOrEmpty(location.RoomName) ? location.RoomName : "Piece"; 
                                string distributionBoardName = !string.IsNullOrEmpty(location.DistributionBoardName) ? location.DistributionBoardName : string.Empty;
                                
                                // Format the location details
                                nameLocation = $"_{formatter.Format(buildingName)}_{formatter.Format(buildingPartName)}_{formatter.Format(floorName)}_{formatter.Format(roomName)}";
                                
                                if (distributionBoardName != string.Empty)
                                {
                                    nameLocation += $"_{formatter.Format(distributionBoardName)}"; 
                                }

                                //Add circuit part to the name
                                Match match = Regex.Match(nameAttr.Value,@"(?=.*[a-zA-Z])(?=.*\d)[a-zA-Z0-9/+]+$");

                                if (match.Success)
                                {
                                    nameLocation += "_" + match.Value;
                                }
                            }
                            else
                            {
                                // Default location details if no location information is found
                                nameLocation = $"_{formatter.Format("Batiment")}_{formatter.Format("FacadeXX")}_{formatter.Format("Etage")}_{formatter.Format("Piece")}";
                                App.ConsoleAndLogWriteLine($"No location found for DeviceInstanceId: {deviceNotRailMounted.DeviceInstanceId}");
                            }

                            // Determine the nameObjectType based on the available device references
                            if (Regex.IsMatch(nameAttr.Value, @"^(?!.*\bie\b).*?\b(cmd)\b(?!.*\bie\b).*$", RegexOptions.IgnoreCase)) //it contains cmd and not ie
                            {
                                nameObjectType = $"{formatter.Format("Cmd")}";
                            }
                            else if (Regex.IsMatch(nameAttr.Value, @"\bie\b", RegexOptions.IgnoreCase))
                            {
                                nameObjectType = $"{formatter.Format("Ie")}";
                            }
                            else if (deviceRailMounted != null && !string.IsNullOrEmpty(deviceRailMounted.ObjectType))
                            {
                                // Format the ObjectType of the rail-mounted device
                                nameObjectType = $"{formatter.Format(deviceRailMounted.ObjectType ?? string.Empty)}";
                            }
                            else if (deviceRefObjectType != null)
                            {
                                // Format the ObjectType of the device with a non-empty ObjectType
                                nameObjectType = $"{formatter.Format(deviceRefObjectType.ObjectType ?? string.Empty)}";
                            }
                            else
                            {
                                // Default nameObjectType if no valid ObjectType is found
                                nameObjectType = $"{formatter.Format("Type")}";
                                App.ConsoleAndLogWriteLine($"No Object Type found for {gdr.Devices.FirstOrDefault()?.GroupAddressRef}");
                            }
                            // Get the GroupRange ancestor element, if any
                            var groupRangeElement = groupAddressElement.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
                            if (groupRangeElement != null)
                            {
                                // Check for a higher-level GroupRange ancestor
                                var ancestorGroupRange = groupRangeElement.Ancestors(_globalKnxNamespace + "GroupRange").FirstOrDefault();
                                if (ancestorGroupRange != null)
                                {
                                    // Format the name of the ancestor GroupRange
                                    nameFunction = $"_{formatter.Format(ancestorGroupRange.Attribute("Name")?.Value ?? string.Empty)}";
                                    
                                    // Translate the group name
                                    if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableDeeplTranslation && ValidDeeplKey)
                                    {
                                        var nameAncestorGrpRange = ancestorGroupRange.Attribute("Name");
                                        string ancestorGroupRangeName = nameAncestorGrpRange?.Value ?? string.Empty;
                                        
                                        // Translated only if not already translated
                                        if (nameAncestorGrpRange != null && ancestorGroupRangeName != string.Empty && !translationCache.Contains(ancestorGroupRangeName))
                                        {
                                            nameAncestorGrpRange.Value = formatter.Translate(ancestorGroupRangeName);
                                            translationCache.Add(nameAncestorGrpRange.Value);
                                        }
                                    }
                                    
                                }
                                // Format the name of the current GroupRange
                                nameFunction += $"_{formatter.Format(groupRangeElement.Attribute("Name")?.Value ?? string.Empty)}";
                                
                                // Translate the group name
                                if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableDeeplTranslation && ValidDeeplKey)
                                {
                                    var nameGrpRange = groupRangeElement.Attribute("Name");
                                    string groupRangeName = nameGrpRange?.Value ?? string.Empty;
                                    
                                    // Translated only if not already translated
                                    if (nameGrpRange != null && groupRangeName != string.Empty && !translationCache.Contains(groupRangeName))
                                    {
                                        nameGrpRange.Value = formatter.Translate(groupRangeName);
                                        translationCache.Add(nameGrpRange.Value);
                                    }
                                }

                                
                            }

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
                    }
                    else
                    {
                        // Log if no GroupAddress element is found for the reference
                        App.ConsoleAndLogWriteLine($"No GroupAddress element found for GroupAddressRef: {deviceNotRailMounted.GroupAddressRef}");
                        
                        
                    }
                }
                else if (deviceRailMounted != null && deviceNotRailMounted == null)
                {
                    // Find the GroupAddress element that matches the device's GroupAddressRef
                    var groupAddressElement = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                        .FirstOrDefault(ga => ga.Attribute("Id")?.Value.EndsWith(deviceRailMounted.GroupAddressRef) == true);

                    if (groupAddressElement != null)
                    {
                        App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                        var nameAttr = groupAddressElement.Attribute("Name");
                        if (nameAttr != null)
                        {
                            // Get the location information for the device reference
                            var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceRailMounted.DeviceInstanceId));

                            string nameLocation;
                            if (location != null)
                            {
                                string buildingName = !string.IsNullOrEmpty(location.BuildingName) ? location.BuildingName : "Batiment";
                                string buildingPartName = !string.IsNullOrEmpty(location.BuildingPartName) ? location.BuildingPartName : "FacadeXx";
                                string floorName = !string.IsNullOrEmpty(location.FloorName) ? location.FloorName : "FacadeXx";
                                string roomName = !string.IsNullOrEmpty(location.RoomName) ? location.RoomName : "Piece";
                                string distributionBoardName = !string.IsNullOrEmpty(location.DistributionBoardName) ? location.DistributionBoardName : string.Empty;
                                
                                // Format the location details
                                nameLocation =
                                    $"_{formatter.Format(buildingName)}_{formatter.Format(buildingPartName)}_{formatter.Format(floorName)}_{formatter.Format(roomName)}";
                                if (distributionBoardName != string.Empty)
                                {
                                    nameLocation += $"_{formatter.Format(distributionBoardName)}"; 
                                }
                                
                                //Add circuit part to the name
                                Match match = Regex.Match(nameAttr.Value,@"(?=.*[a-zA-Z])(?=.*\d)[a-zA-Z0-9/+]+$");

                                if (match.Success)
                                {
                                    nameLocation += "_" + match.Value;
                                }
                            }
                            else
                            {
                                // Default location details if no location information is found
                                nameLocation = $"_{formatter.Format("Batiment")}_{formatter.Format("FacadeXx")}_{formatter.Format("Etage")}_{formatter.Format("Piece")}";
                                App.ConsoleAndLogWriteLine($"No location found for DeviceInstanceId: {deviceRailMounted.DeviceInstanceId}");
                            }

                    
                            // Determine the nameObjectType based on the available device references
                            if (Regex.IsMatch(nameAttr.Value, @"^(?!.*\bie\b).*?\b(cmd)\b(?!.*\bie\b).*$", RegexOptions.IgnoreCase))
                            {
                                nameObjectType = $"{formatter.Format("Cmd")}";
                            }
                            else if (Regex.IsMatch(nameAttr.Value, @"\bie\b", RegexOptions.IgnoreCase))
                            {
                                nameObjectType = $"{formatter.Format("Ie")}";
                            }
                            else if (!string.IsNullOrEmpty(deviceRailMounted.ObjectType))
                            {
                                // Format the ObjectType of the rail-mounted device
                                nameObjectType = $"{formatter.Format(deviceRailMounted.ObjectType ?? string.Empty)}";
                            }
                            else if (deviceRefObjectType != null)
                            {
                                // Format the ObjectType of the device with a non-empty ObjectType
                                nameObjectType = $"{formatter.Format(deviceRefObjectType.ObjectType ?? string.Empty)}";
                            }
                            else
                            {
                                // Default nameObjectType if no valid ObjectType is found
                                nameObjectType = $"{formatter.Format("Type")}";
                                App.ConsoleAndLogWriteLine($"No Object Type found for {gdr.Devices.FirstOrDefault()?.GroupAddressRef}");
                            }
                            // Get the GroupRange ancestor element, if any
                            var groupRangeElement = groupAddressElement.Ancestors(_globalKnxNamespace + "GroupRange")
                                .FirstOrDefault();
                            if (groupRangeElement != null)
                            {
                                // Check for a higher-level GroupRange ancestor
                                var ancestorGroupRange = groupRangeElement.Ancestors(_globalKnxNamespace + "GroupRange")
                                    .FirstOrDefault();
                                if (ancestorGroupRange != null)
                                {
                                    // Format the name of the ancestor GroupRange
                                    nameFunction =
                                        $"_{formatter.Format(ancestorGroupRange.Attribute("Name")?.Value ?? string.Empty)}";
                                    
                                    // Translate the group name
                                    if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableDeeplTranslation && ValidDeeplKey)
                                    {
                                        var nameAncestorGrpRange = ancestorGroupRange.Attribute("Name");
                                        string ancestorGroupRangeName = nameAncestorGrpRange?.Value ?? string.Empty;
                                        
                                        // Translated only if not already translated
                                        if (nameAncestorGrpRange != null && ancestorGroupRangeName != string.Empty && !translationCache.Contains(ancestorGroupRangeName))
                                        {
                                            nameAncestorGrpRange.Value = formatter.Translate(ancestorGroupRangeName);
                                            translationCache.Add(nameAncestorGrpRange.Value);
                                        }
                                    }
                                }
                                
                                // Format the name of the current GroupRange
                                nameFunction +=
                                    $"_{formatter.Format(groupRangeElement.Attribute("Name")?.Value ?? string.Empty)}";

                                // Translate the group name
                                if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableDeeplTranslation && ValidDeeplKey)
                                {
                                    var nameGrpRange = groupRangeElement.Attribute("Name");
                                    string groupRangeName = nameGrpRange?.Value ?? string.Empty;
                                    
                                    // Translated only if not already translated
                                    if (nameGrpRange != null && groupRangeName != string.Empty && !translationCache.Contains(groupRangeName))
                                    {
                                        nameGrpRange.Value = formatter.Translate(groupRangeName);
                                        translationCache.Add(nameGrpRange.Value);
                                    }
                                }
                                
                            }

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
                }
            }
            
            // Load the original XML file without any additional modifications
            XDocument originalKnxDoc;
            try
            {
                originalKnxDoc = XDocument.Load(App.Fm?.ZeroXmlPath ?? string.Empty);
            }
            catch (FileNotFoundException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: File not found. {ex.Message}");
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Directory not found. {ex.Message}");
                return;
            }
            catch (IOException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: IO exception occurred. {ex.Message}");
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Access denied. {ex.Message}");
                return;
            }
            catch (XmlException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Invalid XML. {ex.Message}");
                return;
            }
            
            // Deletes unused (not renamed) GroupAddresses if requested
            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses)
            {
                var allGroupAddresses = originalKnxDoc.Descendants(_globalKnxNamespace + "GroupAddress").ToList();
                foreach (var groupAddress in allGroupAddresses)
                {
                    var groupId = groupAddress.Attribute("Id")?.Value;
                    if (groupId != null && !renamedGroupAddressIds.Contains(groupId))
                    {
                        // Supprimer dans originalKnxDoc
                        groupAddress.Remove();

                        // Supprimer dans knxDoc
                        var correspondingGroupAddressInKnxDoc = knxDoc.Descendants(_globalKnxNamespace + "GroupAddress")
                            .FirstOrDefault(ga => ga.Attribute("Id")?.Value == groupId);

                        if (correspondingGroupAddressInKnxDoc != null)
                            correspondingGroupAddressInKnxDoc.Remove();

                        App.ConsoleAndLogWriteLine($"Removed unrenamed GroupAddress ID: {groupId}");
                    }
                }
            }

            // Save the updated XML files
            App.DisplayElements!.LoadingWindow?.MarkActivityComplete();
            App.DisplayElements.LoadingWindow?.LogActivity(savingUpdatedXml);

            try
            {
                knxDoc.Save($@"{App.Fm?.ProjectFolderPath}/0_updated.xml"); 
                App.ConsoleAndLogWriteLine("Updated XML file saved as '0_updated.xml'");
            }
            catch (UnauthorizedAccessException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: Access denied when saving the file. {ex.Message}");
            }
            catch (IOException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: IO exception occurred when saving the file. {ex.Message}");
            }

            if (App.DisplayElements.SettingsWindow != null && App.DisplayElements.SettingsWindow.RemoveUnusedGroupAddresses)
            {
                try
                {
                    originalKnxDoc.Save($@"{App.Fm?.ProjectFolderPath}/0_original.xml");
                    App.ConsoleAndLogWriteLine("Updated XML file saved as '0_updated.xml'");
                }
                catch (UnauthorizedAccessException ex)
                {
                    App.ConsoleAndLogWriteLine($"Error: Access denied when saving the file. {ex.Message}");
                }
                catch (IOException ex)
                {
                    App.ConsoleAndLogWriteLine($"Error: IO exception occurred when saving the file. {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred during CorrectName(): {ex.Message}");
        }
    }

    // Method that retrieves the ReadFlag and WriteFlag associated with a participant to determine its ObjectType (Cmd/Ie)
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

    public static (bool, string) CheckDeeplKey()
    {
        try
        {
            // Retrieve the DeepL API authentication key
            AuthKey = App.DisplayElements?.SettingsWindow?.DecryptStringFromBytes(App.DisplayElements.SettingsWindow.DeeplKey) ?? string.Empty;
        
            // Vérifiez si la clé d'authentification est null
            if (string.IsNullOrEmpty(AuthKey))
            {
                throw new ArgumentNullException("DeepL API key is not configured.");
            }
            
            // Initialize the DeepL Translator
            Translator = new Translator(AuthKey);

            // Perform a small test translation to check the key
            Task.Run(async () =>
            {
                await Translator.TranslateTextAsync("test", "EN", "FR");
            }).GetAwaiter().GetResult();

            return (true, string.Empty);
        }
        catch (ArgumentNullException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: {ex.Message}");
            return (false, $"The DeepL key API field is empty. The translation function will not work.");
        }
        catch (AuthorizationException ex)
        {
            App.ConsoleAndLogWriteLine($"DeepL API key error: {ex.Message}");
            return (false, $"The DeepL key API is incorrect. The translation function will not work.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}. Please check your internet connection.");
        }
        catch (DeepLException ex)
        {
            App.ConsoleAndLogWriteLine($"DeepL API error: {ex.Message}");
            return (false, $"DeepL API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
            return (false, $"An unexpected error occurred: {ex.Message}");
        }
    }
}


