using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
#if DEBUG // This test code only runs in debug mode
    /// <summary>
    /// Tests the <see cref="DebugLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class DebugLoggingProviderTests
    {
        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new FakeDebugLoggingProvider();
            var validConfiguration = new NameValueCollection();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeDebugLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithUnrecognizedAttributes_ThrowsException()
        {
            // Arrange
            var provider = new FakeDebugLoggingProvider();
            var configurationWithUnrecognizedAttribute = new NameValueCollection();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid provider name", configurationWithUnrecognizedAttribute);
        }

        [TestMethod]
        public void Log_WithValidEntry_WritesExpectedTextToConsole()
        {
            // Arrange
            var provider = new FakeDebugLoggingProvider();
            var validConfiguration = new NameValueCollection();
            provider.Initialize("Valid provider name", validConfiguration);
            var expectedText =
                "LoggingEvent:\r\n" +
                "Severity:\tError\r\n" +
                "Message:\tValid message\r\n";

            // Act
            provider.Log(LoggingEventType.Error, "Valid message");

            // Assert
            string actualText = provider.TextWrittenToDebugWindow;
            Assert.AreEqual(expectedText, actualText);
        }

        private sealed class FakeDebugLoggingProvider : DebugLoggingProvider
        {
            public FakeDebugLoggingProvider()
            {
                Action<string> writeToDebugWindow =
                    (formattedEvent) => this.TextWrittenToDebugWindow += formattedEvent;

                // Override the delegate to redirect the output.
                this.SetWriteToDebugWindow(writeToDebugWindow);
            }

            public string TextWrittenToDebugWindow { get; private set; }
        }
    }
#endif
}
