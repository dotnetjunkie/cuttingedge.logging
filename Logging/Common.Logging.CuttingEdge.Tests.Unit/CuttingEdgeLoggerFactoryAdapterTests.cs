using System;
using System.Configuration;
using System.Linq;

using CuttingEdge.Logging;
using CuttingEdge.Logging.Tests.Common;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSandbox;

namespace Common.Logging.CuttingEdge.Tests.Unit
{
    [TestClass]
    public class CuttingEdgeLoggerFactoryAdapterTests
    {
        [TestMethod]
        public void Constructor_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var validConfiguration = BuildValidConfiguration();

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(validConfiguration))
            {
                // Act
                manager.DomainUnderTest.CallDefaultConstructor();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Constructor_WithInvalidConfiguration_ThrowsException()
        {
            // Arrange
            var invalidConfiguration = (new ConfigurationBuilder()).Build();

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(invalidConfiguration))
            {
                // Act
                manager.DomainUnderTest.CallDefaultConstructor();
            }
        }

        [TestMethod]
        public void ConstructorWithPropertiesArgument_WithValidConfiguration_Succeeds()
        {
            // Arrange
            var validConfiguration = BuildValidConfiguration();

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(validConfiguration))
            {
                // Act
                manager.DomainUnderTest.CallConstructorWithPropertiesArgument();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void ConstructorWithPropertiesArgument_WithInvalidConfiguration_ThrowsException()
        {
            // Arrange
            var invalidConfiguration = (new ConfigurationBuilder()).Build();

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(invalidConfiguration))
            {
                // Act
                manager.DomainUnderTest.CallConstructorWithPropertiesArgument();
            }
        }

        [TestMethod]
        public void GetLogger_WithNameWithoutExactMatch_ReturnsExpectedParentProvider()
        {
            // Arrange
            const string HierarchicalLoggerName = "System.Configuration.Install";
            const string ExpectedLoggerName = "System.Configuration";
            var validConfiguration =
                BuildConfigurationWithProviderNames("System", ExpectedLoggerName, "System.Configuration.FooBar");

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(validConfiguration))
            {
                // Act
                string actualLoggerName = manager.DomainUnderTest.GetLoggerName(HierarchicalLoggerName);

                // Assert
                Assert.AreEqual(ExpectedLoggerName, actualLoggerName);
            }
        }

        [TestMethod]
        public void GetLogger_WithNameWithoutExactMatchConfigurationInDifferentOrder_ReturnsExpectedParentProvider()
        {
            // Arrange
            const string HierarchicalLoggerName = "System.Configuration.Install.FooBar";
            const string ExpectedLoggerName = "System.Configuration.Install";

            // This test looks a lot like the previous, but the providers are in a different order now.
            var validConfiguration = BuildConfigurationWithProviderNames(
                "System.Configuration.FooBar", "System.Configuration", ExpectedLoggerName, "System");

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(validConfiguration))
            {
                // Act
                string actualLoggerName = manager.DomainUnderTest.GetLoggerName(HierarchicalLoggerName);

                // Assert
                Assert.AreEqual(ExpectedLoggerName, actualLoggerName);
            }
        }

        [TestMethod]
        public void GetLogger_WithNameWithoutExactMatchConfigurationInDifferentOrder2_ReturnsExpectedParentProvider()
        {
            // Arrange
            const string HierarchicalLoggerName = "System.Configuration.Install.FooBar";
            const string ExpectedLoggerName = "System.Configuration.Install";

            // This test looks a lot like the previous, but the providers are in a different order now.
            var validConfiguration = BuildConfigurationWithProviderNames(
                "System.Configuration.FooBar", "System.Configuration", ExpectedLoggerName, "System");

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(validConfiguration))
            {
                // Act
                string actualLoggerName = manager.DomainUnderTest.GetLoggerName(HierarchicalLoggerName);

                // Assert
                Assert.AreEqual(ExpectedLoggerName, actualLoggerName);
            }
        }

        [TestMethod]
        public void GetLogger_WithNameThatDoesNotMatchAnyHierarchicalParents_ReturnsDefaultProvider()
        {
            // Arrange
            const string HierarchicalLoggerName = "CuttingEdge.Logging";
            const string ExpectedLoggerName = "DefaultLogger";

            var validConfiguration =
                BuildConfigurationWithProviderNames(ExpectedLoggerName, "Logging");

            using (var manager = new CommonLoggingTestLoggingAppDomainManager(validConfiguration))
            {
                // Act
                string actualLoggerName = manager.DomainUnderTest.GetLoggerName(HierarchicalLoggerName);

                // Assert
                Assert.AreEqual(ExpectedLoggerName, actualLoggerName);
            }
        }

        private static IConfigurationWriter BuildConfigurationWithProviderNames(params string[] providerNames)
        {
            Type providerType = typeof(MemoryLoggingProvider);
            var builder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = providerNames.First(),
                }
            };

            foreach (var providerName in providerNames)
            {
                var provider = new ProviderConfigLine() { Type = providerType, Name = providerName };

                builder.Logging.Providers.Add(provider);
            }

            return builder.Build();
        }

        private static IConfigurationWriter BuildValidConfiguration()
        {
            return BuildConfigurationWithThreshold("ValidProvider", LoggingEventType.Debug);
        }

        private static IConfigurationWriter BuildConfigurationWithThreshold(string providerName,
            LoggingEventType threshold)
        {
            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = providerName,
                    Providers =
                    {
                        // <provider name="[providerName]" type="MemoryLoggingProvider" threshold="..." />
                        new ProviderConfigLine()
                        {
                            Name = providerName, 
                            Type = typeof(MemoryLoggingProvider), 
                            CustomAttributes = @"threshold=""" + threshold.ToString() + @""""
                        }
                    }
                },
            };

            return configBuilder.Build();
        }
    }
}