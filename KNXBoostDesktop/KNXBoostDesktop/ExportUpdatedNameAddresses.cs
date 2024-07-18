using System.Xml.Linq;
namespace KNXBoostDesktop;

static class ExportUpdatedNameAddresses
{
    /// <summary>
    /// Asynchronously exports group address information to an XML file based on the specified source and destination paths.
    /// Translates loading messages based on the application language settings. Builds an XML structure
    /// containing group address and group range information extracted from the source XML document.
    /// Saves the updated XML structure to the destination path.
    /// </summary>
    /// <param name="sourcePath">The path to the source XML document containing group address information.</param>
    /// <param name="destPath">The destination path to save the updated XML document.</param>
    public static async Task Export(String sourcePath, String destPath)
    {
        await Task.Run(() =>
        {
            try
            {
                string exportingAddresses, buildingXmlStructure, savingUpdatedFile;
                
                // Traduction des textes de la fenêtre de chargement
                switch (App.DisplayElements?.SettingsWindow?.AppLang)
                {
                    // Arabe
                    case "AR":
                        exportingAddresses = "تصدير العناوين الجديدة...";
                        buildingXmlStructure = "بناء هيكل XML...";
                        savingUpdatedFile = "حفظ الملف المحدث...";
                        break;

                    // Bulgare
                    case "BG":
                        exportingAddresses = "Експортиране на новите адреси...";
                        buildingXmlStructure = "Изграждане на XML структурата...";
                        savingUpdatedFile = "Запазване на актуализирания файл...";
                        break;

                    // Tchèque
                    case "CS":
                        exportingAddresses = "Export nových adres...";
                        buildingXmlStructure = "Vytváření struktury XML...";
                        savingUpdatedFile = "Ukládání aktualizovaného souboru...";
                        break;

                    // Danois
                    case "DA":
                        exportingAddresses = "Eksport af de nye adresser...";
                        buildingXmlStructure = "Opbygning af XML-strukturen...";
                        savingUpdatedFile = "Gemmer den opdaterede fil...";
                        break;

                    // Allemand
                    case "DE":
                        exportingAddresses = "Exportieren der neuen Adressen...";
                        buildingXmlStructure = "Erstellen der XML-Struktur...";
                        savingUpdatedFile = "Speichern der aktualisierten Datei...";
                        break;

                    // Grec
                    case "EL":
                        exportingAddresses = "Εξαγωγή των νέων διευθύνσεων...";
                        buildingXmlStructure = "Δημιουργία της δομής XML...";
                        savingUpdatedFile = "Αποθήκευση του ενημερωμένου αρχείου...";
                        break;

                    // Anglais
                    case "EN":
                        exportingAddresses = "Exporting the new addresses...";
                        buildingXmlStructure = "Building the XML structure...";
                        savingUpdatedFile = "Saving the updated file...";
                        break;

                    // Espagnol
                    case "ES":
                        exportingAddresses = "Exportando las nuevas direcciones...";
                        buildingXmlStructure = "Construyendo la estructura XML...";
                        savingUpdatedFile = "Guardando el archivo actualizado...";
                        break;

                    // Estonien
                    case "ET":
                        exportingAddresses = "Uute aadresside eksportimine...";
                        buildingXmlStructure = "XML-struktuuri loomine...";
                        savingUpdatedFile = "Uuendatud faili salvestamine...";
                        break;

                    // Finnois
                    case "FI":
                        exportingAddresses = "Uusien osoitteiden vieminen...";
                        buildingXmlStructure = "XML-rakenteen luominen...";
                        savingUpdatedFile = "Päivitetyn tiedoston tallentaminen...";
                        break;

                    // Hongrois
                    case "HU":
                        exportingAddresses = "Új címek exportálása...";
                        buildingXmlStructure = "XML-struktúra létrehozása...";
                        savingUpdatedFile = "A frissített fájl mentése...";
                        break;

                    // Indonésien
                    case "ID":
                        exportingAddresses = "Mengekspor alamat baru...";
                        buildingXmlStructure = "Membangun struktur XML...";
                        savingUpdatedFile = "Menyimpan file yang diperbarui...";
                        break;

                    // Italien
                    case "IT":
                        exportingAddresses = "Esportazione dei nuovi indirizzi...";
                        buildingXmlStructure = "Costruzione della struttura XML...";
                        savingUpdatedFile = "Salvataggio del file aggiornato...";
                        break;

                    // Japonais
                    case "JA":
                        exportingAddresses = "新しいアドレスをエクスポート中...";
                        buildingXmlStructure = "XML構造を構築中...";
                        savingUpdatedFile = "更新されたファイルを保存中...";
                        break;

                    // Coréen
                    case "KO":
                        exportingAddresses = "새 주소를 내보내는 중...";
                        buildingXmlStructure = "XML 구조를 만드는 중...";
                        savingUpdatedFile = "업데이트된 파일을 저장하는 중...";
                        break;

                    // Letton
                    case "LV":
                        exportingAddresses = "Eksportē jaunas adreses...";
                        buildingXmlStructure = "Izveido XML struktūru...";
                        savingUpdatedFile = "Saglabā atjaunināto failu...";
                        break;

                    // Lituanien
                    case "LT":
                        exportingAddresses = "Eksportuojami nauji adresai...";
                        buildingXmlStructure = "Kuriama XML struktūra...";
                        savingUpdatedFile = "Išsaugomas atnaujintas failas...";
                        break;

                    // Norvégien
                    case "NB":
                        exportingAddresses = "Eksporterer de nye adressene...";
                        buildingXmlStructure = "Bygger XML-strukturen...";
                        savingUpdatedFile = "Lagrer den oppdaterte filen...";
                        break;

                    // Néerlandais
                    case "NL":
                        exportingAddresses = "De nieuwe adressen exporteren...";
                        buildingXmlStructure = "De XML-structuur opbouwen...";
                        savingUpdatedFile = "Het bijgewerkte bestand opslaan...";
                        break;

                    // Polonais
                    case "PL":
                        exportingAddresses = "Eksportowanie nowych adresów...";
                        buildingXmlStructure = "Budowanie struktury XML...";
                        savingUpdatedFile = "Zapisywanie zaktualizowanego pliku...";
                        break;

                    // Portugais
                    case "PT":
                        exportingAddresses = "Exportando os novos endereços...";
                        buildingXmlStructure = "Construindo a estrutura XML...";
                        savingUpdatedFile = "Salvando o arquivo atualizado...";
                        break;

                    // Roumain
                    case "RO":
                        exportingAddresses = "Exportul noilor adrese...";
                        buildingXmlStructure = "Construirea structurii XML...";
                        savingUpdatedFile = "Salvarea fișierului actualizat...";
                        break;

                    // Russe
                    case "RU":
                        exportingAddresses = "Экспорт новых адресов...";
                        buildingXmlStructure = "Создание структуры XML...";
                        savingUpdatedFile = "Сохранение обновленного файла...";
                        break;

                    // Slovaque
                    case "SK":
                        exportingAddresses = "Export nových adries...";
                        buildingXmlStructure = "Vytváranie štruktúry XML...";
                        savingUpdatedFile = "Ukladanie aktualizovaného súboru...";
                        break;

                    // Slovène
                    case "SL":
                        exportingAddresses = "Izvoz novih naslovov...";
                        buildingXmlStructure = "Gradnja strukture XML...";
                        savingUpdatedFile = "Shranjevanje posodobljene datoteke...";
                        break;

                    // Suédois
                    case "SV":
                        exportingAddresses = "Exporterar de nya adresserna...";
                        buildingXmlStructure = "Bygger XML-strukturen...";
                        savingUpdatedFile = "Sparar den uppdaterade filen...";
                        break;

                    // Turc
                    case "TR":
                        exportingAddresses = "Yeni adresleri dışa aktarıyor...";
                        buildingXmlStructure = "XML yapısı oluşturuluyor...";
                        savingUpdatedFile = "Güncellenmiş dosya kaydediliyor...";
                        break;

                    // Ukrainien
                    case "UK":
                        exportingAddresses = "Експорт нових адрес...";
                        buildingXmlStructure = "Створення структури XML...";
                        savingUpdatedFile = "Збереження оновленого файлу...";
                        break;

                    // Chinois simplifié
                    case "ZH":
                        exportingAddresses = "导出新地址...";
                        buildingXmlStructure = "构建XML结构...";
                        savingUpdatedFile = "保存更新的文件...";
                        break;

                    // Langue par défaut (français)
                    default:
                        exportingAddresses = "Export des nouvelles adresses...";
                        buildingXmlStructure = "Construction de la structure du fichier XML...";
                        savingUpdatedFile = "Enregistrement du fichier mis à jour...";
                        break;
                }

                
                App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
                App.DisplayElements?.LoadingWindow?.LogActivity(exportingAddresses);

                // Load the updated XML document
                var knxDoc = App.Fm?.LoadXmlDocument(sourcePath);

                // Namespace for GroupAddress-Export
                XNamespace knxExportNs = "http://knx.org/xml/ga-export/01";

                // Root element for UpdatedGroupAddresses.xml
                var root = new XElement(knxExportNs + "GroupAddress-Export",
                    new XAttribute("xmlns", knxExportNs.NamespaceName));

                // Extract GroupAddress information
                var groupAddresses = knxDoc?.Descendants()
                    .Where(e => e.Name.LocalName == "GroupAddress")
                    .Select(ga => new
                    {
                        Name = ga.Attribute("Name")?.Value,
                        Address = ga.Attribute("Address")?.Value,
                        DPTs = ga.Attribute("DatapointType")?.Value,
                        AncestorGroupRangeNames = ga.Ancestors()
                            .Where(a => a.Name.LocalName == "GroupRange")
                            .Select(a => new
                            {
                                Name = a.Attribute("Name")?.Value,
                                RangeStart = a.Attribute("RangeStart")?.Value,
                                RangeEnd = a.Attribute("RangeEnd")?.Value
                            })
                            .Reverse() // Reverse to maintain the hierarchical order
                            .ToList()
                    });

                App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
                App.DisplayElements?.LoadingWindow?.LogActivity(buildingXmlStructure);

                // Group by ancestor GroupRange names and build the XML structure
                if (groupAddresses != null)
                    foreach (var ga in groupAddresses)
                    {
                        var currentParent = root;

                        // Add ancestor GroupRanges
                        foreach (var ancestor in ga.AncestorGroupRangeNames)
                        {
                            if (ancestor.Name == null) continue; // Skip if ancestor name is null

                            var groupRange = currentParent.Elements(knxExportNs + "GroupRange")
                                .FirstOrDefault(gr => gr.Attribute("Name")?.Value == ancestor.Name);

                            if (groupRange == null)
                            {
                                groupRange = new XElement(knxExportNs + "GroupRange",
                                    new XAttribute("Name", ancestor.Name));

                                if (ancestor.RangeStart != null)
                                    groupRange.Add(new XAttribute("RangeStart", ancestor.RangeStart));

                                if (ancestor.RangeEnd != null)
                                    groupRange.Add(new XAttribute("RangeEnd", ancestor.RangeEnd));

                                currentParent.Add(groupRange);
                            }

                            currentParent = groupRange;
                        }

                        // Add GroupAddress under the last GroupRange
                        if (ga.Name != null && ga.Address != null)
                        {
                            var knxAddress = DecimalToKnx3Level(int.Parse(ga.Address));
                            if (ga.DPTs != null)
                            {
                                var groupAddress = new XElement(knxExportNs + "GroupAddress",
                                    new XAttribute("Name", ga.Name),
                                    new XAttribute("Address", knxAddress),
                                    new XAttribute("DPTs", ga.DPTs));

                                currentParent.Add(groupAddress);
                            }
                            else
                            {
                                var groupAddress = new XElement(knxExportNs + "GroupAddress",
                                    new XAttribute("Name", ga.Name),
                                    new XAttribute("Address", knxAddress));

                                currentParent.Add(groupAddress);
                            }
                        }
                    }

                App.DisplayElements?.LoadingWindow?.MarkActivityComplete();
                App.DisplayElements?.LoadingWindow?.LogActivity(savingUpdatedFile);

                // Save to UpdatedGroupAddresses.xml
                var updatedExportDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    root
                );

                App.Fm?.SaveXml(updatedExportDoc,destPath);
               
                App.ConsoleAndLogWriteLine("UpdatedGroupAddresses.xml generated successfully.");
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"Error: {ex.Message}");
            }
        });
        
    }

    /// <summary>
    /// Converts a decimal address to KNX 3-level address format.
    /// KNX addresses are structured as Main Group/Middle Group/Sub Group.
    /// </summary>
    /// <param name="decimalAddress">The decimal address to convert.</param>
    /// <returns>The KNX 3-level address string in the format "MainGroup/MiddleGroup/SubGroup".</returns>
    private static string DecimalToKnx3Level(int decimalAddress)
    {
        var mainGroup = decimalAddress / 2048;
        var middleGroup = (decimalAddress % 2048) / 256;
        var subGroup = decimalAddress % 256;

        return $"{mainGroup}/{middleGroup}/{subGroup}";
    }
}
