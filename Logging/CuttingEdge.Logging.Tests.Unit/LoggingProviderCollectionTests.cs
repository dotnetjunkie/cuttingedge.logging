using System;
using System.Collections.Specialized;
using System.Configuration.Provider;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class LoggingProviderCollectionTests
    {
        [TestMethod]
        public void Add_WithValidArgument_Succeeds()
        {
            // Arrange
            var collection = new LoggingProviderCollection();
            var validProvider = new MemoryLoggingProvider();
            validProvider.Initialize("Valid provider name", new NameValueCollection());

            // Act
            collection.Add(validProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_WithNullArgument_ThrowsException()
        {
            // Arrange
            var collection = new LoggingProviderCollection();
            ProviderBase invalidProvider = null;

            // Act
            collection.Add(invalidProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_WithInvalidType_ThrowsException()
        {
            // Arrange
            var collection = new LoggingProviderCollection();
            var invalidProvider = new UnrelatedProvider();
            invalidProvider.Initialize("Valid provider name", new NameValueCollection());

            // Act
            collection.Add(invalidProvider);
        }

        private sealed class UnrelatedProvider : ProviderBase
        {
        }
    }
}
