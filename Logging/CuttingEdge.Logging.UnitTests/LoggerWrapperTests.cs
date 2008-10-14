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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);
            
            Exception exception = new InvalidOperationException();

            logger.Log(exception);

            Assert.AreEqual(EventType.Error, memoryLogger.FirstLoggedEvent.Type);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsExceptionMessageAsMessage()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string exceptionMessage = "This is a nice message";
            Exception exception = new InvalidOperationException(exceptionMessage);

            logger.Log(exception);

            Assert.AreEqual(exceptionMessage, memoryLogger.FirstLoggedEvent.Message);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsExceptionTypeAsMessage()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new InvalidOperationException(String.Empty);

            logger.Log(exception);

            Assert.AreEqual(exception.GetType().Name, memoryLogger.FirstLoggedEvent.Message);
        }

        [TestMethod]
        public void LoggerLogExceptionLogsOneEvent()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new InvalidOperationException();

            logger.Log(exception);

            Assert.AreEqual(1, memoryLogger.LoggedEvents.Count);
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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);
            
            logger.Log("message");

            Assert.AreEqual(EventType.Information, memoryLogger.FirstLoggedEvent.Type);
        }

        [TestMethod]
        public void LoggerLogMessageLogsCorrectData()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log("message");

            LoggingEvent loggingMessage = memoryLogger.FirstLoggedEvent;

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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log(new Exception(), MethodBase.GetCurrentMethod());

            Assert.AreEqual(1, memoryLogger.LoggedEvents.Count);
        }

        [TestMethod]
        public void LoggerLogExceptionMethodBaseLogsAsError()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log(new Exception(), MethodBase.GetCurrentMethod());

            Assert.AreEqual(EventType.Error, memoryLogger.FirstLoggedEvent.Type);
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageWithInvalidEventType()
        {
            foreach (EventType type in EventTypeEnumerator.GetInvalidValues())
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
            Decorator.Log(EventType.Error, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMesssageWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(EventType.Error, message);
        }

        [TestMethod]
        public void CanCallLogEventTypeMesssage()
        {
            Decorator.Log(EventType.Error, "Nice message.");
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageLogsCorrectEventType()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            foreach (EventType type in Enum.GetValues(typeof(EventType)))
            {
                logger.Log(type, "Nice message.");

                Assert.AreEqual(type, memoryLogger.LastLoggedEvent.Type);
            }
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageLogsCorrectData()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            logger.Log(EventType.Information, "Nice message.");

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Information, e.Type);
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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new Exception();
            logger.Log("message", exception);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Error, e.Type);
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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            MethodBase method = MethodBase.GetCurrentMethod();
            logger.Log("message", method);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Information, e.Type);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(null, e.Exception);
            Assert.IsTrue(e.Source.Contains(method.Name));
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageMethodBaseWithInvalidType()
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            foreach (EventType type in EventTypeEnumerator.GetInvalidValues())
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

            Decorator.Log(EventType.Error, message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageMethodBaseWithEmptyMessage()
        {
            string message = String.Empty;

            Decorator.Log(EventType.Error, message, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageMethodBaseWithNullSource()
        {
            MethodBase method = null;
            Decorator.Log(EventType.Error, "message", method);
        }

        [TestMethod]
        public void CanCallLogEventTypeMessageMethodBase()
        {
            Decorator.Log(EventType.Error, "message", MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageMethodBaseLogsCorrectData()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            MethodBase method = MethodBase.GetCurrentMethod();
            logger.Log(EventType.Warning, "message", method);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Warning, e.Type);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(null, e.Exception);
            Assert.IsTrue(e.Source.Contains(method.Name));
        }

        [TestMethod]
        public void CanCallLogEventTypeMessageString()
        {
            Decorator.Log(EventType.Error, "message", "source");
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageStringWithInvalidType()
        {
            string source = "source";
            foreach (EventType type in EventTypeEnumerator.GetInvalidValues())
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

            Decorator.Log(EventType.Error, message, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageStringWithEmptyMessage()
        {
            string message = String.Empty;

            Decorator.Log(EventType.Error, message, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageStringWithNullSource()
        {
            string source = null;
            Decorator.Log(EventType.Error, "message", source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageStringWithEmptySource()
        {
            string source = string.Empty;
            Decorator.Log(EventType.Error, "message", source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageStringLogsCorrectData()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string source = "source";
            logger.Log(EventType.Warning, "message", source);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Warning, e.Type);
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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string message = "message";
            Exception exception = new Exception();
            MethodBase source = MethodBase.GetCurrentMethod();
            logger.Log(message, exception, source);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Error, e.Type);
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
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            string message = "message";
            Exception exception = new Exception();
            MethodBase source = MethodBase.GetCurrentMethod();
            logger.Log(message, exception, source);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Error, e.Type);
            Assert.AreEqual(message, e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.IsTrue(e.Source.Contains(source.Name));
        }

        [TestMethod]
        public void CanCallEventTypeMessageExceptionMethodBase()
        {
            Decorator.Log(EventType.Warning, "message", new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithInvalidType()
        {
            MethodBase source = MethodBase.GetCurrentMethod();
            foreach (EventType type in EventTypeEnumerator.GetInvalidValues())
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
            Decorator.Log(EventType.Warning, message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(EventType.Warning, message, new Exception(), MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullException()
        {
            Exception exception = null;
            Decorator.Log(EventType.Warning, "message", exception, MethodBase.GetCurrentMethod());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionMethodBaseWithNullSource()
        {
            MethodBase source = null;
            Decorator.Log(EventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageExceptionMethodBaseLogsCorrectData()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new Exception();
            MethodBase source = MethodBase.GetCurrentMethod();

            logger.Log(EventType.Warning, "message", exception, source);

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Warning, e.Type);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.IsTrue(e.Source.Contains(source.Name));
        }

        [TestMethod]
        public void CanCallEventTypeMessageExceptionString()
        {
            Decorator.Log(EventType.Warning, "message", new Exception(), "source");
        }

        [TestMethod]
        public void CanNotCallLogEventTypeMessageExceptionStringWithInvalidType()
        {
            string source = "source";
            foreach (EventType type in EventTypeEnumerator.GetInvalidValues())
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
            Decorator.Log(EventType.Warning, message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithEmptyMessage()
        {
            string message = string.Empty;
            Decorator.Log(EventType.Warning, message, new Exception(), "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullException()
        {
            Exception exception = null;
            Decorator.Log(EventType.Warning, "message", exception, "source");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithNullSource()
        {
            string source = null;
            Decorator.Log(EventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CanNotCallLogEventTypeMessageExceptionStringWithEmptySource()
        {
            string source = string.Empty;
            Decorator.Log(EventType.Warning, "message", new Exception(), source);
        }

        [TestMethod]
        public void LoggerLogEventTypeMessageExceptionStringLogsCorrectData()
        {
            InMemoryLogger memoryLogger = new InMemoryLogger();
            LoggerWrapper logger = new LoggerWrapper(memoryLogger);

            Exception exception = new Exception();

            logger.Log(EventType.Warning, "message", exception, "source");

            LoggingEvent e = memoryLogger.FirstLoggedEvent;

            Assert.AreEqual(EventType.Warning, e.Type);
            Assert.AreEqual("message", e.Message);
            Assert.AreEqual(exception, e.Exception);
            Assert.AreEqual("source", e.Source);
        }
    }
}
