namespace MyConsoleApp1;

public class ProjectFileManager
{
    // ----- Attributs privés -----
    private string _knxprojSourceFilePath; // Adresse du fichier du projet
    private string _knxprojExportFolderPath; // Adresse du dossier projet exporté
    private string _zeroXmlPath;
    
    // ----- Attributs publics -----
    
    // ----- Méthodes privées -----
    
    // ----- Méthodes publiques -----
    
    
    // Constructeur par défaut
    public ProjectFileManager()
    {
        _knxprojSourceFilePath = "";
        _knxprojExportFolderPath = "";
        _zeroXmlPath = "";
    }

    
    
    // Constructeur avec path de source et path de destination
    public ProjectFileManager(string sourceFile, string exportFolder)
    {
        _knxprojSourceFilePath = sourceFile;
        _knxprojExportFolderPath = exportFolder;
        _zeroXmlPath = "";
    }

    
    
    // Fonction permettant de récupérer le contenu de l'archive .knxproj situé à knxprojSourcePath et de le placer dans le dossier knxprojExportPath
    public void ExtractProjectFiles()
    {
        bool managedToExtractProject = false;
        bool managedToNormalizePaths = false;
        bool cancelOperation = false;
        
        // Tant que l'on n'a pas réussi à extraire le projet ou que l'on n'a pas demandé l'annulation de l'extraction
        while ((!managedToExtractProject) && (!cancelOperation))
        {
            /* ------------------------------------------------------------------------------------------------
            ---------------------------------------- GESTION DES PATH -----------------------------------------
            ------------------------------------------------------------------------------------------------ */
            
            // Répéter tant que l'on n'a pas réussi à normaliser les chemins d'accès ou que l'on n'a pas demandé
            // à annuler l'extraction
            while ((!managedToNormalizePaths) && (!cancelOperation))
            {
                if ((_knxprojExportFolderPath.ToLower() != "null") || (_knxprojSourceFilePath.ToLower() != "null"))
                {
                    cancelOperation = true;
                    continue;
                }
                
                // On tente d'abord de normaliser l'adresse du fichier du projet
                try
                {
                    _knxprojSourceFilePath =
                        Path.GetFullPath(_knxprojSourceFilePath); // Normalisation de l'adresse du fichier du projet
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Erreur: le chemin de source du fichier .knxproj est vide. Veuillez réessayer.");
                    _knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (PathTooLongException)
                {
                    Console.WriteLine($"Erreur: le chemin {_knxprojSourceFilePath} est trop long (plus de 255 caractères). Veuillez réessayer.");
                    _knxprojSourceFilePath = AskForPath();
                    continue;
                }
            
                // Une fois que la normalisation de l'adresse du fichier du projet a été effectuée,
                // On tente de normaliser l'adresse du dossier projet exporté
                try
                {
                    _knxprojExportFolderPath =
                        Path.GetFullPath(_knxprojExportFolderPath); // Normalisation de l'adresse du dossier projet exporté
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Erreur: le chemin d'exportation du projet indiqué est vide. Veuillez réessayer.");
                    _knxprojExportFolderPath = AskForPath();
                    continue;
                }
                catch (PathTooLongException)
                {
                    Console.WriteLine($"Erreur: le chemin {_knxprojExportFolderPath} est trop long (plus de 255 caractères). Veuillez réessayer.");
                    _knxprojExportFolderPath = AskForPath();
                    continue;
                }
            }
            


            /* ------------------------------------------------------------------------------------------------
            ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
            ------------------------------------------------------------------------------------------------ */
            
            Console.WriteLine($"Starting to extract {Path.GetFileName(_knxprojSourceFilePath)}...");
            
            string zipArchivePath = ""; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)
            
            
            // Transformation du knxproj en zip
            if (_knxprojSourceFilePath.EndsWith(".knxproj"))
            {
                // Si le fichier entré est un .knxproj
                zipArchivePath =
                    _knxprojSourceFilePath.Substring(0, _knxprojSourceFilePath.Length - ".knxproj".Length) +
                    ".zip"; // On enlève .knxproj et on ajoute .zip
            }
            else
            {
                // Sinon, ce n'est pas le type de fichier que l'on veut
                Console.WriteLine("Erreur: le fichier entré n'est pas au format .knxproj. "
                                  + "Veuillez réessayer. Pour obtenir un fichier dont l'extension est .knxproj, "
                                  + "rendez-vous dans votre tableau de bord ETS et cliquez sur \"Exporter le projet\"\n");
                _knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            
            
            try
            {
                // On essaie de transformer le fichier .knxproj en archive .zip
                File.Move(_knxprojSourceFilePath, zipArchivePath); 
            }
            catch (FileNotFoundException)
            {
                // Si le fichier n'existe pas ou que le path est incorrect
                Console.WriteLine($"Fichier {_knxprojSourceFilePath} introuvable. Veuillez vérifier le path que vous avez entré et réessayer.\n");
                _knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (UnauthorizedAccessException)
            {
                // Si le fichier n'est pas accessible en écriture
                Console.WriteLine($"Impossible d'accéder en écriture au fichier {_knxprojSourceFilePath}. "
                                  + "Veuillez vérifier que le programme a bien accès au fichier ou tentez de l'exécuter "
                                  + "en tant qu'administrateur.");
                _knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (DirectoryNotFoundException)
            {
                // Si le dossier destination n'a pas été trouvé
                Console.WriteLine($"Le dossier {Path.GetDirectoryName(_knxprojSourceFilePath)} est introuvable. "
                    +"Veuillez vérifier le chemin entré et réessayer.");
                _knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }
            catch (PathTooLongException)
            {
                Console.WriteLine($"Erreur: le chemin {_knxprojSourceFilePath} est trop long (plus de 255 caractères). Veuillez réessayer.");
                _knxprojSourceFilePath = AskForPath();
                continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
            }

            
            
            // Si le fichier a bien été transformé en zip, tentative d'extraction
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath, _knxprojExportFolderPath); // On extrait le zip
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"Erreur: Le type d'archive du fichier {Path.GetFileName(_knxprojSourceFilePath)} n'est pas supporté. "
                    + "Veuillez vérifier que le fichier n'est pas corrompu. \nLe cas échéant, veuillez exporter à nouveau votre "
                    + "projet ETS et réessayer de l'extraire.");
                _knxprojExportFolderPath = AskForPath();
                continue;
            }
            File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
            Console.WriteLine($"Done ! New folder created: {_knxprojExportFolderPath}");
            managedToExtractProject = true;
        }
        
        
        
        // Si on quitte la boucle car l'utilisateur a demandé l'annulation de l'extraction, on affiche
        // Un message d'acquittement.
        if (cancelOperation)
        {
            Console.WriteLine("Annulation de l'exportation du projet.");
        }
    }
    
    
    // Fonction permettant de demander à l'utilisateur d'entrer un path
    private string AskForPath()
    {
        string os = " ";
        
        if (OperatingSystem.IsWindows())
            os = " Windows";
        else if (OperatingSystem.IsLinux())
            os = " Linux";
        else if (OperatingSystem.IsMacOS())
            os = " MacOS";
        
        // Note: Lorsque le programme ne sera plus de type ConsoleApp, cette fonction sera remplacée par une fenêtre de type pop-up qui laissera
        // L'utilisateur sélectionner le fichier depuis l'explorateur windows.
        Console.WriteLine($"Veuillez entrer l'adresse du fichier dans l'arborescence des fichiers{os}: "
            + $"{Environment.NewLine}Note: Pour annuler, veuillez entrer \"NULL\".");
        return (Console.ReadLine()); // Lecture du path entré par l'utilisateur dans la console
    }
    
    
    
    // Fonction permettant de trouver un fichier dans un dossier donné
    private static string FindFile(string rootPath, string fileNameToSearch)
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
    public void FindZeroXml()
    {
        string foundPath = FindFile(_knxprojExportFolderPath, "0.xml");
        if (string.IsNullOrEmpty(foundPath))
        {
            Console.WriteLine("Impossible de trouver le fichier '0.xml' dans les dossiers du projet. "
                +"Veuillez vérifier que l'archive extraite soit bien un projet ETS KNX.");
            Environment.Exit(10);
        }
        else
        {
            _zeroXmlPath = foundPath;
            Console.WriteLine($"Found '0.xml' file at {_zeroXmlPath}.");
        }
        
    }
}