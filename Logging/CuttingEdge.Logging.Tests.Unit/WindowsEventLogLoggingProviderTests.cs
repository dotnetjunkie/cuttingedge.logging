using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
#if DEBUG // This test code only runs in debug mode
    /// <summary>
    /// Tests the <see cref="WindowsEventLogLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class WindowsEventLogLoggingProviderTests
    {
        [TestMethod]
        public void Initialize_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var provider = new FakeWindowsEventLogLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);
        }

        [TestMethod]
        public void Initialize_WithValidConfiguration_SetsSourceProperty()
        {
            // Arrange
            var expectedSource = "The expected source";
            var provider = new FakeWindowsEventLogLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["source"] = expectedSource;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedSource, provider.Source);
        }

        [TestMethod]
        public void Initialize_WithValidConfiguration_SetsLogNameProperty()
        {
            // Arrange
            var expectedLogName = "The expected log name";
            var provider = new FakeWindowsEventLogLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["logName"] = expectedLogName;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedLogName, provider.LogName);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Windows event log logging provider";
            var provider = new FakeWindowsEventLogLoggingProvider();
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
            var expectedDescription = "My Windows event logger";
            var provider = new FakeWindowsEventLogLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeWindowsEventLogLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid provider name", invalidConfiguration);
        }

        [TestMethod]
        public void Initialize_WithMissingSourceConfiguration_ThrowsException()
        {
            // Arrange
            var validProviderName = "Valid provider name";
            var provider = new FakeWindowsEventLogLoggingProvider();
            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration.Remove("source");

            try
            {
                // Act
                provider.Initialize(validProviderName, invalidConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains("'source'"),
                    "The exception message should contain the name of the attribute. Actual message: " + ex.Message);
                Assert.IsTrue(ex.Message.Contains(validProviderName),
                    "The exception message should contain the name of the provider. Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void Initialize_WithMissingLogNameConfiguration_ThrowsException()
        {
            // Arrange
            var validProviderName = "Valid provider name";
            var provider = new FakeWindowsEventLogLoggingProvider();
            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration.Remove("logName");

            try
            {
                // Act
                provider.Initialize(validProviderName, invalidConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains("'logName'"),
                    "The exception message should contain the name of the attribute. Actual message: " + ex.Message);
                Assert.IsTrue(ex.Message.Contains(validProviderName),
                    "The exception message should contain the name of the provider. Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        public void Initialize_WithUnrecognizedAttribute_ThrowException()
        {
            // Arrange
            var provider = new FakeWindowsEventLogLoggingProvider();
            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration.Add("bad attribute", "a bad value");

            try
            {
                // Act
                provider.Initialize("Valid provider name", invalidConfiguration);

                // Assert
                Assert.Fail("Initialize was expected to throw an Exception.");
            }
            catch (ProviderException ex)
            {
                Assert.IsTrue(ex.Message.Contains("bad attribute"), "The exception message should contain the name of the bad attribute.");
            }
        }

        [TestMethod]
        public void BuildEventLogMessage_WithSimpleLogEntry_BuildsExpectedLogMessage()
        {
            // Arrange
            var provider = new FakeWindowsEventLogLoggingProvider();
            var entry = new LogEntry(LoggingEventType.Error, "Test message", "This is a source", null);
            var expectedEventLogMessage = "Test message\r\nSeverity: Error\r\nSource: This is a source\r\n";

            // Act
            var actualEventLogMessage = provider.BuildEventLogMessage(entry);

            // Assert
            Assert.AreEqual(expectedEventLogMessage, actualEventLogMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildEventLogMessage_WithNullLogEntry_ThrowsException()
        {
            // Arrange
            var provider = new FakeWindowsEventLogLoggingProvider();
            LogEntry invalidEntry = null;

            // Act
            provider.BuildEventLogMessage(invalidEntry);
        }

        [TestMethod]
        public void BuildEventLogMessage_WithLogEntryWithThrownException_BuildsExpectedLogMessage()
        {
            // Arrange
            var provider = new FakeWindowsEventLogLoggingProvider();
            var exceptionMessage = "Invalid operation";
            Exception thrownException = GetThrownException(exceptionMessage);
            var entry =
                new LogEntry(LoggingEventType.Error, "Test message", "This is a source", thrownException);
            var expectedEventLogMessageStart =
                "Test message\r\n" +
                "Severity: Error\r\n" +
                "Source: This is a source\r\n" +
                "\r\n" +
                "Exception: System.InvalidOperationException\r\n" +
                "Message: Invalid operation\r\n" +
                "StackTrace:\r\n";

            // Act
            var actualEventLogMessage = provider.BuildEventLogMessage(entry);

            // Arrange
            Assert.IsTrue(actualEventLogMessage.StartsWith(expectedEventLogMessageStart),
                "EventLogMessage doesn't start with the expected text.");
            Assert.AreNotEqual(expectedEventLogMessageStart, actualEventLogMessage,
                "The actualEventLogMessage does not contain a stack trace.");
        }

        [TestMethod]
        public void Log_WithLoggingEventTypeCritical_LogsEventLogEntryTypeError()
        {
            // Arrange
            var loggedLoggingEventType = LoggingEventType.Critical;
            var expectedWindowsLogType = EventLogEntryType.Error;
            FakeWindowsEventLogLoggingProvider provider = BuildInitializedProvider();

            // Act
            provider.Log(loggedLoggingEventType, "Valid message");

            // Assert
            Assert.AreEqual(expectedWindowsLogType, provider.LoggedType);
        }

        [TestMethod]
        public void Log_WithLoggingEventTypeError_LogsEventLogEntryTypeError()
        {
            // Arrange
            var loggedLoggingEventType = LoggingEventType.Error;
            var expectedWindowsLogType = EventLogEntryType.Error;
            FakeWindowsEventLogLoggingProvider provider = BuildInitializedProvider();

            // Act
            provider.Log(loggedLoggingEventType, "Valid message");

            // Assert
            Assert.AreEqual(expectedWindowsLogType, provider.LoggedType);
        }

        [TestMethod]
        public void Log_WithLoggingEventTypeWarning_LogsEventLogEntryTypeWarning()
        {
            // Arrange
            var loggedLoggingEventType = LoggingEventType.Warning;
            var expectedWindowsLogType = EventLogEntryType.Warning;
            FakeWindowsEventLogLoggingProvider provider = BuildInitializedProvider();

            // Act
            provider.Log(loggedLoggingEventType, "Valid message");

            // Assert
            Assert.AreEqual(expectedWindowsLogType, provider.LoggedType);
        }

        [TestMethod]
        public void Log_WithLoggingEventTypeInformation_LogsEventLogEntryTypeInformation()
        {
            // Arrange
            var loggedLoggingEventType = LoggingEventType.Information;
            var expectedWindowsLogType = EventLogEntryType.Information;
            FakeWindowsEventLogLoggingProvider provider = BuildInitializedProvider();

            // Act
            provider.Log(loggedLoggingEventType, "Valid message");

            // Assert
            Assert.AreEqual(expectedWindowsLogType, provider.LoggedType);
        }

        [TestMethod]
        public void Log_WithLoggingEventTypeDebug_LogsEventWithoutEventLogEntryType()
        {
            // Arrange
            var loggedLoggingEventType = LoggingEventType.Debug;          
            FakeWindowsEventLogLoggingProvider provider = BuildInitializedProvider();

            // Act
            provider.Log(loggedLoggingEventType, "Valid message");

            // Assert
            Assert.AreEqual(null, provider.LoggedType);
        }

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "EventLog",
                    Providers =
                    {
                        // <provider name="EventLog" type="WindowsEventLogLoggingProvider,..." source=... />
                        new ProviderConfigLine()
                        {
                            Name = "EventLog", 
                            Type = typeof(WindowsEventLogLoggingProvider),
                            CustomAttributes = 
                                "source=\"CuttingEdge.Logging.Tests\" logName=\"CuttingEdge.Logging.Tests\" "
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

        private static FakeWindowsEventLogLoggingProvider BuildInitializedProvider()
        {
            var provider = new FakeWindowsEventLogLoggingProvider();

            var configuration = new NameValueCollection();
            configuration.Add("source", "a valid source name");
            configuration.Add("logName", "a valid log name");

            provider.Initialize(null, configuration);

            return provider;
        }

        private static Exception GetThrownException(string exceptionMessage)
        {
            Exception thrownException;

            try
            {
                throw new InvalidOperationException(exceptionMessage);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            return thrownException;
        }

        private static NameValueCollection CreateValidConfiguration()
        {
            var configuration = new NameValueCollection();

            configuration.Add("source", "a valid source name");
            configuration.Add("logName", "a valid log name");

            return configuration;
        }

        /// <summary>
        /// This class defines a public method that allows calling the protected BuildEventLogMessage method.
        /// </summary>
        private class FakeWindowsEventLogLoggingProvider : WindowsEventLogLoggingProvider
        {
            public FakeWindowsEventLogLoggingProvider()
            {
            }

            public EventLogEntryType? LoggedType { get; private set; }

            // A public method that can be called.
            public new string BuildEventLogMessage(LogEntry entry)
            {
                // This is a protected method.
                return base.BuildEventLogMessage(entry);
            }

            internal override void WriteEntryToEventLog(WindowsEventLogLoggingProvider provider, 
                string eventLogMessage)
            {
                this.LoggedType = null;
            }

            internal override void WriteEntryToEventLog(WindowsEventLogLoggingProvider provider, 
                string eventLogMessage, EventLogEntryType type)
            {
                this.LoggedType = type;
            }
        }
    }
#endif
}
