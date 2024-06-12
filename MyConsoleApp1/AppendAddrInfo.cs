using System;
using System.Linq;
using System.Xml.Linq;

class AppendAddrInfo
{
    public AppendAddrInfo()
    {
        XNamespace ns = "http://knx.org/xml/ga-export/01";
        
        // Load the XML file
        XDocument xmlDoc = XDocument.Load("xml/xml1.xml");

        // Analyze and potentially correct the XML content
        var groupAddresses = xmlDoc.Descendants(ns + "GroupAddress");

        foreach (var address in groupAddresses)
        {
            var nameAttr = address.Attribute("Name");
            var addressAttr = address.Attribute("Address");
            if (nameAttr != null && addressAttr != null)
            {
                string[] addressParts = addressAttr.Value.Split('/');
                if (addressParts.Length == 3)
                {
                    string principal = addressParts[0];
                    string median = addressParts[1];
                    string participant = addressParts[2];
                    string newName = $"{nameAttr.Value}-{principal}-{median}-{participant}";

                    Console.WriteLine($"Original Name: {nameAttr.Value}");
                    Console.WriteLine($"New Name: {newName}");

                    // Update the name attribute
                    nameAttr.Value = newName;
                }
                else
                {
                    Console.WriteLine($"Invalid address format: {addressAttr.Value}");
                }
            }
        }

        // Save the corrected XML document
        xmlDoc.Save("xml/xml1a.xml");
    }
}
