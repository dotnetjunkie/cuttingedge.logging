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
            var validConfiguration = new NameValueCollection();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);
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
            var configurationWithUnrecognizedAttribute = new NameValueCollection();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid provider name", configurationWithUnrecognizedAttribute);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithFallbackProviderAttribute_ThrowsException()
        {
            // Arrange
            var provider = new TerminatorLoggingProvider();
            var configurationWithFallbackProviderAttribute = new NameValueCollection();
            configurationWithFallbackProviderAttribute.Add("fallbackProvider", "some value");

            try
            {
                // Act
                // The 'fallbackProvider' attribute is not supported by the TerminatorLoggingProvider, because
                // it is of no use.
                provider.Initialize("Valid provider name", configurationWithFallbackProviderAttribute);

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
            var configurationWithThresholdAttribute = new NameValueCollection();
            configurationWithThresholdAttribute.Add("threshold", "some value");

            try
            {
                // Act
                // The 'threshold' attribute is not supported by the TerminatorLoggingProvider, because it is
                // of no use.
                provider.Initialize("Valid provider name", configurationWithThresholdAttribute);

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
    }
}
