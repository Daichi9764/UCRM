
using System;
using System.Linq;
using System.Xml.Linq;

class DeleteWhitespace
{
        public DeleteWhitespace()
    {
        XNamespace ns = "http://knx.org/xml/ga-export/01";
        
        // Load the XML file
        XDocument xmlDoc = XDocument.Load("xml/xml1.xml");

        // Analyze and potentially correct the XML content
        var groupAddresses = xmlDoc.Descendants(ns + "GroupAddress");

        foreach (var address in groupAddresses)
        {
            var nameAttr = address.Attribute("Name");
            if (nameAttr != null)
            {
                Console.WriteLine($"Original Name: {nameAttr.Value}");
                nameAttr.Value = nameAttr.Value.Replace(" ", ""); // Example correction: remove spaces
                Console.WriteLine($"Corrected Name: {nameAttr.Value}");
            }
        }

        // Save the corrected XML document
        xmlDoc.Save("xml/xml1w.xml");
    }

}
