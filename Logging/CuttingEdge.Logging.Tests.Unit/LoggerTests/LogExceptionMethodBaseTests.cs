using System;
using System.Linq;
using System.Reflection;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(Exception, MethodBase) method.
    /// </summary>
    [TestClass]
    public class LogExceptionMethodBaseTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            Exception validException = new Exception();
            MethodBase validSource = MethodBase.GetCurrentMethod();

            // Act
            Logger.Log(validException, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullException_ThrowsException()
        {
            // Arrange
            Exception invalidException = null;
            MethodBase validSource = MethodBase.GetCurrentMethod();

            // Act
            Logger.Log(invalidException, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullSource_ThrowsException()
        {
            // Arrange
            Exception validException = new Exception();
            MethodBase invalidSource = null;

            // Act
            Logger.Log(validException, invalidSource);
        }

        [TestMethod]
        public void Log_WithExceptionWithMessage_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";
                var exceptionToLog = new Exception(expectedMessage);
                var expectedSource = "CuttingEdge.Logging.Tests.Unit.LoggerTests.LogExceptionMethodBaseTests." +
                    "Log_WithExceptionWithMessage_LogsExpectedEntry()";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Error, expectedMessage, expectedSource, exceptionToLog);

                // Act
                Logger.Log(exceptionToLog, MethodBase.GetCurrentMethod());

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
                var expectedMessage = "NotSupportedException";
                var exceptionToLog = new NotSupportedException(string.Empty);
                var expectedSource = "CuttingEdge.Logging.Tests.Unit.LoggerTests.LogExceptionMethodBaseTests." +
                    "Log_WithExceptionWithoutMessage_LogsExpectedEntry()";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Error, expectedMessage, expectedSource, exceptionToLog);

                // Act
                Logger.Log(exceptionToLog, MethodBase.GetCurrentMethod());

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
