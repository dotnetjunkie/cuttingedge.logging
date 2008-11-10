using System.Configuration;
using System.Configuration.Provider;

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
            "The Logger hasn't been configured properly. Please register the logger as <section> in the " +
            "<configSections> of your configurations file as follows: " +
            "<configSections><section name=\"logging\" type=\"CuttingEdge.Logging.LoggingSection, " +
            "CuttingEdge.Logging\" allowDefinition=\"MachineToApplication\" /></configSections>.";

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void MissingDefaultProviderInConfigFileShouldFail()
        {
            IConfigurationWriter config = SandboxHelpers.CreateConfiguration("MemoryLoggingProvider", null);

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
            string providers =
                SandboxHelpers.BuildProviderConfigurationLine(typeof(MemoryLoggingProvider));

            IConfigurationWriter config = SandboxHelpers.CreateConfiguration(string.Empty, providers);

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
            string providers = SandboxHelpers.BuildProviderConfigurationLine(typeof(MemoryLoggingProvider));

            IConfigurationWriter config = SandboxHelpers.CreateConfiguration("MemoryLoggingProvider", providers);

            using (LoggingSandboxManager manager = new LoggingSandboxManager(config))
            {
                manager.Logger.Initialize();
            }
        }
    }
}
