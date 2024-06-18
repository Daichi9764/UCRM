using System.IO;

namespace KNXBoostDesktop
{
    public class ProjectFileManager
    {
        // ----- Attributs privés -----
        
        // ----- Attributs publics -----
        
        public string KnxprojSourceFilePath { get; set; }

        public string KnxprojExportFolderPath { get; set; }

        public string ZeroXmlPath { get; private set; }

        // ----- Méthodes privées -----

        // ----- Méthodes publiques -----

        // Constructeur par défaut
        public ProjectFileManager()
        {
            KnxprojSourceFilePath = "";
            KnxprojExportFolderPath = "";
            ZeroXmlPath = "";
        }

        // Constructeur avec path de source et path de destination
        public ProjectFileManager(string sourceFile, string exportFolder)
        {
            KnxprojSourceFilePath = sourceFile;
            KnxprojExportFolderPath = exportFolder;
            ZeroXmlPath = "";
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
                string msg;
                    
                while ((!managedToNormalizePaths) && (!cancelOperation))
                {
                    if ((KnxprojExportFolderPath.ToLower() == "null") || (KnxprojSourceFilePath.ToLower() == "null"))
                    {
                        cancelOperation = true;
                        msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Annulation de l'exportation du projet.";
                        App.ConsoleAndLogWriteLine(msg);
                        continue;
                    }

                    // On tente d'abord de normaliser l'adresse du fichier du projet
                    try
                    {
                        KnxprojSourceFilePath =
                            Path.GetFullPath(KnxprojSourceFilePath); // Normalisation de l'adresse du fichier du projet
                    }
                    catch (ArgumentException)
                    {
                        msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: le chemin de source du fichier .knxproj est vide. Veuillez réessayer.";
                        App.ConsoleAndLogWriteLine(msg);
                        KnxprojSourceFilePath = AskForPath();
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: le chemin {KnxprojSourceFilePath} est trop long (plus de 255 caractères). Veuillez réessayer.";
                        App.ConsoleAndLogWriteLine(msg);
                        KnxprojSourceFilePath = AskForPath();
                        continue;
                    }

                    // Une fois que la normalisation de l'adresse du fichier du projet a été effectuée,
                    // On tente de normaliser l'adresse du dossier projet exporté
                    try
                    {
                        KnxprojExportFolderPath =
                            Path.GetFullPath(KnxprojExportFolderPath); // Normalisation de l'adresse du dossier projet exporté
                    }
                    catch (ArgumentException)
                    {
                        msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: le chemin d'exportation du projet indiqué est vide. Veuillez réessayer.";
                        App.ConsoleAndLogWriteLine(msg);
                        KnxprojExportFolderPath = AskForPath();
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: le chemin {KnxprojExportFolderPath} est trop long (plus de 255 caractères). Veuillez réessayer.";
                        App.ConsoleAndLogWriteLine(msg);
                        KnxprojExportFolderPath = AskForPath();
                        continue;
                    }

                    managedToNormalizePaths = true;
                }

                /* ------------------------------------------------------------------------------------------------
                ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
                ------------------------------------------------------------------------------------------------ */
                    
                msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Starting to extract {Path.GetFileName(KnxprojSourceFilePath)}...";
                App.ConsoleAndLogWriteLine(msg);

                string zipArchivePath; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)

                // Transformation du knxproj en zip
                if (KnxprojSourceFilePath.EndsWith(".knxproj"))
                {
                    // Si le fichier entré est un .knxproj
                    zipArchivePath =
                        KnxprojSourceFilePath.Substring(0, KnxprojSourceFilePath.Length - ".knxproj".Length) +
                        ".zip"; // On enlève .knxproj et on ajoute .zip
                }
                else
                {
                    // Sinon, ce n'est pas le type de fichier que l'on veut
                    msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: le fichier entré n'est pas au format .knxproj. "
                          + "Veuillez réessayer. Pour obtenir un fichier dont l'extension est .knxproj, "
                          + "rendez-vous dans votre tableau de bord ETS et cliquez sur \"Exporter le projet\"\n";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }

                try
                {
                    // On essaie de transformer le fichier .knxproj en archive .zip
                    File.Move(KnxprojSourceFilePath, zipArchivePath);
                }
                catch (FileNotFoundException)
                {
                    // Si le fichier n'existe pas ou que le path est incorrect
                    msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Fichier {KnxprojSourceFilePath} introuvable. Veuillez vérifier le path que vous avez entré et réessayer.\n";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException)
                {
                    // Si le fichier n'est pas accessible en écriture
                    msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Impossible d'accéder en écriture au fichier {KnxprojSourceFilePath}. "
                          + "Veuillez vérifier que le programme a bien accès au fichier ou tentez de l'exécuter "
                          + "en tant qu'administrateur.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (DirectoryNotFoundException)
                {
                    // Si le dossier destination n'a pas été trouvé
                    msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Le dossier {Path.GetDirectoryName(KnxprojSourceFilePath)} est introuvable. "
                          + "Veuillez vérifier le chemin entré et réessayer.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (PathTooLongException)
                {
                    msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: le chemin {KnxprojSourceFilePath} est trop long (plus de 255 caractères). Veuillez réessayer.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }

                // Si le fichier a bien été transformé en zip, tentative d'extraction
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath, KnxprojExportFolderPath); // On extrait le zip
                }
                catch (NotSupportedException)
                {
                    msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Erreur: Le type d'archive du fichier {Path.GetFileName(KnxprojSourceFilePath)} n'est pas supporté. "
                          + "Veuillez vérifier que le fichier n'est pas corrompu. \nLe cas échéant, veuillez exporter à nouveau votre "
                          + "projet ETS et réessayer de l'extraire.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojExportFolderPath = AskForPath();
                    continue;
                }
                    
                File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
                msg = $"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] Done ! New folder created: {KnxprojExportFolderPath}";
                App.ConsoleAndLogWriteLine(msg);
                managedToExtractProject = true;
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

            while (Console.ReadLine() == null);
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
            string foundPath = FindFile(KnxprojExportFolderPath, "0.xml");
            if (string.IsNullOrEmpty(foundPath))
            {
                Console.WriteLine("Impossible de trouver le fichier '0.xml' dans les dossiers du projet. "
                    + "Veuillez vérifier que l'archive extraite soit bien un projet ETS KNX.");
                Environment.Exit(10);
            }
            else
            {
                ZeroXmlPath = foundPath;
                Console.WriteLine($"Found '0.xml' file at {ZeroXmlPath}.");
            }
        }
    }
}
