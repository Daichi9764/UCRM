using System;
using System.Linq;
using System.Xml.Linq;

class ExportUpdatedNameAddresses
{
    public static void Export()
    {
        try
        {
            // Load the updated XML document
            XDocument knxDoc = XDocument.Load("OutputFiles/0_updated.xml");

            // Namespace for GroupAddress-Export
            XNamespace knxExportNs = "http://knx.org/xml/ga-export/01";

            // Root element for UpdatedGroupAddresses.xml
            XElement root = new XElement(knxExportNs + "GroupAddress-Export",
                new XAttribute("xmlns", knxExportNs.NamespaceName));

            // Extract GroupAddress information
            var groupAddresses = knxDoc.Descendants()
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

            // Group by ancestor GroupRange names and build the XML structure
            foreach (var ga in groupAddresses)
            {
                XElement currentParent = root;

                // Add ancestor GroupRanges
                foreach (var ancestor in ga.AncestorGroupRangeNames)
                {
                    if (ancestor.Name == null) continue; // Skip if ancestor name is null

                    XElement groupRange = currentParent.Elements(knxExportNs + "GroupRange")
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
                if (ga.Name != null && ga.Address != null && ga.DPTs != null)
                {
                    string knxAddress = DecimalToKnx3Level(int.Parse(ga.Address));
                    XElement groupAddress = new XElement(knxExportNs + "GroupAddress",
                        new XAttribute("Name", ga.Name),
                        new XAttribute("Address", knxAddress),
                        new XAttribute("DPTs", ga.DPTs));

                    currentParent.Add(groupAddress);
                }
            }

            // Save to UpdatedGroupAddresses.xml
            XDocument updatedExportDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                root
            );

            updatedExportDoc.Save("OutputFiles/UpdatedGroupAddresses.xml");

            Console.WriteLine("UpdatedGroupAddresses.xml generated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Converter from decimal to KNX 3-level address
    public static string DecimalToKnx3Level(int decimalAddress)
    {
        int mainGroup = decimalAddress / 2048;
        int middleGroup = (decimalAddress % 2048) / 256;
        int subGroup = decimalAddress % 256;

        return $"{mainGroup}/{middleGroup}/{subGroup}";
    }
}
