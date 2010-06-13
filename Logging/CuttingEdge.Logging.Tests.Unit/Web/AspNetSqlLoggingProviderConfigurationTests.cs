using System;
using System.ComponentModel;
using CuttingEdge.Logging.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.Web
{
    [TestClass]
    public class AspNetSqlLoggingProviderConfigurationTests
    {
        [TestMethod]
        public void Constructor_WithValidApplicationName_Succeeds()
        {
            // Arrange
            var validApplicationName = "Valid name";

            // Act
            new AspNetSqlLoggingProviderConfiguration(validApplicationName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullApplicationName_ThrowsException()
        {
            // Arrange
            string invalidApplicationName = null;

            // Act
            new AspNetSqlLoggingProviderConfiguration(invalidApplicationName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyApplicationName_ThrowsException()
        {
            // Arrange
            string invalidApplicationName = string.Empty;

            // Act
            new AspNetSqlLoggingProviderConfiguration(invalidApplicationName);
        }

        [TestMethod]
        public void Constructor_WithTooLongApplicationName_ThrowsException()
        {
            // Arrange
            string invalidApplicationName = new string('a', 256);

            try
            {
                // Act
                var configuration = new AspNetSqlLoggingProviderConfiguration(invalidApplicationName);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));

                var message = ex.Message;

                Assert.IsTrue(message.Contains("Value is too long"), "Message not descriptive: " + message);
            }
        }

        [TestMethod]
        public void Constructor_WithValidArguments_SetsApplicationNameProperty()
        {
            // Arrange
            var expectedApplicationName = "Valid name";

            // Act
            var configuration = new AspNetSqlLoggingProviderConfiguration(expectedApplicationName);

            // Assert
            Assert.AreEqual(expectedApplicationName, configuration.ApplicationName);
        }

        [TestMethod]
        public void Constructor_WithValidArguments_InitializesLogFormDataWithFalse()
        {
            // Arrange
            var expectedLogFormData = false;

            // Act
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Assert
            Assert.AreEqual(expectedLogFormData, configuration.LogFormData);
        }

        [TestMethod]
        public void Constructor_WithValidArguments_InitializesLogQueryStringWithTrue()
        {
            // Arrange
            var expectedLogQueryString = true;

            // Act
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Assert
            Assert.AreEqual(expectedLogQueryString, configuration.LogQueryString);
        }

        [TestMethod]
        public void Constructor_WithValidArguments_InitializesRetrievalTypeAsNone()
        {
            // Arrange
            var expectedRetrievalType = UserIdentityRetrievalType.None;

            // Act
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Assert
            Assert.AreEqual(expectedRetrievalType, configuration.RetrievalType);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void RetrievalType_WithInvalidValue_ThrowsException()
        {
            // Arrange
            var invalidRetrievalType = (UserIdentityRetrievalType)(-1);
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Act
            configuration.RetrievalType = invalidRetrievalType;
        }

        [TestMethod]
        public void RetrievalType_ChangingValue_ShouldSucceed()
        {
            // Arrange
            var expectedRetrievalType = UserIdentityRetrievalType.Membership;
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Act
            configuration.RetrievalType = expectedRetrievalType;

            // Arrange
            Assert.AreEqual(expectedRetrievalType, configuration.RetrievalType);
        }

        [TestMethod]
        public void LogQueryString_ChangingValueToFalse_ShouldSucceed()
        {
            // Arrange
            var expectedValue = false;
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Act
            configuration.LogQueryString = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, configuration.LogQueryString);
        }

        [TestMethod]
        public void LogQueryString_ChangingValueToTrue_ShouldSucceed()
        {
            // Arrange
            var expectedValue = true;
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Act
            configuration.LogQueryString = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, configuration.LogQueryString);
        }

        [TestMethod]
        public void LogFormData_ChangingValueToFalse_ShouldSucceed()
        {
            // Arrange
            var expectedValue = false;
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Act
            configuration.LogFormData = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, configuration.LogFormData);
        }

        [TestMethod]
        public void LogFormData_ChangingValueToTrue_ShouldSucceed()
        {
            // Arrange
            var expectedValue = true;
            var configuration = new AspNetSqlLoggingProviderConfiguration("Valid name");

            // Act
            configuration.LogFormData = expectedValue;

            // Assert
            Assert.AreEqual(expectedValue, configuration.LogFormData);
        }
    }
}