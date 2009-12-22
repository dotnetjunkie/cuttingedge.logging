#region Copyright (c) 2008 S. van Deursen
/* The CuttingEdge.Logging library allows developers to plug a logging mechanism into their web- and desktop
 * applications.
 * 
 * Copyright (C) 2008 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Globalization;
using System.Net.Mail;
using System.Security;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages sending of logging information by mail.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returning an identifier for the logged event is not appropriate for this provider. This provider will
    /// always return null (Nothing in VB) from the <see cref="ILogger.Log(LogEntry)">Log</see> method.
    /// </para>
    /// <para>
    /// This class is used by the <see cref="Logger"/> class to provide Logging services for an 
    /// application that can connect to a configured SMTP Server. The <see cref="MailLoggingProvider"/> uses
    /// the configuration of the &lt;system.net&gt;/&lt;mailSettings&gt; section on the application 
    /// configuration file.
    /// </para>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="MailLoggingProvider"/>
    /// configuration:
    /// <list type="table">
    /// <listheader>
    ///     <attribute>Attribute</attribute>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <attribute>name</attribute>
    ///     <description>
    ///         The name of the provider. This attribute is mandatory.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>description</attribute>
    ///     <description>
    ///         A description of the provider. This attribute is optional.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>fallbackProvider</attribute>
    ///     <description>
    ///         A fallback provider that this provider will use when logging failed. The value must contain 
    ///         the name of an existing logging provider. This attribute is optional.
    ///     </description>
    /// </item>  
    /// <item>
    ///     <attribute>threshold</attribute>
    ///     <description>
    ///         The logging threshold. The threshold limits the number of events logged. The threshold can be
    ///         defined as follows: Debug &lt; Information &lt; Warning &lt; Error &lt; Critical. i.e., When
    ///         the threshold is set to Information, events with a severity of Debug  will not be logged. When
    ///         no value is specified, all events are logged. This attribute is optional.
    ///      </description>
    /// </item>  
    /// <item>
    ///     <attribute>to</attribute>
    ///     <description>
    ///         A list of mail addresses, separated by a semicolon (;). This attribute is mandatory.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>subjectFormatString</attribute>
    ///     <description>
    ///         A format string that will be used to build the subject. The following indexed placeholders can
    ///         be used to format the subject.
    ///         <list type="table">
    ///             <listheader>
    ///                 <placeholder>Placeholder</placeholder>
    ///                 <description>Description</description>
    ///             </listheader>
    ///             <item>
    ///                 <placeholder>{0}</placeholder>
    ///                 <description>The severity of the event.</description>
    ///             </item>
    ///             <item>
    ///                 <placeholder>{1}</placeholder>
    ///                 <description>The message of the event</description>
    ///             </item>
    ///             <item>
    ///                 <placeholder>{2}</placeholder>
    ///                 <description>The source of the event</description>
    ///             </item>
    ///             <item>
    ///                 <placeholder>{3}</placeholder>
    ///                 <description>The type of the exception that caused the event (if any)</description>
    ///             </item>
    ///             <item>
    ///                 <placeholder>{4}</placeholder>
    ///                 <description>The current DateTime.</description>
    ///             </item>
    ///         </list>
    ///         This attribute is optional and "<b>{0}: {1}</b>" by default, which means it will be formatted
    ///         as "{severity}: {event message}".
    ///     </description>
    /// </item>
    /// </list>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the
    /// Logging section, which can also be accessed as members of the <see cref="LoggingSection"/> class.
    /// The following configuration file example shows how to specify values declaratively for the
    /// Logging section.
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" />
    ///     </configSections>
    ///     <logging defaultProvider="MailLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="MailLoggingProvider"
    ///                 type="CuttingEdge.Logging.MailLoggingProvider, CuttingEdge.Logging"
    ///                 description="Mail logging provider"
    ///                 threshold="Information"
    ///                 to="developer1@cuttingedge.it;developer2@cuttingedge.it"
    ///                 subjectFormatString="Application error. {1} (Severity: {0})"
    ///             />
    ///         </providers>
    ///     </logging>
    ///     <system.net>
    ///         <mailSettings>
    ///             <smtp from="test@foo.com">
    ///                 <network
    ///                     host="smtpserver1" 
    ///                     port="25" 
    ///                     userName="john" 
    ///                     password="secret" 
    ///                     defaultCredentials="true"
    ///                 />
    ///             </smtp>
    ///         </mailSettings>
    ///     </system.net>   
    /// </configuration>
    /// ]]></code>
    /// </example> 
    public class MailLoggingProvider : LoggingProviderBase
    {
        private const string SubjectFormatStringAttribute = "subjectFormatString";

        private string subjectFormatString;

        /// <summary>Gets the specified list of email addresses to send the logging messages to.</summary>
        /// <value>A collection of email addresses.</value>
        public ReadOnlyCollection<MailAddress> To { get; private set; }

        /// <summary>Gets the string to format the email subject.</summary>
        /// <value>The subject format string.</value>
        public string SubjectFormatString
        {
            get { return this.subjectFormatString; }
        }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
        /// <remarks>
        /// Inheritors should call <b>base.Initialize</b> before performing implementation-specific provider
        /// initialization and call 
        /// <see cref="LoggingProviderBase.CheckForUnrecognizedAttributes">CheckForUnrecognizedAttributes</see>
        /// last.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the name of the provider is null or when the
        /// <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the name of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an attempt is made to call Initialize on a
        /// provider after the provider has already been initialized.</exception>
        /// <exception cref="ProviderException">Thrown when the <paramref name="config"/> contains
        /// unrecognized attributes.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // Call base initialize first. This method prevents initialize from being called more than once.
            base.Initialize(name, config);

            // Performing implementation-specific provider initialization here.
            this.InitializeToProperty(config);
            this.InitializeSubjectFormatStringProperty(config);
            this.ValidateDotNetMailConfiguration();

            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        internal static MailPriority DetermineMailPriority(LoggingEventType severity)
        {
            return severity == LoggingEventType.Critical ? MailPriority.High : MailPriority.Normal;
        }

        internal static string BuildMailMessageSubject(string subjectFormatString, LogEntry entry, 
            DateTime currentTime)
        {
            string exceptionType = entry.Exception != null ? entry.Exception.GetType().Name : null;

            return string.Format(CultureInfo.InvariantCulture, subjectFormatString,
                entry.Severity, // {0}
                entry.Message,  // {1}
                entry.Source,   // {2}
                exceptionType,  // {3}
                currentTime);   // {4}
        }

        internal virtual SmtpClient CreateSmtpClient()
        {
            return new SmtpClient();
        }

        internal virtual MailMessage CreateMailMessage()
        {
            return new MailMessage();
        }

        /// <summary>Sends the given <paramref name="entry"/> as mail message.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>Returns null.</returns>
        /// <exception cref="SmtpException">Thrown when the provider was unable to send the mail message.</exception>
        protected override object LogInternal(LogEntry entry)
        {
            // Create and configure the SMTP client
            var smtpClient = this.CreateSmtpClient();

            MailMessage mailMessage = this.BuildMailMessage(entry);

            smtpClient.Send(mailMessage);

            // Returning an ID is inappropriate for this type of logger.
            return null;
        }

        /// <summary>Builds the mail message using the supplied <paramref name="entry"/>.</summary>
        /// <param name="entry">The entry.</param>
        /// <returns>A new <see cref="MailMessage"/>.</returns>
        protected virtual MailMessage BuildMailMessage(LogEntry entry)
        {
            MailMessage message = this.CreateMailMessage();

            var currentTime = DateTime.Now;

            message.Subject = BuildMailMessageSubject(this.subjectFormatString, entry, currentTime);
            message.Body = this.BuildMailBody(entry);
            message.Priority = DetermineMailPriority(entry.Severity);

            foreach (MailAddress address in this.To)
            {
                message.To.Add(address);
            }

            return message;
        }

        /// <summary>Builds the event log message.</summary>
        /// <param name="entry">The entry that will be used to build the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
        /// <returns>The message.</returns>
        protected virtual string BuildMailBody(LogEntry entry)
        {
            return LoggingHelper.BuildMessageFromLogEntry(entry);
        }

        private void InitializeToProperty(NameValueCollection config)
        {
            const string To = "to";

            string addressesConfigValue = config[To];

            // Throw exception when no from is provided
            if (string.IsNullOrEmpty(addressesConfigValue))
            {
                throw new ProviderException(SR.EmptyOrMissingPropertyInConfiguration(To, this.Name));
            }

            List<MailAddress> addresses = new List<MailAddress>();

            foreach (string address in addressesConfigValue.Split(';'))
            {
                if (string.IsNullOrEmpty(address))
                {
                    throw new ProviderException(
                        SR.InvalidMailAddressAttribute(addressesConfigValue, To, this.Name));
                }

                try
                {
                    addresses.Add(new MailAddress(address));
                }
                catch (FormatException)
                {
                    throw new ProviderException(
                        SR.InvalidMailAddressAttribute(address, To, this.Name));
                }
            }

            this.To = new ReadOnlyCollection<MailAddress>(addresses);

            // Remove this attribute from the configuration. This way the provider can spot unrecognized 
            // attributes after the initialization process.
            config.Remove(To);
        }

        private void InitializeSubjectFormatStringProperty(NameValueCollection config)
        {
            string subjectFormatString = config[SubjectFormatStringAttribute];

            // SubjectFormatString is an optional property.
            if (!String.IsNullOrEmpty(subjectFormatString))
            {
                this.TestIfSubjectFormatStringIsFormattedCorrectly(subjectFormatString);

                this.subjectFormatString = subjectFormatString;
            }
            else
            {
                // No subjectFormatString is specified: we use the default.
                this.subjectFormatString = "{0}: {1}";
            }

            // Remove this attribute from the configuration. This way the provider can spot unrecognized 
            // attributes after the initialization process.
            config.Remove(SubjectFormatStringAttribute);
        }

        private void TestIfSubjectFormatStringIsFormattedCorrectly(string subjectFormatString)
        {
            try
            {
                // Test is the formatString is formatted correctly.
                LogEntry testEntry = new LogEntry(LoggingEventType.Error, "Message", null, null);
                
                BuildMailMessageSubject(subjectFormatString, testEntry, DateTime.MaxValue);
            }
            catch (FormatException ex)
            {
                string exceptionMessage =
                    SR.InvalidFormatStringAttribute(subjectFormatString, SubjectFormatStringAttribute, 
                    this.Name, ex.Message);

                throw new ProviderException(exceptionMessage, ex);
            }
        }

        private void ValidateDotNetMailConfiguration()
        {
            this.TestCreatingSmtpClient();

            this.TestCreatingMailMessage();
        }

        private void TestCreatingSmtpClient()
        {
            SmtpClient client;

            try
            {
                client = this.CreateSmtpClient();
            }
            catch (SecurityException ex)
            {
                // the SmtpClient constructor will throw a SecurityException when the application doesn't
                // have the proper rights to send mail.
                throw new ProviderException(
                    SR.NoPermissionsToAccessSmtpServers(this.Name, ex.Message));
            }

            if (String.IsNullOrEmpty(client.Host))
            {
                throw new ProviderException(SR.MissingAttributeInMailSettings(this.Name, "host", 
                    "/smtp/network") + " " + SR.ExampleMailConfigurationSettings());
            }
        }

        private void TestCreatingMailMessage()
        {
            MailMessage message = null;

            try
            {
                try
                {
                    message = this.CreateMailMessage();
                }
                catch (Exception ex)
                {
                    // We the system.net/mailSettings configuration is invalid, the MailMessage
                    // constructor might throw an exception (for instance, when the from message is not a
                    // valid mail address).
                    throw new ProviderException(
                        SR.PossibleInvalidMailConfigurationInConfigFile(typeof(MailMessage), ex.Message) +
                        " " + SR.ExampleMailConfigurationSettings(), ex);
                }

                if (message.From == null)
                {
                    // The system.net/mailSettings configuration is missing the 'from' mail address.
                    throw new ProviderException(SR.MissingAttributeInMailSettings(this.Name, 
                        "from", "/smtp") + " " + SR.ExampleMailConfigurationSettings());
                }
            }
            finally
            {
                if (message != null)
                {
                    // MailMessage implements IDisposable, so we must dispose it.
                    message.Dispose();
                }
            }
        }
    }
}