using System;
using System.Collections.Specialized;
using System.Configuration.Provider;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
#if DEBUG // This test code only runs in debug mode
    /// <summary>
    /// Tests the <see cref="ConsoleLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class ConsoleLoggingProviderTests
    {
        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new FakeConsoleLoggingProvider();
            var validConfiguration = new NameValueCollection();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeConsoleLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithUnrecognizedAttributes_ThrowsException()
        {
            // Arrange
            var provider = new FakeConsoleLoggingProvider();
            var configurationWithUnrecognizedAttribute = new NameValueCollection();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid provider name", configurationWithUnrecognizedAttribute);
        }

        [TestMethod]
        public void Log_WithValidEntry_WritesExpectedTextToConsole()
        {
            // Arrange
            var provider = new FakeConsoleLoggingProvider();
            var validConfiguration = new NameValueCollection();
            provider.Initialize("Valid provider name", validConfiguration);
            var expectedText =
                "LoggingEvent:\r\n" +
                "Severity:\tError\r\n" +
                "Message:\tValid message\r\n";

            // Act
            provider.Log(LoggingEventType.Error, "Valid message");

            // Assert
            string actualText = provider.TextWrittenToConsole;
            Assert.AreEqual(expectedText, actualText);
        }

        private sealed class FakeConsoleLoggingProvider : ConsoleLoggingProvider
        {
            public FakeConsoleLoggingProvider()
            {
                // Override the delegate to redirect the output.
                this.SetWriteToConsole((formattedEvent) => this.TextWrittenToConsole += formattedEvent);
            }

            public string TextWrittenToConsole { get; private set; }
        }
    }
#endif
}
