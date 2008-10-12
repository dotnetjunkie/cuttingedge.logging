using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Provider;

using CuttingEdge.Logging.UnitTests.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.UnitTests
{
    /// <summary>
    /// Tests the <see cref="LoggingProviderBase"/> class.
    /// </summary>
    [TestClass]
    public class LoggingProviderBaseTests
    {
        [TestMethod]
        public void LoggingProviderBaseLogShouldSucceedWithValidArguments()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Log(EventType.Error, "message", "source", new Exception());
        }

        [TestMethod]
        public void LoggingProviderBaseLogShouldFailOnInvalidEventType()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            foreach (EventType type in EventTypeEnumerator.GetInvalidValues())
            {
                try
                {
                    tester.Log(type, "message", "source", new Exception());
                    Assert.Fail();
                }
                catch (InvalidEnumArgumentException)
                {
                    // An InvalidEnumArgumentException should be thrown.
                }
            }
        }

        [TestMethod]
        public void LoggingProviderBaseLogShouldSucceedOnValidEventType()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            foreach (EventType type in EventTypeEnumerator.GetValidValues())
            {
                tester.Log(type, "message", "source", new Exception());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoggingProviderBaseLogShouldFailOnInvalidMessage1()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Log(EventType.Error, null, "source", new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoggingProviderBaseLogShouldFailOnInvalidMessage2()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Log(EventType.Error, String.Empty, "source", new Exception());
        }

        [TestMethod]
        public void LoggingProviderBaseLogShouldSucceedOnNullSource()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Log(EventType.Error, "message", null, new Exception());
        }

        [TestMethod]
        public void LoggingProviderBaseLogShouldSucceedOnEmptySource()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Log(EventType.Error, "message", string.Empty, new Exception());
        }

        [TestMethod]
        public void LoggingProviderBaseLogShouldSucceedOnNullException()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Log(EventType.Error, "message", "source", null);
        }

        [TestMethod]
        public void InitializeShouldSucceed()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Initialize("MyProvider", new NameValueCollection());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeShouldFailWhenConfigIsNull()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Initialize("MyProvider", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeShouldFailWhenNameIsNull()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            tester.Initialize(null, new NameValueCollection());
        }

        [TestMethod]
        public void InitializeShouldFailWhenUnrecognizedAttributesAreFound()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            NameValueCollection config = new NameValueCollection();
            config.Add("badAttribute", "some value");

            try
            {
                tester.Initialize("MyProvider", config);
            }
            catch (ProviderException pex)
            {
                Assert.AreEqual(typeof(ProviderException), pex.GetType());

                Assert.IsTrue(pex.Message.Contains("badAttribute"));
            }
        }

        [TestMethod]
        public void InitializationOfTheFallbackProviderShouldWork1()
        {
            StubLoggingProvider tester = new StubLoggingProvider();

            NameValueCollection config = new NameValueCollection();

            tester.Initialize("MyProvider", config);

            Assert.AreEqual(null, tester.FallbackProvider);
        }
    }
}
