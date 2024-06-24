using System.Xml;
using System.IO;
using System.Xml.Linq;
namespace KNXBoostDesktop;

public class MyNameCorrector
{
    private static XNamespace GlobalKnxNamespace = string.Empty;
    
    public static void CorrectName()
    {
        try
        {
            // Define the XML namespace used in the KNX project file
            SetNamespaceFromXml(App.Fm?.ZeroXmlPath ?? string.Empty);
            
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
            var locationInfo = knxDoc.Descendants(GlobalKnxNamespace + "Space")
                .Where(s => s.Attribute("Type")?.Value == "Room")
                .Select(room => new
                {
                    RoomId = room.Attribute("Id")?.Value,
                    RoomName = room.Attribute("Name")?.Value,
                    FloorName = room.Ancestors(GlobalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Floor")?.Attribute("Name")?.Value,
                    BuildingPartName = room.Ancestors(GlobalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "BuildingPart")?.Attribute("Name")?.Value,
                    BuildingName = room.Ancestors(GlobalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Building")?.Attribute("Name")?.Value,
                    DeviceRefs = room.Descendants(GlobalKnxNamespace + "DeviceInstanceRef").Select(dir => dir.Attribute("RefId")?.Value)
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

            // Extract device instance references and  their group object instance references from the KNX file
            var deviceRefs = knxDoc.Descendants(GlobalKnxNamespace + "DeviceInstance")
                .Select(di => new
                {
                    Id = di.Attribute("Id")?.Value,
                    Hardware2ProgramRefId = di.Attribute("Hardware2ProgramRefId")?.Value,
                    ProductRefId = di.Attribute("ProductRefId")?.Value,
                    GroupObjectInstanceRefs = di.Descendants(GlobalKnxNamespace + "ComObjectInstanceRef")
                        .Where(cir => cir.Attribute("Links") != null)
                        .SelectMany(cir => (cir.Attribute("Links")?.Value.Split(' ') ?? Array.Empty<string>())
                            .Select(link => new
                            {
                                GroupAddressRef = link,
                                DeviceInstanceId = di.Attribute("Id")?.Value,
                                ComObjectInstanceRefId = cir.Attribute("RefId")?.Value.IndexOf('_') >= 0 ?
                                    cir.Attribute("RefId")?.Value.Substring(0, cir.Attribute("RefId")?.Value.IndexOf('_') ?? 0) :
                                    cir.Attribute("RefId")?.Value
                            }))
                })
                .SelectMany(di => di.GroupObjectInstanceRefs.Select(g => new
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
                    ObjectType = di.Hardware2ProgramRefId != null && g.ComObjectInstanceRefId != null ?
                        GetObjectType(FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).HardwareFileName, FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory, g.ComObjectInstanceRefId) : 
                        null
                }))
                .ToList();


            // Display extracted device instance references
            App.ConsoleAndLogWriteLine("Extracted Device Instance References:");
            foreach (var dr in deviceRefs)
            {
                App.ConsoleAndLogWriteLine($"Device Instance ID: {dr.DeviceInstanceId}, Product Ref ID: {dr.ProductRefId}, Is Device Rail Mounted ? : {dr.IsDeviceRailMounted}, Group Address Ref: {dr.GroupAddressRef}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
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
                    var groupAddressElement = knxDoc.Descendants(GlobalKnxNamespace + "GroupAddress")
                        .FirstOrDefault(ga => ga.Attribute("Id")?.Value.EndsWith(deviceRef.GroupAddressRef) == true);

                    if (groupAddressElement != null)
                    {
                        App.ConsoleAndLogWriteLine($"Matching Group Address ID: {groupAddressElement.Attribute("Id")?.Value}");
                        var nameAttr = groupAddressElement.Attribute("Name");
                        if (nameAttr != null)
                        {
                            // Append room name to group address name only if it hasn't been appended before
                            if (!appendedRoomName.ContainsKey(deviceRef.GroupAddressRef))
                            {
                                string newName = $"{nameAttr.Value}";

                                // Append ObjectType information
                                newName += $"_{formatter.Format(deviceRef.ObjectType ?? string.Empty)}";

                                // Traverse up the hierarchy to find GroupRange and append its name
                                var groupRangeElement = groupAddressElement.Ancestors(GlobalKnxNamespace + "GroupRange").FirstOrDefault();
                                if (groupRangeElement != null)
                                {
                                    // Check if there is a GroupRange ancestor of the GroupRange
                                    var ancestorGroupRange = groupRangeElement.Ancestors(GlobalKnxNamespace + "GroupRange").FirstOrDefault();
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
                                appendedRoomName[deviceRef.GroupAddressRef] = true;
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
                knxDoc.Save(@"C:\Users\coust\Stage UCRM\Projets knx\0_updated.xml"); // Change the path as needed
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
        string projectFilesDirectory = App.Fm?.ExportedProjectPath ?? string.Empty; // Path to the project files directory

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
                var comObjectElement = hardwareDoc.Descendants(GlobalKnxNamespace + "ComObject")
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
            App.ConsoleAndLogWriteLine($"An unexpected error occurred: {ex.Message}");
            return (string.Empty, string.Empty);
        }
    }
    

    private static bool GetIsDeviceRailMounted(string productRefId, string mxxxDirectory)
    {
        // Path to the project files directory
        string projectFilesDirectory = App.Fm?.ExportedProjectPath ?? string.Empty;
        
        // Construct the full path to the Mxxxx directory
        string mxxxxDirectoryPath = Path.Combine(projectFilesDirectory, mxxxDirectory);
        
        // Construct the full path to the Hardware.xml file
        string hardwareFilePath = Path.Combine(mxxxxDirectoryPath, "Hardware.xml");
        
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
            
            // Find the Product element with the matching Id
            var productElement = hardwareDoc.Descendants(GlobalKnxNamespace + "Product")
                .FirstOrDefault(pe => pe.Attribute("Id")?.Value == productRefId);
            
            if (productElement == null)
            {
                App.ConsoleAndLogWriteLine($"Product with Id: {productRefId} not found in file: {hardwareFilePath}");
                return false; // Default to false if the product is not found
            }
            else
            {
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
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"Error reading Hardware.xml: {ex.Message}");
            return false; // Default to false in case of an error
        }
    }

    private static void SetNamespaceFromXml(string zeroXmlFilePath)
    {
        XmlDocument doc = new XmlDocument();
        
        // Load XML file
        doc.Load(zeroXmlFilePath);
        
        // Check the existence of the namespace in the root element
        XmlElement? root = doc.DocumentElement;
        if (root != null)
        {
            // Get the namespace
            XNamespace xmlns = root.GetAttribute("xmlns");
            if (xmlns!=string.Empty)
            {
                GlobalKnxNamespace = xmlns;
            }
        }
    }

}
