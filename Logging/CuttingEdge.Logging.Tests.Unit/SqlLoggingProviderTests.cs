using System;
using System.Collections.Specialized;
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

        private sealed class FakeSqlLoggingProvider : SqlLoggingProvider
        {
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
