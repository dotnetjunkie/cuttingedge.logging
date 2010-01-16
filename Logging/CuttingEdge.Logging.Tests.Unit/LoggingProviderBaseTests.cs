using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests the <see cref="LoggingProviderBase"/> class.
    /// </summary>
    [TestClass]
    public class LoggingProviderBaseTests
    {
        private const string MissingConfigSectionsElementExceptionMessage =
            "The Logger has not been configured properly. Please register the logger as <section> element " +
            "in the <configSections> of your configurations file as follows: " +
            "<configSections><section name=\"logging\" type=\"CuttingEdge.Logging.LoggingSection, " +
            "CuttingEdge.Logging\" allowDefinition=\"MachineToApplication\" /></configSections>.";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullArgument_ThrowsException()
        {
            // Arrange
            ILogger provider = new FakeLoggingProviderBase();

            // Act
            provider.Log(null);
        }

        [TestMethod]
        public void Log_WithValidEntry_CallsLogInternal()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var validLogEntry = new LogEntry(LoggingEventType.Error, "message", "source", new Exception());

            // Act
            ((ILogger)provider).Log(validLogEntry);

            // Assert
            Assert.AreEqual(validLogEntry, provider.LastEntry);
        }

        [TestMethod]
        public void Log_WithThresholdHigherThanSuppliedMessage_DoesNotLog()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var configurationWithHighTreshold = new NameValueCollection();
            configurationWithHighTreshold.Add("threshold", "Information");
            provider.Initialize("Valid name", configurationWithHighTreshold);

            var validLogEntryWithLogSeverity = new LogEntry(LoggingEventType.Debug, "message", null, null);

            // Act
            ((ILogger)provider).Log(validLogEntryWithLogSeverity);

            // Assert
            Assert.IsNull(provider.LastEntry, "The entry should not be logged.");
        }

        [TestMethod]
        public void Initialize_WithEmptyConfiguration_Succeeds()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var emptyConfiguration = new NameValueCollection();
            var validProviderName = "Valid name";

            // Act
            // Using an empty configuration should succeed, the 'threshold' and 'fallbackProvider' attributes
            // are optional.
            provider.Initialize(validProviderName, emptyConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        public void Initialize_WithNonEmptyProviderName_SetsNamePropertyToSuppliedValue()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var validConfiguration = new NameValueCollection();
            var expectedProviderName = "Valid name";

            // Act
            provider.Initialize(expectedProviderName, validConfiguration);

            // Assert
            Assert.AreEqual(expectedProviderName, provider.Name);
        }

        [TestMethod]
        public void Initialize_WithNullProviderName_SetNamePropertyToTypeName()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var validConfiguration = new NameValueCollection();
            string emptyProviderName = null;
            var expectedProviderName = "FakeLoggingProviderBase";

            // Act
            provider.Initialize(emptyProviderName, validConfiguration);

            // Assert
            Assert.AreEqual(expectedProviderName, provider.Name);
        }

        [TestMethod]
        public void Initialize_WithConfigurationWithoutThreshold_SetsThesholdPropertyToDebug()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var configurationWithoutThreshold = new NameValueCollection();
            var expectedThreshold = LoggingEventType.Debug;

            // Act
            provider.Initialize("Valid provider name", configurationWithoutThreshold);

            // Assert
            Assert.AreEqual(expectedThreshold, provider.Threshold);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initiailze_WithConfigurationWithInvalidThreshold_ThrowsException()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var configurationWithInvalidThreshold = new NameValueCollection();
            configurationWithInvalidThreshold.Add("threshold", "Incorrect threshold");

            // Act
            provider.Initialize("Valid provider name", configurationWithInvalidThreshold);
        }

        [TestMethod]
        public void Initialize_WithConfigurationWithThreshold_SetsThesholdPropertyToSuppliedValue()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var configurationWithThreshold = new NameValueCollection();
            configurationWithThreshold.Add("threshold", "Critical");
            var expectedThreshold = LoggingEventType.Critical;

            // Act
            provider.Initialize("Valid provider name", configurationWithThreshold);

            // Assert
            Assert.AreEqual(expectedThreshold, provider.Threshold);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialize_CallsTwice_ThrowsException()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var validConfiguration = new NameValueCollection();
            var validProviderName = "Valid provider name";

            // Act
            provider.Initialize(validProviderName, validConfiguration);

            // This second call should fail.
            provider.Initialize(validProviderName, validConfiguration);
        }

        [TestMethod]
        public void Initialize_WithUnrecognizedAttribute_Succeeds()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var configuration = new NameValueCollection();
            configuration.Add("badAttribute", "some value");

            // Act
            // While the initialization has an unrecognized attribute, the initialization still succeeds.
            // Classes inheriting from LoggingProviderBase should validate and throw an exception.
            provider.Initialize("Valid provider name", configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Log_WithFailingImplementationButWithoutConfiguredFallbackProvider_BubblesThrownException()
        {
            // Arrange
            var provider = new ThrowingLoggingProvider();
            provider.ExceptionToThrowFromLogInternal = new InvalidOperationException();
            
            // Act
            provider.Log("Valid message");
        }

#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void Log_WithFailingImplementationAndConfiguredFallbackProvider_LogsEventToFallbackProvider()
        {
            // Arrange
            var failingProvider = new ThrowingLoggingProvider();
            var fallbackProvider = new MemoryLoggingProvider();
            failingProvider.FallbackProvider = fallbackProvider;
            failingProvider.ExceptionToThrowFromLogInternal = new InvalidOperationException();
            var entryToLog = new LogEntry(LoggingEventType.Critical, "Valid message", null, null);

            // Act
            ((ILogger)failingProvider).Log(entryToLog);

            // Assert
            var loggedEntry = fallbackProvider.GetLoggedEntries().First();
            Assert.AreEqual(entryToLog, loggedEntry);
        }

        [TestMethod]
        public void Log_WithFailingImplementationAndConfiguredFallbackProvider_LogsOriginalFailureToFallbackProvider()
        {
            // Arrange
            var failingProvider = new ThrowingLoggingProvider();
            failingProvider.Initialize("Failing provider name", new NameValueCollection());

            var fallbackProvider = new MemoryLoggingProvider();
            failingProvider.FallbackProvider = fallbackProvider;
            failingProvider.ExceptionToThrowFromLogInternal = new InvalidOperationException();
            var expectedEntry =
                new LogEntry(LoggingEventType.Error,
                    "The event could not be logged with provider 'Failing provider name'.",
                    "CuttingEdge.Logging.Tests.Unit.LoggingProviderBaseTests+ThrowingLoggingProvider", 
                    failingProvider.ExceptionToThrowFromLogInternal);

            // Act
            failingProvider.Log("Valid message");

            // Assert
            var loggedEntry = fallbackProvider.GetLoggedEntries().Last();
            AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
        }

        [TestMethod]
        public void FakeLoggingProviderBase_WithNullName_UsesProviderTypeName()
        {
            // Arrange
            var provider = new FakeLoggingProviderBase();
            var configuration = new NameValueCollection();
            configuration.Add("invalid attribute", "some value");

            try
            {
                // Act
                provider.CheckForUnrecognizedAttributes(null, configuration);
                
                // Assert
                Assert.Fail("Expection expected.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("FakeLoggingProviderBase"),
                    "The exception should contain the type name of the provider.");
            }
        }
#endif

        [TestMethod]
        public void Configuration_WithoutAnyCustomAttributes_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "MemProv",
                    Providers =
                    {
                        // <provider name="MemProv" type="CE.Logging.MemoryLoggingProvider, CE.Logging" />
                        new ProviderConfigLine()
                        {
                            Name = "MemProv",
                            Type = typeof(MemoryLoggingProvider),
                        }
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_WithoutProviders_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "MemoryLoggingProvider"
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("No default Logging provider was found"),
                        "Exception message does contain expected string. Actual: {0}", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_WithoutDefaultProvider_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = string.Empty,
                    Providers =
                    {
                        // <provider name="MemoryLoggingProvider" type="...MemoryLoggingProvider..." />
                        new ProviderConfigLine()
                        {
                            Name = "MemoryLoggingProvider",
                            Type = typeof(MemoryLoggingProvider),
                        }
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("No default Logging provider was found"),
                        "Exception message does not contain expected string. Actual: {0}", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_MissingConfigSectionsElementInConfigFile_ThrowsException()
        {
            // Arrange
            string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?><configuration></configuration>";
            var config = new TemplatedConfigurationWriter(xml);

            using (var manager = new UnitTestAppDomainManager(config))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ProviderException ex)
                {
                    Assert.AreEqual(MissingConfigSectionsElementExceptionMessage, ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_TypeThatNotInheritsFromLoggingProviderBase_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "NonLoggingRelatedProvider",
                    Providers =
                    {
                        new ProviderConfigLine("NonLoggingRelatedProvider", typeof(NonLoggingRelatedProvider))
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("CuttingEdge.Logging.LoggingProviderBase."),
                        "Incorrect exception message returned: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_WithCircularReferencingProviders_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "P1",
                    Providers =
                    {
                        new ProviderConfigLine("P1", typeof(MemoryLoggingProvider), "fallbackProvider=\"P2\""),
                        new ProviderConfigLine("P2", typeof(MemoryLoggingProvider), "fallbackProvider=\"P3\""),
                        new ProviderConfigLine("P3", typeof(MemoryLoggingProvider), "fallbackProvider=\"P1\""),
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("circular"), 
                        "The exception should contain 'circular'. Actual message: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_WithNonExistingFallbackProvider_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "P1",
                    Providers =
                    {
                        new ProviderConfigLine("P1", typeof(MemoryLoggingProvider), "fallbackProvider=\"P2\""),
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ProviderException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("No provider with that name could be found."), 
                        "Exception message does not contain expected string. Actual message: " + ex.Message);
                }
            }
        }

        private sealed class NonLoggingRelatedProvider : ProviderBase
        {
        }

        private sealed class FakeLoggingProviderBase : LoggingProviderBase
        {
            public LogEntry LastEntry { get; private set; }

            public new void CheckForUnrecognizedAttributes(string name, NameValueCollection config)
            {
                // This method is protected.
                base.CheckForUnrecognizedAttributes(name, config);
            }

            protected override object LogInternal(LogEntry entry)
            {
                this.LastEntry = entry;
                return null;
            }
        }

        private sealed class ThrowingLoggingProvider : LoggingProviderBase
        {
            public Exception ExceptionToThrowFromLogInternal { get; set; }

            protected override object LogInternal(LogEntry entry)
            {
                throw this.ExceptionToThrowFromLogInternal;
            }
        }
    }
}