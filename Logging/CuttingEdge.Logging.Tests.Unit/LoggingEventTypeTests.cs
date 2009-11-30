using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    /// <summary>
    /// Tests the <see cref="LoggingEventType"/> enum. These tests confirm the actual values of the enum
    /// constants. Users of the framework depend on these values.
    /// </summary>
    [TestClass]
    public class LoggingEventTypeTests
    {
        [TestMethod]
        public void Debug_HasAvalueOfZero()
        {
            Assert.AreEqual(0, (int)LoggingEventType.Debug);
        }

        [TestMethod]
        public void Information_HasAValueOfOne()
        {
            Assert.AreEqual(1, (int)LoggingEventType.Information);
        }

        [TestMethod]
        public void Warning_HasAValueOfTwo()
        {
            Assert.AreEqual(2, (int)LoggingEventType.Warning);
        }

        [TestMethod]
        public void Error_HasAValueOfThree()
        {
            Assert.AreEqual(3, (int)LoggingEventType.Error);
        }

        [TestMethod]
        public void Critical_HasAValueOfFour()
        {
            Assert.AreEqual(4, (int)LoggingEventType.Critical);
        }

        [TestMethod]
        public void LoggingEventTypeEnumContainsFiveValues()
        {
            int expectedNumberOfValues = 5;
            int actualNumberOfValues = Enum.GetValues(typeof(LoggingEventType)).Length;

            Assert.AreEqual(expectedNumberOfValues, actualNumberOfValues);
        }
    }
}
