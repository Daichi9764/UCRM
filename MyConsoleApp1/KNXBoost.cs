/***************************************************************************
 * Nom du Projet : KNXBoost
 * Fichier       : KNXBoost.cs
 * Auteur        : Votre Nom
 * Date          : 12/06/2024
 * Version       : 1.0.0
 *
 * Description :
 * Ce fichier contient [Description du fichier et son rôle dans le projet].
 * [Donnez des détails supplémentaires sur le contenu et la fonctionnalité
 * du fichier, les classes principales, les méthodes ou toute autre
 * information pertinente].
 *
 * Historique des Modifications :
 * -------------------------------------------------------------------------
 * Date        | Auteur       | Version  | Description
 * -------------------------------------------------------------------------
 * 12/06/2024  | Votre Nom    | 1.0.0    | Création initiale du fichier.
 * [Date]      | [Nom]        | [Ver.]   | [Description de la modification]
 *
 * Remarques :
 * [Ajoutez ici toute information supplémentaire, des notes spéciales, des
 * avertissements ou des recommandations concernant ce fichier].
 *
 * **************************************************************************/


using System;
using System.IO.Compression;
using MyConsoleApp1;

//using System.Linq;
using System.Xml.Linq;

class KNXBoost
{
    static void Main()
    {
        ProjectFileManager fm = new ProjectFileManager(@"T:\Test.knxproj", @"T:\Test_exported\");
        fm.ExtractProjectFiles();
        fm.FindZeroXml();
    }
}
