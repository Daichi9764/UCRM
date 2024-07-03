using System.Reflection; // Ajouter cette directive using pour MethodInfo et BindingFlags
using NUnit.Framework;
using System.IO;

using KNXBoostDesktop;

namespace KNXBoostDesktop.Tests
{
    [TestFixture]
    public class ProjectFileManagerTests
    {
        private string tempFolderPath;

        [SetUp]
        public void Setup()
        {
            // Créer un dossier temporaire pour les tests
            tempFolderPath =
                @"C:\Users\MINIPC\Desktop\UCRM\KNXBoostDesktop\KNXBoostDesktop\bin\Debug\net8.0-windows\logs";//Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //Directory.CreateDirectory(tempFolderPath);
            Console.WriteLine(tempFolderPath);
        }

        [TearDown]
        public void Cleanup()
        {
            // Supprimer le dossier temporaire après les tests
            Directory.Delete(tempFolderPath, true);
        }

        [Test]
        public void Constructor_ShouldInitializeProjectFolderPath()
        {
            // Arrange
            string expectedPath = "test_path";

            // Act
            var fileManager = new ProjectFileManager(expectedPath);

            // Assert
            Assert.AreEqual(expectedPath, fileManager.ProjectFolderPath);
        }

        [Test]
        public void ExtractProjectFiles_WhenSourcePathIsNull_ShouldCancelOperation()
        {
            // Arrange
            var fileManager = new ProjectFileManager();

            // Act
            bool result = fileManager.ExtractProjectFiles(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ExtractProjectFiles_WhenSourcePathIsEmpty_ShouldAskForPath()
        {
            // Arrange
            var fileManager = new ProjectFileManager();

            // Act
            bool result = fileManager.ExtractProjectFiles("");

            // Assert - You might assert specific behavior here based on your application flow
            Assert.IsFalse(result);
        }

        [Test]
        public void ExtractProjectFiles_WhenFileNotFound_ShouldAskForPath()
        {
            // Arrange
            var fileManager = new ProjectFileManager();

            // Act
            bool result = fileManager.ExtractProjectFiles("non_existing_file.knxproj");

            // Assert - You might assert specific behavior here based on your application flow
            Assert.IsFalse(result);
        }

        [Test]
        public void ExtractProjectFiles_WhenValidPath_ShouldExtractSuccessfully()
        {
            // Arrange
            var fileManager = new ProjectFileManager(tempFolderPath);
            string knxprojFilePath = Path.Combine(tempFolderPath, "test.knxproj");
            File.Create(knxprojFilePath); // Créer un fichier factice

            // Act
            bool result = fileManager.ExtractProjectFiles(knxprojFilePath);

            // Assert - You might assert specific behavior here based on your application flow
            Assert.IsTrue(result);
        }

        [Test]
        public void FindFile_WhenDirectoryDoesNotExist_ShouldReturnEmpty()
        {
            // Arrange
            var fileManager = new ProjectFileManager();

            // Utilisation de la réflexion pour accéder à la méthode interne FindFile
            MethodInfo methodInfo = typeof(ProjectFileManager).GetMethod("FindFile", BindingFlags.NonPublic | BindingFlags.Static);
            object[] parameters = { "non_existing_directory", "file.txt" };

            // Act
            string result = (string)methodInfo.Invoke(null, parameters);

            // Assert
            Assert.AreEqual("", result);
        }

        [Test]
        public void FindFile_WhenFileExists_ShouldReturnFilePath()
        {
            // Arrange
            var fileManager = new ProjectFileManager(tempFolderPath);
            string testFilePath = Path.Combine(tempFolderPath, "file.txt");
            File.Create(testFilePath); // Créer un fichier factice

            // Utilisation de la réflexion pour accéder à la méthode interne FindFile
            var methodInfo = typeof(ProjectFileManager).GetMethod("FindFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            object[] parameters = { tempFolderPath, "file.txt" };

            // Act
            string result = (string)methodInfo.Invoke(null, parameters);

            // Assert
            Assert.AreEqual(testFilePath, result);
        }

        [Test]
        public void FindZeroXml_WhenFileExists_ShouldSetZeroXmlPath()
        {
            // Arrange
            var fileManager = new ProjectFileManager(tempFolderPath);
            string zeroXmlFilePath = Path.Combine(tempFolderPath, "0.xml");
            File.Create(zeroXmlFilePath); // Créer un fichier '0.xml'

            // Act
            fileManager.FindZeroXml();

            // Assert
            Assert.AreEqual(zeroXmlFilePath, fileManager.ZeroXmlPath);
        }

        [Test]
        public void FindZeroXml_WhenFileDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var fileManager = new ProjectFileManager(tempFolderPath);

            // Act & Assert
            Assert.Throws<IOException>(() => fileManager.FindZeroXml());
        }
    }
}
