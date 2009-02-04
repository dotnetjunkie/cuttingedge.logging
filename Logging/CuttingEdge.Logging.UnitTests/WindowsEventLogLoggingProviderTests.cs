using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    /// <summary>
    /// Tests the <see cref="WindowsEventLogLoggingProvider"/> class.
    /// </summary>
    [TestClass]
    public class WindowsEventLogLoggingProviderTests
    {
        [TestMethod]
        public void BuildEventLogMessageTest01()
        {
            WindowsEventLogLoggingProviderTester tester = new WindowsEventLogLoggingProviderTester();

            LogEntry entry = 
                new LogEntry(LoggingEventType.Error, "Test message", "This is a source", null);

            string logMessage = tester.PublicBuildEventLogMessage(entry);

            Assert.AreEqual("Test message\r\nSeverity: Error\r\nSource: This is a source\r\n", logMessage);
        }

        [TestMethod]
        public void BuildEventLogMessageTest02()
        {
            WindowsEventLogLoggingProviderTester tester = new WindowsEventLogLoggingProviderTester();

            Exception exception;
            try
            {
                throw new InvalidOperationException("Invalid operation");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            LogEntry entry =
                new LogEntry(LoggingEventType.Error, "Test message", "This is a source", exception);

            string logMessage = tester.PublicBuildEventLogMessage(entry);

            Assert.IsTrue(logMessage.StartsWith(@"Test message
Severity: Error
Source: This is a source

Exception: System.InvalidOperationException
Message: Invalid operation
Stacktrace:"));
        }

        private class WindowsEventLogLoggingProviderTester : WindowsEventLogLoggingProvider
        {
            public string PublicBuildEventLogMessage(LogEntry entry)
            {
                return this.BuildEventLogMessage(entry);
            }
        }
    }
}
