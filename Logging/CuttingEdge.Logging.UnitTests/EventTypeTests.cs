using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    /// <summary>
    /// Tests the <see cref="EventType"/> enum.
    /// </summary>
    [TestClass]
    public class EventTypeTests
    {
        [TestMethod]
        public void EventTypeErrorHasIntValueZero()
        {
            Assert.AreEqual(0, (int)EventType.Error);
        }

        [TestMethod]
        public void EventTypeWarningHasIntValueOne()
        {
            Assert.AreEqual(1, (int)EventType.Warning);
        }

        [TestMethod]
        public void EventTypeInformationHasIntValueTwo()
        {
            Assert.AreEqual(2, (int)EventType.Information);
        }

        [TestMethod]
        public void EventTypeEnumContainsThreeValues()
        {
            Assert.AreEqual(3, Enum.GetValues(typeof(EventType)).Length);
        }
    }
}
