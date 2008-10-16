using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    /// <summary>
    /// Tests the <see cref="LoggingEventType"/> enum.
    /// </summary>
    [TestClass]
    public class LoggingEventTypeTests
    {
        [TestMethod]
        public void EventTypeErrorHasIntValueZero()
        {
            Assert.AreEqual(0, (int)LoggingEventType.Debug);
        }

        [TestMethod]
        public void EventTypeWarningHasIntValueOne()
        {
            Assert.AreEqual(1, (int)LoggingEventType.Information);
        }

        [TestMethod]
        public void EventTypeInformationHasIntValueTwo()
        {
            Assert.AreEqual(2, (int)LoggingEventType.Warning);
        }

        [TestMethod]
        public void EventTypeInformationHasIntValueThree()
        {
            Assert.AreEqual(3, (int)LoggingEventType.Error);
        }

        [TestMethod]
        public void EventTypeInformationHasIntValueFour()
        {
            Assert.AreEqual(4, (int)LoggingEventType.Critical);
        }

        [TestMethod]
        public void EventTypeEnumContainsThreeValues()
        {
            Assert.AreEqual(5, Enum.GetValues(typeof(LoggingEventType)).Length);
        }
    }
}
