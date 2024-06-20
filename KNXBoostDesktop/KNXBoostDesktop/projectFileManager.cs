using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace KNXBoostDesktop
{
    public class ProjectFileManager
    {
        // ----- Attributs privés -----
        
        // ----- Attributs publics -----
        
        public string KnxprojSourceFilePath { get; set; }
        
        public string ExportedProjectPath { get; private set; }
        
        public string ZeroXmlPath { get; private set; }

        // ----- Méthodes privées -----

        // ----- Méthodes publiques -----

        // Constructeur par défaut
        public ProjectFileManager()
        {
            KnxprojSourceFilePath = "";
            ExportedProjectPath = "";
            ZeroXmlPath = "";
        }

        // Constructeur avec path de source et path de destination
        public ProjectFileManager(string sourceFile)
        {
            KnxprojSourceFilePath = sourceFile;
            ExportedProjectPath = "";
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
                    if (KnxprojSourceFilePath.ToLower() == "null")
                    {
                        cancelOperation = true;
                        App.ConsoleAndLogWriteLine("User cancelled the project extraction process.");
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
                        App.ConsoleAndLogWriteLine("Error: the .knxproj source file path is empty. Please try selecting the file again.");
                        KnxprojSourceFilePath = AskForPath();
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        App.ConsoleAndLogWriteLine($"Error: the path {KnxprojSourceFilePath} is too long (more than 255 characters). " +
                                                   $"Please try selecting another path.");
                        KnxprojSourceFilePath = AskForPath();
                        continue;
                    }

                    managedToNormalizePaths = true;
                }

                /* ------------------------------------------------------------------------------------------------
                ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
                ------------------------------------------------------------------------------------------------ */
                
                App.ConsoleAndLogWriteLine($"Starting to extract {Path.GetFileName(KnxprojSourceFilePath)}...");

                string zipArchivePath; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)
                string knxprojExportFolderPath =
                    $@"./{Path.GetFileNameWithoutExtension(KnxprojSourceFilePath)}_exported/";

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
                    msg = "Error: the selected file is not a .knxproj file. "
                          + "Please try again. To obtain a .knxproj file, "
                          + "please head into the ETS app and click the \"Export Project\" button.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }

                
                try
                {
                    // On essaie de transformer le fichier .knxproj en archive .zip
                    File.Copy(KnxprojSourceFilePath, zipArchivePath);
                }
                catch (FileNotFoundException)
                {
                    // Si le fichier n'existe pas ou que le path est incorrect
                    App.ConsoleAndLogWriteLine($"Error: the file {KnxprojSourceFilePath} was not found. Please check the selected file path and try again.");
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException)
                {
                    // Si le fichier n'est pas accessible en écriture
                    msg = $"Unable to write to the file {KnxprojSourceFilePath}. "
                          + "Please check that the program has access to the file or try running it "
                          + "as an administrator.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (DirectoryNotFoundException)
                {
                    // Si le dossier destination n'a pas été trouvé
                    msg = $"The folder {Path.GetDirectoryName(KnxprojSourceFilePath)} cannot be found. "
                          + "Please check the entered path and try again.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (PathTooLongException)
                {
                    App.ConsoleAndLogWriteLine($"Error: the path {KnxprojSourceFilePath} is too long (more than 255 characters). Please try again.");
                    KnxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                
                
                // Si le dossier d'exportation existe déjà, on le supprime pour laisser place au nouveau
                if (Path.Exists(knxprojExportFolderPath))
                {   
                    App.ConsoleAndLogWriteLine($"The folder {knxprojExportFolderPath} already exists, deleting...");
                    Directory.Delete(knxprojExportFolderPath, true);
                }
                
                
                // Si le fichier a bien été transformé en zip, tentative d'extraction
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath, knxprojExportFolderPath); // On extrait le zip
                }
                catch (NotSupportedException)
                {
                    msg = $"Error: The archive type of the file {Path.GetFileName(KnxprojSourceFilePath)} is not supported. "
                          + "Please check that the file is not corrupted. \nIf necessary, please export your "
                          + "ETS project again and try to extract it.";
                    App.ConsoleAndLogWriteLine(msg);
                    KnxprojSourceFilePath = AskForPath();
                    continue;
                }
                    
                File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
                App.ConsoleAndLogWriteLine($"Done! New folder created: {Path.GetFullPath(knxprojExportFolderPath)}");
                ExportedProjectPath = knxprojExportFolderPath;
                managedToExtractProject = true;
            }
        }

        // Fonction permettant de demander à l'utilisateur d'entrer un path
        private string AskForPath()
        {
            // Créer une nouvelle instance de OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Définir des propriétés optionnelles
                Title = "Sélectionnez un projet KNX à importer",
                Filter = "ETS KNX Project File (*.knxproj)|*.knxproj|other file|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Récupérer le chemin du fichier sélectionné
                return openFileDialog.FileName;
            }
            else
            {
                return "";
            }
        }

        // Fonction permettant de trouver un fichier dans un dossier donné
        private static string FindFile(string rootPath, string fileNameToSearch)
        {
            if (!Directory.Exists(rootPath))
            {
                App.ConsoleAndLogWriteLine($"The directory {rootPath} does not exist.");
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
                    App.ConsoleAndLogWriteLine($"Access denied to {currentDirectory}: {unAuthEx.Message}");
                }
                catch (DirectoryNotFoundException dirNotFoundEx)
                {
                    App.ConsoleAndLogWriteLine($"Directory not found: {currentDirectory}: {dirNotFoundEx.Message}");
                }
                catch (IOException ioEx)
                {
                    App.ConsoleAndLogWriteLine($"I/O Error while accessing {currentDirectory}: {ioEx.Message}");
                }
            }

            return null; // File not found
        }

        // Fonction permettant de trouver le fichier 0.xml dans le projet exporté
        // ATTENTION: Nécessite que le projet .knxproj ait déjà été extrait avec la fonction extractProjectFiles().
        public void FindZeroXml()
        {
            string foundPath = FindFile(ExportedProjectPath, "0.xml");
            if (string.IsNullOrEmpty(foundPath))
            {
                App.ConsoleAndLogWriteLine("Unable to find the file '0.xml' in the project folders. "
                                           + "Please ensure that the extracted archive is indeed a KNX ETS project.");
                Application.Current.Shutdown();
            }
            else
            {
                ZeroXmlPath = foundPath;
                App.ConsoleAndLogWriteLine($"Found '0.xml' file at {Path.GetFullPath(ZeroXmlPath)}.");
            }
        }
    }
}
