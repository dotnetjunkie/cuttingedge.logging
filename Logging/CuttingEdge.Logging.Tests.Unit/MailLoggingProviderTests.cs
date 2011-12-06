using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
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

        private const LoggingEventType ValidThreshold = LoggingEventType.Debug;
        private const string ValidSubjectFormatString = "{0}: {1}";
        private static readonly LoggingProviderBase ValidFallbackProvider = null;
        private static readonly MailAddress ValidRecipient = new MailAddress("dev1@cuttingedge.it");

        [TestMethod]
        public void Log_WithInitializedProvider_Succeeds()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            provider.Initialize("Valid name", CreateValidConfiguration());

            // Act
            provider.Log("Some message");
        }

        [TestMethod]
        public void Log_WithUninitializedProvider_ThrowsDescriptiveException()
        {
            // Arrange
            var provider = new MailLoggingProvider();

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

                Assert.IsTrue(ex.Message.Contains("MailLoggingProvider"),
                    "The message should contain the type name of the unitialized provider. Actual: " + 
                    ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogInternal_WithNullArgument_ThrowsException()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();

            LogEntry invalidEntry = null;

            // Act
            provider.Public_LogInternal(invalidEntry);
        }

        [TestMethod]
        public void Constructor_WithValidArguments_Succeeds()
        {
            // Act
            new MailLoggingProvider(ValidThreshold, ValidSubjectFormatString, ValidFallbackProvider,
                ValidRecipient);
        }

        [TestMethod]
        public void Constructor_WithoutFallbackProvider_RegistersNoFallbackProvider()
        {
            // Act
            var provider = new MailLoggingProvider(ValidThreshold, ValidRecipient);

            // Assert
            Assert.IsNull(provider.FallbackProvider);
        }

        [TestMethod]
        public void Constructor_WithoutSubjectFormatString_RegistersDefaultSubjectFormatString()
        {
            // Act
            var provider = new MailLoggingProvider(ValidThreshold, ValidRecipient);

            // Assert
            Assert.AreEqual("{0}: {1}", provider.SubjectFormatString);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void Constructor_WithInvalidThreshold_ThrowsException()
        {
            // Arrange
            var invalidThreshold = (LoggingEventType)(-1);

            // Act
            new MailLoggingProvider(invalidThreshold, ValidSubjectFormatString, ValidFallbackProvider,
                ValidRecipient, ValidRecipient, ValidRecipient);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullRecipients_ThrowsException()
        {
            // Arrange
            MailAddress[] invalidRecipients = null;

            // Act
            new MailLoggingProvider(ValidThreshold, ValidSubjectFormatString, ValidFallbackProvider, 
                invalidRecipients);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithEmptyRecipientsCollection_ThrowsException()
        {
            // Arrange
            MailAddress[] invalidRecipients = new MailAddress[0];

            // Act
            new MailLoggingProvider(ValidThreshold, ValidSubjectFormatString, ValidFallbackProvider, 
                invalidRecipients);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithNullElementInRecipientsCollection_ThrowsException()
        {
            // Arrange
            MailAddress[] invalidRecipients = new MailAddress[] { null };

            // Act
            new MailLoggingProvider(ValidThreshold, ValidSubjectFormatString, ValidFallbackProvider,
                invalidRecipients);
        }

        [TestMethod]
        public void Constructor_SubjectFormatStringWithInvalidCharacters_ThrowsException()
        {
            // Arrange
            var invalidSubjectFormatString = "Test\n";

            try
            {
                // Act
                new MailLoggingProvider(ValidThreshold, invalidSubjectFormatString, ValidFallbackProvider,
                    ValidRecipient);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (ArgumentException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));

                Assert.IsTrue(ex.Message.Contains("illegal characters") && ex.Message.Contains("Line breaks"),
                    "Exception message is not descriptive enough. Actual message: " + ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithInvalidSubjectFormatString_ThrowsException()
        {
            // Arrange
            var invalidSubjectFormatString = "{";

            // Act
            new MailLoggingProvider(ValidThreshold, invalidSubjectFormatString, ValidFallbackProvider,
                ValidRecipient);
        }

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
        public void Initialize_WithMultipleMailAddressesInToAttribute_Succeeds()
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
            var provider = new MailLoggingProvider();
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

#if DEBUG // This test code only runs in debug mode
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

        [TestMethod]
        public void BuildMailMessageSubject_SubjectFormatStringWithSeverityFormatItem_ReturnsSeverity()
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
        public void BuildMailMessageSubject_SubjectFormatStringWithMessageFormatItem_ReturnsMessage()
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
        public void BuildMailMessageSubject_SubjectFormatStringWithSourceFormatItem_ReturnsSource()
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
        public void BuildMailMessageSubject_SubjectFormatStringWithExeptionTypeFormatItem_ReturnsExceptionType()
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
        public void BuildMailMessageSubject_SubjectFormatStringWithDateFormatItem_ReturnsDate()
        {
            string subjectFormatString = "{4}";
            DateTime currentTime = new DateTime(2009, 11, 30, 18, 27, 12);
            var entryToFormat = new LogEntry(LoggingEventType.Debug, "Some message", null, null);

            // Act
            string subject =
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entryToFormat, currentTime);

            Assert.AreEqual("11/30/2009 18:27:12", subject);
        }

        [TestMethod]
        public void BuildMailMessageSubject_EntryWithLongMessage_TruncatesMessageAccordingly()
        {
            string subjectFormatString = "{1}";
            string message = string.Join(",", Enumerable.Repeat("123456789_", 20).ToArray());
            DateTime currentTime = new DateTime(2009, 11, 30, 18, 27, 12);
            var entryToFormat = new LogEntry(LoggingEventType.Debug, message, null, null);

            // Act
            string subject =
                MailLoggingProvider.BuildMailMessageSubject(subjectFormatString, entryToFormat, currentTime);

            Assert.IsFalse(subject.Contains(message));
            Assert.IsTrue(subject.Contains(message.Substring(0, 100)));
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
        public void BuildMailMessage_EntryContainingLineBreaks_Succeeds()
        {
            // Arrange
            var provider = new FakeMailLoggingProvider();
            var configuration = new NameValueCollection();
            configuration.Add("to", "valid@mail.address");
            provider.Initialize("Valid name", configuration);

            var entry = new LogEntry(LoggingEventType.Error,
                "Message with \r\n breaks.", "Log source \r\n with breaks", null);

            // Act
            provider.BuildMailMessage(entry);
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
        public void Configuration_SubjectFormatStringWithNewLineCharacter_ThrowsException()
        {
            // Arrange
            string validToAttributeWithMultipleMailAddresses = 
                "to=\"dev@cuttingedge.it\" " +
                "subjectFormatString=\"Message subject\n\" ";

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
                    Assert.IsTrue(ex.Message.Contains("host"), 
                        "Message should contain the text 'host'. Actual message: " + ex.Message);
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

#if DEBUG // This test code only runs in debug mode

        [TestMethod]
        public void Log_AfterInitializingProvider_DisposedCreatedSmtpClient()
        {
            // Arrange
            var provider = new DisposeTesterMailLoggingProvider();

            var smtpClient = new DisposableSmtpClient();

            Assert.IsFalse(smtpClient.IsDisposed, "Test setup failed.");

            // Set the SmtpClient before calling Initialize.
            provider.SmtpClientToReturnFromCreateSmtpClient = smtpClient;

            // Act
            provider.Initialize("valid name", CreateValidConfiguration());

            Assert.IsTrue(smtpClient.IsDisposed);
        }

        [TestMethod]
        public void Log_Always_DisposesCreatedSmtpClient()
        {
            // Arrange
            var smtpClient = new DisposableSmtpClient();
            Assert.IsFalse(smtpClient.IsDisposed, "Test setup failed.");

            DisposeTesterMailLoggingProvider provider = CreateInitializedDisposeTesterMailLoggingProvider();

            provider.SmtpClientToReturnFromCreateSmtpClient = smtpClient;

            // By letting the BuildMailMessage return null, we force the SmtpClient.Send method to throw an
            // exception. This prevents a mail message from actually being send and also checks whether an
            // exception will still result in the SmtpClient being disposed.
            provider.MailMessageToReturnFromBuildMailMessage = null;

            try
            {
                // Act
                provider.Log("Some message");

                // Assert
                Assert.Fail("ArgumentNullException expected.");
            }
            catch (ArgumentNullException)
            {
                Assert.IsTrue(smtpClient.IsDisposed, "The StmpClient should have been disposed.");
            }            
        }
#endif

        private static NameValueCollection CreateValidConfiguration()
        {
            var configuration = new NameValueCollection();

            configuration.Add("to", "john@do.com");

            return configuration;
        }

#if DEBUG // This test code only runs in debug mode
        private static DisposeTesterMailLoggingProvider CreateInitializedDisposeTesterMailLoggingProvider()
        {
            var provider = new DisposeTesterMailLoggingProvider();

            // Set the SmtpClient before calling Initialize.
            provider.SmtpClientToReturnFromCreateSmtpClient = new SmtpClient();

            provider.Initialize("valid name", CreateValidConfiguration());

            return provider;
        }

        private sealed class DisposeTesterMailLoggingProvider : MailLoggingProvider
        {
            public DisposeTesterMailLoggingProvider()
            {
                this.SetInitialized(true);
            }

            public MailMessage MailMessageToReturnFromBuildMailMessage { get; set; }

            public SmtpClient SmtpClientToReturnFromCreateSmtpClient { get; set; }

            internal override SmtpClient CreateSmtpClient()
            {
                return this.SmtpClientToReturnFromCreateSmtpClient;
            }
            
            protected override MailMessage BuildMailMessage(LogEntry entry)
            {
                return this.MailMessageToReturnFromBuildMailMessage;
            }
        }

        private sealed class DisposableSmtpClient : SmtpClient, IDisposable
        {
            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                this.IsDisposed = true;
                base.Dispose(disposing);
            }
        }
#endif

        private sealed class FakeMailLoggingProvider : MailLoggingProvider
        {
#if DEBUG // This test code only runs in debug mode
            public Exception ExceptionThrownBySmtpClientConstructor { get; set; }

            public Exception ExceptionThrownByMailMessageConstructor { get; set; }
#endif

            public object Public_LogInternal(LogEntry entry)
            {
                return base.LogInternal(entry);
            }

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

#if DEBUG // This code only runs in debug mode
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
#endif

            protected override object LogInternal(LogEntry entry)
            {
                // The base implementation send the mail here, so we want to stub that out.
                return null;
            }
        }
    }
}
