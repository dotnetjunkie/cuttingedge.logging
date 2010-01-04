using System;
using System.Linq;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(String, Exception, String) method.
    /// </summary>
    [TestClass]
    public class LogStringExceptionStringTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeds()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception validException = new Exception();
            string validSource = "Valid source";

            // Act
            Logger.Log(validMessage, validException, validSource);            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = null;
            Exception validException = new Exception();
            string validSource = "Valid source";

            // Act
            Logger.Log(invalidMessage, validException, validSource);      
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptyMessage_ThrowsException()
        {
            // Arrange
            string invalidMessage = string.Empty;
            Exception validException = new Exception();
            string validSource = "Valid source";

            // Act
            Logger.Log(invalidMessage, validException, validSource);      
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullException_ThrowsException()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception invalidException = null;
            string validSource = "Valid source";

            // Act
            Logger.Log(validMessage, invalidException, validSource);      
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_WithNullSource_ThrowsException()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception validException = new Exception();
            string invalidSource = null;

            // Act
            Logger.Log(validMessage, validException, invalidSource);      
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Log_WithEmptySource_ThrowsException()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception validException = new Exception();
            string invalidSource = string.Empty;

            // Act
            Logger.Log(validMessage, validException, invalidSource);      
        }

        [TestMethod]
        public void Log_WithValidArguments_LogsExpectedEntry()
        {
            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Arrange
                var expectedMessage = "Expected message in entry";
                var expectedException = new Exception();
                var expectedSource = "Expected source text";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Error, expectedMessage, expectedSource, expectedException);

                // Act
                Logger.Log(expectedMessage, expectedException, expectedSource);      

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
