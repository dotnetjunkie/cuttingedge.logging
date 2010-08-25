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
        public void Constructor_WithValidArguments_Succeeds()
        {
            // Arrange
            var validThreshold = LoggingEventType.Critical;
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("Valid app name");
            var validConnectionString = "Valid constr";

            // Act
            new AspNetSqlLoggingProvider(validThreshold, validConfiguration, validConnectionString, null);
        }

        [TestMethod]
        public void Constructor_WithValidConfiguration_SetsLogQueryStringProperty()
        {
            var expectedLogQueryString = false;
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("Valid app name")
            {
                LogQueryString = expectedLogQueryString
            };

            // Act
            var provider = new AspNetSqlLoggingProvider(LoggingEventType.Critical, validConfiguration, 
                "Valid constr", null);

            // Assert
            Assert.AreEqual(expectedLogQueryString, provider.LogQueryString);
        }

        [TestMethod]
        public void Constructor_WithValidConfiguration_SetsLogFormDataProperty()
        {
            var expectedLogFormData = true;
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("Valid app name")
            {
                LogFormData = expectedLogFormData
            };

            // Act
            var provider = new AspNetSqlLoggingProvider(LoggingEventType.Critical, validConfiguration,
                "Valid constr", null);

            // Assert
            Assert.AreEqual(expectedLogFormData, provider.LogFormData);
        }

        [TestMethod]
        public void Constructor_WithValidConfiguration_SetsRetrievalTypeProperty()
        {
            var expectedRetrievalType = UserIdentityRetrievalType.WindowsIdentity;
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("Valid app name")
            {
                RetrievalType = expectedRetrievalType
            };

            // Act
            var provider = new AspNetSqlLoggingProvider(LoggingEventType.Critical, validConfiguration,
                "Valid constr", null);

            // Assert
            Assert.AreEqual(expectedRetrievalType, provider.RetrievalType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullConfiguration_ThrowsException()
        {
            // Act
            new AspNetSqlLoggingProvider(LoggingEventType.Debug, null, "Valid string", null);
        }

        [TestMethod]
        public void Constructor_ChangingLogFormDataInConfigurationAfterConstructorCall_HasNoEffect()
        {
            // Arrange
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("valid name")
            {
                LogFormData = true
            };

            var provider = new AspNetSqlLoggingProvider(LoggingEventType.Critical, validConfiguration,
                "Valid constr", null);

            // Act
            validConfiguration.LogFormData = false;

            // Assert
            Assert.AreEqual(true, provider.LogFormData);
        }

        [TestMethod]
        public void Constructor_ChangingLogQueryStringInConfigurationAfterConstructorCall_HasNoEffect()
        {
            // Arrange
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("valid name")
            {
                LogQueryString = true
            };

            var provider = new AspNetSqlLoggingProvider(LoggingEventType.Critical, validConfiguration,
                "Valid constr", null);

            // Act
            validConfiguration.LogQueryString = false;

            // Assert
            Assert.AreEqual(true, provider.LogQueryString);
        }

        [TestMethod]
        public void Constructor_ChangingRetrievalTypeInConfigurationAfterConstructorCall_HasNoEffect()
        {
            // Arrange
            var validConfiguration = new AspNetSqlLoggingProviderConfiguration("valid name")
            {
                RetrievalType = UserIdentityRetrievalType.None
            };

            var provider = new AspNetSqlLoggingProvider(LoggingEventType.Critical, validConfiguration,
                "Valid constr", null);

            // Act
            validConfiguration.RetrievalType = UserIdentityRetrievalType.Membership;

            // Assert
            Assert.AreEqual(UserIdentityRetrievalType.None, provider.RetrievalType);
        }

        [TestMethod]
        public void Log_UninitializedProvider_ThrowsDescriptiveException()
        {
            // Arrange
            var provider = new AspNetSqlLoggingProvider();

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

                Assert.IsTrue(ex.Message.Contains("AspNetSqlLoggingProvider"),
                    "The message should contain the type name of the unitialized provider. Actual: " +
                    ex.Message);
            }
        }

        [TestMethod]
        public void Log_CodeConfiguredFailingProvider_LogsToFallbackProvider()
        {
            // Arrange
            var fallbackProvider = new MemoryLoggingProvider();

            var provider = new FakeAspNetSqlLoggingProvider(fallbackProvider);

            // Act
            provider.Log("Message");

            // Assert
            Assert.AreEqual(2, fallbackProvider.GetLoggedEntries().Length, "Logging failed.");
        }

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
            public FakeAspNetSqlLoggingProvider()
            {
            }

            public FakeAspNetSqlLoggingProvider(LoggingProviderBase fallbackProvider)
                : base(LoggingEventType.Debug, new AspNetSqlLoggingProviderConfiguration("Valid app name"),
                    "some unimportant connection string", fallbackProvider)
            {
            }

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
