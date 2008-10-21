using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    /// <summary>
    /// Tests for the <see cref="LogEntry"/> class.
    /// </summary>
    [TestClass]
    public class LogEntryTests
    {
        [TestMethod]
        public void CreatingALogEntryWithValidArgumentsShouldSucceed1()
        {
            LogEntry entry = new LogEntry(LoggingEventType.Error, "message", "source", new Exception());
        }

        [TestMethod]
        public void CreatingALogEntryWithValidArgumentsShouldSucceed2()
        {
            LogEntry entry = new LogEntry(LoggingEventType.Error, "message", "source", null);
        }

        [TestMethod]
        public void CreatingALogEntryWithValidArgumentsShouldSucceed3()
        {
            LogEntry entry = new LogEntry(LoggingEventType.Error, "message", null, new Exception());
        }

        [TestMethod]
        public void CreatingALogEntryWithValidArgumentsShouldSucceed4()
        {
            LogEntry entry = new LogEntry(LoggingEventType.Error, "message", null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatingALogEntryWithValidArgumentsShouldFail1()
        {
            LogEntry entry = new LogEntry(LoggingEventType.Error, String.Empty, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatingALogEntryWithValidArgumentsShouldFail2()
        {
            LogEntry entry = new LogEntry(LoggingEventType.Error, null, "source", new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void CreatingALogEntryWithValidArgumentsShouldFail3()
        {
            LogEntry entry = new LogEntry((LoggingEventType)(-1), "message", "source", new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void CreatingALogEntryWithValidArgumentsShouldFail4()
        {
            LogEntry entry = new LogEntry((LoggingEventType)5, "message", "source", new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void CreatingALogEntryWithValidArgumentsShouldFail5()
        {
            LogEntry entry = new LogEntry((LoggingEventType)6, "message", "source", new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void CreatingALogEntryWithValidArgumentsShouldFail6()
        {
            LogEntry entry = new LogEntry((LoggingEventType)7, "message", "source", new Exception());
        }

        [TestMethod]
        public void LogEntryCanBeSerializedBinary()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            
            LogEntry entry1 = CreateLogEntryWithExceptionChain();

            byte[] binaryEntry;

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, entry1);

                binaryEntry = stream.ToArray();
            }

            LogEntry entry2;

            using (MemoryStream stream = new MemoryStream(binaryEntry))
            {
                entry2 = (LogEntry)formatter.Deserialize(stream);
            }

            AssertObjectsToBeEqual(entry1, entry2);
        }

        private static void AssertObjectsToBeEqual(LogEntry entry1, LogEntry entry2)
        {
            Assert.AreEqual(entry1.Severity, entry2.Severity);
            Assert.AreEqual(entry1.Message, entry2.Message);
            Assert.AreEqual(entry1.Source, entry2.Source);
            Exception ex1 = entry1.Exception;
            Exception ex2 = entry2.Exception;

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
    }
}
