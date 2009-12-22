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
using System.Globalization;
using System.Web.Management;

namespace CuttingEdge.Logging.Web
{
    /// <summary>
    /// Implements an event provider that saves event notifications using the <see cref="Logger"/> 
    /// infrastructure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="LoggingWebEventProvider"/>:
    /// <list type="table">  
    /// <listheader>
    ///     <attribute>Attribute</attribute>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <attribute>loggingProvider</attribute>
    ///     <description>
    ///         The logging provider that will be used when writing an event. The value must contain the name 
    ///         of an existing logging provider. The default logging provider will be used when this attribute
    ///         is omitted. This attribute is optional.
    ///     </description>
    /// </item>  
    /// </list>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates how to configure the <see cref="LoggingWebEventProvider"/> in the web.config
    /// file. The configuration must contain a valid logging provider configuration. The 
    /// <see cref="DebugLoggingProvider"/> is used in this example.
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" />
    ///     </configSections>
    ///     <logging defaultProvider="DebugLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="DebugLoggingProvider"
    ///                 type="CuttingEdge.Logging.DebugLoggingProvider, CuttingEdge.Logging"
    ///                 description="Debug logging provider"
    ///                 threshold="Debug"
    ///             />
    ///         </providers>
    ///     </logging>   
    ///     <system.web>
    ///         <healthMonitoring heartbeatInterval="0" enabled="true">
    ///             <providers>
    ///                 <add name="LoggingWebEventProvider" 
    ///                     type="CuttingEdge.Logging.Web.LoggingWebEventProvider, CuttingEdge.Logging" 
    ///                     loggingProvider="AspNetSqlLoggingProvider"
    ///                     />
    ///             </providers>
    ///             <rules>
    ///                 <add name="Custom Event Provider"
    ///                      eventName="All Events"
    ///                      provider="LoggingWebEventProvider"
    ///                      profile="Default"
    ///                     />
    ///             </rules>
    ///         </healthMonitoring>
    ///     </system.web>
    /// </configuration>
    /// ]]></code>
    /// See the &lt;healthMonitoring&gt; web.config configuration element for more information about
    /// configuring <see cref="WebEventProvider"/> classes and logging <see cref="WebBaseEvent"/> objects.
    /// </example> 
    /// <seealso cref="WebEventProvider" />
    /// <seealso cref="WebBaseEvent" />
    public class LoggingWebEventProvider : WebEventProvider
    {
        private ILogger loggingProvider;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name of the provider is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the name of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = this.GetType().Name;
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config["description"] = "Logging Web Event Provider";
            }

            base.Initialize(name, config);

            this.InitializeLoggingProvider(config);

            // The config argument should contain no elements.
            if (config.Count > 0)
            {
                string invalidKey = config.GetKey(0);
                throw new ProviderException(SR.UnrecognizedAttributeInProviderConfiguration(name, invalidKey));
            }
        }

        /// <summary>Processes the event passed to the provider.</summary>
        /// <param name="raisedEvent">The <see cref="WebBaseEvent"/> object to process.</param>
        public override void ProcessEvent(WebBaseEvent raisedEvent)
        {
            if (raisedEvent == null)
            {
                return;
            }

            LogEntry logEntry = CreateLogEntry(raisedEvent);

            this.loggingProvider.Log(logEntry);
        }

        /// <summary>Moves the events from the provider's buffer into the event log.</summary>
        public override void Flush()
        {
        }

        /// <summary>Performs tasks associated with shutting down the provider.</summary>
        public override void Shutdown()
        {
        }

        private static LogEntry CreateLogEntry(WebBaseEvent eventRaised)
        {
            LoggingEventType eventType = GetEventTypeFromWebEvent(eventRaised);

            string message = string.Format(CultureInfo.InvariantCulture, "{0} (Event Code: {1})",
                eventRaised.Message, eventRaised.EventCode);

            string source = eventRaised.GetType().Name;

            Exception exception = GetExceptionFromWebEvent(eventRaised);

            return new LogEntry(eventType, message, source, exception);
        }

        private static LoggingEventType GetEventTypeFromWebEvent(WebBaseEvent eventRaised)
        {
            if (eventRaised is WebFailureAuditEvent)
            {
                return LoggingEventType.Warning;
            }
            else if (eventRaised is WebBaseErrorEvent)
            {
                return LoggingEventType.Error;
            }
            else
            {
                // All other events are information (also WebHeartbeatEvent)
                return LoggingEventType.Information;
            }
        }

        private static Exception GetExceptionFromWebEvent(WebBaseEvent eventRaised)
        {
            WebBaseErrorEvent errorEvent = eventRaised as WebBaseErrorEvent;

            if (errorEvent != null)
            {
                return errorEvent.ErrorException;
            }

            return null;
        }

        private void InitializeLoggingProvider(NameValueCollection config)
        {
            const string LoggingProviderAttribute = "loggingProvider";

            string loggingProviderName = config[LoggingProviderAttribute];

            // Throw exception when no connectionStringName is provided
            if (string.IsNullOrEmpty(loggingProviderName))
            {
                this.loggingProvider = Logger.Provider;
            }
            else
            {
                this.loggingProvider = Logger.Providers[loggingProviderName];

                if (this.loggingProvider == null)
                {
                    throw new ProviderException(SR.MissingLoggingProviderInConfig(loggingProviderName,
                        this.Name));
                }
            }

            // Remove this attribute from the configuration. This way the provider can spot unrecognized 
            // attributes after the initialization process.
            config.Remove(LoggingProviderAttribute);
        }
    }
}