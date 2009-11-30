using System;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Integration
{
    [TestClass]
    public class AspNetSqlLoggingProviderTests
    {
        [TestMethod]
        public void Initialize_WithInitializeSchemaTrue_CreatesSchemaSuccesfully()
        {
            // Arrange
            bool initializeSchema = true;
            IConfigurationWriter configuration = BuildValidConfiguration(initializeSchema);

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
            IConfigurationWriter configuration = BuildValidConfiguration(initializeSchema);
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

        private static IConfigurationWriter BuildValidConfiguration(bool initializeSchema)
        {
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
                            Type = typeof(AspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""" + initializeSchema + @""" 
                                userNameRetrievalType=""None""
                                applicationName=""MyApplication"" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" 
                            connectionString=""" + TestConfiguration.ConnectionString + @""" />
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
