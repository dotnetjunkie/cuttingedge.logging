using System;
using System.Collections.Specialized;
using System.Configuration.Provider;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests the <see cref="DebugLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class DebugLoggingProviderTests
    {
#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void Log_ProviderInitializedWithDebugThresholdThroughConstructor_LogsMessage()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new FakeDebugLoggingProvider(LoggingEventType.Debug);

            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.IsTrue(provider.TextWrittenToDebugWindow.Contains(expectedMessage), "Message not logged");
        }

        [TestMethod]
        public void Log_ProviderInitializedWithDebugThresholdThroughConstructor_Succeeds()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new DebugLoggingProvider(LoggingEventType.Debug);

            // Act
            provider.Log(expectedMessage);
        }

        [TestMethod]
        public void Log_ProviderInitializedWithCriticalThresholdThroughConstructor_DoesNotLogMessage()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new FakeDebugLoggingProvider(LoggingEventType.Critical);

            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.AreEqual(string.Empty, provider.TextWrittenToDebugWindow,
                "Actual text: " + provider.TextWrittenToDebugWindow);
        }

        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new FakeDebugLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

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
            var configurationWithUnrecognizedAttribute = CreateValidConfiguration();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid provider name", configurationWithUnrecognizedAttribute);
        }
        
        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Debug logging provider";
            var provider = new DebugLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithCustomDescription_SetsSpecifiedDescription()
        {
            // Arrange
            var expectedDescription = "My debug logger";
            var provider = new DebugLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
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
#endif

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Debug",
                    Providers =
                    {
                        // <provider name="Debug" type="CuttingEdge.Logging.DebugLoggingProvider, ..." />
                        new ProviderConfigLine()
                        {
                            Name = "Debug", 
                            Type = typeof(DebugLoggingProvider),
                        },
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        private static NameValueCollection CreateValidConfiguration()
        {
            return new NameValueCollection();
        }

#if DEBUG // This test code only runs in debug mode
        private sealed class FakeDebugLoggingProvider : DebugLoggingProvider
        {
            private string textWrittenToDebugWindow = string.Empty;

            public FakeDebugLoggingProvider(LoggingEventType threshold) : base(threshold)
            {
                this.OverrideDelegate();
            }

            public FakeDebugLoggingProvider()
            {
                this.OverrideDelegate();
            }

            public string TextWrittenToDebugWindow
            { 
                get { return this.textWrittenToDebugWindow; }
            }

            private void OverrideDelegate()
            {
                Action<string> writeToDebugWindow =
                    (formattedEvent) => this.textWrittenToDebugWindow += formattedEvent;

                // Override the delegate to redirect the output.
                this.SetWriteToDebugWindow(writeToDebugWindow);
            }
        }
#endif
    }
}