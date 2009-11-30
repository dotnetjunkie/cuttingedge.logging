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
        public void Initialize_WithInitializeSchemaTrue_CreatesSchemaSuccesfully()
        {
            // Arrange
            bool initializeSchema = true;
            string validConnectionString = TestConfiguration.ConnectionString;
            IConfigurationWriter configuration = 
                BuildValidConfiguration(initializeSchema, validConnectionString);

            using (var manager = new IntegrationTestLoggingAppDomainManager(configuration))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Log_WithValidLogEntry_Succeeds()
        {
            // Arrange
            bool initializeSchema = true;
            string validConnectionString = TestConfiguration.ConnectionString;
            IConfigurationWriter configuration =
                BuildValidConfiguration(initializeSchema, validConnectionString);
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
        public void Initialize_WithInitializeSchemaTrueButInvalidConnectionString_ThrowsException()
        {
            // Arrange
            bool initializeSchema = true;
            string badConnectionString =
                "server=.;Initial Catalog=" + TestConfiguration.DatabaseName + ";User Id=bad;Password=bad;";
            IConfigurationWriter configuration = 
                BuildValidConfiguration(initializeSchema, badConnectionString);

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
