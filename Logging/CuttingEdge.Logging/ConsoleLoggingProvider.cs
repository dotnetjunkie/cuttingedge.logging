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
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Text;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages the writing of logging information to the <see cref="Console"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is used by the <see cref="Logger"/> class to provide Logging services to the 
    /// <see cref="Console"/>.
    /// </para>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="ConsoleLoggingProvider"/>:
    /// <list type="table">  
    /// <listheader>
    ///     <attribute>Attribute</attribute>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <attribute>fallbackProvider</attribute>
    ///     <description>
    ///         A fallback provider that the Logger class will use when logging failed on this logging 
    ///         provider. The value must be the name of an existing logging provider. This attribute is
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
    ///     &lt;logging defaultProvider="ConsoleLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="ConsoleLoggingProvider"
    ///                 type="CuttingEdge.Logging.ConsoleLoggingProvider, CuttingEdge.Logging"
    ///                 description="Console logging provider"
    ///                 threshold="Warning"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </example>
    public class ConsoleLoggingProvider : LoggingProviderBase
    {
        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
        /// <remarks>
        /// Inheritors should call <b>base.Initialize</b> before performing implementation-specific provider
        /// initialization and call <see cref="LoggingProviderBase.CheckForUnrecognizedAttributes"/> last.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the name of the provider is null or when the
        /// <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the name of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown wen an attempt is made to call Initialize on a
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
                config.Add("description", "Console logging provider");
            }

            // Call initialize first.
            base.Initialize(name, config);

            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>
        /// Implements the functionality to log the event.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>Returns null.</returns>
        protected override object LogInternal(LogEntry entry)
        {
            string formattedEvent = FormatEvent(entry);

            Console.Write(formattedEvent);

            // Returning an ID is inappropriate for this type of logger.
            return null;
        }

        private static string FormatEvent(LogEntry entry)
        {
            StringBuilder builder = new StringBuilder(256);
            
            builder.AppendLine("LoggingEvent:");
            builder.Append("Severity:\t").AppendLine(entry.Severity.ToString());
            builder.Append("Message:\t").AppendLine(entry.Message);

            if (entry.Source != null)
            {
                builder.Append("Source\t").AppendLine(entry.Source);
            }

            if (entry.Exception != null)
            {
                builder.Append("Exception:\t").AppendLine(entry.Exception.Message);
                builder.AppendLine(entry.Exception.StackTrace);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
