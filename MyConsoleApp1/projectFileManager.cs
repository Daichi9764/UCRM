namespace MyConsoleApp1;

public class projectFileManager
{
    // ----- Attributs privés -----
    private string knxprojSourceFilePath; // Adresse du fichier du projet
    private string knxprojExportFolderPath; // Adresse du dossier projet exporté
    private string zeroXmlPath;
    
    // ----- Attributs publics -----
    
    // ----- Méthodes privées -----
    
    // ----- Méthodes publiques -----
    
    
    
    // Constructeur par défaut
    public projectFileManager()
    {
        knxprojSourceFilePath = "";
        knxprojExportFolderPath = "";
        zeroXmlPath = "";
    }

    
    
    // Constructeur avec path de source et path de destination
    public projectFileManager(string sourceFile, string exportFolder)
    {
        knxprojSourceFilePath = sourceFile;
        knxprojExportFolderPath = exportFolder;
        zeroXmlPath = "";
    }

    
    
    // Fonction permettant de récupérer le contenu de l'archive .knxproj situé à knxprojSourcePath et de le placer dans le dossier knxprojExportPath
    public void extractProjectFiles()
    {
        string zipArchivePath = ""; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)
        bool managedToExtractProject = false;
        
        // Tant que l'on n'a pas réussi à extraire le projet ou que l'on n'a pas demandé l'annulation de l'extraction
        while ((!managedToExtractProject) && (knxprojSourceFilePath!="NULL"))
        {
            /* ------------------------------------------------------------------------------------------------
            ---------------------------------------- GESTION DES PATH -----------------------------------------
            ------------------------------------------------------------------------------------------------ */

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
                                  + "rendez-vous dans votre tableau de bord ETS et cliquez sur \"Exporter le projet\"\n");
                System.Threading.Thread.Sleep(1000);
                knxprojSourceFilePath = askForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            
            try
            {
                // On essaie de transformer le fichier .knxproj en archive .zip
                System.IO.File.Move(knxprojSourceFilePath, zipArchivePath);
            }
            catch (FileNotFoundException)
            {
                // Si le fichier n'existe pas ou que le path est incorrect
                Console.WriteLine($"Fichier {knxprojSourceFilePath} introuvable. Veuillez vérifier le path que vous avez entré et réessayer.");
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

            
            
            // Si le fichier a bien été transformé en zip, tentative d'extraction
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath, knxprojExportFolderPath); // On extrait le zip
            }
            catch (NotSupportedException)
            {
                
            }
            System.IO.File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
            Console.WriteLine($"Done ! New folder created: {knxprojExportFolderPath}");
            managedToExtractProject = true;
        }
        
        
        
        // Si on quitte la boucle car l'utilisateur a demandé l'annulation de l'extraction, on affiche
        // Un message d'acquittement.
        if (knxprojSourceFilePath == "NULL")
        {
            Console.WriteLine("Annulation de l'exportation du projet.");
        }
        
    }
    
    
    
    // Fonction permettant de demander à l'utilisateur d'entrer un path
    public string askForPath()
    {
        // Note: Lorsque le programme ne sera plus de type ConsoleApp, cette fonction sera remplacée par une fenêtre de type pop-up qui laissera
        // L'utilisateur sélectionner le fichier depuis l'explorateur windows.
        Console.WriteLine("Veuillez entrer l'adresse du fichier du projet (terminant par .knxproj) dans l'arborescence des fichiers: "
            + $"{Environment.NewLine}Note: Pour annuler, veuillez entrer \"NULL\".");
        return (Console.ReadLine()); // Lecture du path entré par l'utilisateur dans la console
    }
    
    
    
    // Fonction permettant de trouver un fichier dans un dossier donné
    public static string FindFile(string rootPath, string fileNameToSearch)
    {
        if (!Directory.Exists(rootPath))
        {
            Console.WriteLine($"The directory {rootPath} does not exist.");
            return null;
        }

        Queue<string> directoriesQueue = new Queue<string>();
        directoriesQueue.Enqueue(rootPath);

        while (directoriesQueue.Count > 0)
        {
            string currentDirectory = directoriesQueue.Dequeue();
            try
            {
                // Check files in the current directory
                string[] files = Directory.GetFiles(currentDirectory);
                foreach (string file in files)
                {
                    if (Path.GetFileName(file).Equals(fileNameToSearch, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }

                // Enqueue subdirectories
                string[] subDirectories = Directory.GetDirectories(currentDirectory);
                foreach (string subDirectory in subDirectories)
                {
                    directoriesQueue.Enqueue(subDirectory);
                }
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Console.WriteLine($"Access denied to {currentDirectory}: {unAuthEx.Message}");
            }
            catch (DirectoryNotFoundException dirNotFoundEx)
            {
                Console.WriteLine($"Directory not found: {currentDirectory}: {dirNotFoundEx.Message}");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O Error while accessing {currentDirectory}: {ioEx.Message}");
            }
        }

        return null; // File not found
    }
    
    
    
    // Fonction permettant de trouver le fichier 0.xml dans le projet exporté
    // ATTENTION: Nécessite que le projet .knxproj ait déjà été extrait avec la fonction extractProjectFiles().
    public void findZeroXml()
    {
        string foundPath = FindFile(knxprojExportFolderPath, "0.xml");
        if (string.IsNullOrEmpty(foundPath))
        {
            Console.WriteLine("Impossible de trouver le fichier '0.xml' dans les dossiers du projet. "
                +"Veuillez vérifier que l'archive extraite soit bien un projet ETS KNX.");
        }
        else
        {
            zeroXmlPath = foundPath;
            Console.WriteLine($"Found '0.xml' file at {zeroXmlPath}.");
        }
        
    }
}