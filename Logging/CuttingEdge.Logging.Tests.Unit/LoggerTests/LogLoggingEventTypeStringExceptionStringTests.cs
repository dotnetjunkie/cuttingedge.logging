using System;
using System.ComponentModel;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(LoggingEventType, String, Exception, String) method.
    /// </summary>
    [TestClass]
    public class LogLoggingEventTypeStringExceptionStringTests
    {
        private readonly LoggingEventType validSeverity = LoggingEventType.Critical;
        private readonly string validMessage = "Valid message";
        private readonly Exception validException = new Exception();
        private readonly string validSource = "Valid source";

        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            Logger.Log(this.validSeverity, this.validMessage, this.validException, this.validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void Log_WithInvalidSeverity_ThrowsException()
        {
            // Arrange
            LoggingEventType invalidSeverity = (LoggingEventType)6;

            // Act
            Logger.Log(invalidSeverity, this.validMessage, this.validException, this.validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = null;

            // Act
            Logger.Log(this.validSeverity, invalidMessage, this.validException, this.validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsExceptoin()
        {
            // Arrange
            string invalidMessage = string.Empty;

            // Act
            Logger.Log(this.validSeverity, invalidMessage, this.validException, this.validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullException_ThrowsException()
        {
            // Arrange
            Exception invalidException = null;

            // Act
            Logger.Log(this.validSeverity, this.validMessage, invalidException, this.validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullSource_ThrowsException()
        {
            // Arrange
            string invalidSource = null;

            // Act
            Logger.Log(this.validSeverity, this.validMessage, this.validException, invalidSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptySource_ThrowsException()
        {
            // Arrange
            string invalidSource = string.Empty;

            // Act
            Logger.Log(this.validSeverity, this.validMessage, this.validException, invalidSource);
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Error;
                var expectedMessage = "Expected message in entry";
                var expectedException = new Exception();
                var expectedSource = "Expected source";

                var expectedEntry =
                    new LogEntry(expectedSeverity, expectedMessage, expectedSource, expectedException);

                // Act
                Logger.Log(expectedSeverity, expectedMessage, expectedException, expectedSource);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
