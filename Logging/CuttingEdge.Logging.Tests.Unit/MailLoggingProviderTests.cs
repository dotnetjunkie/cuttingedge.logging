using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Net.Mail;
using System.Security;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Tests.Unit.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit
{
    [TestClass]
    public class MailLoggingProviderTests
    {
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
        public void Initialize_WithValidArguments_Succeeds()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid name", validConfiguration);
        }

        [TestMethod]
        public void Initiailze_WithMultipleMailAddressesInToAttribute_Succeeds()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["to"] = "developer1@cuttingedge.it;developer2@cuttingedge.it";
            
            // Act
            provider.Initialize("Valid name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            NameValueCollection invalidConfiguration = null;

            // Act
            provider.Initialize("Valid name", invalidConfiguration);
        }

        [TestMethod]
        public void Initialize_WithValidSubjectFormatString_SetsSubjectFormatStringProperty()
        {
            // Arrange
            var validSubjectFormatString = 
                "severity {0} message {1} source {2} type {3} time {4}";
            var provider = new FakeMailLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["subjectFormatString"] = validSubjectFormatString;

            // Act
            provider.Initialize("Valid name", validConfiguration);

            // Assert
            Assert.AreEqual(validSubjectFormatString, provider.SubjectFormatString);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithoutDescription_SetsDefaultDescription()
        {
            // Arrange
            var expectedDescription = "Mail logging provider";
            var provider = new FakeMailLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        public void Initialize_ConfigurationWithCustomDescription_SetsSpecifiedDescription()
        {
            // Arrange
            var expectedDescription = "My mail logger";
            var provider = new FakeMailLoggingProvider();
            var validConfiguration = CreateValidConfiguration();
            validConfiguration["description"] = expectedDescription;

            // Act
            provider.Initialize("Valid provider name", validConfiguration);

            // Assert
            Assert.AreEqual(expectedDescription, provider.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_WithInvalidSubjectFormatString_ThrowsException()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var validConfiguration = CreateValidConfiguration();

            // The format item {5} is invalid, because only {0} to {4} are (currently) supported.
            validConfiguration["subjectFormatString"] = "{5}";

            // Act
            provider.Initialize("Valid name", validConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_ConfigurationWithUnrecognizedAttributes_ThrowsException()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var configurationWithUnrecognizedAttribute = CreateValidConfiguration();
            configurationWithUnrecognizedAttribute.Add("unknown attribute", "some value");

            // Act
            provider.Initialize("Valid name", configurationWithUnrecognizedAttribute);
        }

        [TestMethod]
        public void Initialize_WithInsufficientRightsToSendMail_ThrowsExpectedException()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var providerName = "Valid name";
            var securityErrorMessage = "Security message.";
            provider.ExceptionThrownBySmtpClientConstructor = new SecurityException(securityErrorMessage);
            var validConfiguration = CreateValidConfiguration();

            try
            {
                // Act
                provider.Initialize(providerName, validConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                var msg = ex.Message ?? string.Empty;

                Assert.IsTrue(msg.Contains("error while initializing the provider") &&
                    msg.Contains("application does not have permission"),
                    "Exception message should contain the problem. Actual: " + msg);

                Assert.IsTrue(msg.Contains(providerName),
                    "Exception message should contain the provider causing the exception. Actual: " + msg);

                Assert.IsTrue(msg.Contains(securityErrorMessage),
                    "Exception message should contain the message from the inner exception. Actual: " + msg);
            }
        }

        [TestMethod]
        public void Initialize_ExceptionThrownByMailMessageConstructor_ThrowsExpectedException()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var providerName = "Valid name";
            var formatErrorMessage = "Format error message.";
            provider.ExceptionThrownByMailMessageConstructor = new FormatException(formatErrorMessage);
            var validConfiguration = CreateValidConfiguration();

            try
            {
                // Act
                provider.Initialize(providerName, validConfiguration);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ProviderException ex)
            {
                var msg = ex.Message ?? string.Empty;

                Assert.IsTrue(msg.Contains("possible miss configuration") &&
                    msg.Contains("configuration/system.net/mailSetting") &&
                    msg.Contains("MailMessage class could not be created"),
                    "Exception message should contain the problem. Actual: " + msg);

                Assert.IsTrue(msg.Contains("<configuration>") && msg.Contains("<system.net>") &&
                    msg.Contains("<mailSettings>"),
                    "Exception message should contain a example configuration. Actual: " + msg);

                Assert.IsTrue(msg.Contains(formatErrorMessage),
                    "Exception message should contain the message from the inner exception. Actual: " + msg);
            }
        }

#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void BuildSubjectWithSubjectFormatString_SubjectFormatStringWithSeverityFormatItem_ReturnsSeverity()
        {
            // Arrange
            string subjectFormatString = "{0}";
            var expectedSeverity = LoggingEventType.Critical;
            var entry = new LogEntry(expectedSeverity, "Some message", null, null);

            // Act
            string subject = 
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entry, DateTime.MaxValue);

            // Assert
            Assert.AreEqual(expectedSeverity.ToString(), subject);
        }

        [TestMethod]
        public void BuildSubjectWithSubjectFormatString_SubjectFormatStringWithMessageFormatItem_ReturnsMessage()
        {
            // Arrange
            string subjectFormatString = "{1}";
            var expectedMessage = "Expected message";
            var entry = new LogEntry(LoggingEventType.Debug, expectedMessage, null, null);

            // Act
            string subject =
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entry, DateTime.MaxValue);

            // Assert
            Assert.AreEqual(expectedMessage, subject);
        }

        [TestMethod]
        public void BuildSubjectWithSubjectFormatString_SubjectFormatStringWithSourceFormatItem_ReturnsSource()
        {
            // Arrange
            string subjectFormatString = "{2}";
            var expectedSource = "Expected source";
            var entry = new LogEntry(LoggingEventType.Debug, "Some message", expectedSource, null);

            // Act
            string subject =
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entry, DateTime.MaxValue);

            // Assert
            Assert.AreEqual(expectedSource, subject);
        }

        [TestMethod]
        public void BuildSubjectWithSubjectFormatString_SubjectFormatStringWithExeptionTypeFormatItem_ReturnsExceptionType()
        {
            // Arrange
            string subjectFormatString = "{3}";
            var expectedException = new InvalidOperationException();
            var entry = new LogEntry(LoggingEventType.Debug, "Some message", null, expectedException);

            // Act
            string subject =
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entry, DateTime.MaxValue);

            // Assert
            Assert.AreEqual(expectedException.GetType().Name, subject);
        }

        [TestMethod]
        public void BuildSubjectWithSubjectFormatString_SubjectFormatStringWithDateFormatItem_ReturnsDate()
        {
            string subjectFormatString = "{4}";
            DateTime currentTime = new DateTime(2009, 11, 30, 18, 27, 12);
            var entryToFormat = new LogEntry(LoggingEventType.Debug, "Some message", null, null);

            // Act
            string subject =
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entryToFormat, currentTime);

            Assert.AreEqual("11/30/2009 18:27:12", subject);
        }
