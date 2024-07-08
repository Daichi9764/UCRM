﻿using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace KNXBoostDesktop
{
    public class ProjectFileManager
    {
        /* ------------------------------------------------------------------------------------------------
        ------------------------------------------- ATTRIBUTS  --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        public string ProjectFolderPath { get; private set; } = ""; // Chemin d'accès au dossier exporté du projet

        public string ZeroXmlPath { get; private set; } = ""; // Chemin d'accès au fichier 0.xml du projet


        /* ------------------------------------------------------------------------------------------------
        --------------------------------------------- METHODES --------------------------------------------
        ------------------------------------------------------------------------------------------------ */
        // Fonction permettant de récupérer le contenu de l'archive .knxproj situé à knxprojSourcePath et de le placer dans le dossier knxprojExportPath
        public bool ExtractProjectFiles(string knxprojSourceFilePath)
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
                    if (knxprojSourceFilePath.ToLower() == "null")
                    {
                        cancelOperation = true;
                        App.ConsoleAndLogWriteLine("User cancelled the project extraction process.");
                        continue;
                    }

                    // On tente d'abord de normaliser l'adresse du fichier du projet
                    try
                    {
                        knxprojSourceFilePath =
                            Path.GetFullPath(knxprojSourceFilePath); // Normalisation de l'adresse du fichier du projet
                    }
                    catch (ArgumentException)
                    {
                        // Si l'adresse du fichier du projet est vide
                        App.ConsoleAndLogWriteLine(
                            "Error: the .knxproj source file path is empty. Please try selecting the file again.");
                        knxprojSourceFilePath = AskForPath();
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        // Si l'adresse du fichier du projet est trop longue
                        App.ConsoleAndLogWriteLine(
                            $"Error: the path {knxprojSourceFilePath} is too long (more than 255 characters). " +
                            $"Please try selecting another path.");
                        knxprojSourceFilePath = AskForPath();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        // Gestion générique des exceptions non prévues
                        App.ConsoleAndLogWriteLine($"Error normalizing file path: {ex.Message}");
                        knxprojSourceFilePath = AskForPath();
                        continue;
                    }

                    managedToNormalizePaths = true;
                }

                /* ------------------------------------------------------------------------------------------------
                ---------------------------------- EXTRACTION DU FICHIER KNXPROJ ----------------------------------
                ------------------------------------------------------------------------------------------------ */
                
                App.ConsoleAndLogWriteLine($"Starting to extract {Path.GetFileName(knxprojSourceFilePath)}...");

                string zipArchivePath; // Adresse du fichier zip (utile pour la suite de manière à rendre le projet extractable)
                string knxprojExportFolderPath = $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/knxproj_exported/";

                // Transformation du knxproj en zip
                if (knxprojSourceFilePath.EndsWith(".knxproj"))
                {
                    // Si le fichier entré est un .knxproj
                    zipArchivePath = $"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}.zip"; // On enlève .knxproj et on ajoute .zip
                }
                else
                {
                    // Sinon, ce n'est pas le type de fichier que l'on veut
                    msg = "Error: the selected file is not a .knxproj file. "
                          + "Please try again. To obtain a .knxproj file, "
                          + "please head into the ETS app and click the \"Export Project\" button.";
                    App.ConsoleAndLogWriteLine(msg);
                    knxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }

                if (File.Exists(zipArchivePath))
                {
                    App.ConsoleAndLogWriteLine($"{zipArchivePath} already exists. Removing the file before creating the new archive.");
                    try
                    {
                        File.Delete(zipArchivePath);
                    }
                    catch (IOException ex)
                    {
                        App.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}");
                        knxprojSourceFilePath = AskForPath();
                        continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Si on a pas les droits de supprimer le fichier
                        App.ConsoleAndLogWriteLine($"Error deleting existing file {zipArchivePath}: {ex.Message}. " +
                                                   $"Please change the rights of the file so the program can delete {zipArchivePath}");
                        knxprojSourceFilePath = AskForPath();
                        continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                    }
                }

                try
                {
                    // On essaie de transformer le fichier .knxproj en archive .zip
                    File.Copy(knxprojSourceFilePath, zipArchivePath);
                }
                catch (FileNotFoundException)
                {
                    // Si le fichier n'existe pas ou que le path est incorrect
                    App.ConsoleAndLogWriteLine(
                        $"Error: the file {knxprojSourceFilePath} was not found. Please check the selected file path and try again.");
                    knxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (UnauthorizedAccessException)
                {
                    // Si le fichier n'est pas accessible en écriture
                    msg = $"Unable to write to the file {knxprojSourceFilePath}. "
                          + "Please check that the program has access to the file or try running it "
                          + "as an administrator.";
                    App.ConsoleAndLogWriteLine(msg);
                    knxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (DirectoryNotFoundException)
                {
                    // Si le dossier destination n'a pas été trouvé
                    msg = $"The folder {Path.GetDirectoryName(knxprojSourceFilePath)} cannot be found. "
                          + "Please check the entered path and try again.";
                    App.ConsoleAndLogWriteLine(msg);
                    knxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (PathTooLongException)
                {
                    // Si le chemin est trop long
                    App.ConsoleAndLogWriteLine(
                        $"Error: the path {knxprojSourceFilePath} is too long (more than 255 characters). Please try again.");
                    knxprojSourceFilePath = AskForPath();
                    continue; // Retour au début de la boucle pour retenter l'extraction avec le nouveau path
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    App.ConsoleAndLogWriteLine(
                        $"Error copying file {knxprojSourceFilePath} to {zipArchivePath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }


                // Si le dossier d'exportation existe déjà, on le supprime pour laisser place au nouveau
                if (Path.Exists(knxprojExportFolderPath))
                {   
                    try
                    {
                        App.ConsoleAndLogWriteLine($"The folder {knxprojExportFolderPath} already exists, deleting...");
                        Directory.Delete(knxprojExportFolderPath, true);
                    }
                    catch (IOException ex)
                    {
                        App.ConsoleAndLogWriteLine($"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}");
                        knxprojSourceFilePath = AskForPath();
                        continue;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Si on a pas les droits de supprimer le fichier
                        App.ConsoleAndLogWriteLine($"Error deleting existing folder {knxprojExportFolderPath}: {ex.Message}" +
                                                   $"Please change the rights of the file so the program can delete {zipArchivePath}");
                        knxprojSourceFilePath = AskForPath();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        // Gestion générique des exceptions non prévues
                        App.ConsoleAndLogWriteLine($"Error deleting folder {knxprojExportFolderPath}: {ex.Message}");
                        knxprojSourceFilePath = AskForPath();
                        continue;
                    }
                }
                
                
                // Si le fichier a bien été transformé en zip, tentative d'extraction
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipArchivePath,
                        knxprojExportFolderPath); // On extrait le zip
                    File.Delete(zipArchivePath); // On n'a plus besoin du zip, on le supprime
                }
                catch (NotSupportedException)
                {
                    // Si le type d'archive n'est pas supporté
                    msg =
                        $"Error: The archive type of the file {Path.GetFileName(knxprojSourceFilePath)} is not supported. "
                        + "Please check that the file is not corrupted. \nIf necessary, please export your "
                        + "ETS project again and try to extract it.";
                    App.ConsoleAndLogWriteLine(msg);
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (IOException ex)
                {
                    // Gestion des erreurs d'entrée/sortie générales
                    App.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Si l'accès aux fichiers ou aux répertoires n'est pas autorisé
                    App.ConsoleAndLogWriteLine($"Unauthorized access extracting file {zipArchivePath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    App.ConsoleAndLogWriteLine($"Error extracting file {zipArchivePath}: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }


                /* ------------------------------------------------------------------------------------------------
                -------------------------------- GESTION DES PROJETS KNX PROTEGES ---------------------------------
                ------------------------------------------------------------------------------------------------ */
                
                // S'il existe un fichier P-XXXX.zip, alors le projet est protégé par un mot de passe
                try
                {
                    if (Directory.GetFiles(knxprojExportFolderPath, "P-*.zip", SearchOption.TopDirectoryOnly).Length > 0)
                    {
                        App.ConsoleAndLogWriteLine(
                            $"Encountered an error while extracting {knxprojSourceFilePath} : the project is locked with a password in ETS6");
                        MessageBox.Show(
                            "Error: The project you have selected is password-protected and cannot be operated. Please unlock it in ETS and try again.",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        cancelOperation = true;
                        continue;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Gestion des erreurs d'accès non autorisé
                    App.ConsoleAndLogWriteLine($"Unauthorized access checking for protected project files: {ex.Message}.");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }
                catch (Exception ex)
                {
                    // Gestion générique des exceptions non prévues
                    App.ConsoleAndLogWriteLine($"Error checking for protected project files: {ex.Message}");
                    knxprojSourceFilePath = AskForPath();
                    continue;
                }


                /* ------------------------------------------------------------------------------------------------
                ------------------------------- SUPPRESSION DES FICHIERS RESIDUELS --------------------------------
                ------------------------------------------------------------------------------------------------ */
            
                // Suppression du fichier zip temporaire
                App.ConsoleAndLogWriteLine($"Done! New folder created: {Path.GetFullPath(knxprojExportFolderPath)}");
                ProjectFolderPath = $@"./{Path.GetFileNameWithoutExtension(knxprojSourceFilePath)}/";
                managedToExtractProject = true;
            }
            
            return (!cancelOperation) && (managedToExtractProject);
        }


        
        // Fonction permettant de demander à l'utilisateur d'entrer un path
        private static string AskForPath()
        {
            try
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
            catch (InvalidOperationException ex)
            {
                // Gérer les exceptions liées à l'état non valide de l'OpenFileDialog
                App.ConsoleAndLogWriteLine($"Error: Could not open file dialog. Details: {ex.Message}");
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                // Gérer les exceptions liées aux erreurs internes des bibliothèques de l'OS
                App.ConsoleAndLogWriteLine($"Error: An external error occurred while trying to open the file dialog. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Gérer toutes autres exceptions génériques
                App.ConsoleAndLogWriteLine($"Error: An unexpected error occurred. Details: {ex.Message}");
            }

            return "";
        }

        
        
        // Fonction permettant de trouver un fichier dans un dossier donné
        private static string FindFile(string rootPath, string fileNameToSearch)
        {
            if (!Directory.Exists(rootPath))
            {
                App.ConsoleAndLogWriteLine($"Directory {rootPath} does not exist.");
                return "";
            }

            // Création d'une file d'attente pour les répertoires à explorer
            Queue<string> directoriesQueue = new Queue<string>();
            directoriesQueue.Enqueue(rootPath);

            while (directoriesQueue.Count > 0)
            {
                string currentDirectory = directoriesQueue.Dequeue();
                try
                {
                    // Vérifier les fichiers dans le répertoire actuel
                    string[] files = Directory.GetFiles(currentDirectory);
                    foreach (string file in files)
                    {
                        if (Path.GetFileName(file).Equals(fileNameToSearch, StringComparison.OrdinalIgnoreCase))
                        {
                            return file; // Fichier trouvé, on retourne son chemin
                        }
                    }

                    // Ajouter les sous-répertoires à la file d'attente
                    string[] subDirectories = Directory.GetDirectories(currentDirectory);
                    foreach (string subDirectory in subDirectories)
                    {
                        directoriesQueue.Enqueue(subDirectory);
                    }
                }
                catch (UnauthorizedAccessException unAuthEx)
                {
                    // Si l'accès au répertoire est refusé
                    App.ConsoleAndLogWriteLine($"Access refused to {currentDirectory} : {unAuthEx.Message}");
                }
                catch (DirectoryNotFoundException dirNotFoundEx)
                {
                    // Si le répertoire est introuvable
                    App.ConsoleAndLogWriteLine($"Directory not found : {currentDirectory} : {dirNotFoundEx.Message}");
                }
                catch (IOException ioEx)
                {
                    // Si une erreur d'entrée/sortie survient
                    App.ConsoleAndLogWriteLine($"I/O Error while accessing {currentDirectory} : {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    // Gérer toutes autres exceptions génériques
                    App.ConsoleAndLogWriteLine($"An unexpected error occurred while accessing {currentDirectory} : {ex.Message}");
                }
            }
            return ""; // Fichier non trouvé
        }

        
        
        // Fonction permettant de trouver le fichier 0.xml dans le projet exporté
        // ATTENTION: Nécessite que le projet .knxproj ait déjà été extrait avec la fonction extractProjectFiles().
        public async Task FindZeroXml()
        {
            string searchingFile, foundFile;
            
            // Traduction des textes de la fenêtre de chargement
            switch (App.DisplayElements?.SettingsWindow?.AppLang)
            {
                // Arabe
                case "AR":
                    searchingFile = "البحث عن الملف '0.xml' ...";
                    foundFile = "تم العثور على الملف '0.xml'.";
                    break;

                // Bulgare
                case "BG":
                    searchingFile = "Търсене на файла '0.xml' ...";
                    foundFile = "Файлът '0.xml' е намерен.";
                    break;

                // Tchèque
                case "CS":
                    searchingFile = "Hledání souboru '0.xml' ...";
                    foundFile = "Soubor '0.xml' nalezen.";
                    break;

                // Danois
                case "DA":
                    searchingFile = "Søger efter filen '0.xml' ...";
                    foundFile = "Filen '0.xml' fundet.";
                    break;

                // Allemand
                case "DE":
                    searchingFile = "Suche nach Datei '0.xml' ...";
                    foundFile = "Datei '0.xml' gefunden.";
                    break;

                // Grec
                case "EL":
                    searchingFile = "Αναζήτηση του αρχείου '0.xml' ...";
                    foundFile = "Το αρχείο '0.xml' βρέθηκε.";
                    break;

                // Anglais
                case "EN":
                    searchingFile = "Searching for file '0.xml' ...";
                    foundFile = "File '0.xml' found.";
                    break;

                // Espagnol
                case "ES":
                    searchingFile = "Buscando el archivo '0.xml' ...";
                    foundFile = "Archivo '0.xml' encontrado.";
                    break;

                // Estonien
                case "ET":
                    searchingFile = "Otsitakse faili '0.xml' ...";
                    foundFile = "Fail '0.xml' leitud.";
                    break;

                // Finnois
                case "FI":
                    searchingFile = "Etsitään tiedostoa '0.xml' ...";
                    foundFile = "Tiedosto '0.xml' löytyi.";
                    break;

                // Hongrois
                case "HU":
                    searchingFile = "A(z) '0.xml' fájl keresése ...";
                    foundFile = "A(z) '0.xml' fájl megtalálva.";
                    break;

                // Indonésien
                case "ID":
                    searchingFile = "Mencari file '0.xml' ...";
                    foundFile = "File '0.xml' ditemukan.";
                    break;

                // Italien
                case "IT":
                    searchingFile = "Ricerca del file '0.xml' ...";
                    foundFile = "File '0.xml' trovato.";
                    break;

                // Japonais
                case "JA":
                    searchingFile = "'0.xml'ファイルを検索中 ...";
                    foundFile = "'0.xml'ファイルが見つかりました。";
                    break;

                // Coréen
                case "KO":
                    searchingFile = "'0.xml' 파일 검색 중 ...";
                    foundFile = "'0.xml' 파일을 찾았습니다.";
                    break;

                // Letton
                case "LV":
                    searchingFile = "Meklēju failu '0.xml' ...";
                    foundFile = "Fails '0.xml' atrasts.";
                    break;

                // Lituanien
                case "LT":
                    searchingFile = "Ieškoma failo '0.xml' ...";
                    foundFile = "Failas '0.xml' rastas.";
                    break;

                // Norvégien
                case "NB":
                    searchingFile = "Søker etter filen '0.xml' ...";
                    foundFile = "Filen '0.xml' funnet.";
                    break;

                // Néerlandais
                case "NL":
                    searchingFile = "Zoeken naar bestand '0.xml' ...";
                    foundFile = "Bestand '0.xml' gevonden.";
                    break;

                // Polonais
                case "PL":
                    searchingFile = "Wyszukiwanie pliku '0.xml' ...";
                    foundFile = "Plik '0.xml' znaleziony.";
                    break;

                // Portugais
                case "PT":
                    searchingFile = "Procurando o arquivo '0.xml' ...";
                    foundFile = "Arquivo '0.xml' encontrado.";
                    break;

                // Roumain
                case "RO":
                    searchingFile = "Căutare fișier '0.xml' ...";
                    foundFile = "Fișierul '0.xml' găsit.";
                    break;

                // Russe
                case "RU":
                    searchingFile = "Поиск файла '0.xml' ...";
                    foundFile = "Файл '0.xml' найден.";
                    break;

                // Slovaque
                case "SK":
                    searchingFile = "Hľadanie súboru '0.xml' ...";
                    foundFile = "Súbor '0.xml' nájdený.";
                    break;

                // Slovène
                case "SL":
                    searchingFile = "Iskanje datoteke '0.xml' ...";
                    foundFile = "Datoteka '0.xml' najdena.";
                    break;

                // Suédois
                case "SV":
                    searchingFile = "Söker efter filen '0.xml' ...";
                    foundFile = "Filen '0.xml' hittades.";
                    break;

                // Turc
                case "TR":
                    searchingFile = "'0.xml' dosyası aranıyor ...";
                    foundFile = "'0.xml' dosyası bulundu.";
                    break;

                // Ukrainien
                case "UK":
                    searchingFile = "Пошук файлу '0.xml' ...";
                    foundFile = "Файл '0.xml' знайдено.";
                    break;

                // Chinois simplifié
                case "ZH":
                    searchingFile = "正在搜索文件 '0.xml' ...";
                    foundFile = "文件 '0.xml' 已找到。";
                    break;

                // Langue par défaut (français)
                default:
                    searchingFile = "Recherche du fichier '0.xml' ...";
                    foundFile = "Fichier '0.xml' trouvé.";
                    break;
            }
            
            App.DisplayElements.LoadingWindow.LogActivity(searchingFile);
            
            try
            {
                string foundPath = FindFile(ProjectFolderPath, "0.xml");

                // Si le fichier n'a pas été trouvé
                if (string.IsNullOrEmpty(foundPath))
                {
                    App.ConsoleAndLogWriteLine("Unable to find the file '0.xml' in the project folders. "
                                               + "Please ensure that the extracted archive is indeed a KNX ETS project.");
                    // Utilisation de Dispatcher.Invoke pour fermer l'application depuis un thread non-UI
                    await Application.Current.Dispatcher.InvokeAsync(() => Application.Current.Shutdown());
                }
                else // Sinon
                {
                    ZeroXmlPath = foundPath;
                    App.ConsoleAndLogWriteLine($"Found '0.xml' file at {Path.GetFullPath(ZeroXmlPath)}.");
                    App.DisplayElements.LoadingWindow.MarkActivityComplete();
                    App.DisplayElements.LoadingWindow.LogActivity(foundFile);
                }
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                // Gérer les erreurs d'accès non autorisé
                App.ConsoleAndLogWriteLine($"Access refused while searching for '0.xml': {unAuthEx.Message}");
            }
            catch (DirectoryNotFoundException dirNotFoundEx)
            {
                // Gérer les erreurs où le répertoire n'est pas trouvé
                App.ConsoleAndLogWriteLine($"Directory not found while searching for '0.xml': {dirNotFoundEx.Message}");
            }
            catch (IOException ioEx)
            {
                // Gérer les erreurs d'entrée/sortie
                App.ConsoleAndLogWriteLine($"I/O Error while searching for '0.xml': {ioEx.Message}");
            }
            catch (Exception ex)
            {
                // Gérer toutes autres exceptions génériques
                App.ConsoleAndLogWriteLine($"An unexpected error occurred while searching for '0.xml': {ex.Message}");
            }
        }
    }
}
