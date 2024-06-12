namespace MyConsoleApp1;

public class projectFileManager
{
    // ----- Attributs privés -----
    private string knxprojSourceFilePath; // Adresse du fichier du projet
    private string knxprojExportFolderPath; // Adresse du dossier projet exporté
    
    // ----- Attributs publics -----
    
    // ----- Méthodes privées -----
    
    // ----- Méthodes publiques -----
    
    
    // Constructeur par défaut
    public projectFileManager()
    {
        knxprojSourceFilePath = "";
        knxprojExportFolderPath = "";
    }

    
    // Constructeur avec path de source et path de destination
    public projectFileManager(string sourceFile, string exportFolder)
    {
        knxprojSourceFilePath = sourceFile;
        knxprojExportFolderPath = exportFolder;
    }

    
    // Fonction permettant de récupérer le contenu de l'archive .knxproj situé à knxprojSourcePath et de le placer dans le dossier knxprojExportPath
    public void extractProjectFiles()
    {
        string zipArchivePath = ""; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)
        
        /* ------------------------------------------------------------------------------------------------
        ---------------------------------------- GESTION DES PATH -----------------------------------------
        ------------------------------------------------------------------------------------------------ */

        // CETTE PORTION DE CODE PERMET EVENTUELLEMENT DE DEMANDER A L'UTILISATEUR DE DONNER LE PATH DU FICHIER .knxproj
        // IDEALEMENT DANS LA VERSION "INTERFACE" DU LOGICIEL, ON AURA UN POPUP POUR QUE L'UTILISATEUR SELECTIONNE LE FICHIER
        // DIRECTEMENT DANS L'ARBORESCENCE WINDOWS.
        /*string knxprojSourcePath;
        Console.WriteLine("Veuillez entrer l'adresse du fichier du projet dans l'arborescence des fichiers:");
        knxprojSourcePath = Console.ReadLine(); // Lecture du path entré par l'utilisateur dans la console */

        knxprojSourceFilePath = Path.GetFullPath(knxprojSourceFilePath); // Normalisation de l'adresse du fichier du projet
        knxprojExportFolderPath = Path.GetFullPath(knxprojExportFolderPath); // Normalisation de l'adresse du dossier projet exporté


        /* ------------------------------------------------------------------------------------------------
        ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
        ------------------------------------------------------------------------------------------------ */
        
        Console.WriteLine($"Starting to extract {Path.GetFileName(knxprojSourceFilePath)}...");
        
        // Transformation du knxproj en zip
        if (knxprojSourceFilePath.EndsWith(".knxproj"))
        {
            // Si le fichier entré est un .knxproj
            zipArchivePath =
                knxprojSourceFilePath.Substring(0, knxprojSourceFilePath.Length - ".knxproj".Length) +
                ".zip"; // On enlève .knxproj et on ajoute .zip
        }
        else
        {
            // Sinon, ce n'est pas le type de fichier que l'on veut
            Console.WriteLine("Erreur: le fichier entré n'est pas au format .knxproj. "
                              + "Veuillez réessayer. Pour obtenir un fichier dont l'extension est .knxproj, "
                              + "rendez-vous dans votre tableau de bord ETS et cliquez sur \"Exporter le projet\"");
            Environment.Exit(1); // Arrêt du programme avec un code erreur
        }
        
        try
        {
            // On essaie de transformer le fichier .knxproj en archive .zip
            System.IO.File.Move(knxprojSourceFilePath, zipArchivePath);
        }
        catch (FileNotFoundException)
        {
            // Si le fichier n'existe pas ou que le path est incorrect
            Console.WriteLine("Fichier introuvable. Veuillez vérifier le path que vous avez entré et réessayer.");
            Environment.Exit(
                2); // On arrête ici l'application avec un code erreur, mais on pourraît éventuellement redemander un nouveau path

        }
        catch (UnauthorizedAccessException)
        {
            // Si le fichier n'est pas accessible en écriture
            Console.WriteLine($"Impossible d'accéder en écriture au fichier {knxprojSourceFilePath}. "
                              + "Veuillez vérifier que le programme a bien accès au fichier ou tentez de l'exécuter "
                              + "en tant qu'administrateur.");
            Environment.Exit(3);
        }
        catch (DirectoryNotFoundException)
        {
            Environment.Exit(4);
        }
        catch (NotSupportedException)
        {
            Environment.Exit(5);
        }
        catch (PathTooLongException)
        {
            Environment.Exit(6);
        }

        // Si le fichier a bien été transformé en zip
        System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath, knxprojExportFolderPath); // On extrait le zip
        System.IO.File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
        
        Console.WriteLine($"Done ! New folder created: {knxprojExportFolderPath}");
    }
}