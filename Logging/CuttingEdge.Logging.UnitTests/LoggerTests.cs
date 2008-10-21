using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using CuttingEdge.Logging.UnitTests.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class.
    /// </summary>
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void CanAccessProvider()
        {
            Assert.IsNotNull(Logger.Provider);
        }

        [TestMethod]
        public void ProviderPropertyReturnsSameInstanceOnEachCall()
        {
            Assert.AreEqual(Logger.Provider, Logger.Provider);
        }

        [TestMethod]
        public void ProvidersPropertyReturnsSameInstanceOnEachCall()
        {
            Assert.AreEqual(Logger.Providers, Logger.Providers);
        }

        [TestMethod]
        public void ProvidersCollectionContainsOneElement()
        {
            Assert.AreEqual(2, Logger.Providers.Count);
        }

        [TestMethod]
        public void ProviderHasFallbackProvider()
        {
            Assert.AreNotEqual(null, Logger.Provider.FallbackProvider);
        }

        [TestMethod]
        public void FallbackProviderHasNoFallbackProvider()
        {
            Assert.AreEqual(null, Logger.Provider.FallbackProvider.FallbackProvider);
        }

        [TestMethod]
        public void CanIterateProvidersCollection()
        {
            foreach (LoggingProviderBase loggingProvider in Logger.Providers)
            {
                Assert.IsNotNull(loggingProvider);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CanNotAddProviderToProvidersCollection()
        {
            Logger.Providers.Add(new StubLoggingProvider());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogExceptionWithNullReference()
        {
            Exception exception = null;

            Logger.Log(exception);
        }

        [TestMethod]
        public void CanCallLogExceptionWithUnthrownException()
        {
            Exception unthrownException = new InvalidOperationException();

            Logger.Log(unthrownException);
        }

        [TestMethod]
        public void CanCallLogExceptionWithThrownException()
        {
            Exception thrownException = null;
            try
            {
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Logger.Log(thrownException);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsEventAsError()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Exception exception = new InvalidOperationException();

                Logger.Log(exception);

                Assert.AreEqual(LoggingEventType.Error, scope.Logger.GetLoggedEntries()[0].Severity);
            }
        }

        [TestMethod]
        public void LoggerLogExceptionLogsExceptionMessageAsMessage()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                string exceptionMessage = "This is a nice message";
                Exception exception = new InvalidOperationException(exceptionMessage);

                Logger.Log(exception);

                Assert.AreEqual(exceptionMessage, scope.Logger.GetLoggedEntries()[0].Message);
            }
        }

        [TestMethod]
        public void LoggerLogExceptionLogsExceptionTypeAsMessage()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Exception exception = new InvalidOperationException(String.Empty);

                Logger.Log(exception);

                Assert.AreEqual(exception.GetType().Name, scope.Logger.GetLoggedEntries()[0].Message);
            }
        }

        [TestMethod]
        public void LoggerLogExceptionLogsOneEvent()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Exception exception = new InvalidOperationException();

                Logger.Log(exception);

                Assert.AreEqual(1, scope.Logger.GetLoggedEntries().Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageWithNullReference()
        {
            string message = null;

            Logger.Log(message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageWithEmptyString()
        {
            string message = String.Empty;

            Logger.Log(message);
        }

        [TestMethod]
        public void CanCallLogMessageWithNonEmptyString()
        {
            string message = "message";
            Logger.Log(message);
        }

        [TestMethod]
        public void LoggerLogMessageLogsEventAsInformation()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Logger.Log("message");

                Assert.AreEqual(LoggingEventType.Information, scope.Logger.GetLoggedEntries()[0].Severity);
            }
        }

        [TestMethod]
        public void LoggerLogMessageLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Logger.Log("message");

                LogEntry entry = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual("message", entry.Message);
                Assert.AreEqual(null, entry.Source);
                Assert.AreEqual(null, entry.Exception);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            MethodBase method = null;
            Logger.Log(exception, method);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogExceptionMethodBaseWithNullMethodBase()
        {
            Exception exception = new Exception();
            MethodBase method = null;
            Logger.Log(exception, method);
        }

        [TestMethod]
        public void CanCallLogExceptionMethodBase()
        {
            Exception exception = new Exception();
            MethodBase method = MethodBase.GetCurrentMethod();
            Logger.Log(exception, method);
        }

        [TestMethod]
        public void LoggerLogExceptionMethodBaseLogsOneEvent()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Logger.Log(new Exception(), MethodBase.GetCurrentMethod());

                Assert.AreEqual(1, scope.Logger.GetLoggedEntries().Length);
            }
        }

        [TestMethod]
        public void LoggerLogExceptionMethodBaseLogsAsError()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Logger.Log(new Exception(), MethodBase.GetCurrentMethod());

                Assert.AreEqual(LoggingEventType.Error, scope.Logger.GetLoggedEntries()[0].Severity);
            }
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageWithInvalidEventType()
        {
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Logger.Log(type, "message");
                    Assert.Fail();
                }
                catch (InvalidEnumArgumentException)
                {
                    // We expect an InvalidEnumArgumentException here.
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMesssageWithNullMessage()
        {
            string message = null;
            Logger.Log(LoggingEventType.Error, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMesssageWithEmptyMessage()
        {
            string message = string.Empty;
            Logger.Log(LoggingEventType.Error, message);
        }

        [TestMethod]
        public void CanCallLogEventTypeMesssage()
        {
            Logger.Log(LoggingEventType.Error, "Nice message.");
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageLogsCorrectEventType()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                foreach (LoggingEventType type in Enum.GetValues(typeof(LoggingEventType)))
                {
                    Logger.Log(type, "Nice message.");

                    LogEntry[] entries = scope.Logger.GetLoggedEntries();

                    LogEntry lastLoggedEvent = entries[entries.Length - 1];

                    Assert.IsNotNull(lastLoggedEvent);

                    Assert.AreEqual(type, lastLoggedEvent.Severity);
                }
            }
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Logger.Log(LoggingEventType.Information, "Nice message.");

                LogEntry e = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Information, e.Severity);
                Assert.AreEqual("Nice message.", e.Message);
                Assert.AreEqual(null, e.Exception);
                Assert.AreEqual(null, e.Source);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionWithNullMessage()
        {
            string message = null;

            Logger.Log(message, new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageExceptionWithEmptyMessage()
        {
            string message = String.Empty;

            Logger.Log(message, new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionWithNullException()
        {
            Exception exception = null;
            Logger.Log("message", exception);
        }

        [TestMethod]
        public void CanCallLogMessageException()
        {
            Logger.Log("message", new Exception());
        }

        [TestMethod]
        public void LoggerLogMessageExceptionLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Exception exception = new Exception();
                Logger.Log("message", exception);

                LogEntry entries = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Error, entries.Severity);
                Assert.AreEqual("message", entries.Message);
                Assert.AreEqual(exception, entries.Exception);
                Assert.AreEqual(null, entries.Source);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageMethodBaseWithNullMessage()
        {
            string message = null;

            Logger.Log(message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageMethodBaseWithEmptyMessage()
        {
            string message = String.Empty;

            Logger.Log(message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageMethodBaseWithNullSource()
        {
            MethodBase method = null;
            Logger.Log("message", method);
        }

        [TestMethod]
        public void CanCallLogMessageMethodBase()
        {
            Logger.Log("message", MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void LoggerLogMessageMethodBaseLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                MethodBase method = MethodBase.GetCurrentMethod();
                Logger.Log("message", method);

                LogEntry entry = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Information, entry.Severity);
                Assert.AreEqual("message", entry.Message);
                Assert.AreEqual(null, entry.Exception);
                Assert.IsTrue(entry.Source.Contains(method.Name));
            }
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageMethodBaseWithInvalidType()
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Logger.Log(type, "message", method);
                    Assert.Fail();
                }
                catch (InvalidEnumArgumentException)
                {
                    // We'd expect an InvalidEnumArgumentException to be thrown.
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageMethodBaseNullMessage()
        {
            string message = null;

            Logger.Log(LoggingEventType.Error, message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageMethodBaseWithEmptyMessage()
        {
            string message = String.Empty;

            Logger.Log(LoggingEventType.Error, message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageMethodBaseWithNullSource()
        {
            MethodBase method = null;
            Logger.Log(LoggingEventType.Error, "message", method);
        }

        [TestMethod]
        public void CanCallLogEventTypeMessageMethodBase()
        {
            Logger.Log(LoggingEventType.Error, "message", MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageMethodBaseLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                MethodBase method = MethodBase.GetCurrentMethod();
                Logger.Log(LoggingEventType.Warning, "message", method);

                LogEntry entry = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Warning, entry.Severity);
                Assert.AreEqual("message", entry.Message);
                Assert.AreEqual(null, entry.Exception);
                Assert.IsTrue(entry.Source.Contains(method.Name));
            }
        }

        [TestMethod]
        public void CanCallLogEventTypeMessageString()
        {
            Logger.Log(LoggingEventType.Error, "message", "source");
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageStringWithInvalidType()
        {
            string source = "source";
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Logger.Log(type, "message", source);
                    Assert.Fail();
                }
                catch (InvalidEnumArgumentException)
                {
                    // We'd expect an InvalidEnumArgumentException to be thrown.
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageStringNullMessage()
        {
            string message = null;

            Logger.Log(LoggingEventType.Error, message, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageStringWithEmptyMessage()
        {
            string message = String.Empty;

            Logger.Log(LoggingEventType.Error, message, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageStringWithNullSource()
        {
            string source = null;
            Logger.Log(LoggingEventType.Error, "message", source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageStringWithEmptySource()
        {
            string source = string.Empty;
            Logger.Log(LoggingEventType.Error, "message", source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageStringLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                string source = "source";
                Logger.Log(LoggingEventType.Warning, "message", source);

                LogEntry entry = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Warning, entry.Severity);
                Assert.AreEqual("message", entry.Message);
                Assert.AreEqual(null, entry.Exception);
                Assert.AreEqual(source, entry.Source);
            }
        }

        [TestMethod]
        public void CanCallMessageExceptionMethodBase()
        {
            Logger.Log("message", new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionMethodBaseWithNullMessage()
        {
            string message = null;
            Logger.Log(message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageExceptionMethodBaseWithEmptyMessage()
        {
            string message = string.Empty;
            Logger.Log(message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            Logger.Log("message", exception, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallMessageExceptionMethodBaseWithNullSource()
        {
            MethodBase source = null;
            Logger.Log("message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogMessageExceptionMethodBaseLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                string message = "message";
                Exception exception = new Exception();
                MethodBase source = MethodBase.GetCurrentMethod();
                Logger.Log(message, exception, source);

                LogEntry entries = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Error, entries.Severity);
                Assert.AreEqual(message, entries.Message);
                Assert.AreEqual(exception, entries.Exception);
                Assert.IsTrue(entries.Source.Contains(source.Name));
            }
        }

        [TestMethod]
        public void CanCallMessageExceptionString()
        {
            Logger.Log("message", new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionStringWithNullMessage()
        {
            string message = null;
            Logger.Log(message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageExceptionStringWithEmptyMessage()
        {
            string message = string.Empty;
            Logger.Log(message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionStringWithNullException()
        {
            Exception exception = null;
            Logger.Log("message", exception, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallMessageExceptionStringWithNullSource()
        {
            string source = null;
            Logger.Log("message", new Exception(), source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallMessageExceptionStringWithEmptySource()
        {
            string source = string.Empty;
            Logger.Log("message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogMessageExceptionStringLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                string message = "message";
                Exception exception = new Exception();
                MethodBase source = MethodBase.GetCurrentMethod();
                Logger.Log(message, exception, source);

                LogEntry entries = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Error, entries.Severity);
                Assert.AreEqual(message, entries.Message);
                Assert.AreEqual(exception, entries.Exception);
                Assert.IsTrue(entries.Source.Contains(source.Name));
            }
        }

        [TestMethod]
        public void CanCallEventTypeMessageExceptionMethodBase()
        {
            Logger.Log(LoggingEventType.Warning, "message", new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithInvalidType()
        {
            MethodBase source = MethodBase.GetCurrentMethod();
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Logger.Log(type, "message", new Exception(), source);
                    Assert.Fail();
                }
                catch (InvalidEnumArgumentException)
                {
                    // We'd expect an InvalidEnumArgumentException to be thrown.
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullMessage()
        {
            string message = null;
            Logger.Log(LoggingEventType.Warning, message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithEmptyMessage()
        {
            string message = string.Empty;
            Logger.Log(LoggingEventType.Warning, message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            Logger.Log(LoggingEventType.Warning, "message", exception, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullSource()
        {
            MethodBase source = null;
            Logger.Log(LoggingEventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageExceptionMethodBaseLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Exception exception = new Exception();
                MethodBase source = MethodBase.GetCurrentMethod();

                Logger.Log(LoggingEventType.Warning, "message", exception, source);

                LogEntry entries = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Warning, entries.Severity);
                Assert.AreEqual("message", entries.Message);
                Assert.AreEqual(exception, entries.Exception);
                Assert.IsTrue(entries.Source.Contains(source.Name));
            }
        }

        [TestMethod]
        public void CanCallEventTypeMessageExceptionString()
        {
            Logger.Log(LoggingEventType.Warning, "message", new Exception(), "source");
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageExceptionStringWithInvalidType()
        {
            string source = "source";
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Logger.Log(type, "message", new Exception(), source);
                    Assert.Fail();
                }
                catch (InvalidEnumArgumentException)
                {
                    // We'd expect an InvalidEnumArgumentException to be thrown.
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullMessage()
        {
            string message = null;
            Logger.Log(LoggingEventType.Warning, message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithEmptyMessage()
        {
            string message = string.Empty;
            Logger.Log(LoggingEventType.Warning, message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullException()
        {
            Exception exception = null;
            Logger.Log(LoggingEventType.Warning, "message", exception, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullSource()
        {
            string source = null;
            Logger.Log(LoggingEventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithEmptySource()
        {
            string source = string.Empty;
            Logger.Log(LoggingEventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageExceptionStringLogsCorrectData()
        {
            using (var scope = new LoggingProviderTestingScope<MemoryLoggingProvider>())
            {
                Exception exception = new Exception();

                Logger.Log(LoggingEventType.Warning, "message", exception, "source");

                LogEntry entry = scope.Logger.GetLoggedEntries()[0];

                Assert.AreEqual(LoggingEventType.Warning, entry.Severity);
                Assert.AreEqual("message", entry.Message);
                Assert.AreEqual(exception, entry.Exception);
                Assert.AreEqual("source", entry.Source);
            }
        }
    }
}
