using System;
using System.ComponentModel;
using System.Linq;

using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(LoggingEventType, String, String) method.
    /// </summary>
    [TestClass]
    public class LogLoggingEventTypeStringStringTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string validMessage = "Valid message";
            string validSource = "Valid source";

            // Act
            Logger.Log(validSeverity, validMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void Log_WithInvalidSeverity_ThrowsException()
        {
            // Arrange
            LoggingEventType invalidSeverity = (LoggingEventType)6;
            string validMessage = "Valid message";
            string validSource = "Valid source";

            // Act
            Logger.Log(invalidSeverity, validMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string invalidMessage = null;
            string validSource = "Valid source";

            // Act
            Logger.Log(validSeverity, invalidMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsException()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string invalidMessage = string.Empty;
            string validSource = "Valid source";

            // Act
            Logger.Log(validSeverity, invalidMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullSource_ThrowsException()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string validMessage = "Valid message";
            string invalidSource = null;

            // Act
            Logger.Log(validSeverity, validMessage, invalidSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptySource_ThrowsException()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string validMessage = "Valid message";
            string invalidSource = string.Empty;

            // Act
            Logger.Log(validSeverity, validMessage, invalidSource);
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Error;
                var expectedMessage = "Expected message in entry";
                var expectedSource = "Valid source";

                var expectedEntry =
                    new LogEntry(expectedSeverity, expectedMessage, expectedSource, null);

                // Act
                Logger.Log(expectedSeverity, expectedMessage, expectedSource);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
