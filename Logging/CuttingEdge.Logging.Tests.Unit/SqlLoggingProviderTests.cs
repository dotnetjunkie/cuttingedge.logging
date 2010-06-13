using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;

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

        private static NameValueCollection CreateValidConfiguration()
        {
            var configuration = new NameValueCollection();

            configuration.Add("connectionStringName", ConfigurationManager.ConnectionStrings[0].Name);

            return configuration;
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
    }
}
