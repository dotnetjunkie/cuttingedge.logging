using System;
using System.Linq;
using System.Reflection;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class Log(String, Exception, MethodBase) method.
    /// </summary>
    [TestClass]
    public class LogStringExceptionMethodBaseTests
    {
        [TestMethod]
        public void Log_WithValidArguments_Succeeds()
        {
            // Arrange
            string validMessage = "Valid message";
            Exception validException = new Exception();
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase validSource = MethodBase.GetCurrentMethod();

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
            MethodBase invalidSource = null;

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
                Exception expectedException = new Exception();
                var expectedSource = this.GetType().FullName + ".Log_WithValidArguments_LogsExpectedEntry()";

                var expectedEntry =
                    new LogEntry(LoggingEventType.Error, expectedMessage, expectedSource, expectedException);

                // Act
                Logger.Log(expectedMessage, expectedException, MethodBase.GetCurrentMethod());

                // Assert
                var loggedEntry = scope.LoggedEntries.First();
                AssertHelper.LogEntriesAreEqual(expectedEntry, loggedEntry);
            }
        }
    }
}
