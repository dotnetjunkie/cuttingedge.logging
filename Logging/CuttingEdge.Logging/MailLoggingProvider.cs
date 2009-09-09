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
    /// This class is used by the <see cref="Logger"/> class to provide Logging services for an 
    /// application that can connect to a configured SMTP Server. The <see cref="MailLoggingProvider"/> uses
    /// the configuration of the &lt;system.net&gt;/&lt;mailSettings&gt; section on the application 
    /// configuration file.
    /// </para>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="MailLoggingProvider"/>:
    /// <list type="table">
    /// <listheader>
    ///     <attribute>Attribute</attribute>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <attribute>fallbackProvider</attribute>
    ///     <description>
    ///         A fallback provider that the Logger class will use when logging failed on this logging 
    ///         provider. The value must contain the name of an existing logging provider. This attribute is
    ///         optional.
    ///     </description>
    /// </item>  
    /// <item>
    ///     <attribute>threshold</attribute>
    ///     <description>
    ///         The logging threshold. The threshold limits the number of event logged. The threshold can be
    ///         defined as follows: Debug &lt; Information &lt; Warning &lt; Error &lt; Fatal. i.e., When the 
    ///         threshold is set to Information, Debug events will not be logged. When no value is specified
    ///         all events are logged. This attribute is optional.
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
    /// <code lang="xml">
    /// &lt;?xml version="1.0"?&gt;
    /// &lt;configuration&gt;
    ///     &lt;configSections&gt;
    ///         &lt;section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" /&gt;
    ///     &lt;/configSections&gt;
    ///     &lt;logging defaultProvider="MailLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="MailLoggingProvider"
    ///                 description="Mail logging provider"
    ///                 threshold="Information"
    ///                 to="developer1@cuttingedge.it;developer2@cuttingedge.it"
    ///                 type="CuttingEdge.Logging.MailLoggingProvider, CuttingEdge.Logging"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    ///     &lt;system.net&gt;
    ///         &lt;mailSettings&gt;
    ///             &lt;smtp from="test@foo.com"&gt;
    ///                 &lt;network
    ///                     host="smtpserver1" 
    ///                     port="25" 
    ///                     userName="john" 
    ///                     password="secret" 
    ///                     defaultCredentials="true"
    ///                 /&gt;
    ///             &lt;/smtp&gt;
    ///         &lt;/mailSettings&gt;
    ///     &lt;/system.net&gt;   
    /// &lt;/configuration&gt;
    /// </code>
    /// </example> 
    public class MailLoggingProvider : LoggingProviderBase
    {
        private string subjectFormatString = "{0}: {1}";

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

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Mail logging provider");
            }

            // Call base initialize first. This method prevents initialize from being called more than once.
            base.Initialize(name, config);

            // Performing implementation-specific provider initialization here.
            this.InitializeToProperty(name, config);
            this.InitializeSubjectFormatStringProperty(name, config);

            TestMailConfiguration(name);

            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>Sends the given <paramref name="entry"/> as mail message.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>Returns null.</returns>
        protected override object LogInternal(LogEntry entry)
        {
            // Create and configure the smtp client
            SmtpClient smtpClient = new SmtpClient();

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
            MailMessage mailMessage = new MailMessage();
            
            mailMessage.Subject = MailLoggingProvider.BuildSubject(this.SubjectFormatString, entry);
            mailMessage.Body = this.BuildMailBody(entry);
            mailMessage.Priority =
                entry.Severity == LoggingEventType.Critical ? MailPriority.High : MailPriority.Normal;

            foreach (MailAddress address in this.To)
            {
                mailMessage.To.Add(address);
            }

            return mailMessage;
        }

        /// <summary>Builds the event log message.</summary>
        /// <param name="entry">The entry that will be used to build the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
        /// <returns>The message.</returns>
        protected virtual string BuildMailBody(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            return LoggingHelper.BuildMessageFromLogEntry(entry);
        }

        private static string BuildSubject(string subjectFormatString, LogEntry entry)
        {
            string exceptionType = entry.Exception != null ? entry.Exception.GetType().Name : null;

            return string.Format(CultureInfo.InvariantCulture, subjectFormatString,
                entry.Severity, // {0}
                entry.Message,  // {1}
                entry.Source,   // {2}
                exceptionType,  // {3}
                DateTime.Now);  // {4}
        }

        private static void TestMailConfiguration(string name)
        {
            SmtpClient client;
            try
            {
                client = new SmtpClient();
            }
            catch (SecurityException ex)
            {
                // the SmtpClient constructor will throw a SecurityException when the application doesn't
                // have the proper rights to send mail.
                throw new ProviderException(SR.GetString(SR.NoPermissionsToAccessSmtpServers, name,
                    ex.Message));
            }

            if (String.IsNullOrEmpty(client.Host))
            {
                throw new ProviderException(SR.GetString(SR.MissingAttributeInMailSettings, name, "host",
                    "/smtp/network") + " " + SR.GetString(SR.ExampleMailConfigurationSettings));
            }

            MailMessage message;

            try
            {
                message = new MailMessage();
            }
            catch (Exception ex)
            {
                // We the system.net/mailSettings configuration is invalid, the MailMessage constructor might
                // throw an exception (for instance, when the from message is not a valid mail address).
                throw new ProviderException(
                    SR.GetString(SR.PossibleInvalidMailConfigurationInConfigFile, typeof(MailMessage)) + " " +
                    ex.Message + " " + SR.GetString(SR.ExampleMailConfigurationSettings), ex);
            }

            try
            {
                if (message.From == null)
                {
                    throw new ProviderException(SR.GetString(SR.MissingAttributeInMailSettings, name, "from",
                        "/smtp") + " " + SR.GetString(SR.ExampleMailConfigurationSettings));
                }
            }
            finally
            {
                // MailMessage implements IDisposable, so we must dispose it.
                message.Dispose();
            }
        }

        private void InitializeToProperty(string name, NameValueCollection config)
        {
            string addressesConfigValue = config["to"];

            // Throw exception when no from is provided
            if (string.IsNullOrEmpty(addressesConfigValue))
            {
                throw new ProviderException(SR.GetString(SR.EmptyOrMissingPropertyInConfiguration, "to",
                    name));
            }

            List<MailAddress> addresses = new List<MailAddress>();

            foreach (string address in addressesConfigValue.Split(';'))
            {
                if (string.IsNullOrEmpty(address))
                {
                    throw new ProviderException(SR.GetString(SR.InvalidMailAddressAttribute, 
                        addressesConfigValue, "to", name));
                }

                try
                {
                    addresses.Add(new MailAddress(address));
                }
                catch (FormatException)
                {
                    throw new ProviderException(SR.GetString(SR.InvalidMailAddressAttribute, address, "to",
                        name));
                }
            }

            this.To = new ReadOnlyCollection<MailAddress>(addresses);

            // Remove this attribute from the configuration. This way the provider can spot unrecognized 
            // attributes after the initialization process.
            config.Remove("to");
        }

        private void InitializeSubjectFormatStringProperty(string name, NameValueCollection config)
        {
            string formatString = config["subjectFormatString"];

            // SubjectFormatString is an optional property.
            if (!String.IsNullOrEmpty(formatString))
            {
                try
                {
                    // Test is the formatString is formatted correctly.
                    LogEntry entry = new LogEntry(LoggingEventType.Error, "Message", null, null);
                    BuildSubject(formatString, entry);
                }
                catch (FormatException ex)
                {
                    throw new ProviderException(SR.GetString(SR.InvalidFormatStringAttribute, formatString, 
                        "subjectFormatString", name) + " " + ex.Message, ex);
                }

                this.subjectFormatString = formatString;
            }
            else
            {
                // No subjectFormatString is specified: we use the default.
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove("subjectFormatString");
        }
    }
}