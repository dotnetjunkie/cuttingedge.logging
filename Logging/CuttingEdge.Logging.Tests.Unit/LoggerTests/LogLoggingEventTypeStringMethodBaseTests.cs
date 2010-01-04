using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(LoggingEventType, String, MethodBase) method.
    /// </summary>
    [TestClass]
    public class LogLoggingEventTypeStringMethodBaseTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string validMessage = "Valid message";
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase invalidSource = null;

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
                var expectedSource = this.GetType().FullName + ".Log_WithValidArguments_LogsExpectedEntry()";

                var expectedEntry =
                    new LogEntry(expectedSeverity, expectedMessage, expectedSource, null);

                // Act
                Logger.Log(expectedSeverity, expectedMessage, MethodBase.GetCurrentMethod());

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
