using System;
using System.ComponentModel;
using System.Reflection;

using CuttingEdge.Logging.UnitTests.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    [TestClass]
    public class LoggerWrapperTests
    {
        private static readonly LoggerWrapper Decorator = new LoggerWrapper(new StubLoggingProvider());

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogExceptionWithNullReference()
        {
            Exception exception = null;

            Decorator.Log(exception);
        }

        [TestMethod]
        public void CanCallLogExceptionWithUnthrownException()
        {
            Exception unthrownException = new InvalidOperationException();

            Decorator.Log(unthrownException);
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

            Decorator.Log(thrownException);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsEventAsError()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);
            
            Exception exception = new InvalidOperationException();

            logger.Log(exception);

            Assert.AreEqual(LoggingEventType.Error, memoryLogger.GetLoggedEvents()[0].Severity);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsExceptionMessageAsMessage()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string exceptionMessage = "This is a nice message";
            Exception exception = new InvalidOperationException(exceptionMessage);

            logger.Log(exception);

            Assert.AreEqual(exceptionMessage, memoryLogger.GetLoggedEvents()[0].Message);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsExceptionTypeAsMessage()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new InvalidOperationException(String.Empty);

            logger.Log(exception);

            Assert.AreEqual(exception.GetType().Name, memoryLogger.GetLoggedEvents()[0].Message);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsOneEvent()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new InvalidOperationException();

            logger.Log(exception);

            Assert.AreEqual(1, memoryLogger.GetLoggedEvents().Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageWithNullReference()
        {
            string message = null;

            Decorator.Log(message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageWithEmptyString()
        {
            string message = String.Empty;

            Decorator.Log(message);
        }

        [TestMethod]
        public void CanCallLogMessageWithNonEmptyString()
        {
            string message = "message";
            Decorator.Log(message);
        }

        [TestMethod]
        public void LoggerLogMessageLogsEventAsInformation()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);
            
            logger.Log("message");

            Assert.AreEqual(LoggingEventType.Information, memoryLogger.GetLoggedEvents()[0].Severity);
        }

        [TestMethod]
        public void LoggerLogMessageLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log("message");

            MemoryLoggingEvent loggingMessage = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual("message", loggingMessage.Message);
            Assert.AreEqual(null, loggingMessage.Source);
            Assert.AreEqual(null, loggingMessage.Exception);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            MethodBase method = null;
            Decorator.Log(exception, method);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogExceptionMethodBaseWithNullMethodBase()
        {
            Exception exception = new Exception();
            MethodBase method = null;
            Decorator.Log(exception, method);
        }

        [TestMethod]
        public void CanCallLogExceptionMethodBase()
        {
            Exception exception = new Exception();
            MethodBase method = MethodBase.GetCurrentMethod();
            Decorator.Log(exception, method);
        }

        [TestMethod]
        public void LoggerLogExceptionMethodBaseLogsOneEvent()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log(new Exception(), MethodBase.GetCurrentMethod());

            Assert.AreEqual(1, memoryLogger.GetLoggedEvents().Length);
        }

        [TestMethod]
        public void LoggerLogExceptionMethodBaseLogsAsError()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log(new Exception(), MethodBase.GetCurrentMethod());

            Assert.AreEqual(LoggingEventType.Error, memoryLogger.GetLoggedEvents()[0].Severity);
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageWithInvalidEventType()
        {
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Decorator.Log(type, "message");
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
            Decorator.Log(LoggingEventType.Error, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMesssageWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(LoggingEventType.Error, message);
        }

        [TestMethod]
        public void CanCallLogEventTypeMesssage()
        {
            Decorator.Log(LoggingEventType.Error, "Nice message.");
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageLogsCorrectEventType()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            foreach (LoggingEventType type in Enum.GetValues(typeof(LoggingEventType)))
            {
                logger.Log(type, "Nice message.");

                MemoryLoggingEvent[] events = memoryLogger.GetLoggedEvents();
                MemoryLoggingEvent lastEvent = events[events.Length - 1];

                Assert.AreEqual(type, lastEvent.Severity);
            }
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log(LoggingEventType.Information, "Nice message.");

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Information, e.Severity);
            Assert.AreEqual("Nice message.", e.Message);
            Assert.AreEqual(null, e.Exception);
            Assert.AreEqual(null, e.Source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionWithNullMessage()
        {
            string message = null;

            Decorator.Log(message, new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageExceptionWithEmptyMessage()
        {
            string message = String.Empty;

            Decorator.Log(message, new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionWithNullException()
        {
            Exception exception = null;
            Decorator.Log("message", exception);
        }

        [TestMethod]
        public void CanCallLogMessageException()
        {
            Decorator.Log("message", new Exception());
        }

        [TestMethod]
        public void LoggerLogMessageExceptionLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new Exception();
            logger.Log("message", exception);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Error, e.Severity);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.AreEqual(null, e.Source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageMethodBaseWithNullMessage()
        {
            string message = null;

            Decorator.Log(message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageMethodBaseWithEmptyMessage()
        {
            string message = String.Empty;

            Decorator.Log(message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageMethodBaseWithNullSource()
        {
            MethodBase method = null;
            Decorator.Log("message", method);
        }

        [TestMethod]
        public void CanCallLogMessageMethodBase()
        {
            Decorator.Log("message", MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void LoggerLogMessageMethodBaseLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            MethodBase method = MethodBase.GetCurrentMethod();
            logger.Log("message", method);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Information, e.Severity);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(null, e.Exception);
            Assert.IsTrue(e.Source.Contains(method.Name));
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageMethodBaseWithInvalidType()
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Decorator.Log(type, "message", method);
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

            Decorator.Log(LoggingEventType.Error, message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageMethodBaseWithEmptyMessage()
        {
            string message = String.Empty;

            Decorator.Log(LoggingEventType.Error, message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageMethodBaseWithNullSource()
        {
            MethodBase method = null;
            Decorator.Log(LoggingEventType.Error, "message", method);
        }

        [TestMethod]
        public void CanCallLogEventTypeMessageMethodBase()
        {
            Decorator.Log(LoggingEventType.Error, "message", MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageMethodBaseLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            MethodBase method = MethodBase.GetCurrentMethod();
            logger.Log(LoggingEventType.Warning, "message", method);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Warning, e.Severity);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(null, e.Exception);
            Assert.IsTrue(e.Source.Contains(method.Name));
        }

        [TestMethod]
        public void CanCallLogEventTypeMessageString()
        {
            Decorator.Log(LoggingEventType.Error, "message", "source");
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageStringWithInvalidType()
        {
            string source = "source";
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Decorator.Log(type, "message", source);
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

            Decorator.Log(LoggingEventType.Error, message, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageStringWithEmptyMessage()
        {
            string message = String.Empty;

            Decorator.Log(LoggingEventType.Error, message, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageStringWithNullSource()
        {
            string source = null;
            Decorator.Log(LoggingEventType.Error, "message", source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageStringWithEmptySource()
        {
            string source = string.Empty;
            Decorator.Log(LoggingEventType.Error, "message", source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageStringLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string source = "source";
            logger.Log(LoggingEventType.Warning, "message", source);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Warning, e.Severity);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(null, e.Exception);
            Assert.AreEqual(source, e.Source);
        }

        [TestMethod]
        public void CanCallMessageExceptionMethodBase()
        {
            Decorator.Log("message", new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionMethodBaseWithNullMessage()
        {
            string message = null;
            Decorator.Log(message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageExceptionMethodBaseWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            Decorator.Log("message", exception, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallMessageExceptionMethodBaseWithNullSource()
        {
            MethodBase source = null;
            Decorator.Log("message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogMessageExceptionMethodBaseLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string message = "message";
            Exception exception = new Exception();
            MethodBase source = MethodBase.GetCurrentMethod();
            logger.Log(message, exception, source);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Error, e.Severity);
            Assert.AreEqual(message, e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.IsTrue(e.Source.Contains(source.Name));
        }

        [TestMethod]
        public void CanCallMessageExceptionString()
        {
            Decorator.Log("message", new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionStringWithNullMessage()
        {
            string message = null;
            Decorator.Log(message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogMessageExceptionStringWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogMessageExceptionStringWithNullException()
        {
            Exception exception = null;
            Decorator.Log("message", exception, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallMessageExceptionStringWithNullSource()
        {
            string source = null;
            Decorator.Log("message", new Exception(), source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallMessageExceptionStringWithEmptySource()
        {
            string source = string.Empty;
            Decorator.Log("message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogMessageExceptionStringLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string message = "message";
            Exception exception = new Exception();
            MethodBase source = MethodBase.GetCurrentMethod();
            logger.Log(message, exception, source);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Error, e.Severity);
            Assert.AreEqual(message, e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.IsTrue(e.Source.Contains(source.Name));
        }

        [TestMethod]
        public void CanCallEventTypeMessageExceptionMethodBase()
        {
            Decorator.Log(LoggingEventType.Warning, "message", new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithInvalidType()
        {
            MethodBase source = MethodBase.GetCurrentMethod();
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Decorator.Log(type, "message", new Exception(), source);
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
            Decorator.Log(LoggingEventType.Warning, message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(LoggingEventType.Warning, message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            Decorator.Log(LoggingEventType.Warning, "message", exception, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullSource()
        {
            MethodBase source = null;
            Decorator.Log(LoggingEventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageExceptionMethodBaseLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new Exception();
            MethodBase source = MethodBase.GetCurrentMethod();

            logger.Log(LoggingEventType.Warning, "message", exception, source);

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Warning, e.Severity);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.IsTrue(e.Source.Contains(source.Name));
        }

        [TestMethod]
        public void CanCallEventTypeMessageExceptionString()
        {
            Decorator.Log(LoggingEventType.Warning, "message", new Exception(), "source");
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageExceptionStringWithInvalidType()
        {
            string source = "source";
            foreach (LoggingEventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    Decorator.Log(type, "message", new Exception(), source);
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
            Decorator.Log(LoggingEventType.Warning, message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(LoggingEventType.Warning, message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullException()
        {
            Exception exception = null;
            Decorator.Log(LoggingEventType.Warning, "message", exception, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullSource()
        {
            string source = null;
            Decorator.Log(LoggingEventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithEmptySource()
        {
            string source = string.Empty;
            Decorator.Log(LoggingEventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageExceptionStringLogsCorrectData()
        {
            MemoryLoggingProvider memoryLogger = new MemoryLoggingProvider();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new Exception();

            logger.Log(LoggingEventType.Warning, "message", exception, "source");

            MemoryLoggingEvent e = memoryLogger.GetLoggedEvents()[0];

            Assert.AreEqual(LoggingEventType.Warning, e.Severity);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.AreEqual("source", e.Source);
        }
    }
}
