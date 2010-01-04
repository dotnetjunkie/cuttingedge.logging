using System;
using System.Linq;
using System.Reflection;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(String, MethodBase) method.
    /// </summary>
    [TestClass]
    public class LogStringMethodBaseTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            string validMessage = "Valid message";
            MethodBase validSource = MethodBase.GetCurrentMethod();

            // Act
            Logger.Log(validMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = null;
            MethodBase validSource = MethodBase.GetCurrentMethod();

            // Act
            Logger.Log(invalidMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = string.Empty;
            MethodBase validSource = MethodBase.GetCurrentMethod();

            // Act
            Logger.Log(invalidMessage, validSource);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullSource_ThrowsException()
        {
            // Arrange
            string validMessage = "Valid message";
            MethodBase invalidSource = null;

            // Act
            Logger.Log(validMessage, invalidSource);
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";
                var expectedSource =
                    this.GetType().FullName + ".Log_WithValidArguments_LogsExpectedEntry()";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Information, expectedMessage, expectedSource, null);

                // Act
                Logger.Log(expectedMessage, MethodBase.GetCurrentMethod());

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }

        [TestMethod]
        public void Log_WithMethodBaseWithParameters_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";
                int notImportant;
                MethodBase sourceToLog = CreateMethodBaseFromThisMethod("not important", out notImportant);
                var expectedSource = 
                    this.GetType().FullName + ".CreateMethodBaseFromThisMethod(String, out Int32&)";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Information, expectedMessage, expectedSource, null);

                // Act
                Logger.Log(expectedMessage, sourceToLog);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }

        private static MethodBase CreateMethodBaseFromThisMethod(string a, out int b)
        {
            b = 0;
            return MethodBase.GetCurrentMethod();
        }
    }
}
