using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Management;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.Web
{
    [TestClass]
    public class LoggingWebEventProviderTests
    {
        [TestMethod]
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new LoggingWebEventProvider();
            var validProviderName = "Valid name";
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize(validProviderName, validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new LoggingWebEventProvider();

            // Act
            provider.Initialize("Valid name", null);
        }

        [TestMethod]
        public void Initialize_WithEmptyProviderName_GetsTypeNameAsProviderName()
        {
            // Arrange
            var expectedProviderName = "LoggingWebEventProvider";
            var provider = new LoggingWebEventProvider();
            var validEmptyProviderName = string.Empty;
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize(validEmptyProviderName, validConfiguration);

            // Arrange
            Assert.AreEqual(expectedProviderName, provider.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_WithUnrecognizedAttribute_ThrowsException()
        {
            // Arrange
            var provider = new LoggingWebEventProvider();
            var validProviderName = "Valid name";
            var invalidConfiguration = CreateValidConfiguration();
            invalidConfiguration.Add("bad attribute", "bad value");

            // Act
            provider.Initialize(validProviderName, invalidConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_WithIncorrectLoggingProviderName_LogsToExpectedProvider()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var provider = new LoggingWebEventProvider();
                var invalidConfiguration = CreateValidConfiguration();
                invalidConfiguration.Add("loggingProvider", "NON EXISTING PROVIDER");

                // Act
                provider.Initialize("Valid name", invalidConfiguration);
            }
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Logging Web Event Provider";
            var provider = new LoggingWebEventProvider();
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
            var expectedDescription = "My web app trace logging provider";
            var provider = new LoggingWebEventProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void ProcessEvent_WithCorrectlyConfiguredProviderWithNoProviderSet_LogsToDefaultProvider()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var provider = CreateInitializedProvider();
                WebBaseEvent webEvent = new DerivedWebErrorEvent("Valid message", null, 1, new Exception());

                // Act
                provider.ProcessEvent(webEvent);

                // Assert
                Assert.AreEqual(1, scope.LoggedEntries.Count);
            }
        }

        [TestMethod]
        public void ProcessEvent_WithCorrectlyConfiguredProviderWithProviderSet_LogsToExpectedProvider()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var provider = new LoggingWebEventProvider();
                var configuration = new NameValueCollection();
                configuration.Add("loggingProvider", "UnitTestingLoggingProvider");
                provider.Initialize("Valid name", configuration);
                WebBaseEvent webEvent = new DerivedWebErrorEvent("Valid message", null, 1, new Exception());

                // Act
                provider.ProcessEvent(webEvent);

                // Assert
                Assert.AreEqual(1, scope.LoggedEntries.Count);
            }
        }

        [TestMethod]
        public void ProcessEvent_WithNullEvent_SucceedsButDoesNotLog()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var provider = CreateInitializedProvider();

                // Act
                provider.ProcessEvent(null);

                // Assert
                Assert.AreEqual(0, scope.LoggedEntries.Count);
            }
        }

        [TestMethod]
        public void ProcessEvent_WithWebHeartbeatEvent_LogsAsInformation()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Information;
                var provider = CreateInitializedProvider();
                WebBaseEvent webEvent = new DerivedWebHeartbeatEvent("Valid message", 1);

                // Act
                provider.ProcessEvent(webEvent);

                // Assert
                var actualSeverity = scope.LoggedEntries.First().Severity;
                Assert.AreEqual(expectedSeverity, actualSeverity);
            }
        }

        [TestMethod]
        public void ProcessEvent_WithWebFailureAuditEvent_LogsAsWarning()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Warning;
                var provider = CreateInitializedProvider();
                WebBaseEvent webEvent = new DerivedWebFailureAuditEvent("Valid message", null, 1);

                // Act
                provider.ProcessEvent(webEvent);

                // Assert
                var actualSeverity = scope.LoggedEntries.First().Severity;
                Assert.AreEqual(expectedSeverity, actualSeverity);
            }
        }

        [TestMethod]
        public void ProcessEvent_WithDerivedWebErrorEvent_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Error;
                var provider = CreateInitializedProvider();
                WebBaseEvent webEvent = new DerivedWebErrorEvent("Valid message", null, 1, new Exception());

                // Act
                provider.ProcessEvent(webEvent);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                Assert.AreEqual(expectedSeverity, loggedEntry.Severity);
                Assert.IsNotNull(loggedEntry.Exception);
            }
        }

        [TestMethod]
        public void Flush_Always_SucceedsButDoesNotLog()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var provider = CreateInitializedProvider();

                // Act
                provider.Flush();

                // Assert
                Assert.AreEqual(0, scope.LoggedEntries.Count);
            }
        }

        [TestMethod]
        public void Shutdown_Always_SucceedsButDoesNotLog()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var provider = CreateInitializedProvider();

                // Act
                provider.Shutdown();

                // Assert
                Assert.AreEqual(0, scope.LoggedEntries.Count);
            }
        }

        private static LoggingWebEventProvider CreateInitializedProvider()
        {
            var provider = new LoggingWebEventProvider();

            provider.Initialize("Valid name", new NameValueCollection());

            return provider;
        }

        private static NameValueCollection CreateValidConfiguration()
        {
            return new NameValueCollection();
        }

        private sealed class DerivedWebErrorEvent : WebErrorEvent
        {
            public DerivedWebErrorEvent(string message, object eventSource, int eventCode, Exception exception)
                : base(message, eventSource, eventCode, exception)
            {
            }
        }

        private sealed class DerivedWebFailureAuditEvent : WebFailureAuditEvent
        {
            public DerivedWebFailureAuditEvent(string message, object eventSource, int eventCode)
                : base(message, eventSource, eventCode)
            {
            }
        }

        private sealed class DerivedWebHeartbeatEvent : WebHeartbeatEvent
        {
            public DerivedWebHeartbeatEvent(string message, int eventCode)
                : base(message, eventCode)
            {
            }
        }
    }
}
