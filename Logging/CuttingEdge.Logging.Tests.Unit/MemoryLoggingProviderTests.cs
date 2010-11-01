using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests the <see cref="MemoryLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class MemoryLoggingProviderTests
    {
        [TestMethod]
        public void Log_ProviderInitializedWithDebugThresholdThroughConstructor_LogsMessage()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new MemoryLoggingProvider(LoggingEventType.Debug);

            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.AreEqual(1, provider.GetLoggedEntries().Length, "Message not logged");
        }

        [TestMethod]
        public void Log_InitializedProvider_Succeeds()
        {
            // Arrange
            var provider = new MemoryLoggingProvider();
            provider.Initialize("Valid name", CreateValidConfiguration());

            // Act
            provider.Log("Some message");
        }

        [TestMethod]
        public void Log_ProviderInitializedWithCriticalThresholdThroughConstructor_DoesNotLogMessage()
        {
            // Arrange
            var expectedMessage = "Hello";
            var provider = new MemoryLoggingProvider(LoggingEventType.Critical);

            // Act
            provider.Log(expectedMessage);

            // Assert
            Assert.AreEqual(0, provider.GetLoggedEntries().Length, "Message was expected not to be logged");
        }

        [TestMethod]
        public void Log_ProviderInitializedWithDefaultConstructor_LogsSuccesfully()
        {
            // Arrange
            string expectedMessage = "Some message";

            var provider = new MemoryLoggingProvider();

            // Act
            // In contrast with most other providers, this provider should succeed in logging the event when
            // it was created with the default constructor, and not initialized with Initialize(string, NVC).
            // This behavior is different, because the the only initialization argument the provider needs is
            // the severity, which will be retain its default value of 'Debug' when not set.
            provider.Log(LoggingEventType.Debug, expectedMessage);

            // Arrange
            Assert.AreEqual(1, provider.GetLoggedEntries().Length, "The provider did not log.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogInternal_WithNullArgument_ThrowsException()
        {
            // Arrange
            var provider = new FakeMemoryLoggingProvider();

            // Act
            provider.LogInternal(null);
        }

        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new MemoryLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new MemoryLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid name", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithUnrecognizedAttributes_ThrowsException()
        {
            // Arrange
            var provider = new MemoryLoggingProvider();
            var configurationWithUnrecognizedAttribute = CreateValidConfiguration();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid name", configurationWithUnrecognizedAttribute);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Memory logging provider";
            var provider = new MemoryLoggingProvider();
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
            var expectedDescription = "My memory logger";
            var provider = new MemoryLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Log_WithValidEntry_AddsEntryToCollection()
        {
            // Arrange
            MemoryLoggingProvider provider = CreateInitializedMemoryLoggingProvider();
            LogEntry expectedEntry = new LogEntry(LoggingEventType.Critical, "Valid message", null, null);

            // Act
            ((ILogger)provider).Log(expectedEntry);

            // Assert
            Assert.AreEqual(expectedEntry, provider.GetLoggedEntries().First());
        }

        [TestMethod]
        public void Clear_OnANonEmptyProvivder_ResultsInAnEmptyLoggedEntriesCollection()
        {
            // Arrange
            MemoryLoggingProvider provider = CreateInitializedMemoryLoggingProvider();
            provider.Log("Valid message");
            int expectedNumberOfEntriesAfterCallingClear = 0;

            // Act
            provider.Clear();

            // Assert
            Assert.AreEqual(expectedNumberOfEntriesAfterCallingClear, provider.GetLoggedEntries().Length);
        }

        [TestMethod]
        public void Log_WithValidEntry_ReturnsAnUniqueNumberPerCall()
        {
            // Arrange
            MemoryLoggingProvider provider = CreateInitializedMemoryLoggingProvider();

            // Act
            int id0 = (int)provider.Log("Valid message");
            int id1 = (int)provider.Log("Valid message");
            int id2 = (int)provider.Log("Valid message");
            int id3 = (int)provider.Log("Valid message");
            int id4 = (int)provider.Log("Valid message");

            // Assert
            Assert.AreEqual(0, id0);
            Assert.AreEqual(1, id1);
            Assert.AreEqual(2, id2);
            Assert.AreEqual(3, id3);
            Assert.AreEqual(4, id4);
        }

        [TestMethod]
        public void Configuration_WithSimplestPossibleConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "MemLogger",
                    Providers =
                    {
                        // <provider name="MemLogger" type="CuttingEdge.Logging.MemoryLoggingProvider, ..." />
                        new ProviderConfigLine()
                        {
                            Name = "MemLogger", 
                            Type = typeof(MemoryLoggingProvider),
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

        private static MemoryLoggingProvider CreateInitializedMemoryLoggingProvider()
        {
            var provider = new MemoryLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            provider.Initialize("MemoryLoggingProvider", validConfiguration);

            return provider;
        }

        private static NameValueCollection CreateValidConfiguration()
        {
            return new NameValueCollection();
        }

        private sealed class FakeMemoryLoggingProvider : MemoryLoggingProvider
        {
            public new object LogInternal(LogEntry entry)
            {
                return base.LogInternal(entry);
            }
        }
    }
}
