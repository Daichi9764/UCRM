using System;
using System.Linq;
using System.Xml.Linq;

class AppendGroupInfo
{
    public AppendGroupInfo()
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

                    // Find the parent GroupRange elements
                    var firstGroupRange = address.Parent;
                    var secondGroupRange = firstGroupRange?.Parent;

                    if (firstGroupRange != null && secondGroupRange != null)
                    {
                        var firstGroupNameAttr = firstGroupRange.Attribute("Name");
                        var secondGroupNameAttr = secondGroupRange.Attribute("Name");

                        if (firstGroupNameAttr != null && secondGroupNameAttr != null)
                        {
                            string firstGroupName = firstGroupNameAttr.Value;
                            string secondGroupName = secondGroupNameAttr.Value;
                            string newName = $"{nameAttr.Value}-{secondGroupName}-{firstGroupName}-{principal}-{median}-{participant}";

                            Console.WriteLine($"Original Name: {nameAttr.Value}");
                            Console.WriteLine($"New Name: {newName}");

                            // Update the name attribute
                            nameAttr.Value = newName;
                        }
                        else
                        {
                            Console.WriteLine("One of the GroupRange elements does not have a Name attribute.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("GroupAddress does not belong to the expected hierarchy of GroupRange elements.");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid address format: {addressAttr.Value}");
                }
            }
        }

        // Save the corrected XML document
        xmlDoc.Save("xml/xml1g.xml");
    }
}