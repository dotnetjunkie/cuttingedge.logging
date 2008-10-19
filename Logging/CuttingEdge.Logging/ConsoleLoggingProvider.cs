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
    /// This class is used by the <see cref="Logger"/> class to provide Logging services to the 
    /// <see cref="Console"/>.
    /// </remarks>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the
    /// Logging section, which can also be accessed as members of the <see cref="LoggingSection"/> class.
    /// The following configuration file example shows how to specify values declaratively for the
    /// Logging section.
    /// <code>
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
    ///                 threshold="Warning"
    ///                 description="Console logging provider"
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
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">An optional source where the event occured.</param>
        /// <returns>
        /// The id of the logged event or null when an id is inappropriate.
        /// </returns>
        protected override object LogInternal(LoggingEventType severity, string message, Exception exception, 
            string source)
        {
            string formattedEvent = FormatEvent(severity, message, exception, source);

            Console.Write(formattedEvent);

            return null;
        }

        private static string FormatEvent(LoggingEventType severity, string message, Exception exception, 
            string source)
        {
            StringBuilder builder = new StringBuilder(256);
            
            builder.AppendLine("LoggingEvent:");
            builder.Append("Severity:\t").AppendLine(severity.ToString());
            builder.Append("Message:\t").AppendLine(message);

            if (source != null)
            {
                builder.Append("Source\t").AppendLine(source);
            }

            if (exception != null)
            {
                builder.Append("Exception:\t").AppendLine(exception.Message);
                builder.AppendLine(exception.StackTrace);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