#endif

        [TestMethod]
        public void BuildMailBody_WithValidEntry_ReturnsExpectedMailBody()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var entry = new LogEntry(LoggingEventType.Error, "Log message", "Log source", null);
            var expectedMailBody =
                "Log message\r\n" +
                "Severity: Error\r\n" +
                "Source: Log source\r\n";

            // Act
            string actualMailBody = provider.BuildMailBody(entry);

            // Assert
            Assert.AreEqual(expectedMailBody, actualMailBody);
        }

#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void BuildMailPriority_WithSeverityCritical_ReturnsMailPriorityHigh()
        {
            // Arrange
            var expectedPriority = MailPriority.High;

            // Act
            MailPriority actualPriority = MailLoggingProvider.DetermineMailPriority(LoggingEventType.Critical);

            // Assert
            Assert.AreEqual(expectedPriority, actualPriority);
        }

        [TestMethod]
        public void BuildMailPriority_WithSeverityError_ReturnsMailPriorityNormal()
        {
            // Arrange
            var expectedPriority = MailPriority.Normal;

            // Act
            MailPriority actualPriority = MailLoggingProvider.DetermineMailPriority(LoggingEventType.Error);

            // Assert
            Assert.AreEqual(expectedPriority, actualPriority);
        }

        [TestMethod]
        public void BuildMailPriority_WithSeverityWarning_ReturnsMailPriorityNormal()
        {
            // Arrange
            var expectedPriority = MailPriority.Normal;

            // Act
            MailPriority actualPriority = MailLoggingProvider.DetermineMailPriority(LoggingEventType.Warning);

            // Assert
            Assert.AreEqual(expectedPriority, actualPriority);
        }

        [TestMethod]
        public void BuildMailPriority_WithSeverityInformation_ReturnsMailPriorityNormal()
        {
            // Arrange
            var expectedPriority = MailPriority.Normal;

            // Act
            MailPriority actualPriority = 
                MailLoggingProvider.DetermineMailPriority(LoggingEventType.Information);

            // Assert
            Assert.AreEqual(expectedPriority, actualPriority);
        }

        [TestMethod]
        public void BuildMailPriority_WithSeverityDebug_ReturnsMailPriorityNormal()
        {
            // Arrange
            var expectedPriority = MailPriority.Normal;

            // Act
            MailPriority actualPriority = MailLoggingProvider.DetermineMailPriority(LoggingEventType.Debug);

            // Assert
            Assert.AreEqual(expectedPriority, actualPriority);
        }
