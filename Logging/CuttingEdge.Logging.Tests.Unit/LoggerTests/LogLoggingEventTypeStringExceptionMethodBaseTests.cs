using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(LoggingEventType, String, Exception, MethodBase) method.
    /// </summary>
    [TestClass]
    public class LogLoggingEventTypeStringExceptionMethodBaseTests
    {
        private readonly LoggingEventType validSeverity = LoggingEventType.Critical;
        private readonly string validMessage = "Valid Message";
        private readonly Exception validException = new Exception();
        private readonly MethodBase validSource = MethodBase.GetCurrentMethod();

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
            LoggingEventType invalidSeverity = (LoggingEventType)10;

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
        public void Log_WithEmptyMessage_ThrowsException()
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
            MethodBase invalidSource = null;

            // Act
            Logger.Log(this.validSeverity, this.validMessage, this.validException, invalidSource);
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Warning;
                var expectedMessage = "Expected message in entry";
                var expectedException = new Exception();
                var expectedSource = this.GetType().FullName + ".Log_WithValidArguments_LogsExpectedEntry()";

                var expectedEntry =
                    new LogEntry(expectedSeverity, expectedMessage, expectedSource, expectedException);

                // Act
                Logger.Log(expectedSeverity, expectedMessage, expectedException, MethodBase.GetCurrentMethod());

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
