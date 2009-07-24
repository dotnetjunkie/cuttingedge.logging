using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Security;

using CuttingEdge.Logging.UnitTests.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSandbox;

namespace CuttingEdge.Logging.UnitTests
{
    [TestClass]
    public class LoggerConfigurationTests
    {
        private const string MissingDefaultProviderExceptionMessage =
            "No default Logging provider was found in the <logging> section of the the configuration file " +
            "or the <logging> section was missing. Please provide a default provider as follows: " +
            "<logging defaultProvider=\"[NameOfYourDefaultProvider]\">" +
            "<add name=\"[NameOfYourDefaultProvider]\" type=\"[ProviderType]\" /></logging>.";

        private const string MissingConfigSectionsElementExceptionMessage =
            "The Logger hasn't been configured properly. Please register the logger as <section> element " +
            "in the <configSections> of your configurations file as follows: " +
            "<configSections><section name=\"logging\" type=\"CuttingEdge.Logging.LoggingSection, " +
            "CuttingEdge.Logging\" allowDefinition=\"MachineToApplication\" /></configSections>.";

        private const string ValidMailConfiguration = @"
          <system.net>
            <mailSettings>
              <smtp from=""test@foo.com"">
                <network host=""smtpserver1"" port=""25"" userName=""username""
                    password=""secret"" defaultCredentials=""true"" />
              </smtp>
            </mailSettings>
          </system.net>
        ";

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void MissingDefaultProviderInConfigFileShouldFail()
        {
            string defaultProviderName = "MemoryLoggingProvider";
            string providerConfigurationLines = null;

            IConfigurationWriter config = 
                SandboxHelpers.CreateConfiguration(defaultProviderName, providerConfigurationLines);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                try
                {
                    manager.Logger.Initialize();
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith(MissingDefaultProviderExceptionMessage));
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void MissingDefaultProviderAttributeInConfigShouldFail()
        {
            string defaultProviderName = string.Empty;
            string providerConfigurationLines =
                SandboxHelpers.BuildProviderConfigurationLine(typeof(MemoryLoggingProvider));

            IConfigurationWriter config = 
                SandboxHelpers.CreateConfiguration(defaultProviderName, providerConfigurationLines);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                try
                {
                    manager.Logger.Initialize();
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith(MissingDefaultProviderExceptionMessage));
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void MissingConfigSectionsElementInConfigFileShouldFail()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?><configuration></configuration>";

            TemplatedConfigurationWriter config = new TemplatedConfigurationWriter(xml);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                try
                {
                    manager.Logger.Initialize();
                }
                catch (ProviderException ex)
                {
                    Assert.AreEqual(MissingConfigSectionsElementExceptionMessage, ex.Message);
                    throw;
                }
            }
        }

        [TestMethod]
        public void ASimpleConfigurationShouldPass()
        {
            string defaultProviderName = "MemoryLoggingProvider";
            string providerConfigurationLines = 
                SandboxHelpers.BuildProviderConfigurationLine(typeof(MemoryLoggingProvider));

            IConfigurationWriter config = 
                SandboxHelpers.CreateConfiguration(defaultProviderName, providerConfigurationLines);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                manager.Logger.Initialize();
            }
        }

        [TestMethod]
        public void ValidMailLoggingProviderConfigurationShouldSucceed01()
        {
            string customAttributes = "to=\"dev2@cuttingedge.it\" ";

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        public void ValidMailLoggingProviderConfigurationShouldSucceed02()
        {
            string customAttributes = 
                "to=\"dev2@cuttingedge.it;dev3@cuttingedge.it\" ";

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        public void ConfigurationExceptionShouldContainInnerException()
        {
            // Missing 'to'
            string customAttributes = string.Empty;

            try
            {
                ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.InnerException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail01()
        {
            // Missing 'to'
            string customAttributes = string.Empty;

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail02()
        {
            string customAttributes = "to=\"dev1@cuttingedge.it\" ";

            // Missing 'from'
            string invalidMailConfiguration = @"
              <system.net>
                <mailSettings>
                  <smtp>
                    <network host=""smtpserver1"" port=""25"" userName=""username""
                        password=""secret"" defaultCredentials=""true"" />
                  </smtp>
                </mailSettings>
              </system.net>";

            try
            {
                ConfigureMailLoggingProvider(customAttributes, invalidMailConfiguration);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("'from'"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail03()
        {
            string customAttributes = "to=\"d2@ce.it\" ";

            // Missing 'host'
            string invalidMailConfiguration = @"
              <system.net>
                <mailSettings>
                  <smtp from=""test@foo.com"">
                    <network />
                  </smtp>
                </mailSettings>
              </system.net>";

            try
            {
                ConfigureMailLoggingProvider(customAttributes, invalidMailConfiguration);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("'host'"));
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail04()
        {
            // 'to' is not a valid mail address
            string customAttributes = "to=\"d2ce.it\" ";

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail05()
        {
            // 'to' is not valid. Must not end with a semicolon.
            string customAttributes = "to=\"d2@ce.it;\" ";

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail06()
        {
            // 'subjectFormatString' is invalid.
            string customAttributes = "to=\"d2@ce.it\" subjectFormatString=\"{\" ";

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail07()
        {
            // 'subjectFormatString' is invalid. {5} is not a valid indexed placeholder.
            string customAttributes = "to=\"d2@ce.it\" subjectFormatString=\"{5}\" ";

            ConfigureMailLoggingProvider(customAttributes, ValidMailConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void InvalidMailLoggingProviderConfigurationShouldFail08()
        {
            string customAttributes = "to=\"dev1@cuttingedge.it\" ";

            // Missing 'from'
            string invalidMailConfiguration = @"
              <system.net>
                <mailSettings>
                  <smtp from=""invalid_mailaddress.com"">
                    <network host=""smtpserver1"" port=""25"" userName=""username""
                        password=""secret"" defaultCredentials=""true"" />
                  </smtp>
                </mailSettings>
              </system.net>";

            try
            {
                ConfigureMailLoggingProvider(customAttributes, invalidMailConfiguration);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException.InnerException is FormatException,
                    "Of type " + ex.InnerException.InnerException.GetType().Name + " instead.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void ConfiguringAnInvalidTypeOfProviderShouldFail01()
        {
            string defaultProviderName = "MyTestSqlMembershipProvider";
            string providerConfigurationLines =
                SandboxHelpers.BuildProviderConfigurationLine(typeof(TestSqlMembershipProvider));

            IConfigurationWriter config = SandboxHelpers.CreateConfiguration(defaultProviderName,
                providerConfigurationLines);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                manager.Logger.Initialize();
            }
        }

        private static void ConfigureMailLoggingProvider(string customAttributes, string mailConfiguration)
        {
            string defaultProviderName = "MailLoggingProvider";
            string providerConfigurationLines =
                SandboxHelpers.BuildProviderConfigurationLine(typeof(MailLoggingProvider), customAttributes);

            IConfigurationWriter config = SandboxHelpers.CreateConfiguration(defaultProviderName, 
                providerConfigurationLines, mailConfiguration);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                try
                {
                    manager.Logger.Initialize();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
