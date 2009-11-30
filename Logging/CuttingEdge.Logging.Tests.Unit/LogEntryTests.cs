using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests for the <see cref="LogEntry"/> class.
    /// </summary>
    [TestClass]
    public class LogEntryTests
    {
        private const LoggingEventType ValidEventType = LoggingEventType.Error;
        private const string ValidMessage = "message";
        private const string ValidSource = "source";
        private static readonly Exception ValidException = new Exception();

        [TestMethod]
        public void Constructor_WithValidArguments_Succeeds()
        {
            LogEntry entry = new LogEntry(ValidEventType, ValidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidCriticalLoggingEventType_Succeeds()
        {
            LoggingEventType validEventType = LoggingEventType.Critical;

            LogEntry entry = new LogEntry(validEventType, ValidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidDebugLoggingEventType_Succeeds()
        {
            LoggingEventType validEventType = LoggingEventType.Debug;

            LogEntry entry = new LogEntry(validEventType, ValidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidErrorLoggingEventType_Succeeds()
        {
            LoggingEventType validEventType = LoggingEventType.Error;

            LogEntry entry = new LogEntry(validEventType, ValidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidInformationLoggingEventType_Succeeds()
        {
            LoggingEventType validEventType = LoggingEventType.Information;

            LogEntry entry = new LogEntry(validEventType, ValidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidWarningLoggingEventType_Succeeds()
        {
            LoggingEventType validEventType = LoggingEventType.Warning;

            LogEntry entry = new LogEntry(validEventType, ValidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidNullExceptioArgument_Succeeds()
        {
            LogEntry entry = new LogEntry(ValidEventType, ValidMessage, ValidSource, null);
        }

        [TestMethod]
        public void Constructor_WithValidNullSource_Succeeds()
        {
            LogEntry entry = new LogEntry(ValidEventType, ValidMessage, null, ValidException);
        }

        [TestMethod]
        public void Constructor_WithValidNullSourceAndNullException_Succeeds()
        {
            LogEntry entry = new LogEntry(ValidEventType, ValidMessage, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithInvalidEmptyMessage_ThrowsException()
        {
            string invalidMessage = string.Empty;

            LogEntry entry =
                new LogEntry(ValidEventType, invalidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithInvalidNullMessage_ThrowsException()
        {
            string invalidMessage = null;

            LogEntry entry = new LogEntry(ValidEventType, invalidMessage, ValidSource, ValidException);
        }

        [TestMethod]
        public void Constructor_WithInvalidLoggingEventType_ThrowsException()
        {
            LoggingEventType[] invalidEventTypes =
            {
                (LoggingEventType)(-1),
                (LoggingEventType)5,
                (LoggingEventType)6,
                (LoggingEventType)7,
                (LoggingEventType)8,
                (LoggingEventType)9,
            };

            foreach (LoggingEventType invalidEventType in invalidEventTypes)
            {
                this.AssertConstructorWithInvalidLoggingEventTypeThrowsInvalidEnumArgumentException(
                    invalidEventType);
            }
        }

        [TestMethod]
        public void LogEntry_Deserialized_EqualsOriginalLogEntry()
        {
            // Arrange
            LogEntry originalEntry = CreateLogEntryWithExceptionChain();

            BinaryFormatter formatter = new BinaryFormatter();

            byte[] binaryEntry;

            // Act
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, originalEntry);

                binaryEntry = stream.ToArray();
            }

            LogEntry deserializedEntry;

            using (MemoryStream stream = new MemoryStream(binaryEntry))
            {
                deserializedEntry = (LogEntry)formatter.Deserialize(stream);
            }

            // Assert
            AssertLogEntriesAreEqual(originalEntry, deserializedEntry);            
        }

        [TestMethod]
        public void ToString_HasAValidPresentation()
        {
            // Arrange
            Exception exception = new Exception();

            LogEntry entry = new LogEntry(LoggingEventType.Error, "message", "source", exception);

            // Note: this is not a pretty presentation.
            string expectedPresentation =
                "Severity: Error, Message: message, Source: source, Exception: " + exception.ToString();

            // Act
            string actualPresentation = entry.ToString();

            // Assert
            Assert.AreEqual(expectedPresentation, actualPresentation);
        }

        private static void AssertLogEntriesAreEqual(LogEntry entry1, LogEntry entry2)
        {
            Assert.AreEqual(entry1.Severity, entry2.Severity);
            Assert.AreEqual(entry1.Message, entry2.Message);
            Assert.AreEqual(entry1.Source, entry2.Source);
            Exception ex1 = entry1.Exception;
            Exception ex2 = entry2.Exception;

            // Note that the exceptions will never point to the same instance after deserialization.
            while (ex1 != null && ex2 != null)
            {
                Assert.AreEqual(ex1.Message, ex2.Message);
                Assert.AreEqual(ex1.GetType(), ex2.GetType());
                Assert.AreEqual(ex1.StackTrace, ex2.StackTrace);

                ex1 = ex1.InnerException;
                ex2 = ex2.InnerException;
            }

            Assert.IsTrue(ex1 == null && ex2 == null);
        }

        private static LogEntry CreateLogEntryWithExceptionChain()
        {
            Exception caughtException;

            try
            {
                try
                {
                    try
                    {
                        throw new FormatException("format error");
                    }
                    catch (FormatException fex)
                    {
                        throw new InvalidOperationException("invalid", fex);
                    }
                }
                catch (InvalidOperationException ioex)
                {
                    throw new ObjectDisposedException("disposed", ioex);
                }
            }
            catch (ObjectDisposedException odex)
            {
                caughtException = odex;
            }

            return new LogEntry(LoggingEventType.Error, "message", "source", caughtException);
        }

        private void AssertConstructorWithInvalidLoggingEventTypeThrowsInvalidEnumArgumentException(
            LoggingEventType invalidEventType)
        {
            try
            {
                LogEntry entry = new LogEntry(invalidEventType, ValidMessage, ValidSource, ValidException);
            }
            catch (InvalidEnumArgumentException)
            {
                // expected
                return;
            }
            catch
            {
            }

            Assert.Fail("Supplying an invalid LoggingEventType with value {0} should throw an " +
                "InvalidEnumArgumentException.", invalidEventType);
        }
    }
}
