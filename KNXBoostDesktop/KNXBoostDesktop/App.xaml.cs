/***************************************************************************
 * Nom du Projet : KNX Boost Desktop
 * Fichier       : App.xaml.cs
 * Auteurs       : MICHEL Hugo, COUSTON Emma, MALBRANCHE Daichi,
 *                 BRUGIERE Nathan, OLIVEIRA LOPES Maxime
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

using System.IO;
using System.IO.Compression;
using System.Windows;

namespace KNXBoostDesktop
{
    public partial class App
    {
        
        public static readonly string AppName = "KNX Boost Desktop"; // Nom de l'application
        public static readonly string AppVersion = "1.0"; // Version de l'application
        
        
        private static readonly string LogPath = $"./logs/logs-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"; // Chemin du fichier logs
        
        private static readonly StreamWriter Writer = new StreamWriter(LogPath); // Permet l'écriture du fichier de logging
        
        public static ProjectFileManager? Fm { get; private set; } // Gestionnaire de fichiers du projet
        
        public static DisplayElements? DisplayElements { get; private set; } // Gestionnaire de l'affichage (contient les fenêtres, boutons, ...)

        public Formatter? Formatter; // Formatteur d'adresses de groupe

        
        
        
        // Fonction s'exécutant à l'ouverture de l'application
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            Writer.AutoFlush = true;
            
            
            ConsoleAndLogWriteLine($"STARTING {AppName.ToUpper()} APP...");

            
            ConsoleAndLogWriteLine("Opening main window");
            DisplayElements = new DisplayElements();
            DisplayElements.ShowMainWindow();
            
            
            ConsoleAndLogWriteLine("Opening project file manager");
            Fm = new ProjectFileManager();

            
            //ConsoleAndLogWriteLine($"Opening string formatters");
            //formatter = new FormatterNormalize();
            
            
            ConsoleAndLogWriteLine("Trying to archive log files");
            ArchiveLogs();
            
            
            ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP STARTED !");
            ConsoleAndLogWriteLine("-----------------------------------------------------------");
        }
        
        
        
        // Fonction s'exécutant lorsque l'on ferme l'application
        protected override void OnExit(ExitEventArgs e)
        {
            ConsoleAndLogWriteLine("-----------------------------------------------------------");
            ConsoleAndLogWriteLine($"CLOSING {AppName.ToUpper()} APP...");
            base.OnExit(e);
            
            
            ConsoleAndLogWriteLine($"{AppName.ToUpper()} APP CLOSED !");
            Writer.Close();
        }

        
        
        // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'écrivant dans les
        // logs sans sauter de ligne après le message.
        public static void ConsoleAndLogWrite(string msg)
        {
            Console.Write(msg);
            DisplayElements.ConsoleWindow.ConsoleTextBox.ScrollToEnd();
            Writer.Write(msg);
            Writer.Flush(); // On vide le buffer du streamwriter au cas ou il resterait des caractères
        }

        
        
        // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'écrivant dans les
        // logs. Ajoute la date et l'heure avant affichage. Saut d'une ligne en fin de message.
        public static void ConsoleAndLogWriteLine(string msg)
        {
            Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
            
            // Si la console est ouverte, on scrolle après l'envoi du message pour être sur d'afficher les derniers évènements
            if ((DisplayElements != null) && (DisplayElements.ConsoleWindow.IsVisible))
            {
                DisplayElements.ConsoleWindow.ConsoleTextBox.ScrollToEnd();
            }
            
            Writer.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
        }

        
        
        // Fonction d'archivage des logs
        // Fonctionnement: S'il y a plus de 50 fichiers logs.txt, ces fichiers sont rassemblés et compressés dans une archive zip
        // S'il y a plus de 10 archives, ces dernières sont supprimées avant la création de la nouvelle archive
        // Conséquence: on ne stocke les logs que des 50 derniers lancements de l'application
        private void ArchiveLogs()
        {
            string logDirectory = @"./logs/";
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
                    string zipFileName = Path.Combine(logDirectory, $"LogsArchive-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");

                    // Créer l'archive zip et y ajouter les fichiers log
                    using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                    {
                        foreach (var logFile in logFiles)
                        {
                            if (logFile != LogPath)
                            {
                                zip.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                                File.Delete(logFile);
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

    }
}