#endif

        [TestMethod]
        public void BuildMailMessage_ProviderWithThreeToMailAddresses_Succeeds()
        {
            // Arrange
            string mailAddress1 = "dev1@ce.it";
            string mailAddress2 = "dev2@ce.it";
            string mailAddress3 = "dev3@ce.it";

            var provider = new FakeMailLoggingProvider();
            var configuration = new NameValueCollection();
            configuration.Add("to", string.Join(";", new[] { mailAddress1, mailAddress2, mailAddress3 }));
            provider.Initialize("Valid name", configuration);

            var entry = new LogEntry(LoggingEventType.Error, "Log message", "Log source", null);

            // Act
            MailMessage message = provider.BuildMailMessage(entry);

            // Assert
            Assert.AreEqual(3, message.To.Count);
            Assert.IsTrue(message.To.Contains(new MailAddress(mailAddress1)), 
                "MailAddress does not contain expected address: " + mailAddress1);
            Assert.IsTrue(message.To.Contains(new MailAddress(mailAddress2)),
                "MailAddress does not contain expected address: " + mailAddress2);
            Assert.IsTrue(message.To.Contains(new MailAddress(mailAddress3)),
                "MailAddress does not contain expected address: " + mailAddress3);
        }

        [TestMethod]
        public void Configuration_WithValidSingleMailAddressInToAttribute_Succeeds()
        {
            // Arrange
            var validToAttributeWithSingleMailAddress = "to=\"dev2@cuttingedge.it\" ";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "MailProv",
                    Providers =
                    {
                        // <provider name="MailProv" type="CE.Logging.MailLoggingProvider, CE.Logging" to=".." />
                        new ProviderConfigLine()
                        {
                            Name = "MailProv",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = validToAttributeWithSingleMailAddress
                        }
                    }
                },
                Xml = ValidMailConfiguration
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_WithMultipleValidMailAddressesInToAttribute_Succeeds()
        {
            // Arrange
            string validToAttributeWithMultipleMailAddresses =
                "to=\"dev2@cuttingedge.it;dev3@cuttingedge.it\" ";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "DefaultProvider",
                    Providers =
                    {
                        // <provider name="Provider1" type="CE.Logging.MailLoggingProvider, CE.Logging" to=".." />
                        new ProviderConfigLine()
                        {
                            Name = "DefaultProvider",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = validToAttributeWithMultipleMailAddresses
                        }
                    }
                },
                Xml = ValidMailConfiguration,
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Configuration_MissingMandatoryToAttribute_ThrowsException()
        {
            // Arrange
            string missingMandatoryToAttribute = string.Empty;

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Provider1",
                    Providers =
                    {
                        // <provider name="Provider1" type="CE.Logging.MailLoggingProvider, CE.Logging" />
                        new ProviderConfigLine()
                        {
                            Name = "Provider1",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = missingMandatoryToAttribute
                        }
                    }
                },
                Xml = ValidMailConfiguration,
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_MissingToAttribute_ThrowsException()
        {
            // Arrange
            string missingMandatoryToAttribute = string.Empty;

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "DefaultProvider",
                    Providers =
                    {
                        // <provider name="DefaultProvider" type="CE.Logging.MailLoggingProvider, CE.Logging" />
                        new ProviderConfigLine()
                        {
                            Name = "DefaultProvider",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = missingMandatoryToAttribute
                        }
                    }
                },
                Xml = ValidMailConfiguration,
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
                catch (Exception ex)
                {
                    Assert.IsNotNull(ex.InnerException, 
                        "The thrown exception is expected to have an inner exception.");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Configuration_InvalidMailAddressInToAttribute_ThrowsException()
        {
            // Arrange
            string invalidMailAddressInToAttribute = "to=\"d2ce.it\" ";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Mail",
                    Providers =
                    {
                        // <provider name="Mail" type="CE.Logging.MailLoggingProvider, ..." to="d2ce.it" />
                        new ProviderConfigLine()
                        {
                            Name = "Mail",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = invalidMailAddressInToAttribute
                        }
                    }
                },
                Xml = ValidMailConfiguration,
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Configuration_MissingFromAttributeInMailConfiguration_ThrowsException()
        {
            // Arrange
            string validToAttributeWithSingleMailAddress = "to=\"dev1@cuttingedge.it\" ";

            string invalidMailConfigurationWithMissingFromAttribute = @"
              <system.net>
                <mailSettings>
                  <smtp>
                    <network host=""smtpserver1"" port=""25"" userName=""username""
                        password=""secret"" defaultCredentials=""true"" />
                  </smtp>
                </mailSettings>
              </system.net>";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Mail",
                    Providers =
                    {
                        // <provider name="Mail" type="CE.Logging.MailLoggingProvider, CE.Logging" to="..." />
                        new ProviderConfigLine()
                        {
                            Name = "Mail",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = validToAttributeWithSingleMailAddress
                        }
                    }
                },
                Xml = invalidMailConfigurationWithMissingFromAttribute,
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_MissingHostAttributeInMailConfiguration_ThrowsException()
        {
            // Arrange
            string validToAttributeWithSingleMailAddress = "to=\"d2@ce.it\" ";

            string invalidMailConfigurationWithMissingHost = @"
              <system.net>
                <mailSettings>
                  <smtp from=""test@foo.com"">
                    <network />
                  </smtp>
                </mailSettings>
              </system.net>";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Default",
                    Providers =
                    {
                        // <provider name="Mail" type="CE.Logging.MailLoggingProvider, CE.Logging" to="..." />
                        new ProviderConfigLine()
                        {
                            Name = "Default",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = validToAttributeWithSingleMailAddress
                        }
                    }
                },
                Xml = invalidMailConfigurationWithMissingHost,
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                try
                {
                    // Act
                    manager.DomainUnderTest.InitializeLoggingSystem();

                    // Assert
                    Assert.Fail("Exception excepted.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(ConfigurationErrorsException));
                    Assert.IsTrue(ex.Message.Contains("'host'"), "Message should contain the text 'host'.");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void Configuration_InvalidCharacterInToAttribute_ThrowsException()
        {
            // Arrange
            // 'to' is not valid. Must not end with a semicolon.
            string invalidMailAddressInToAttribute = "to=\"d2@ce.it;\" ";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "Provider",
                    Providers =
                    {
                        // <provider name="Provider" type="CE.Logging.MailLoggingProvider, ..." to="d2@ce.it;" />
                        new ProviderConfigLine()
                        {
                            Name = "Provider",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = invalidMailAddressInToAttribute
                        }
                    }
                },
                Xml = ValidMailConfiguration,
            };

            using (var manager = new UnitTestAppDomainManager(configBuilder.Build()))
            {
                // Act
                manager.DomainUnderTest.InitializeLoggingSystem();
            }
        }

        [TestMethod]
        public void Configuration_InvalidSubjectFormatString_ThrowsException()
        {
            // Arrange
            // 'subjectFormatString' is invalid.
            string validToAttribute = "to=\"d2@ce.it\" ";
            string invalidSubjectFormatStringAttribute = "subjectFormatString=\"{\" ";

            var configBuilder = new ConfigurationBuilder()
            {
                Logging = new LoggingConfigurationBuilder()
                {
                    DefaultProvider = "MP",
                    Providers =
                    {
                        // <provider name="MP" type="MailLoggingProvider, ..." to="d2@ce.it;" subjectFormat... />
                        new ProviderConfigLine()
                        {
                            Name = "MP",
                            Type = typeof(MailLoggingProvider),
                            CustomAttributes = validToAttribute + invalidSubjectFormatStringAttribute
                        }
                    }
                },
                Xml = ValidMailConfiguration,
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
                    Assert.IsTrue(ex.Message.Contains("subjectFormatString"),
                        "The exception message should contain the string 'subjectFormatString'.");
                }
            }
        }

        private static NameValueCollection CreateValidConfiguration()
        {
            var configuration = new NameValueCollection();

            configuration.Add("to", "john@do.com");

            return configuration;
        }

        private sealed class FakeMailLoggingProvider : MailLoggingProvider
        {
            public Exception ExceptionThrownBySmtpClientConstructor { get; set; }

            public Exception ExceptionThrownByMailMessageConstructor { get; set; }

            public new string BuildMailBody(LogEntry entry)
            {
                // base implementation is protected.
                return base.BuildMailBody(entry);
            }

            public new MailMessage BuildMailMessage(LogEntry entry)
            {
                // base implementation is protected.
                return base.BuildMailMessage(entry);
            }
            
            internal override SmtpClient CreateSmtpClient()
            {
                if (this.ExceptionThrownBySmtpClientConstructor != null)
                {
                    throw this.ExceptionThrownBySmtpClientConstructor;
                }

                return base.CreateSmtpClient();
            }

            internal override MailMessage CreateMailMessage()
            {
                if (this.ExceptionThrownByMailMessageConstructor != null)
                {
                    throw this.ExceptionThrownByMailMessageConstructor;
                }

                return base.CreateMailMessage();
            }

            protected override object LogInternal(LogEntry entry)
            {
                // The base implementation send the mail here, so we want to stub that out.
                return null;
            }
        }
    }
}
