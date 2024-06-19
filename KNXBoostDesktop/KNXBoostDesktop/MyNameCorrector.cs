using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace KNXBoostDesktop;

class MyNameCorrector
{
    public static void CorrectName()
    {
        // Namespace
        XNamespace knxNs = "http://knx.org/xml/project/23";

        // Load the XML file
        XDocument knxDoc = XDocument.Load(App.Fm.ZeroXmlPath);

        Formatter formatter = new FormatterNormalize();

        // Extract location information from the KNX file
        var locationInfo = knxDoc.Descendants(knxNs + "Space")
            .Where(s => s.Attribute("Type")?.Value == "Room")
            .Select(room => new
            {
                RoomId = room.Attribute("Id")?.Value,
                RoomName = room.Attribute("Name")?.Value,
                FloorName = room.Ancestors(knxNs + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Floor")?.Attribute("Name")?.Value,
                BuildingPartName = room.Ancestors(knxNs + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "BuildingPart")?.Attribute("Name")?.Value,
                BuildingName = room.Ancestors(knxNs + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Building")?.Attribute("Name")?.Value,
                DeviceRefs = room.Descendants(knxNs + "DeviceInstanceRef").Select(dir => dir.Attribute("RefId")?.Value)
            })
            .ToList();

        Console.WriteLine("Extracted Location Information:");
        foreach (var loc in locationInfo)
        {
            Console.WriteLine($"Room ID: {loc.RoomId}, Room Name: {loc.RoomName}, Floor: {loc.FloorName}, Building Part: {loc.BuildingPartName}, Building: {loc.BuildingName}");
            foreach (var deviceRef in loc.DeviceRefs)
            {
                Console.WriteLine($"  DeviceRef: {deviceRef}");
            }
        }

       // Extract device instance references from the KNX file
        var deviceRefs = knxDoc.Descendants(knxNs + "DeviceInstance")
            .Select(di => new
            {
                Id = di.Attribute("Id")?.Value,
                Hardware2ProgramRefId = di.Attribute("Hardware2ProgramRefId")?.Value,
                GroupObjectInstanceRefs = di.Descendants(knxNs + "ComObjectInstanceRef")
                    .Where(cir => cir.Attribute("Links") != null)
                    .Select(cir => new
                    {
                        GroupAddressRef = cir.Attribute("Links")?.Value,
                        DeviceInstanceId = di.Attribute("Id")?.Value,
                        ComObjectInstanceRefId = cir.Attribute("RefId")?.Value?.IndexOf('_') >= 0 ?
                            cir.Attribute("RefId")?.Value?.Substring(0, cir.Attribute("RefId")?.Value?.IndexOf('_') ?? 0) :
                            cir.Attribute("RefId")?.Value
                    })
            })
            .SelectMany(di => di.GroupObjectInstanceRefs.Select(g => new
            {
                di.Id,
                HardwareFileName = di.Hardware2ProgramRefId != null ? 
                   FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).ModifiedHardwareFileName : 
                   null,
                MxxxxPart = di.Hardware2ProgramRefId != null ? 
                    FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxPart : 
                null,
                g.GroupAddressRef,
                g.DeviceInstanceId,
                g.ComObjectInstanceRefId,             
                ComObjectType = di.Hardware2ProgramRefId != null && g.ComObjectInstanceRefId != null ?
                    ProcessHardwareFile(FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).ModifiedHardwareFileName, FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxPart, g.ComObjectInstanceRefId) : 
                    null
            }))
            .ToList();

        Console.WriteLine("Extracted Device Instance References:");
        foreach (var dr in deviceRefs)
        {
            Console.WriteLine($"Device Instance ID: {dr.DeviceInstanceId}, Group Address Ref: {dr.GroupAddressRef}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ComObjectType: {dr.ComObjectType}");
        }


        // Dictionary to store whether room name has been appended to each group address
        var appendedRoomName = new Dictionary<string, bool>();

        // Update Group Addresses with Room Names
        foreach (var deviceRef in deviceRefs)
        {
            var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceRef.DeviceInstanceId));
            if (location != null)
            {
                Console.WriteLine($"Matching Device Instance ID: {deviceRef.DeviceInstanceId}");
                var groupAddressElement = knxDoc.Descendants(knxNs + "GroupAddress")
                    .FirstOrDefault(ga => ga.Attribute("Id")?.Value?.EndsWith(deviceRef.GroupAddressRef ?? string.Empty) == true);

                if (groupAddressElement != null)
                {
                    Console.WriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                    var nameAttr = groupAddressElement.Attribute("Name");
                    if (nameAttr != null )
                    {
                        // Append room name to group address name only if it hasn't been appended before
                        if (!appendedRoomName.ContainsKey(deviceRef.GroupAddressRef ?? string.Empty))
                        {
                            string newName = $"{nameAttr.Value}";

                            newName += $"_{formatter.Format(deviceRef.ComObjectType ?? string.Empty)}";

                            // Traverse up the hierarchy to find GroupRange and append its name
                            var groupRangeElement = groupAddressElement.Ancestors(knxNs + "GroupRange").FirstOrDefault();
                            if (groupRangeElement != null)
                            {
                                // Check if there is a GroupRange ancestor of the GroupRange
                                var ancestorGroupRange = groupRangeElement.Ancestors(knxNs + "GroupRange").FirstOrDefault();
                                if (ancestorGroupRange != null)
                                {
                                    newName += $"_{formatter.Format(ancestorGroupRange.Attribute("Name")?.Value ?? string.Empty)}";
                                }

                                newName += $"_{formatter.Format(groupRangeElement.Attribute("Name")?.Value ?? string.Empty)}";
                            }

                            // Append building-related information
                            newName += $"_{formatter.Format(location.BuildingName ?? string.Empty)}_{formatter.Format(location.BuildingPartName ?? string.Empty)}_{formatter.Format(location.FloorName ?? string.Empty)}_{formatter.Format(location.RoomName ?? string.Empty)}";

                            Console.WriteLine($"Original Name: {nameAttr.Value}");
                            Console.WriteLine($"New Name: {newName}");
                            nameAttr.Value = newName;
                            appendedRoomName[deviceRef.GroupAddressRef ?? string.Empty] = true;
                        }



                    }
                }
                else
                {
                    Console.WriteLine($"No GroupAddress element found for GroupAddressRef: {deviceRef.GroupAddressRef}");
                }
            }
            else
            {
                Console.WriteLine($"No location found for DeviceInstanceId: {deviceRef.DeviceInstanceId}");
            }
        }

        // Save the updated XML file
        knxDoc.Save(@"OutputFiles/0_updated.xml");
        Console.WriteLine("Updated XML file saved as 'OutputFiles/0_updated.xml'");
    }

     private static string ProcessHardwareFile(string hardwareFileName, string mxxxxPart, string comObjectInstanceRefId)
    {
        string projectFilesDirectory = App.Fm.KnxprojExportFolderPath; // Chemin vers le répertoire des fichiers

        XNamespace knxNs = "http://knx.org/xml/project/23";

        // Construire le chemin complet du dossier Mxxxx
        string mxxxxDirectory = Path.Combine(projectFilesDirectory, mxxxxPart);

        // Vérifier si le dossier Mxxxx existe
        if (!Directory.Exists(mxxxxDirectory))
        {
            Console.WriteLine($"Directory not found: {mxxxxDirectory}");
            return string.Empty;
        }

        // Construire le chemin complet du fichier à traiter
        string filePath = Path.Combine(mxxxxDirectory, hardwareFileName);

        // Vérifier si le fichier existe
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return string.Empty;
        }
        else
        {
            Console.WriteLine($"Opening file: {filePath}");

            // Load the XML file
            XDocument hardwareDoc = XDocument.Load(filePath);

            // Find the ComObject element with the matching Id
            var comObjectElement = hardwareDoc.Descendants(knxNs + "ComObject")
        .FirstOrDefault(co => co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefId) == true);

            if (comObjectElement == null)
            {  
                Console.WriteLine($"ComObject with Id ending in: {comObjectInstanceRefId} not found in file: {filePath}");
                return string.Empty ;
            }
            else
            {
                Console.WriteLine($"Found ComObject with Id ending in: {comObjectInstanceRefId}");
                var readFlag = comObjectElement.Attribute("ReadFlag")?.Value;
                var writeFlag = comObjectElement.Attribute("WriteFlag")?.Value;

                Console.WriteLine($"ReadFlag: {readFlag}, WriteFlag: {writeFlag}");

                // Return the appropriate string based on the flags
                if (readFlag == "Enabled" && writeFlag == "Disabled")
                {
                    return "Ie";
                }
                else if (writeFlag == "Enabled" && readFlag == "Disabled")
                {
                    return "Cmd";
                }
                else{
                    return string.Empty;
                }
            }
        }
    }

  

     private static (string ModifiedHardwareFileName, string MxxxxPart) FormatHardware2ProgramRefId(string hardware2ProgramRefId)
    {
        // Extract the part before "HP" and the part after "HP"
        var parts = hardware2ProgramRefId.Split(new[] { "HP" }, StringSplitOptions.None);
        if (parts.Length < 2) return (string.Empty,string.Empty); // If "HP" is not found, return null null

        var beforeHP = parts[0].TrimEnd('-');
        var afterHP = parts[1].TrimStart('-'); // Remove any leading hyphen after "HP"

        // Extract "M-XXXX" from the part before "HP"
        var mxxxxPart = beforeHP.Split('_').FirstOrDefault(part => part.StartsWith("M-"));

        if (string.IsNullOrEmpty(mxxxxPart)) return (string.Empty,string.Empty); // If "M-XXXX" is not found, return null null
        
        var modifiedHardwareFileName = $"{mxxxxPart}_A-{afterHP}.xml";
        return (modifiedHardwareFileName, mxxxxPart);
    }
}
