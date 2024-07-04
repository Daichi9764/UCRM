/***************************************************************************
 * Nom du Projet : KNX Boost Desktop
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime
 * Date          : 12/06/2024
 * Version       : 1.1
 *
 * Description :
 * Fichier principal contenant la structure de l'application et toutes les
 * fonctions nécessaires à son utilisation.
 *
 * Remarques :
 * Repo GitHub --> https://github.com/Daichi9764/UCRM
 *
 * **************************************************************************/

using System.IO;
using System.IO.Compression;
using System.Windows;

namespace KNXBoostDesktop
{
    public partial class App
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Données de l'application
        public static readonly string AppName = "KNX Boost Desktop"; // Nom de l'application
        public static readonly string AppVersion = "1.6"; // Version de l'application
        
        // Gestion des logs
        private static string? _logPath; // Chemin du fichier logs
        private static StreamWriter? _writer; // Permet l'écriture du fichier de logging
        
        // Composants de l'application
        public static ProjectFileManager? Fm { get; private set; } // Gestionnaire de fichiers du projet
        public static DisplayElements? DisplayElements { get; private set; } // Gestionnaire de l'affichage (contient les fenêtres, boutons, ...)
        
        
        /* ------------------------------------------------------------------------------------------------
        -------------------------------------------- METHODES  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Fonction s'exécutant à l'ouverture de l'application
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }
            
            _logPath = $"./logs/logs-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            _writer = new StreamWriter(_logPath);
            
            base.OnStartup(e);

            // Activation de l'auto-vidage du buffer du stream d'écriture
            _writer.AutoFlush = true;


            ConsoleAndLogWriteLine($"STARTING {AppName.ToUpper()} APP...");

            
            // Ouverture la fenêtre principale
            ConsoleAndLogWriteLine("Opening main window");
            DisplayElements = new DisplayElements();
            
            // Mise à jour de la fenêtre de renommage des adresses de groupe
            DisplayElements.GroupAddressRenameWindow.UpdateWindowContents();

            // Mise à jour de la fenêtre principale
            DisplayElements.MainWindow.UpdateWindowContents();
            
            DisplayElements.ShowMainWindow();

            
            // Ouverture du gestionnaire de fichiers de projet
            ConsoleAndLogWriteLine("Opening project file manager");
            Fm = new ProjectFileManager();

            
            // Tentative d'archivage des fichiers de log
            ConsoleAndLogWriteLine("Trying to archive log files");
            ArchiveLogs();
            
            
            // Nettoyage des dossiers restants de la dernière session
            ConsoleAndLogWriteLine("Starting to remove folders from projects extracted last time");
            DeleteAllExceptLogsAndResources();

            
            ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
            ConsoleAndLogWriteLine("-----------------------------------------------------------");

            DisplayElements?.ShowGroupAddressRenameWindow("Cmd_Eclairage_OnOff_Batiment_FacadeXx_Etage_Piece_Circuit");
            
            // Appel au garbage collector pour nettoyer les variables issues 
            GC.Collect();
        }

        
        
        // Fonction s'exécutant lorsque l'on ferme l'application
        protected override void OnExit(ExitEventArgs e)
        {
            ConsoleAndLogWriteLine("-----------------------------------------------------------");
            ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP...");
            
            base.OnExit(e);
            
            ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
            _writer?.Close(); // Fermeture du stream d'écriture des logs
        }

        
        
        // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'écrivant dans les
        // logs sans sauter de ligne après le message.
        public static void ConsoleAndLogWrite(string msg)
        {
            Console.Write(msg); // Ecriture du message dans la console
            
            // Si la fenêtre de la console est ouverte, on scrolle tout en bas
            if (DisplayElements is { ConsoleWindow.IsVisible: true })
            {
                DisplayElements.ConsoleWindow.ConsoleTextBox.ScrollToEnd();
            }
            
            _writer?.Write(msg); // Ecriture du message dans le fichier logs
        }

        
        
        // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'écrivant dans les
        // logs. Ajoute la date et l'heure avant affichage. Saut d'une ligne en fin de message.
        public static void ConsoleAndLogWriteLine(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
            
            // Si la console est ouverte, on scrolle après l'envoi du message pour être sûr d'afficher les derniers évènements
            if (DisplayElements is { ConsoleWindow.IsVisible: true })
            {
                DisplayElements.ConsoleWindow.ConsoleTextBox.ScrollToEnd();
            }
            
            _writer?.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
        }

        
        
        // Fonction d'archivage des logs
        // Fonctionnement: S'il y a plus de 50 fichiers logs.txt, ces fichiers sont rassemblés et compressés dans une archive zip
        // S'il y a plus de 10 archives, ces dernières sont supprimées avant la création de la nouvelle archive
        // Conséquence: on ne stocke les logs que des 50 derniers lancements de l'application
        private static void ArchiveLogs()
        {
            string logDirectory = @"./logs/"; // Chemin du dossier de logs
            
            try
            {
                // Vérifier si le répertoire existe
                if (!Directory.Exists(logDirectory))
                {
                    ConsoleAndLogWriteLine($"The specified directory does not exist : {logDirectory}");
                    return;
                }

                // Obtenir tous les fichiers log dans le répertoire
                var logFiles = Directory.GetFiles(logDirectory, "*.txt");

                // Vérifier s'il y a plus de 50 fichiers log
                if (logFiles.Length > 50)
                {
                    // Obtenir tous les fichiers d'archive dans le répertoire
                    var archiveFiles = Directory.GetFiles(logDirectory, "LogsArchive-*.zip");

                    // Supprimer les archives existantes si elles sont plus de 10
                    if (archiveFiles.Length >= 10)
                    {
                        foreach (var archiveFile in archiveFiles)
                        {
                            File.Delete(archiveFile);
                        }
                        ConsoleAndLogWriteLine("Deleted all existing archive files as they exceeded the limit of 10.");
                    }

                    // Créer le nom du fichier zip avec la date actuelle
                    var zipFileName = Path.Combine(logDirectory, $"LogsArchive-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");

                    // Créer l'archive zip et y ajouter les fichiers log
                    using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                    {
                        foreach (var logFile in logFiles)
                        {
                            if (logFile != _logPath) // Si le fichier logs n'est pas celui que l'on vient de créer pour le lancement actuel
                            {
                                zip.CreateEntryFromFile(logFile, Path.GetFileName(logFile)); // On l'ajoute à l'archive
                                File.Delete(logFile); // Puis on le supprime
                            }
                        }
                    }

                    ConsoleAndLogWriteLine($"Successfully archived log files to {zipFileName}");
                }
                else
                {
                    ConsoleAndLogWriteLine("Not enough log files to archive.");
                }
            }
            catch (Exception ex)
            {
                ConsoleAndLogWriteLine($"An error occured while creating the archive : {ex.Message}");
            }
        }
        
        
        
        // Fonction permettant de supprimer tous les dossiers présents dans le dossier courant
        // Sauf le fichier logs. Cela permet de supprimer tous les projets exportés à la session précédente.
        // Fonction pour supprimer tous les dossiers sauf le dossier 'logs'
        private static void DeleteAllExceptLogsAndResources()
        {
            // Liste tous les sous-répertoires dans le répertoire de base
            string[] directories = Directory.GetDirectories("./");

            foreach (string directory in directories)
            {
                // Exclure le dossier 'logs' et 'resources'
                if ((Path.GetFileName(directory).Equals("logs", StringComparison.OrdinalIgnoreCase))||(Path.GetFileName(directory).Equals("resources", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Supprimer le dossier et son contenu
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Access denied: {ex.Message}");
                    continue;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"I/O error: {ex.Message}");
                    continue;
                }
                App.ConsoleAndLogWriteLine($"Deleted directory: {directory}");
            }
        }
    }
}
