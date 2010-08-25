using System;
using System.Collections.Specialized;
using System.Configuration.Provider;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests the <see cref="ConsoleLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class ConsoleLoggingProviderTests
    {
#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void Log_ProviderInitializedWithDefaultConstructor_LogsSuccesfully()
        {
            // Arrange
            string expectedMessage = "Some message";

            var provider = new FakeConsoleLoggingProvider();

            // Act
            // In contrast with most other providers, this provider should succeed in logging the event when
            // it was created with the default constructor, and not initialized with Initialize(string, NVC).
            // This behavior is different, because the the only initialization argument the provider needs is
            // the severity, which will be retain its default value of 'Debug' when not set.
            provider.Log(LoggingEventType.Debug, expectedMessage);

            // Arrange
            Assert.IsTrue(provider.TextWrittenToConsole.Contains(expectedMessage),
                "The expected message was not logged. Actual: " + provider.TextWrittenToConsole);
        }

        [TestMethod]
        public void Log_ProviderInitializedWithDebugThresholdThroughConstructor_LogsMessage()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new FakeConsoleLoggingProvider(LoggingEventType.Debug);
            
            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.IsTrue(provider.TextWrittenToConsole.Contains(expectedMessage), "Message not logged");
        }

        [TestMethod]
        public void Log_ProviderInitializedWithDebugThresholdThroughConstructor_Succeeds()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new ConsoleLoggingProvider(LoggingEventType.Debug);

            // Act
            provider.Log(expectedMessage);
        }

        [TestMethod]
        public void Log_ProviderInitializedWithCriticalThresholdThroughConstructor_DoesNotLogMessage()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new FakeConsoleLoggingProvider(LoggingEventType.Critical);

            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.AreEqual(string.Empty, provider.TextWrittenToConsole, 
                "Actual text: " + provider.TextWrittenToConsole);
        }
        
        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new FakeConsoleLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

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
            var configurationWithUnrecognizedAttribute = CreateValidConfiguration();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid provider name", configurationWithUnrecognizedAttribute);
        }
        
        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Console logging provider";
            var provider = new ConsoleLoggingProvider();
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
            var expectedDescription = "My console logger";
            var provider = new ConsoleLoggingProvider();
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
#endif

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Console",
                    Providers =
                    {
                        // <provider name="Console" type="CuttingEdge.Logging.ConsoleLoggingProvider, ..." />
                        new ProviderConfigLine()
                        {
                            Name = "Console", 
                            Type = typeof(ConsoleLoggingProvider),
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
        private sealed class FakeConsoleLoggingProvider : ConsoleLoggingProvider
        {
            private string textWrittenToConsole = string.Empty;

            public FakeConsoleLoggingProvider(LoggingEventType threshold) : base(threshold)
            {
                this.OverrideDelegate();
            }

            public FakeConsoleLoggingProvider()
            {
                this.OverrideDelegate();
            }

            public string TextWrittenToConsole
            {
                get { return this.textWrittenToConsole; }
            }

            private void OverrideDelegate()
            {
                // Override the delegate to redirect the output.
                this.SetWriteToConsole((formattedEvent) => this.textWrittenToConsole += formattedEvent);
            }
        }
#endif
    }
}