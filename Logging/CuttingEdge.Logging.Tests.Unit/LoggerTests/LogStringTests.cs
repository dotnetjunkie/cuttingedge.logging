using System;
using System.Linq;

using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(String) method.
    /// </summary>
    [TestClass]
    public class LogStringTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            Logger.Log("Valid message");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = null;

            // Act
            Logger.Log(invalidMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsException()
        {
            Logger.Log(string.Empty);
        }

        [TestMethod]
        public void Log_WithExceptionWithMessage_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Information, expectedMessage, null, null);

                // Act
                Logger.Log(expectedMessage);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
