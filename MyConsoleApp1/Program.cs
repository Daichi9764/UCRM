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
//using System.Linq;
//using System.Xml.Linq;

class Program
{
    static void Main()
    {

        /* ------------------------------------------------------------------------------------------------
        ---------------------------------------- GESTION DES PATH ----------------------------------------- 
        ------------------------------------------------------------------------------------------------ */

        // CETTE PORTION DE CODE PERMET EVENTUELLEMENT DE DEMANDER A L'UTILISATEUR DE DONNER LE PATH DU FICHIER .knxproj
        // IDEALEMENT DANS LA VERSION "INTERFACE" DU LOGICIEL, ON AURA UN POPUP POUR QUE L'UTILISATEUR SELECTIONNE LE FICHIER
        // DIRECTEMENT DANS L'ARBORESCENCE WINDOWS.
        /*string knxprojSourcePath;
        Console.WriteLine("Veuillez entrer l'adresse du fichier du projet dans l'arborescence des fichiers:");
        knxprojSourcePath = Console.ReadLine(); // Lecture du path entré par l'utilisateur dans la console */

        string knxprojSourcePath =  "T:\\test.knxproj"; // Adresse du fichier du projet
        string knxprojExportPath =  @"T:\knxproj_exported\"; // Adresse du dossier projet exporté
        string zipArchivePath = ""; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)

        knxprojSourcePath = Path.GetFullPath(knxprojSourcePath); // Normalisation de l'adresse du fichier du projet
        knxprojExportPath = Path.GetFullPath(knxprojExportPath); // Normalisation de l'adresse du dossier projet exporté


        /* ------------------------------------------------------------------------------------------------
        ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ---------------------------------- 
        ------------------------------------------------------------------------------------------------ */

        // Transformation du knxproj en zip
        if (knxprojSourcePath.EndsWith(".knxproj")){ // Si le fichier entré est un .knxproj
            zipArchivePath = knxprojSourcePath.Substring(0, knxprojSourcePath.Length - ".knxproj".Length) + ".zip"; // On enlève .knxproj et on ajoute .zip
        } else { // Sinon, ce n'est pas le type de fichier que l'on veut
            Console.WriteLine("Erreur: le fichier entré n'est pas au format .knxproj. " 
            + "Veuillez réessayer. Pour obtenir un fichier dont l'extension est .knxproj,"
            +" rendez-vous dans votre tableau de bord ETS et cliquez sur \"Exporter le projet\"");
            Environment.Exit(1); // Arrêt du programme avec un code erreur
        }

        try { // On essaie de transformer le fichier .knxproj en archive .zip
            System.IO.File.Move(knxprojSourcePath, zipArchivePath);

        } catch (System.IO.FileNotFoundException){ // Si le fichier n'existe pas ou que le path est incorrect
            Console.WriteLine("Fichier introuvable. Veuillez vérifier le path que vous avez entré et réessayer.");
            Environment.Exit(2); // On arrête ici l'application avec un code erreur, mais on pourraît éventuellement redemander un nouveau path

        } catch (UnauthorizedAccessException){ // Si le fichier n'est pas accessible en écriture
            Console.WriteLine($"Impossible d'accéder en écriture au fichier {knxprojSourcePath}." 
            + "Veuillez vérifier que le programme a bien accès au fichier ou tentez de l'exécuter"
            + " en tant qu'administrateur.");
            Environment.Exit(3);
        }

        // Si le fichier a bien été transformé en zip
        System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath, knxprojExportPath); // On extrait le zip
        System.IO.File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
    }
}
