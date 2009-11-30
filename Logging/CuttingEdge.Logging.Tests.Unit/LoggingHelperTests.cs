using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class LoggingHelperTests
    {
#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void BuildMessageFromLogEntry_WithEntryWithMessageAndSeverity_ReturnsExpectedString()
        {
            // Arrange
            LogEntry entry = new LogEntry(LoggingEventType.Critical, "Valid message", null, null);
            string expectedMessage = "Valid message\r\nSeverity: Critical\r\n";

            // Act
            string createdMessage = LoggingHelper.BuildMessageFromLogEntry(entry);

            // Assert
            Assert.AreEqual(expectedMessage, createdMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildMessageFromLogEntry_WithNullArgument_ThrowsException()
        {
            LoggingHelper.BuildMessageFromLogEntry(null);
        }

        [TestMethod]
        public void FormatEvent_WithValidEntryWithoutException_ReturnsExpectedFormattedString()
        {
            // Arrange
            LogEntry entry = new LogEntry(LoggingEventType.Critical, "Valid message", "Log source", null);
            string expectedFormattedEvent = 
                "LoggingEvent:\r\n" + 
                "Severity:\tCritical\r\n" +
                "Message:\tValid message\r\n" +
                "Source:\tLog source\r\n";

            // Act
            string actualFormattedEvent = LoggingHelper.FormatEvent(entry);

            // Assert
            Assert.AreEqual(expectedFormattedEvent, actualFormattedEvent);
        }

        [TestMethod]
        public void FormatEvent_WithValidEntryWithException_ReturnsExpectedFormattedString()
        {
            // Arrange
            LogEntry entry = 
                new LogEntry(LoggingEventType.Critical, "Valid message", "Log source", new Exception("foo"));
            string expectedFormattedEvent =
                "LoggingEvent:\r\n" +
                "Severity:\tCritical\r\n" +
                "Message:\tValid message\r\n" +
                "Source:\tLog source\r\n" +
                "Exception:\tfoo\r\n" +
                "\r\n" +
                "\r\n";

            // Act
            string actualFormattedEvent = LoggingHelper.FormatEvent(entry);

            // Assert
            Assert.AreEqual(expectedFormattedEvent, actualFormattedEvent);
        }
#endif
    }
}
