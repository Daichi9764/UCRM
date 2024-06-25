using System.Xml;
using System.IO;
using System.Xml.Linq;
namespace KNXBoostDesktop;

public class MyNameCorrector
{
    public static void CorrectName()
    {
        try
        {
            // Define the XML namespace used in the KNX project file
            XNamespace knxNs = $"http://knx.org/xml/project/23";

            // Load the XML file from the specified path
            XDocument knxDoc;
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

            App.ConsoleAndLogWriteLine("Extracted Location Information:");
            // Display extracted location information
            foreach (var loc in locationInfo)
            {
                App.ConsoleAndLogWriteLine($"Room ID: {loc.RoomId}, Room Name: {loc.RoomName}, Floor: {loc.FloorName}, Building Part: {loc.BuildingPartName}, Building: {loc.BuildingName}");
                foreach (var deviceRef in loc.DeviceRefs)
                {
                    App.ConsoleAndLogWriteLine($"  DeviceRef: {deviceRef}");
                }
            }

            // Extract device instance references from the KNX file their group object instance references from the KNX file
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
                            ComObjectInstanceRefId = cir.Attribute("RefId")?.Value.IndexOf('_') >= 0 ?
                                cir.Attribute("RefId")?.Value.Substring(0, cir.Attribute("RefId")?.Value.IndexOf('_') ?? 0) :
                                cir.Attribute("RefId")?.Value
                        })
                })
                .SelectMany(di => di.GroupObjectInstanceRefs.Select(g => new
                {
                    di.Id,
                    HardwareFileName = di.Hardware2ProgramRefId != null ? 
                       FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).ModifiedHardwareFileName : 
                       null,
                    MxxxxDirectory = di.Hardware2ProgramRefId != null ? 
                        FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory : 
                        null,
                    g.GroupAddressRef,
                    g.DeviceInstanceId,
                    g.ComObjectInstanceRefId,             
                    ObjectType = di.Hardware2ProgramRefId != null && g.ComObjectInstanceRefId != null ?
                        GetObjectType(FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).ModifiedHardwareFileName, FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory, g.ComObjectInstanceRefId) : 
                        null
                }))
                .ToList();

            // Display extracted device instance references
            App.ConsoleAndLogWriteLine("Extracted Device Instance References:");
            foreach (var dr in deviceRefs)
            {
                App.ConsoleAndLogWriteLine($"Device Instance ID: {dr.DeviceInstanceId}, Group Address Ref: {dr.GroupAddressRef}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
            }

            // Dictionary to store whether room name has been appended to each group address
            var appendedRoomName = new Dictionary<string, bool>();

            // Update Group Addresses with Room Names
            foreach (var deviceRef in deviceRefs)
            {
                var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceRef.DeviceInstanceId));
                if (location != null)
                {
                    App.ConsoleAndLogWriteLine($"Matching Device Instance ID: {deviceRef.DeviceInstanceId}");
                    var groupAddressElement = knxDoc.Descendants(knxNs + "GroupAddress")
                        .FirstOrDefault(ga => ga.Attribute("Id")?.Value.EndsWith(deviceRef.GroupAddressRef ?? string.Empty) == true);

                    if (groupAddressElement != null)
                    {
                        App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                        var nameAttr = groupAddressElement.Attribute("Name");
                        if (nameAttr != null)
                        {
                            // Append room name to group address name only if it hasn't been appended before
                            if (!appendedRoomName.ContainsKey(deviceRef.GroupAddressRef ?? string.Empty))
                            {
                                string newName = $"{nameAttr.Value}";

                                // Append ObjectType information
                                newName += $"_{formatter.Format(deviceRef.ObjectType ?? string.Empty)}";

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

                                App.ConsoleAndLogWriteLine($"Original Name: {nameAttr.Value}");
                                App.ConsoleAndLogWriteLine($"New Name: {newName}");
                                nameAttr.Value = newName;
                                appendedRoomName[deviceRef.GroupAddressRef ?? string.Empty] = true;
                            }
                        }
                    }
                    else
                    {
                        App.ConsoleAndLogWriteLine($"No GroupAddress element found for GroupAddressRef: {deviceRef.GroupAddressRef}");
                    }
                }
                else
                {
                    App.ConsoleAndLogWriteLine($"No location found for DeviceInstanceId: {deviceRef.DeviceInstanceId}");
                }
            }

            // Save the updated XML file
            try
            {
                knxDoc.Save($@"{App.Fm?.ProjectFolderPath}/0_updated.xml"); // Change the path as needed
                App.ConsoleAndLogWriteLine("Updated XML file saved as 'OutputFiles/0_updated.xml'");
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
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

     private static string GetObjectType(string hardwareFileName, string mxxxxDirectory, string comObjectInstanceRefId)
    {
        string projectFilesDirectory = App.Fm?.ProjectFolderPath ?? string.Empty; // Path to the project files directory

        XNamespace knxNs = "http://knx.org/xml/project/23";

        // Construct the full path to the Mxxxx directory
        string mxxxxDirectoryPath = Path.Combine(projectFilesDirectory, mxxxxDirectory);

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
            else
            {
                App.ConsoleAndLogWriteLine($"Opening file: {filePath}");

                // Load the XML file
                XDocument hardwareDoc = XDocument.Load(filePath);

                // Find the ComObject element with the matching Id
                var comObjectElement = hardwareDoc.Descendants(knxNs + "ComObject")
                    .FirstOrDefault(co => co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefId) == true);

                if (comObjectElement == null)
                {
                    App.ConsoleAndLogWriteLine($"ComObject with Id ending in: {comObjectInstanceRefId} not found in file: {filePath}");
                    return string.Empty;
                }
                else
                {
                    App.ConsoleAndLogWriteLine($"Found ComObject with Id ending in: {comObjectInstanceRefId}");
                    var readFlag = comObjectElement.Attribute("ReadFlag")?.Value;
                    var writeFlag = comObjectElement.Attribute("WriteFlag")?.Value;

                    App.ConsoleAndLogWriteLine($"ReadFlag: {readFlag}, WriteFlag: {writeFlag}");

                    // Return the appropriate string based on the flags
                    if (readFlag == "Enabled" && writeFlag == "Disabled")
                    {
                        return "Ie";
                    }
                    else if (writeFlag == "Enabled" && readFlag == "Disabled")
                    {
                        return "Cmd";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
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
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
            return string.Empty;
        }
    }


  

    private static (string ModifiedHardwareFileName, string MxxxxDirectory) FormatHardware2ProgramRefId(string hardware2ProgramRefId)
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

            var modifiedHardwareFileName = $"{mxxxxDirectory}_A-{afterHp}.xml";
            return (modifiedHardwareFileName, mxxxxDirectory);
        }
        catch (ArgumentNullException ex)
        {
            App.ConsoleAndLogWriteLine($"Error: Argument null exception occurred. {ex.Message}");
            return (string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
            return (string.Empty, string.Empty);
        }
    }

}
