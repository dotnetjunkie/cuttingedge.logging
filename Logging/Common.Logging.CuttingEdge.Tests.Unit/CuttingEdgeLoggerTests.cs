using System.Linq;
using CuttingEdge.Logging;
using CuttingEdge.Logging.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace Common.Logging.CuttingEdge.Tests.Unit
{
    [TestClass]
    public class CuttingEdgeLoggerTests
    {
        [TestMethod]
        public void Debug_WithNullMessage_LogsTemplateMessage()
        {
            // Arrange
            const string TemplateMessage = "No message supplied";
            object message = null;
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Debug(message);

                // Assert
                Assert.AreEqual(TemplateMessage, scope.LoggedEntries.First().Message);
            }
        }

        [TestMethod]
        public void Debug_WithEmptyMessage_LogsTemplateMessage()
        {
            // Arrange
            const string TemplateMessage = "No message supplied";
            object message = string.Empty;
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Debug(message);

                // Assert
                Assert.AreEqual(TemplateMessage, scope.LoggedEntries.First().Message);
            }
        }

        [TestMethod]
        public void Debug_WithMessage_LogsThatMessage()
        {
            // Arrange
            object expectedMessage = "some message";
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Debug(expectedMessage);

                // Assert
                Assert.AreEqual(expectedMessage, scope.LoggedEntries.First().Message);
            }
        }

        [TestMethod]
        public void Debug_WithValidMessage_LogsEventWithSeverityDebug()
        {
            // Arrange
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Debug("message to log");

                // Assert
                Assert.AreEqual(LoggingEventType.Debug, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Trace_WithValidMessage_LogsEventWithSeverityDebug()
        {
            // Arrange
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Trace("message to log");

                // Assert
                Assert.AreEqual(LoggingEventType.Debug, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Info_WithValidMessage_LogsEventWithSeverityInformation()
        {
            // Arrange
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Info("message to log");

                // Assert
                Assert.AreEqual(LoggingEventType.Information, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Warn_WithValidMessage_LogsEventWithSeverityWarning()
        {
            // Arrange
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Warn("message to log");

                // Assert
                Assert.AreEqual(LoggingEventType.Warning, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Error_WithValidMessage_LogsEventWithSeverityError()
        {
            // Arrange
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Error("message to log");

                // Assert
                Assert.AreEqual(LoggingEventType.Error, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Fatal_WithValidMessage_LogsEventWithSeverityCritical()
        {
            // Arrange
            ILog cuttingEdgeLogger = GetLoggerForUseWithScope();

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                cuttingEdgeLogger.Fatal("message to log");

                // Assert
                Assert.AreEqual(LoggingEventType.Critical, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void GetLogger_ProviderWithThresholdDebug_AllIsXXXEnabledReturnTrue()
        {
            // Arrange
            ILog log = GetLoggerWithThreshold(LoggingEventType.Debug);

            // Assert
            Assert.IsTrue(log.IsDebugEnabled, "Debug");
            Assert.IsTrue(log.IsTraceEnabled, "Trace");
            Assert.IsTrue(log.IsInfoEnabled, "Info");
            Assert.IsTrue(log.IsWarnEnabled, "Warn");
            Assert.IsTrue(log.IsErrorEnabled, "Error");
            Assert.IsTrue(log.IsFatalEnabled, "Fatal");
        }

        [TestMethod]
        public void GetLogger_ProviderWithThresholdInformation_IsDebugAndIsTraceReturnFalse()
        {
            // Arrange
            ILog log = GetLoggerWithThreshold(LoggingEventType.Information);

            // Assert
            Assert.IsFalse(log.IsDebugEnabled, "Debug");
            Assert.IsFalse(log.IsTraceEnabled, "Trace");

            Assert.IsTrue(log.IsInfoEnabled, "Info");
            Assert.IsTrue(log.IsWarnEnabled, "Warn");
            Assert.IsTrue(log.IsErrorEnabled, "Error");
            Assert.IsTrue(log.IsFatalEnabled, "Fatal");
        }

        [TestMethod]
        public void GetLogger_ProviderWithThresholdWarning_IsDebugIsTraceAndIsInfoReturnFalse()
        {
            // Arrange
            ILog log = GetLoggerWithThreshold(LoggingEventType.Warning);

            // Assert
            Assert.IsFalse(log.IsDebugEnabled, "Debug");
            Assert.IsFalse(log.IsTraceEnabled, "Trace");
            Assert.IsFalse(log.IsInfoEnabled, "Info");

            Assert.IsTrue(log.IsWarnEnabled, "Warn");
            Assert.IsTrue(log.IsErrorEnabled, "Error");
            Assert.IsTrue(log.IsFatalEnabled, "Fatal");
        }

        [TestMethod]
        public void GetLogger_ProviderWithThresholdError_IsDebugIsTraceIsInfoAndIsWarnReturnFalse()
        {
            // Arrange
            ILog log = GetLoggerWithThreshold(LoggingEventType.Error);

            // Assert
            Assert.IsFalse(log.IsDebugEnabled, "Debug");
            Assert.IsFalse(log.IsTraceEnabled, "Trace");
            Assert.IsFalse(log.IsInfoEnabled, "Info");
            Assert.IsFalse(log.IsWarnEnabled, "Warn");

            Assert.IsTrue(log.IsErrorEnabled, "Error");
            Assert.IsTrue(log.IsFatalEnabled, "Fatal");
        }

        [TestMethod]
        public void GetLogger_ProviderWithThresholdCritical_IsDebugIsTraceIsInfoIsWarnAndIsErrorReturnFalse()
        {
            // Arrange
            ILog log = GetLoggerWithThreshold(LoggingEventType.Critical);

            // Assert
            Assert.IsFalse(log.IsDebugEnabled, "Debug");
            Assert.IsFalse(log.IsTraceEnabled, "Trace");
            Assert.IsFalse(log.IsInfoEnabled, "Info");
            Assert.IsFalse(log.IsWarnEnabled, "Warn");
            Assert.IsFalse(log.IsErrorEnabled, "Error");

            Assert.IsTrue(log.IsFatalEnabled, "Fatal");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void WriteInternal_WithInvalidLogLevel_ThrowsException()
        {
            // Arrange
            var logger = new FakeCuttingEdgeLogger();
            LogLevel invalidLevel = (LogLevel)100;

            // Act
            logger.PublicWriteInternal(invalidLevel);
        }
       
        private static ILog GetLoggerForUseWithScope()
        {
            var adapter = new CuttingEdgeLoggerFactoryAdapter();

            var scopeProvider = LoggingProviderScope.GetProviderFromConfiguration();

            return adapter.GetLogger(scopeProvider.Name);
        }

        private static ILog GetLoggerWithThreshold(LoggingEventType threshold)
        {
            var loggingProvider =
                (from provider in Logger.Providers
                 where provider.Threshold == threshold
                 select provider).FirstOrDefault();

            if (loggingProvider == null)
            {
                Assert.Fail("No LoggingProvider with threshold of {0} was found.", threshold);
            }

            var adapter = new CuttingEdgeLoggerFactoryAdapter();

            return adapter.GetLogger(loggingProvider.Name);
        }

        private sealed class FakeCuttingEdgeLogger : CuttingEdgeLogger
        {
            public FakeCuttingEdgeLogger() : base(null)
            {
            }

            public void PublicWriteInternal(LogLevel level)
            {
                this.WriteInternal(level, "some message", null);
            }
        }
    }
}
