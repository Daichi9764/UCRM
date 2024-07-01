using System.Xml;
using System.IO;
using System.Xml.Linq;
namespace KNXBoostDesktop;

public class MyNameCorrector
{
    private static XNamespace _globalKnxNamespace = string.Empty;
    private static string _projectFilesDirectory = string.Empty;
    
    public static async Task CorrectName(LoadingWindow loadingWindow)
{
    try
    {
        // Define the project path
        _projectFilesDirectory = Path.Combine(App.Fm?.ProjectFolderPath ?? string.Empty, @"knxproj_exported");

        // Define the XML namespace used in the KNX project file
        SetNamespaceFromXml(App.Fm?.ZeroXmlPath ?? string.Empty);

        // Load the XML file from the specified path
        XDocument knxDoc;
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Load XML file...");

        try
        {
            knxDoc = XDocument.Load(App.Fm?.ZeroXmlPath ?? string.Empty);
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException || ex is IOException || ex is UnauthorizedAccessException || ex is XmlException)
        {
            App.ConsoleAndLogWriteLine($"Error loading XML file: {ex.Message}");
            return;
        }
        
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Extracting infos...");

        // Create a formatter object for normalizing names
        Formatter formatter = new FormatterNormalize();

        // Extract location information from the KNX file
        var locationInfo = knxDoc.Descendants(_globalKnxNamespace + "Space")
            .Where(s => s.Attribute("Type")?.Value == "Room" || s.Attribute("Type")?.Value == "Corridor")
            .Select(room => new
            {
                RoomId = room.Attribute("Id")?.Value, // peut être inutile
                RoomName = room.Attribute("Name")?.Value,
                FloorName = room.Ancestors(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Floor")?.Attribute("Name")?.Value,
                BuildingPartName = room.Ancestors(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "BuildingPart")?.Attribute("Name")?.Value,
                BuildingName = room.Ancestors(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "Building")?.Attribute("Name")?.Value,
                DistributionBoardName = room.Descendants(_globalKnxNamespace + "Space").FirstOrDefault(s => s.Attribute("Type")?.Value == "DistributionBoard")?.Attribute("Name")?.Value,
                DeviceRefs = room.Descendants(_globalKnxNamespace + "DeviceInstanceRef").Select(dir => dir.Attribute("RefId")?.Value)
            })
            .ToList();
        
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Infos extracted.");

        App.ConsoleAndLogWriteLine("Extracted Location Information:");

        // Display extracted location information
        /*int totalIterationLocation = (endPercent - startPercent)*5;
        int iterationLocation = 0;
        foreach (var loc in locationInfo)
        {
            string message = loc.DistributionBoardName != null ? $"Distribution Board Name: {loc.DistributionBoardName} " : string.Empty;
            message += $"Room ID: {loc.RoomId}, Room Name: {loc.RoomName}, Floor: {loc.FloorName}, Building Part: {loc.BuildingPartName}, Building: {loc.BuildingName}";
            App.ConsoleAndLogWriteLine(message);
            foreach (var deviceRef in loc.DeviceRefs)
            {
                App.ConsoleAndLogWriteLine($"  DeviceRef: {deviceRef}");
                iterationLocation++;
                progress.Report(startPercent + ((iterationLocation * (endPercent - startPercent)) / (totalIterationLocation * 3)));
            }
        }*/
        
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Extracting device references...");

        // Extract device instance references and their group object instance references from the KNX file
        var deviceRefsTemp1 = knxDoc.Descendants(_globalKnxNamespace + "DeviceInstance")
            .Select(di => new
            {
                Id = di.Attribute("Id")?.Value,
                Hardware2ProgramRefId = di.Attribute("Hardware2ProgramRefId")?.Value,
                ProductRefId = di.Attribute("ProductRefId")?.Value,
                GroupObjectInstanceRefs = di.Descendants(_globalKnxNamespace + "ComObjectInstanceRef")
                    .Where(cir => cir.Attribute("Links") != null)
                    .SelectMany(cir => (cir.Attribute("Links")?.Value.Split(' ') ?? Array.Empty<string>())
                        .Select((link, index) => new
                        {
                            GroupAddressRef = link,
                            DeviceInstanceId = di.Attribute("Id")?.Value,
                            ComObjectInstanceRefId = cir.Attribute("RefId")?.Value,
                            IsFirstLink = index == 0  // Mark if it's the first link
                        }))
            });
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Extracting device information... (This step can take some time)");
            var deviceRefs = deviceRefsTemp1
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
                ObjectType = di.Hardware2ProgramRefId != null && g.ComObjectInstanceRefId != null && g.IsFirstLink ?
                    GetObjectType(FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).HardwareFileName, FormatHardware2ProgramRefId(di.Hardware2ProgramRefId).MxxxxDirectory, g.ComObjectInstanceRefId) :
                    null
            }))
            .ToList();
            
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Device references and information extracted.");

        // Display extracted device instance references
        /*App.ConsoleAndLogWriteLine("Extracted Device Instance References:");
        foreach (var dr in deviceRefs)
        {
            App.ConsoleAndLogWriteLine($"Device Instance ID: {dr.DeviceInstanceId}, Product Ref ID: {dr.ProductRefId}, Is Device Rail Mounted? : {dr.IsDeviceRailMounted}, Group Address Ref: {dr.GroupAddressRef}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
        }*/
        

        // Group deviceRefs by GroupAddressRef
        var groupedDeviceRefs = deviceRefs.GroupBy(dr => dr.GroupAddressRef)
            .Select(g => new
            {
                GroupAddressRef = g.Key,
                Devices = g.ToList()
            })
            .ToList();

        // Display grouped device instance references
        /*int totalIterationGroupDeviceRef = groupedDeviceRefs.Count();
        int iterationGroupDeviceRef = 0;
        App.ConsoleAndLogWriteLine("Grouped Device Instance References:");
        foreach (var group in groupedDeviceRefs)
        {
            App.ConsoleAndLogWriteLine($"Group Address Ref: {group.GroupAddressRef}");
            foreach (var dr in group.Devices)
            {
                App.ConsoleAndLogWriteLine($"  Device Instance ID: {dr.DeviceInstanceId}, Product Ref ID: {dr.ProductRefId}, Is Device Rail Mounted? : {dr.IsDeviceRailMounted}, HardwareFileName: {dr.HardwareFileName}, ComObjectInstanceRefId: {dr.ComObjectInstanceRefId}, ObjectType: {dr.ObjectType}");
            }
            iterationGroupDeviceRef++;
            progress.Report(startPercent + ((iterationGroupDeviceRef * (endPercent - startPercent)) / (totalIterationGroupDeviceRef * 3)));
        }*/

        // Construct the new name of the group address by iterating through each group of device references
        loadingWindow.MarkActivityComplete();
        loadingWindow.LogActivity($"Constructing the new group addresses...");
        
        int totalIteration = groupedDeviceRefs.Count();
        int iteration = 0;
        foreach (var gdr in groupedDeviceRefs)
        {
            string nameObjectType;
            string nameFunction = string.Empty;

            // Get the first rail-mounted device reference, if any
            var deviceRailMounted = gdr.Devices.FirstOrDefault(dr => dr.IsDeviceRailMounted);
            // Get the first device reference with a non-empty ObjectType, if any
            var deviceRefObjectType = gdr.Devices.FirstOrDefault(dr => !string.IsNullOrEmpty(dr.ObjectType));

            // Determine the nameObjectType based on the available device references
            if (deviceRailMounted != null && !string.IsNullOrEmpty(deviceRailMounted.ObjectType))
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
                nameObjectType = $"Type";
                App.ConsoleAndLogWriteLine($"No Object Type found for {gdr.Devices.FirstOrDefault()?.GroupAddressRef}");
            }

            // Get the first non-rail-mounted device reference, if any
            var deviceNotRailMounted = gdr.Devices.FirstOrDefault(dr => !dr.IsDeviceRailMounted);
            if (deviceNotRailMounted != null)
            {
                // Get the location information for the device reference
                var location = locationInfo.FirstOrDefault(loc => loc.DeviceRefs.Contains(deviceNotRailMounted.DeviceInstanceId));

                string nameLocation;
                if (location != null)
                {
                    string buildingName = !string.IsNullOrEmpty(location.BuildingName) ? formatter.Format(location.BuildingName ?? string.Empty) : string.Empty;
                    string buildingPartName = !string.IsNullOrEmpty(location.BuildingPartName) ? formatter.Format(location.BuildingPartName ?? string.Empty) : string.Empty;
                    string floorName = !string.IsNullOrEmpty(location.FloorName) ? formatter.Format(location.FloorName ?? string.Empty) : string.Empty;
                    string roomName = !string.IsNullOrEmpty(location.RoomName) ? formatter.Format(location.RoomName ?? string.Empty) : string.Empty;
                    string distributionBoardName = !string.IsNullOrEmpty(location.DistributionBoardName) ? formatter.Format(location.DistributionBoardName ?? string.Empty) : string.Empty;

                    nameLocation = !string.IsNullOrEmpty(buildingName) ? $"{buildingName}" : string.Empty;
                    nameLocation += !string.IsNullOrEmpty(buildingPartName) ? $"_{buildingPartName}" : string.Empty;
                    nameLocation += !string.IsNullOrEmpty(floorName) ? $"_{floorName}" : string.Empty;
                    nameLocation += !string.IsNullOrEmpty(roomName) ? $"_{roomName}" : string.Empty;
                    nameLocation += !string.IsNullOrEmpty(distributionBoardName) ? $"_{distributionBoardName}" : string.Empty;
                }
                else
                {
                    // Default nameLocation if no valid location is found
                    nameLocation = "Building_BuildingPart_Floor_Room_DistributionBoard";
                }

                // Assign the nameLocation as nameFunction
                nameFunction = nameLocation;
            }
            else
            {
                // Default nameFunction if no valid device reference is found
                nameFunction = "Building_BuildingPart_Floor_Room_DistributionBoard";
            }

            // Combine the formatted ObjectType and Function names
            string newName = $"{nameObjectType}_{nameFunction}";

            // Log the new group address name
            App.ConsoleAndLogWriteLine($"New Group Address Name: {newName}");
        }
    }
    catch (Exception ex)
    {
        App.ConsoleAndLogWriteLine($"An error occurred: {ex.Message}");
    }
}


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
            else
            {
                App.ConsoleAndLogWriteLine($"Opening file: {filePath}");

                // Load the XML file
                XDocument hardwareDoc = XDocument.Load(filePath);
                
                // Find the ComObject element with the matching Id
                var comObjectRefElement = hardwareDoc.Descendants(_globalKnxNamespace + "ComObjectRef")
                    .FirstOrDefault(co => co.Attribute("Id")?.Value.EndsWith(comObjectInstanceRefId) == true);
                
                if (comObjectRefElement == null)
                {
                    App.ConsoleAndLogWriteLine($"ComObjectRef with Id ending in: {comObjectInstanceRefId} not found in file: {filePath}");
                    return string.Empty;
                }
                else
                {
                    App.ConsoleAndLogWriteLine($"Found ComObjectRef with Id ending in: {comObjectInstanceRefId}");
                    var readFlag = comObjectRefElement.Attribute("ReadFlag")?.Value;
                    var writeFlag = comObjectRefElement.Attribute("WriteFlag")?.Value;

                   // Return the appropriate string based on the flags
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
                        else
                        {
                            App.ConsoleAndLogWriteLine($"Found ComObject with Id ending in: {comObjectInstanceRefIdCut}");
                            
                            // ??= is used to assert the expression if the variable is null
                            readFlag ??= comObjectElement.Attribute("ReadFlag")?.Value;
                            writeFlag ??= comObjectElement.Attribute("WriteFlag")?.Value;
                        }
                    }
                    
                    App.ConsoleAndLogWriteLine($"ReadFlag: {readFlag}, WriteFlag: {writeFlag}");
                    
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
        // Construct the full path to the Mxxxx directory
        string mxxxxDirectoryPath = Path.Combine(_projectFilesDirectory, mxxxDirectory);
        
        // Construct the full path to the Hardware.xml file
        string hardwareFilePath = Path.Combine(mxxxxDirectoryPath, "Hardware.xml");
        if (!Directory.Exists(mxxxxDirectoryPath))
        { 
            App.ConsoleAndLogWriteLine($"{mxxxDirectory} not found in directory: {mxxxxDirectoryPath}");
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

            // Find the Product element with the matching Id
            var productElement = hardwareDoc.Descendants(_globalKnxNamespace + "Product")
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
                _globalKnxNamespace = xmlns;
            }
        }
    }

}
