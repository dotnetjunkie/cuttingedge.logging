using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Prevents the logging of events from occurring.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default behavior of CuttingEdge.Logging is to let exceptions, caused by a failing provider, bubble
    /// up the call stack. The philosophy of <b>CuttingEdge.Logging</b> is that a failing provider is a
    /// serious problem and should not be ignored. For this reason <b>CuttingEdge.Logging</b> contains the
    /// notion of <see cref="LoggingProviderBase.FallbackProvider">FallbackProvider</see>s. When a provider
    /// fails, the original event (and the exception thrown by the provider) will be routed to the fallback
    /// provider. In certain scenario's however, developers want the application to be more robust and 
    /// continue execution, even when logging fails. This can be the case when no suited fallback provider is 
    /// available or the fallback provider could also fail. While these scenario's should be rare, the 
    /// <b>TerminatorLoggingProvider</b> enables this. By using a <b>TerminatorLoggingProvider</b>, 
    /// events going through that provider will get trashed and the application continues execution.</para>
    /// <para>
    /// The <b>TerminatorLoggingProvider</b> will always return null after a call to
    /// <see cref="ILogger.Log">Log</see>.</para>
    /// <para>
    /// The table below shows the list of valid attributes for the <b>TerminatorLoggingProvider</b>
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
    /// </list>
    /// Note that the <b>fallbackProvider</b> and <b>threshold</b> attributes are not valid for this type of
    /// provider.</para>
    /// <para>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.</para>
    /// <example>
    /// <para>
    /// This example demonstrates how to specify values declaratively for several attributes of the logging
    /// section, which can also be accessed as members of the <see cref="LoggingSection"/> class. The
    /// following configuration file example shows how to specify values declaratively for the logging
    /// section.</para>
    /// <para>
    /// In this example a <b>TerminatorLoggingProvider</b> is configured as fallback provider to prevent a
    /// possible exception caused by a failing provider to bubble up the call stack.</para>
    /// <para>
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" allowDefinition="MachineToApplication"
    ///             type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging" />
    ///     </configSections>
    ///     <connectionStrings>
    ///         <add name="SqlLoggingConnection" 
    ///             connectionString="Data Source=.;Integrated Security=SSPI;Initial Catalog=Logging;" />
    ///     </connectionStrings>
    ///     <logging defaultProvider="SqlLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="SqlLoggingProvider"
    ///                 type="CuttingEdge.Logging.SqlLoggingProvider, CuttingEdge.Logging"
    ///                 fallbackProvider="Terminator"
    ///                 connectionStringName="SqlLoggingConnection"
    ///             />
    ///             <add 
    ///                 name="Terminator"
    ///                 type="CuttingEdge.Logging.TerminatorLoggingProvider, CuttingEdge.Logging"
    ///             />
    ///         </providers>
    ///     </logging>
    /// </configuration>
    /// ]]></code>
    /// </para>
    /// </example>
    /// </remarks>
    public class TerminatorLoggingProvider : LoggingProviderBase
    {
        /// <summary>Initializes a new instance of the <see cref="TerminatorLoggingProvider"/> class.</summary>
        public TerminatorLoggingProvider()
        {
        }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
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

            LoggingHelper.SetDescriptionWhenMissing(config, "Terminator logging provider");

            // Check for unsupported attributes fallbackProvider and threshold before calling base.Initialize.
            this.ThrowWhenConfigContainsFallbackProvider(name, config);
            this.ThrowWhenConfigContainsThreshold(name, config);

            // Call base initialize first. This method prevents initialize from being called more than once.
            base.Initialize(name, config);

            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>Returns null.</returns>
        protected override object LogInternal(LogEntry entry)
        {
            // Do nothing.
            return null;
        }

        private void ThrowWhenConfigContainsFallbackProvider(string name, NameValueCollection config)
        {
            const string FallbackProviderAttributeName = "fallbackProvider";

            bool configurationContainsFallbackProviderAttribute =
                ConfigurationContainsKey(config, FallbackProviderAttributeName);

            if (configurationContainsFallbackProviderAttribute)
            {
                this.ThrowUnrecognizedAttributeException(name, FallbackProviderAttributeName);
            }
        }

        private void ThrowWhenConfigContainsThreshold(string name, NameValueCollection config)
        {
            const string ThresholdAttributeName = "threshold";

            bool configurationContainsThresholdAttribute =
                ConfigurationContainsKey(config, ThresholdAttributeName);

            if (configurationContainsThresholdAttribute)
            {
                this.ThrowUnrecognizedAttributeException(name, ThresholdAttributeName);
            }
        }

        private static bool ConfigurationContainsKey(NameValueCollection config, string key)
        {
            // We need to support .NET 2.0 so we can't use the Enumerable extension methods.
            List<string> configurationKeys = new List<string>(config.AllKeys);

            return configurationKeys.Contains(key);
        }

        private void ThrowUnrecognizedAttributeException(string name, string attributeName)
        {
            // Create a config with a single attribute (its value is not important)
            NameValueCollection config = new NameValueCollection();
            config[attributeName] = string.Empty;

            // Throw an ProviderException describing the unrecognized attribute.
            this.CheckForUnrecognizedAttributes(name, config);
        }
    }
}