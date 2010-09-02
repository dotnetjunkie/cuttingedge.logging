using System;
using System.Collections.Specialized;
using System.Configuration.Provider;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class TerminatorLoggingProviderTests
    {
        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);
        }

        [TestMethod]
        public void Log_InitializedProvider_Succeeds()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            provider.Initialize("Valid name", CreateValidConfiguration());

            // Act
            provider.Log("Some message");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithUnrecognizedAttributes_ThrowsException()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            var configurationWithUnrecognizedAttribute = CreateValidConfiguration();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid provider name", configurationWithUnrecognizedAttribute);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithFallbackProviderAttribute_ThrowsException()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration.Add("fallbackProvider", "some value");

            try
            {
                // Act
                // The 'fallbackProvider' attribute is not supported by the TerminatorLoggingProvider, because
                // it is of no use.
                provider.Initialize("Valid provider name", invalidConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains("fallbackProvider"),
                    "The exception message should contain the name of the invalid attribute.");
            }
        }

        [TestMethod]
        public void Initialize_ConfigurationWithThresholdAttribute_ThrowsException()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration.Add("threshold", "some value");

            try
            {
                // Act
                // The 'threshold' attribute is not supported by the TerminatorLoggingProvider, because it is
                // of no use.
                provider.Initialize("Valid provider name", invalidConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains("threshold"),
                    "The exception message should contain the name of the invalid attribute.");
            }
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Terminator logging provider";
            var provider = new TerminatorLoggingProvider();
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
            var expectedDescription = "My terminator, I'll be back :-)";
            var provider = new TerminatorLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Log_ProviderInitializedWithDefaultConstructor_LogsSuccesfully()
        {
            // Arrange
            string expectedMessage = "Some message";

            var provider = new TerminatorLoggingProvider();

            // Act
            // In contrast with most other providers, this provider should succeed in logging the event when
            // it was created with the default constructor, and not initialized with Initialize(string, NVC).
            // This behavior is different, because the the only initialization argument the provider needs is
            // the severity, which will be retain its default value of 'Debug' when not set.
            provider.Log(LoggingEventType.Debug, expectedMessage);
        }

        [TestMethod]
        public void Log_WithValidLogEntry_ReturnsNull()
        {
            // Arrange
            ILogger provider = new TerminatorLoggingProvider();
            var entry = new LogEntry(LoggingEventType.Error, "error", null, null);

            // Act
            object id = provider.Log(entry);

            // Assert
            Assert.IsNull(id, "Provider should always return null.");
        }

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Terminator",
                    Providers =
                    {
                        // <provider name="Terminator" type="CE.Logging.TerminatorLoggingProvider, ..." />
                        new ProviderConfigLine()
                        {
                            Name = "Terminator",
                            Type = typeof(TerminatorLoggingProvider),
                        }
                    }
                },
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
    }
}
