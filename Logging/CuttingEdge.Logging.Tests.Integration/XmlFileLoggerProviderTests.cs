using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Integration
{
    [TestClass]
    public class XmlFileLoggerProviderTests
    {
        [TestMethod]
        public void Initialize_WithValidPath_CreatesFile()
        {
            // Arrange
            string path = "error.log";

            Assert.IsFalse(File.Exists(path), "Test setup failed.");

            try
            {
                // Act
                new XmlFileLoggingProvider(LoggingEventType.Debug, path, null);

                // Assert
                Assert.IsTrue(File.Exists(path), "The provider did not create the file during initialization.");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void Log_WhenLogFileDoesNotExists_CreatesFileAndLogsText()
        {
            // Arrange
            string path = "error.log";

            Assert.IsFalse(File.Exists(path), "Test setup failed.");

            var provider = new XmlFileLoggingProvider(LoggingEventType.Debug, path, null);

            File.Delete(path);

            Assert.IsFalse(File.Exists(path), "Test setup failed.");

            try
            {
                // Act
                provider.Log("Some message");

                // Assert
                Assert.IsTrue(File.Exists(path), "The provider did not create the file and did not log.");
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}