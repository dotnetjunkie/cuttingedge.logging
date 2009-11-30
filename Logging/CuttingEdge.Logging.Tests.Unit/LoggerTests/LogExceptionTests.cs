using System;
using System.Linq;

using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(Exception) method.
    /// </summary>
    [TestClass]
    public class LogExceptionTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            Exception validException = new Exception();

            // Act
            Logger.Log(validException);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullException_ThrowsException()
        {
            // Arrange
            Exception invalidException = null;

            // Act
            Logger.Log(invalidException);
        }

        [TestMethod]
        public void Log_WithExceptionWithMessage_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";
                var exceptionToLog = new Exception(expectedMessage);

                var expectedEntry = 
                    new LogEntry(LoggingEventType.Error, expectedMessage, null, exceptionToLog);

                // Act
                Logger.Log(exceptionToLog);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }

        [TestMethod]
        public void Log_WithExceptionWithoutMessage_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "InvalidOperationException";
                var exceptionToLog = new InvalidOperationException(string.Empty);

                var expectedEntry =
                    new LogEntry(LoggingEventType.Error, expectedMessage, null, exceptionToLog);

                // Act
                Logger.Log(exceptionToLog);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
