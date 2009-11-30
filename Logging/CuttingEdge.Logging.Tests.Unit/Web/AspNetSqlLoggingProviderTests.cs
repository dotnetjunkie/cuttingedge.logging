using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;
using CuttingEdge.Logging.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.Web
{
    [TestClass]
    public class AspNetSqlLoggingProviderTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeAspNetSqlLoggingProvider();

            // Act
            provider.Initialize("Valid name", null);
        }

        [TestMethod]
        public void Initiailze_WithValidConfiguration_SetsExpectedProperties()
        {
            // Arrange
            var provider = new FakeAspNetSqlLoggingProvider();
            var validConfiguration = new NameValueCollection();
            validConfiguration.Add("logFormData", "false");
            validConfiguration.Add("logQueryString", "false");
            validConfiguration.Add("userNameRetrievalType", "WindowsIdentity");
            validConfiguration.Add("applicationName", "MyApplication");
            validConfiguration.Add("connectionStringName", "myConnection");

            // Act
            provider.Initialize("Valid name", validConfiguration);

            // Assert
            Assert.AreEqual(false, provider.LogFormData, "LogFormData");
            Assert.AreEqual(false, provider.LogQueryString, "LogQueryString");
            Assert.AreEqual(UserIdentityRetrievalType.WindowsIdentity, provider.RetrievalType, "RetrievalType");
            Assert.AreEqual("MyApplication", provider.ApplicationName, "ApplicationName");
            Assert.AreEqual(ConfigurationManager.ConnectionStrings["myConnection"].ConnectionString, 
                provider.ConnectionString, "ConnectionString");
        }

        [TestMethod]
        public void Initialize_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "AspNetSql",
                    Providers =
                    {
                        // <provider name="AspNetSql" type="FakeAspNetSql..." connectionStringName="..." ini... />
                        new ProviderConfigLine()
                        {
                            Name = "AspNetSql", 
                            Type = typeof(FakeAspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""false""
                                userNameRetrievalType=""None""
                                applicationName=""MyApplication"" "
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
        public void Initialize_ApplicationNameAttributeValueWithMaximumLength_Succeeds()
        {
            // Arrange
            const int MaximumApplicationNameLength = 255;

            string validApplicationName = new string('x', MaximumApplicationNameLength);

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "AspNetSql",
                    Providers =
                    {
                        // <provider name="AspNetSql" type="FakeAspNetSql..." connectionStringName="..." ini... />
                        new ProviderConfigLine()
                        {
                            Name = "AspNetSql", 
                            Type = typeof(FakeAspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""false""
                                userNameRetrievalType=""None""
                                applicationName=""" + validApplicationName + @""" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" connectionString=""some connection value"" />
                    </connectionStrings>"
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Initialize_TooLongApplicationNameAttribute_ThrowsException()
        {
            // Arrange
            const int MaximumApplicationNameLength = 255;

            string invalidApplicationName = new string('x', MaximumApplicationNameLength + 1);

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "AspNetSql",
                    Providers =
                    {
                        // <provider name="AspNetSql" type="FakeAspNetSql..." connectionStringName="..." ini... />
                        new ProviderConfigLine()
                        {
                            Name = "AspNetSql", 
                            Type = typeof(FakeAspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                applicationName=""" + invalidApplicationName + @""" 
                                connectionStringName=""validConnection""
                                initializeSchema=""false""
                                userNameRetrievalType=""None"" "
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
                    Assert.IsTrue(ex.Message.Contains("applicationName"),
                        "Exception message should state about the 'applicationName' attribute.");
                }
            }
        }

        [TestMethod]
        public void Initialize_MissingApplicationName_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "AspNetSql",
                    Providers =
                    {
                        // <provider name="AspNetSql" type="FakeAspNetSql..." connectionStringName="..." ini... />
                        new ProviderConfigLine()
                        {
                            Name = "AspNetSql", 
                            Type = typeof(FakeAspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""false""
                                userNameRetrievalType=""None"" "
                        }
                    }
                },
                Xml = @"
                    <connectionStrings>
                        <add name=""validConnection"" connectionString=""some connection value"" />
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
                    Assert.IsTrue(ex.Message.Contains("applicationName"),
                        "Exception message should state about the 'applicationName' attribute.");
                }
            }
        }

        [TestMethod]
        public void Initialize_InvalidUseNameRetrievalTypeAttribute_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "AspNetSql",
                    Providers =
                    {
                        // <provider name="AspNetSql" type="FakeAspNetSql..." connectionStringName="..." ini... />
                        new ProviderConfigLine()
                        {
                            Name = "AspNetSql", 
                            Type = typeof(FakeAspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                userNameRetrievalType=""INVALID_USER_NAME_RETRIEVAL_TYPE""
                                connectionStringName=""validConnection""
                                initializeSchema=""false""
                                applicationName=""validApplicationName"" "
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
                    Assert.Fail("Exception expected");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("Invalid userNameRetrievalType"),
                        "Exception message should state about the invalid userNameRetrievalType attribute");
                }
            }
        }

        [TestMethod]
        public void Initialize_MissingUseNameRetrievalTypeAttribute_ThrowsException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "AspNetSql",
                    Providers =
                    {
                        // <provider name="AspNetSql" type="FakeAspNetSql..." connectionStringName="..." ini... />
                        new ProviderConfigLine()
                        {
                            Name = "AspNetSql", 
                            Type = typeof(FakeAspNetSqlLoggingProvider), 
                            CustomAttributes = @"
                                connectionStringName=""validConnection""
                                initializeSchema=""false""
                                applicationName=""validApplicationName"" "
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
                    Assert.Fail("Exception expected");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("Empty or missing userNameRetrievalType attribute"),
                        "Exception message should state about the missing userNameRetrievalType attribute");
                }
            }
        }

        internal sealed class FakeAspNetSqlLoggingProvider : AspNetSqlLoggingProvider
        {
            protected override void InitializeDatabaseSchema()
            {
                throw new NotSupportedException();
            }

            protected override int SaveEventToDatabase(SqlTransaction transaction, LoggingEventType severity,
                string message, string source)
            {
                throw new NotSupportedException();
            }

            protected override int SaveExceptionToDatabase(SqlTransaction transaction, Exception exception,
                int parentEventId, int? parentExceptionId)
            {
                throw new NotSupportedException();
            }
        }
    }
}
