using System;
using System.ComponentModel;
using System.Reflection;

using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class LoggerExtensionsTests
    {
        // This class contains just one test, because all other possible tests on the LoggerExtensions class
        // are covered by the LoggerTests class. The static Logger facade calls into the LoggingProviderBase
        // which on it's turn calls the LoggerExtensions class.
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoggerExtensionsLog_WithNullLogger_ThrowsException()
        {
            // Arrange
            ILogger invalidLogger = null;

            // Act
            LoggerExtensions.Log(invalidLogger, "Valid message");
        }
    }
}