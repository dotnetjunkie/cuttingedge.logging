using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.Helpers
{
    public static class AssertHelper
    {
        public static void LogEntriesAreEqual(LogEntry expected, LogEntry actual)
        {
            if (expected == actual)
            {
                Assert.Fail("Programming error in unit test. Expected and actual are expected to be " + 
                    "different objects.");
            }

            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Severity, actual.Severity);
            Assert.AreEqual(expected.Message, actual.Message);
            Assert.AreEqual(expected.Source, actual.Source);
            Assert.AreEqual(expected.Exception, actual.Exception);
        }
    }
}
