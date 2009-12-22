using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class CompositeExceptionTests
    {
        [TestMethod]
        public void ConstructorDefault_Always_ContainsNoInnerExceptions()
        {
            // Arrange
            var exceptionUnderTest = new CompositeException();

            // Act
            var innerExceptions = exceptionUnderTest.InnerExceptions;

            // Assert
            Assert.IsNotNull(innerExceptions);
            Assert.AreEqual(0, innerExceptions.Count);
        }

        [TestMethod]
        public void ConstructorDefault_Always_ContainsDefaultMessage()
        {
            // Arrange
            var exceptionUnderTest = new CompositeException();

            // Assert
            Assert.AreEqual("One or more errors occurred.", exceptionUnderTest.Message);
        }
        
        [TestMethod]
        public void ConstructorMessage_ValidMessage_ContainsSuppliedMessage()
        {
            // Arrange
            var expectedMessage = "Valid message";
            var exceptionUnderTest = new CompositeException(expectedMessage);

            // Arrange
            var actualMessage = exceptionUnderTest.Message;

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void ConstructorMessageInnerException_ValidMessage_ContainsSuppliedMessage()
        {
            // Arrange
            var expectedMessage = "Valid message";
            var exceptionUnderTest = new CompositeException(expectedMessage, new Exception());

            // Arrange
            var actualMessage = exceptionUnderTest.Message;

            // Assert
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void ConstructorMessageInnerException_ValidException_AssignsExceptionToInnerExceptionProperty()
        {
            // Arrange
            var expectedInnerException = new Exception();
            var exceptionUnderTest = new CompositeException("Valid message", expectedInnerException);

            // Arrange
            var actualInnerException = exceptionUnderTest.InnerException;

            // Assert
            Assert.AreEqual(expectedInnerException, actualInnerException);
        }

        [TestMethod]
        public void ConstructorMessageInnerException_ValidException_AddsExceptionToInnerExceptionsCollection()
        {
            // Arrange
            var expectedInnerException = new Exception();
            var exceptionUnderTest = new CompositeException("Valid message", expectedInnerException);

            // Arrange
            var actualInnerExceptions = exceptionUnderTest.InnerExceptions;

            // Assert
            Assert.AreEqual(1, actualInnerExceptions.Count);
            Assert.AreEqual(expectedInnerException, actualInnerExceptions[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorMessageInnerException_InvalidException_ThrowsException()
        {
            // Arrange
            Exception invalidInnerException = null;
            
            // Act
            new CompositeException("Valid message", invalidInnerException);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorMessageInnerExceptions_NullArgument_ThrowsException()
        {
            // Arrange
            Exception[] invalidInnerExceptions = null;

            // Act
            new CompositeException("Valid message", invalidInnerExceptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMessageInnerExceptions_NullElement_ThrowsException()
        {
            // Arrange
            Exception[] invalidInnerExceptions = new Exception[] { new Exception(), null };

            // Act
            new CompositeException("Valid message", invalidInnerExceptions);
        }

        [TestMethod]
        public void ToString_WithSingleInnerException_ReturnsExpectedText()
        {
            // Arrange
            Exception innerException = new InvalidOperationException("bad, bad things have happened.");
            var exceptionUnderTest = new CompositeException("Errors have occurred.", innerException);
            var expectedText = @"CuttingEdge.Logging.CompositeException: Errors have occurred. ---> System.InvalidOperationException: bad, bad things have happened.
   --- End of inner exception stack trace ---
---> (Inner Exception #0) System.InvalidOperationException: bad, bad things have happened.<---
";

            // Act
            var actualText = exceptionUnderTest.ToString();

            // Arrange
            Assert.AreEqual(expectedText, actualText);
        }

        [TestMethod]
        public void GetObjectData_InnerExceptions_SerializesAndDeserializesSuccessfully()
        {
            // Arrange
            CompositeException exceptionToSerialize = null;
            Exception firstInnerException = new Exception("a");
            Exception secondInnerException = new Exception("b");

            try
            {
                throw new CompositeException(string.Empty, new[] { firstInnerException, secondInnerException });
            }
            catch (CompositeException ex)
            {
                exceptionToSerialize = ex;
            }

            // Act
            byte[] serializedException = SerializeCompositeException(exceptionToSerialize);
            CompositeException deserializedException = DeserializeCompositeException(serializedException);

            // Assert
            Assert.IsNotNull(deserializedException);
            Assert.AreEqual(2, deserializedException.InnerExceptions.Count);
            Assert.AreEqual(firstInnerException.Message, deserializedException.InnerExceptions[0].Message);
            Assert.AreEqual(secondInnerException.Message, deserializedException.InnerExceptions[1].Message);
        }

        private static byte[] SerializeCompositeException(CompositeException exceptionToSerialize)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, exceptionToSerialize);

                return stream.ToArray();
            }
        }

        private static CompositeException DeserializeCompositeException(byte[] serializedException)
        {
            using (MemoryStream stream = new MemoryStream(serializedException))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                return (CompositeException)formatter.Deserialize(stream);
            }
        }
    }
}
