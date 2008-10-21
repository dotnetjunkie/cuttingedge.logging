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
using System.ComponentModel;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages storage of Logging information to the Windows event log.
    /// </summary>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the 
    /// Logging section, which can also be accessed as members of the
    /// <see cref="LoggingSection"/> class. The following configuration file example shows
    /// how to specify values declaratively for the Logging section.
    /// <code>
    /// &lt;?xml version="1.0"?&gt;
    /// &lt;configuration&gt;
    ///     &lt;configSections&gt;
    ///         &lt;section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" /&gt;
    ///     &lt;/configSections&gt;
    ///     &lt;logging defaultProvider="WindowsEventLogLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                 threshold="Warning"
    ///                 source="MyApplication"
    ///                 logName="MyApplication"
    ///                 description="Windows event log logging provider"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </example>
    public class WindowsEventLogLoggingProvider : LoggingProviderBase
    {
        private string source;
        private string logName;

        /// <summary>
        /// Gets the source name to register and use when writing to the event log. This is the source name by
        /// which the application is registered on the local computer.
        /// </summary>
        /// <value>The source.</value>
        public string Source
        {
            get { return this.source; }
        }

        /// <summary>
        /// Gets the name of the log the source's entries are written to. Possible values include:
        /// Application, System, or a custom event log.
        /// </summary>
        /// <value>The name of the log.</value>
        public string LogName
        {
            get { return this.logName; }
        }

        /// <summary>Overridden from base.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific 
        /// attributes specified in the configuration for this provider.</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Windows event log logging provider");
            }

            // Call initialize first.
            base.Initialize(name, config);

            // Performing implementation-specific provider initialization here.
            this.InitializeSource(name, config);

            this.InitializeLogName(name, config);
            
            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>Returns null.</returns>
        protected override object LogInternal(LogEntry entry)
        {
            EventLogEntryType? eventLogType = ConvertToEventLogEntry(entry.Severity);

            using (EventLog eventLog = new EventLog(this.logName))
            {
                Exception exception = entry.Exception;
                string message = entry.Message;

                if (entry.Source != null)
                {
                    message += "\nSource: " + entry.Source;
                }

                if (exception != null)
                {
                    while (exception != null)
                    {
                        message += "\nException: " + exception.GetType().FullName + "\n";
                        message += "Message:\n" + exception.Message + "\n";
                        message += "Stacktrace:\n" + exception.StackTrace + "\n";
                        exception = exception.InnerException;
                    }
                }

                if (!EventLog.SourceExists(this.source))
                {
                    EventLog.CreateEventSource(this.source, this.logName);
                }

                eventLog.Source = this.source;

                if (entry != null)
                {
                    eventLog.WriteEntry(message, eventLogType.Value);
                }
                else
                {
                    eventLog.WriteEntry(message);
                }
            }

            // Returning an ID is inappropriate for this type of logger.
            return null;
        }

        private static EventLogEntryType? ConvertToEventLogEntry(LoggingEventType severity)
        {
            switch (severity)
            {
                case LoggingEventType.Critical:
                    return EventLogEntryType.Error;

                case LoggingEventType.Error:
                    return EventLogEntryType.Error;

                case LoggingEventType.Warning:
                    return EventLogEntryType.Warning;

                case LoggingEventType.Information:
                    return EventLogEntryType.Information;

                case LoggingEventType.Debug:
                    return null;

                default:
                    throw new InvalidEnumArgumentException("severity", (int)severity, typeof(LoggingEventType));
            }
        }

        private void InitializeLogName(string name, NameValueCollection config)
        {
            string logName = config["logName"];
            
            // Throw exception when no logname is provided
            if (string.IsNullOrEmpty(logName))
            {
                throw new ProviderException(SR.GetString(SR.EmptyOrMissingPropertyInConfiguration,
                    "logName", name));
            }

            this.logName = logName;

            config.Remove("logName");
        }

        private void InitializeSource(string name, NameValueCollection config)
        {
            string source = config["source"];
            
            // Throw exception when no source is provided
            if (string.IsNullOrEmpty(source))
            {
                throw new ProviderException(SR.GetString(SR.EmptyOrMissingPropertyInConfiguration,
                    "source", name));
            }

            this.source = source;

            config.Remove("source");
        }
    }
}