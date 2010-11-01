using System;
using System.Configuration;

using CuttingEdge.Logging.Tests.Common;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Integration
{
    [TestClass]
    public class SqlLoggingProviderTests
    {
        [TestMethod]
        public void Configuration_WithInitializeSchemaTrue_CreatesSchemaSuccesfully()
        {
            // Arrange
            const bool InitializeSchema = true;
            string validConnectionString = TestConfiguration.ConnectionString;
            IConfigurationWriter configuration = 
                BuildValidConfiguration(InitializeSchema, validConnectionString);

            using (var manager = new IntegrationTestLoggingAppDomainManager(configuration))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_LoggingAValidLogEntry_Succeeds()
        {
            // Arrange
            const bool InitializeSchema = true;
            string validConnectionString = TestConfiguration.ConnectionString;
            IConfigurationWriter configuration =
                BuildValidConfiguration(InitializeSchema, validConnectionString);
            Exception exceptionWithStackTrace = GetExceptionWithStackTrace();
            LogEntry entry =
                new LogEntry(LoggingEventType.Critical, "my message", "my source", exceptionWithStackTrace);

            using (var manager = new IntegrationTestLoggingAppDomainManager(configuration))
            {
                manager.DomainUnderTest.InitializeLoggingSystem();

                // Act
                manager.DomainUnderTest.Log(entry);
            }
        }

        [TestMethod]
        public void Configuration_LoggingAValidLogEntryWithoutAnException_Succeeds()
        {
            // Arrange
            const bool InitializeSchema = true;
            string validConnectionString = TestConfiguration.ConnectionString;
            IConfigurationWriter configuration =
                BuildValidConfiguration(InitializeSchema, validConnectionString);
            
            LogEntry entry = new LogEntry(LoggingEventType.Critical, "my message", "my source", null);

            using (var manager = new IntegrationTestLoggingAppDomainManager(configuration))
            {
                manager.DomainUnderTest.InitializeLoggingSystem();

                // Act
                manager.DomainUnderTest.Log(entry);
            }
        }

        [TestMethod]
        public void Configuration_WithInitializeSchemaTrueButInvalidConnectionString_ThrowsException()
        {
            // Arrange
            const bool InitializeSchema = true;
            string badConnectionString =
                "server=.;Initial Catalog=" + TestConfiguration.DatabaseName + ";User Id=bad;Password=bad;";
            IConfigurationWriter configuration = 
                BuildValidConfiguration(InitializeSchema, badConnectionString);

            using (var manager = new IntegrationTestLoggingAppDomainManager(configuration))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();
                }
                catch (ConfigurationErrorsException ex)
                {
                    bool messageIsValid = 
                        ex.Message.Contains("Initialization of database schema") && 
                        ex.Message.Contains("failed");

                    Assert.IsTrue(messageIsValid, "Exception message should describe the failure. " +
                        "Actual message: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_TwoProvidersWithInitializeSchemaTrue_ThrowsException()
        {
            // Arrange
            const bool InitializeSchema = true;
            const string NameOfFailingProvider = "Failing provider";
            string validConnectionString = TestConfiguration.ConnectionString;

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Default",
                    Providers =
                    {
                        // <provider name="Default" type="SqlLoggingProvider" connectionStringName="..." />
                        new ProviderConfigLine()
                        {
                            Name = "Default", 
                            Type = typeof(SqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""" + InitializeSchema + @""" "
                        },

                        // <provider name="Second" type="SqlLoggingProvider" connectionStringName="..." />
                        new ProviderConfigLine()
                        {
                            Name = NameOfFailingProvider, 
                            Type = typeof(SqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""" + InitializeSchema + @""" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" connectionString=""" + validConnectionString + @""" />
                    </connectionStrings>
                "
            };

            using (var manager = new IntegrationTestLoggingAppDomainManager(configBuilder.Build()))
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
                    var msg = ex.Message ?? string.Empty;

                    Assert.IsTrue(msg.Contains("already") && msg.Contains("initialized"),
                        "Exception message should contain the problem. Actual: " + msg);

                    Assert.IsTrue(msg.Contains(NameOfFailingProvider),
                        "Exception message should contain the name of the failing provider. Actual: " + msg);

                    Assert.IsTrue(msg.Contains(
                        "remove the 'initializeSchema' attribute from the provider configuration."),
                        "Exception message should contain the solution. Actual: " + msg);
                }
            }
        }

        private static IConfigurationWriter BuildValidConfiguration(bool initializeSchema, 
            string connectionString)
        {
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "SqlProv",
                    Providers =
                    {
                        // <provider name="SqlProv" type="SqlLoggingProvider..." connectionStringName="..." />
                        new ProviderConfigLine()
                        {
                            Name = "SqlProv", 
                            Type = typeof(SqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""" + initializeSchema + @""" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" connectionString=""" + connectionString + @""" />
                    </connectionStrings>
                "
            };

            return configBuilder.Build();
        }

        private static Exception GetExceptionWithStackTrace()
        {
            try
            {
                throw new InvalidOperationException("Exception message");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
