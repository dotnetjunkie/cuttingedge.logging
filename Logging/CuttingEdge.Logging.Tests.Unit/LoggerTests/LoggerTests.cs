﻿using System;
using System.Configuration;
using System.Configuration.Provider;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.LoggerTests
{
    /// <summary>
    /// Tests the static <see cref="Logger"/> class.
    /// </summary>
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Provider_WithCorrectlyConfiguredEnvironment_ReturnsAValue()
        {
            Assert.IsNotNull(Logger.Provider);
        }

        [TestMethod]
        public void Provider_WithCorrectlyConfiguredEnvironment_HasFallbackProvider()
        {
            Assert.IsNotNull(Logger.Provider.FallbackProvider);
        }

        [TestMethod]
        public void Provider_InCorrectlyConfiguredEnvironment_FallbackProviderHasNoFallbackProvider()
        {
            Assert.IsNull(Logger.Provider.FallbackProvider.FallbackProvider);
        }

        [TestMethod]
        public void Providers_WithCorrectlyConfiguredEnvironment_ReturnsAValue()
        {
            Assert.IsNotNull(Logger.Providers);
        }

        [TestMethod]
        public void Providers_WithCorrectlyConfiguredEnvironment_DoesNotContainNullElements()
        {
            foreach (LoggingProviderBase loggingProvider in Logger.Providers)
            {
                Assert.IsNotNull(loggingProvider);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Providers_CallingAddMethod_ThrowsException()
        {
            // Adding a new provider to the collection is not supported.
            Logger.Providers.Add(new MemoryLoggingProvider());
        }

        [TestMethod]
        public void Configuration_WithInvalidSection_ThrowsException()
        {
            string incorrectSectionTypeName = typeof(IncorrectSection).FullName + ", " +
                typeof(IncorrectSection).Assembly.GetName().Name;

            // Arrange
            string xml =
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                <configuration>
                    <configSections>
	                    <section name=""logging"" 
                            type=""" + incorrectSectionTypeName + @""" 
                            allowDefinition=""MachineToApplication"" />
                    </configSections>
                </configuration>";

            var config = new TemplatedConfigurationWriter(xml);

            using (var manager = new UnitTestAppDomainManager(config))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception expected.");
                }
                catch (ProviderException ex)
                {
                    Assert.IsTrue(ex.Message.Contains(typeof(LoggingSection).FullName), "Actual message: " + ex.Message);
                    Assert.IsTrue(ex.Message.Contains(typeof(IncorrectSection).FullName), "Actual message: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_WithInvalidProviderType_ThrowsException()
        {
            // Arrange
            string xmlWithMissingProviderType =
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                 <configuration>
                     <configSections>
                         <section name=""logging"" 
                             type=""CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging""
                             allowDefinition=""MachineToApplication"" />
                     </configSections>
                     <logging defaultProvider=""MemoryLoggingProvider"">
                        <providers>
                             <!-- next line misses the type -->
                             <add 
                                 name=""MemoryLoggingProvider""
                                 type=""    ""
                             />
                        </providers>
                     </logging>
                 </configuration>";

            var config = new TemplatedConfigurationWriter(xmlWithMissingProviderType);

            using (var manager = new UnitTestAppDomainManager(config))
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
                    Assert.IsTrue(ex.Message.Contains("MemoryLoggingProvider"),
                        "The exception message should contain the name of the incorrect provider." +
                        "Actual message: " + ex.Message);

                    Assert.IsTrue(ex.Message.Contains("no type attribute"),  "Actual message: " + ex.Message);
                }
            }
        }

        [TestMethod]
        public void Configuration_WithNonExistingTypeInProviderType_ThrowsException()
        {
            // Arrange
            string xmlWithMissingProviderType =
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                 <configuration>
                     <configSections>
                         <section name=""logging"" 
                             type=""CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging""
                             allowDefinition=""MachineToApplication"" />
                     </configSections>
                     <logging defaultProvider=""MemoryLoggingProvider"">
                        <providers>
                             <!-- next line has invalid 'type' attribute -->
                             <add 
                                 name=""MemoryLoggingProvider""
                                 type=""NonExistingType, CuttingEdge.Logging""
                             />
                        </providers>
                     </logging>
                 </configuration>";

            var config = new TemplatedConfigurationWriter(xmlWithMissingProviderType);

            using (var manager = new UnitTestAppDomainManager(config))
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
                    Assert.IsTrue(ex.Message.Contains("MemoryLoggingProvider"),
                        "The exception message should contain the name of the incorrect provider. " +
                        "Actual message: " + ex.Message);

                    Assert.IsTrue(ex.Message.Contains("NonExistingType, CuttingEdge.Logging"),
                        "The exception message should contain the incorrect type. " +
                        "Actual message: " + ex.Message);

                    Assert.IsTrue(ex.Message.Contains("could not be resolved"),
                        "The exception message should explain the reason for the error. " +
                        "Actual message: " + ex.Message);

                    Assert.IsNotNull(ex.InnerException, 
                        "The exception is expected to have an inner exception");
                    
                    Assert.IsInstanceOfType(ex.InnerException, typeof(TypeLoadException),
                        "The inner exception is expected to be a TypeLoadException.");
                }
            }
        }

        [TestMethod]
        public void Configuration_ProviderTypeWithNoDefaultConstructor_ThrowsException()
        {
            // Arrange
            string providerName = "Constructor";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = providerName,
                    Providers =
                    {
                        // <provider name="Constructor" type="ProviderWithNoDefaultConstructor, ..."  />
                        new ProviderConfigLine()
                        {
                            Name = providerName,
                            Type = typeof(ProviderWithNoDefaultConstructor),
                        }
                    }
                },
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
                    Assert.IsTrue(ex.Message.Contains(providerName),
                        "The exception message should contain the name of the provider. Actual message: " +
                        ex.Message);

                    Assert.IsTrue(ex.Message.Contains(typeof(ProviderWithNoDefaultConstructor).FullName),
                        "The exception message should contain the type name of the provider. " +
                        "Actual message: " + ex.Message);
                }
            }
        }

        private sealed class IncorrectSection : ConfigurationSection
        {
        }

        private class ProviderWithNoDefaultConstructor : LoggingProviderBase
        {
            public ProviderWithNoDefaultConstructor(int someUnimportantParameter)
            {
            }

            protected override object LogInternal(LogEntry entry)
            {
                return null;
            }
        }
    }
}
