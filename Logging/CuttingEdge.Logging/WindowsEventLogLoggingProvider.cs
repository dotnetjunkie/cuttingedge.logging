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
using System.Text;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages storage of Logging information to the Windows event log.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returning an identifier for the logged event is not appropriate for this provider. This provider will
    /// always return null (Nothing in VB) from the <see cref="ILogger.Log(LogEntry)">Log</see> method.
    /// </para>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="WindowsEventLogLoggingProvider"/>
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
    ///     <attribute>source</attribute>
    ///     <description>
    ///         The source name to register and use when writing to the event log. This is the source name by
    ///         which the application is registered on the local computer. This attribute is mandatory.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>logName</attribute>
    ///     <description>
    ///         The name of the log where  the source's entries are written to. Possible values include: 
    ///         Application, System, or a custom event log. This attribute is mandatory.
    ///     </description>
    /// </item>
    /// </list>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the 
    /// Logging section, which can also be accessed as members of the
    /// <see cref="LoggingSection"/> class. The following configuration file example shows
    /// how to specify values declaratively for the Logging section.
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" />
    ///     </configSections>
    ///     <logging defaultProvider="WindowsEventLogLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                 threshold="Warning"
    ///                 source="MyApplication"
    ///                 logName="MyApplication"
    ///                 description="Windows event log logging provider"
    ///             />
    ///         </providers>
    ///     </logging>
    /// </configuration>
    /// ]]></code>
    /// </example>
    public class WindowsEventLogLoggingProvider : LoggingProviderBase
    {
        private string source;
        private string logName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsEventLogLoggingProvider"/> class.
        /// </summary>
        public WindowsEventLogLoggingProvider()
        {
        }

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
        /// Gets the name of the log where the source's entries are written to. Possible values include:
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

            // Call base initialize first. This method prevents initialize from being called more than once.
            base.Initialize(name, config);

            // Performing implementation-specific provider initialization here.
            this.InitializeSource(config);
            this.InitializeLogName(config);
            
            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        internal virtual void WriteEntryToEventLog(WindowsEventLogLoggingProvider provider,
            string eventLogMessage)
        {
            using (EventLog eventLog = CreateEventLog(provider))
            {
                eventLog.WriteEntry(eventLogMessage);
            }
        }

        internal virtual void WriteEntryToEventLog(WindowsEventLogLoggingProvider provider,
            string eventLogMessage, EventLogEntryType type)
        {
            using (EventLog eventLog = CreateEventLog(provider))
            {
                eventLog.WriteEntry(eventLogMessage, type);
            }
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>Returns null.</returns>
        /// <exception cref="Win32Exception">
        /// Thrown when the provider fails writing the <paramref name="entry"/> to the windows event log.
        /// </exception>
        protected override object LogInternal(LogEntry entry)
        {
            EventLogEntryType? eventLogType = ConvertToEventLogEntry(entry.Severity);

            string eventLogMessage = this.BuildEventLogMessage(entry);

            if (eventLogType != null)
            {
                this.WriteEntryToEventLog(this, eventLogMessage, eventLogType.Value);
            }
            else
            {
                this.WriteEntryToEventLog(this, eventLogMessage);
            }

            // Returning an ID is inappropriate for this type of logger.
            return null;
        }

        /// <summary>Builds the event log message.</summary>
        /// <param name="entry">The entry that will be used to build the message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
        /// <returns>The message.</returns>
        protected virtual string BuildEventLogMessage(LogEntry entry)
        {
            return LoggingHelper.BuildMessageFromLogEntry(entry);
        }

        private static EventLog CreateEventLog(WindowsEventLogLoggingProvider provider)
        {
            EventLog eventLog = new EventLog(provider.LogName);
            try
            {
                if (!EventLog.SourceExists(provider.Source))
                {
                    EventLog.CreateEventSource(provider.Source, provider.LogName);
                }

                eventLog.Source = provider.Source;
            }
            catch
            {
                eventLog.Dispose();
                throw;
            }

            return eventLog;
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

        private void InitializeLogName(NameValueCollection config)
        {
            const string LogName = "logName";

            this.logName = config[LogName];
            
            // Throw exception when no logname is provided
            if (string.IsNullOrEmpty(this.logName))
            {
                throw new ProviderException(SR.EmptyOrMissingPropertyInConfiguration(LogName, this.Name));
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove(LogName);
        }

        private void InitializeSource(NameValueCollection config)
        {
            const string Source = "source";

            this.source = config[Source];
            
            // Throw exception when no source is provided
            if (string.IsNullOrEmpty(this.source))
            {
                throw new ProviderException(SR.EmptyOrMissingPropertyInConfiguration(Source, this.Name));
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove(Source);
        }
    }
}