using System;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(String, Exception) method.
    /// </summary>
    [TestClass]
    public class LogStringExceptionTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception validException = new Exception();

            // Act
            Logger.Log(validMessage, validException);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = null;
            Exception validException = new Exception();

            // Act
            Logger.Log(invalidMessage, validException);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = string.Empty;
            Exception validException = new Exception();

            // Act
            Logger.Log(invalidMessage, validException);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullException_ThrowsException()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception invalidException = null;

            // Act
            Logger.Log(validMessage, invalidException);
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";
                var expectedException = new Exception();

                var expectedEntry =
                    new LogEntry(LoggingEventType.Error, expectedMessage, null, expectedException);

                // Act
                Logger.Log(expectedMessage, expectedException);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
