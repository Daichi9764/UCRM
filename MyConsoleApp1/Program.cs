/***************************************************************************
 * Nom du Projet : NomDuProjet
 * Fichier       : NomDuFichier.cs
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

class Program
{
    static void Main()
    {
        projectFileManager fm = new projectFileManager(@"T:\test 2.knxproj", @"T:\knxproj_exported\");
        fm.extractProjectFiles();
    }
}
