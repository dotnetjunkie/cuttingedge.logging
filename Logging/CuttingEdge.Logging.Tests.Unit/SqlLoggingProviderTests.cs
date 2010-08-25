using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.Serialization;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class SqlLoggingProviderTests
    {
        private const LoggingEventType ValidThreshold = LoggingEventType.Debug;
        private const string ValidConnectionString = "some connection string";
        private static readonly LoggingProviderBase ValidFallbackProvider = null;

        [TestMethod]
        public void Constructor_WithValidArguments_Succeeds()
        {
            // Act
            new SqlLoggingProvider(ValidThreshold, ValidConnectionString);
        }

        [TestMethod]
        public void Constructor_WithValidArguments2_Succeeds()
        {
            // Act
            new SqlLoggingProvider(ValidThreshold, ValidConnectionString, ValidFallbackProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void Constructor_WithInvalidThreshold_ThrowsException()
        {
            // Arrange
            var invalidThreshold = (LoggingEventType)(-1);

            // Act
            new SqlLoggingProvider(invalidThreshold, ValidConnectionString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConnectionString_ThrowsException()
        {
            // Arrange
            string invalidConnectionString = null;

            // Act
            new SqlLoggingProvider(ValidThreshold, invalidConnectionString, ValidFallbackProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyConnectionString_ThrowsException()
        {
            // Arrange
            string invalidConnectionString = string.Empty;

            // Act
            new SqlLoggingProvider(ValidThreshold, invalidConnectionString, ValidFallbackProvider);
        }

        [TestMethod]
        public void Log_CodeConfiguredFailingProvider_LogsToFallbackProvider()
        {
            // Arrange
            var fallbackProvider = new MemoryLoggingProvider(LoggingEventType.Debug);

            var provider = new FailingSqlLoggingProvider(fallbackProvider);

            // Act
            provider.Log("Test");

            // Assert
            Assert.AreEqual(2, fallbackProvider.GetLoggedEntries().Length, "To events were expected to be logged.");
        }

        [TestMethod]
        public void Log_UninitializedProvider_ThrowsDescriptiveException()
        {
            // Arrange
            var provider = new SqlLoggingProvider();

            try
            {
                // Act
                provider.Log("Some message");

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("The provider has not been initialized"),
                     "A provider that hasn't been initialized correctly, should throw a descriptive " +
                     "exception. Actual: " + ex.Message + Environment.NewLine + ex.StackTrace);

                Assert.IsTrue(ex.Message.Contains("SqlLoggingProvider"),
                    "The message should contain the type name of the unitialized provider. Actual: " +
                    ex.Message);
            }
        }

        [TestMethod]
        public void Initialize_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var provider = new FakeSqlLoggingProvider();
            NameValueCollection validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeSqlLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid name", invalidConfiguration);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "SQL logging provider";
            var provider = new FakeSqlLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithCustomDescription_SetsSpecifiedDescription()
        {
            // Arrange
            var expectedDescription = "My SQL logger";
            var provider = new FakeSqlLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "SqlProv",
                    Providers =
                    {
                        // <provider name="SqlProv" type="FakeSql..." connectionStringName="..." initializeS... />
                        new ProviderConfigLine()
                        {
                            Name = "SqlProv", 
                            Type = typeof(FakeSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""false"" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" connectionString=""some connection value"" />
                    </connectionStrings>
                "
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_MissingConnectionStringName_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Provider Name",
                    Providers =
                    {
                        // <provider name="Provider Name" type="FakeSqlLoggingProvider" initializeSchema="false" />
                        new ProviderConfigLine()
                        {
                            Name = "Provider Name", 
                            Type = typeof(FakeSqlLoggingProvider), 
                            CustomAttributes = @"initializeSchema=""false"" "
                        }
                    }
                }
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("Empty or missing 'connectionStringName'"),
                        "The exception message should express the fact that there is no connectionStringName: "
                        + ex.Message);

                    Assert.IsTrue(ex.Message.Contains("'Provider Name'"),
                        "The exception message should contain the name of the provider. Actual message: "
                        + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_MissingConnectionString_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Sql",
                    Providers =
                    {
                        // <provider name="Sql" type="FakeSql.." connectionStringName="..." initializeSchema=... />
                        new ProviderConfigLine()
                        {
                            Name = "Sql", 
                            Type = typeof(FakeSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""myConnection""
                                initializeSchema=""false"" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <!-- Missing connection string -->
                    </connectionStrings>"
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("Missing connection string 'myConnection'"),
                        "The exception message should express the fact that there is no connection string: " 
                        + ex.Message);

                    Assert.IsTrue(ex.Message.Contains("<connectionStrings>"),
                        "The exception message should note the location where this connection is missing: " 
                        + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_InvalidInitializeSchema_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Sql",
                    Providers =
                    {
                        // <provider name="Sql" type="FakeSql.." initializeSchema="invalidValue" connectionS... />
                        new ProviderConfigLine()
                        {
                            Name = "Sql", 
                            Type = typeof(FakeSqlLoggingProvider), 
                            CustomAttributes = @"
                                initializeSchema=""invalidValue"" 
                                connectionStringName=""validConnection""
                                "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" connectionString=""some connection value"" />
                    </connectionStrings>
                "
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("initializeSchema"),
                        "The exception message should note the invalid attribute: " + ex.Message);
                }
            }
        }

#if DEBUG
        [TestMethod]
        public void Log_ExceptionWithoutInnerException_LogsThatSingleException()
        {
            // Arrange
            int expectedNumberOfLoggedExceptions = 1;

            Exception exceptionToLog = CreateThrownException<ArgumentNullException>();

            var logger = new CachingSqlLoggingProvider();

            // Act
            logger.Log(exceptionToLog);

            // Arrange
            Assert.AreEqual(expectedNumberOfLoggedExceptions, logger.LoggedExceptions.Count,
                "We'd expect a single exception to be logged and nothing else..");
            Assert.AreEqual(exceptionToLog, logger.LoggedExceptions[0].Exception);
        }

        [TestMethod]
        public void Log_ExceptionWithInnerException_LogsExceptionAndItsInnerException()
        {
            // Arrange
            int expectedNumberOfLoggedExceptions = 2;

            Exception exceptionToLog = new InvalidCastException("message", new FormatException());

            var logger = new CachingSqlLoggingProvider();

            // Act
            logger.Log(exceptionToLog);

            // Arrange
            Assert.AreEqual(expectedNumberOfLoggedExceptions, logger.LoggedExceptions.Count,
                "We'd expect a single exception to be logged and nothing else..");
            Assert.AreEqual(exceptionToLog.InnerException, logger.LoggedExceptions[1].Exception);
        }

        [TestMethod]
        public void Log_CompositeExceptionWithMultipleInnerExceptions_LogsMultipleInnerExceptions()
        {
            // Arrange
            int expectedNumberOfLoggedExceptions = 3;
            Exception exceptionWithMultipleInnerExceptions;

            try
            {
                throw new CompositeException("failure 1 / parent exception",
                    new Exception[]
                    {
                        CreateThrownException<ArgumentException>(),
                        CreateThrownException<InvalidOperationException>()
                    });
            }
            catch (Exception ex)
            {
                exceptionWithMultipleInnerExceptions = ex;
            }

            var logger = new CachingSqlLoggingProvider();

            // Act
            logger.Log(exceptionWithMultipleInnerExceptions);

            // Arrange
            Assert.AreEqual(expectedNumberOfLoggedExceptions, logger.LoggedExceptions.Count,
                "The logged exception contains two inner exceptions and they are expected to be logged.");
        }

        [TestMethod]
        public void Logging_CustomExceptionWithMultipleInnerExceptions_LogsMultipleInnerExceptions()
        {
            // Arrange
            int expectedNumberOfLoggedExceptions = 3;

            var exceptionWithMultipleInnerExceptions = new MultipleFailuresException("failure 1 / parent")
            {
                InnerExceptions = new Exception[]
                {
                    new ArgumentException("failure 2 / inner exception 1"),
                    new InvalidOperationException("failure 3 / inner exception 2")
                }
            };

            var logger = new CachingSqlLoggingProvider();

            // Act
            logger.Log(exceptionWithMultipleInnerExceptions);

            // Arrange
            Assert.AreEqual(expectedNumberOfLoggedExceptions, logger.LoggedExceptions.Count,
                "The logged exception exposed two inner exceptions through it's public 'InnerExceptions' " +
                "property and they are expected to be logged.");
        }

        [TestMethod]
        public void Log_ExceptionWithMultipleLevelsOfInnerExceptions_LogsWholeHierachyAsExpected()
        {
            // Arrange
            int expectedNumberOfLoggedExceptions = 6;

            var exceptionWithMultipleInnerExceptions = new CompositeException("failure 1 / parent",
                new Exception[]
                {
                    new ArgumentException("failure 2 / inner level 1"),
                    new InvalidOperationException("failure 3 / inner level 1"),
                    new MultipleFailuresException("failure 4 / parent level 1")
                    {
                        InnerExceptions = new Exception[]
                        {
                            new ArgumentException("failure 5 / inner level 2"),
                            new InvalidOperationException("failure 6 / inner level 2")
                        },
                    }
                });

            var logger = new CachingSqlLoggingProvider();

            // Act
            logger.Log(exceptionWithMultipleInnerExceptions);

            // Arrange
            Assert.AreEqual(expectedNumberOfLoggedExceptions, logger.LoggedExceptions.Count,
                "Inner exceptions that contain multiple exceptions by them selves should be logged.");
        }
#endif

        private static NameValueCollection CreateValidConfiguration()
        {
            var configuration = new NameValueCollection();

            configuration.Add("connectionStringName", ConfigurationManager.ConnectionStrings[0].Name);

            return configuration;
        }

        private static Exception CreateThrownException<TException>() where TException : Exception, new()
        {
            try
            {
                throw new TException();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private sealed class FailingSqlLoggingProvider : FakeSqlLoggingProvider
        {
            public FailingSqlLoggingProvider(LoggingProviderBase fallbackProvider)
                : base(fallbackProvider)
            {
            }

            protected override object LogInternal(LogEntry entry)
            {
                throw new InvalidOperationException("Fail!");
            }
        }

#if DEBUG
        private sealed class CachingSqlLoggingProvider : FakeSqlLoggingProvider
        {
            private int lastExceptionId = 0;

            public CachingSqlLoggingProvider() : base(null)
            {
                this.LoggedExceptions = new Collection<ExceptionRecord>();
            }

            public Collection<ExceptionRecord> LoggedExceptions { get; private set; }

            // Override log internal and prevent the creation of a SqlConnection and Transaction.
            protected override object LogInternal(LogEntry entry)
            {
                return this.LogWithinTransaction(entry, null);
            }

            protected override int SaveEventToDatabase(SqlTransaction transaction, LoggingEventType severity,
                string message, string source)
            {
                return 0;
            }

            protected override int SaveExceptionToDatabase(SqlTransaction transaction, Exception exception,
                int parentEventId, int? parentExceptionId)
            {
                this.lastExceptionId++;

                this.LoggedExceptions.Add(new ExceptionRecord()
                {
                    ExceptionId = this.lastExceptionId,
                    ParentExceptionId = parentExceptionId,
                    Exception = exception
                });

                return this.lastExceptionId;
            }
        }
#endif

        private class FakeSqlLoggingProvider : SqlLoggingProvider
        {
            public FakeSqlLoggingProvider()
            {
            }

            protected FakeSqlLoggingProvider(LoggingProviderBase fallbackProvider)
                : base(LoggingEventType.Debug, ValidConnectionString, fallbackProvider)
            {
            }

            protected override void InitializeDatabaseSchema()
            {
                Assert.Fail("This method should not be called.");
            }

            protected override int SaveEventToDatabase(SqlTransaction transaction, LoggingEventType severity,
                string message, string source)
            {
                Assert.Fail("This method should not be called.");
                throw new NotSupportedException();
            }

            protected override int SaveExceptionToDatabase(SqlTransaction transaction, Exception exception,
                int parentEventId, int? parentExceptionId)
            {
                Assert.Fail("This method should not be called.");
                throw new NotSupportedException();
            }
        }

        [DebuggerDisplay("{Exception.GetType().Name}, Id: {ExceptionId}, ParentExceptionId: {ParentExceptionId}, Message: {Exception.Message}")]
        private sealed class ExceptionRecord
        {
            public int ExceptionId { get; set; }

            public int? ParentExceptionId { get; set; }

            public Exception Exception { get; set; }
        }

        [Serializable]
        private class MultipleFailuresException : Exception
        {
            public MultipleFailuresException()
            {
            }

            public MultipleFailuresException(string message)
                : base(message)
            {
            }

            public MultipleFailuresException(string message, Exception inner)
                : base(message, inner)
            {
            }

            protected MultipleFailuresException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            public IEnumerable<Exception> InnerExceptions { get; set; }
        }
    }
}