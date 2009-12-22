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
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "ASP.NET SQL logging provider";
            var provider = new FakeAspNetSqlLoggingProvider();
            var settings = CreateValidAspNetSqlLoggingSettings();
            settings.Description = null;
            var validConfiguration = settings.BuildConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithCustomDescription_SetsSpecifiedDescription()
        {
            // Arrange
            var expectedDescription = "My web app logging provider";
            var provider = new FakeAspNetSqlLoggingProvider();
            var settings = CreateValidAspNetSqlLoggingSettings();
            settings.Description = expectedDescription;
            var validConfiguration = settings.BuildConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Initialize_WithValidConfiguration_SetsExpectedProperties()
        {
            // Arrange
            var provider = new FakeAspNetSqlLoggingProvider();
            var settings = CreateValidAspNetSqlLoggingSettings();
            var validConfiguration = settings.BuildConfiguration();
            var expectedConnectionString =
                ConfigurationManager.ConnectionStrings[settings.ConnectionStringName].ConnectionString;

            // Act
            provider.Initialize("Valid name", validConfiguration);

            // Assert
            Assert.AreEqual(settings.LogFormData, provider.LogFormData, "LogFormData");
            Assert.AreEqual(settings.LogQueryString, provider.LogQueryString, "LogQueryString");
            Assert.AreEqual(settings.RetrievalType, provider.RetrievalType, "RetrievalType");
            Assert.AreEqual(settings.ApplicationName, provider.ApplicationName, "ApplicationName");
            Assert.AreEqual(expectedConnectionString, provider.ConnectionString, "ConnectionString");
        }

        [TestMethod]
        public void Configuration_WithValidConfiguration_Succeeds()
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
        public void Configuration_ApplicationNameAttributeValueWithMaximumLength_Succeeds()
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
        public void Configuration_TooLongApplicationNameAttribute_ThrowsException()
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
        public void Configuration_MissingApplicationName_ThrowsException()
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
        public void Configuration_InvalidUseNameRetrievalTypeAttribute_ThrowsException()
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
        public void Configuration_MissingUseNameRetrievalTypeAttribute_ThrowsException()
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

        private static AspNetSqlLoggingSettings CreateValidAspNetSqlLoggingSettings()
        {
            return new AspNetSqlLoggingSettings
            {
                Description = null,
                LogFormData = false,
                LogQueryString = false,
                RetrievalType = UserIdentityRetrievalType.WindowsIdentity,
                ApplicationName = "MyApplication",
                ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name,
            };
        }

        internal class AspNetSqlLoggingSettings
        {
            public AspNetSqlLoggingSettings()
            {
                this.LogFormData = true;
                this.LogQueryString = true;
            }

            public bool LogFormData { get; set; }

            public bool LogQueryString { get; set; }

            public UserIdentityRetrievalType RetrievalType { get; set; }

            public string ApplicationName { get; set; }

            public string ConnectionStringName { get; set; }

            public string Description { get; set; }

            public NameValueCollection BuildConfiguration()
            {
                var configuration = new NameValueCollection();

                configuration.Add("logFormData", this.LogFormData.ToString());
                configuration.Add("logQueryString", this.LogQueryString.ToString());
                configuration.Add("userNameRetrievalType", this.RetrievalType.ToString());
                configuration.Add("applicationName", this.ApplicationName);
                configuration.Add("connectionStringName", this.ConnectionStringName);

                if (this.Description != null)
                {
                    configuration.Add("description", this.Description);
                }

                return configuration;
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
