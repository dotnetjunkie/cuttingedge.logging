﻿using System;
using System.ComponentModel;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(LoggingEventType, String) method.
    /// </summary>
    [TestClass]
    public class LogLoggingEventTypeStringTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string validMessage = "Valid message";

            // Act
            Logger.Log(validSeverity, validMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void Log_WithInvalidSeverity_ThrowsException()
        {
            // Arrange
            LoggingEventType invalidSeverity = (LoggingEventType)9;
            string validMessage = "Valid message";

            // Act
            Logger.Log(invalidSeverity, validMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string invalidMessage = null;

            // Act
            Logger.Log(validSeverity, invalidMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsException()
        {
            // Arrange
            LoggingEventType validSeverity = LoggingEventType.Critical;
            string invalidMessage = string.Empty;

            // Act
            Logger.Log(validSeverity, invalidMessage);
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedSeverity = LoggingEventType.Error;
                var expectedMessage = "Expected message in entry";

                var expectedEntry =
                    new LogEntry(expectedSeverity, expectedMessage, null, null);

                // Act
                Logger.Log(expectedSeverity, expectedMessage);

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
