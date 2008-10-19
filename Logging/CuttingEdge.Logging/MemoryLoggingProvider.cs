using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages storage of logging information into memory. Use the <see cref="GetLoggedEvents"/> to retrieve
    /// a copy of the internal cache and <see cref="Clear"/> to clear the internal cache.
    /// </summary>
    /// <remarks>
    /// This class is used by the <see cref="Logger"/> class to provide Logging services that logs to an
    /// in-memory cache.
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
    ///     &lt;logging defaultProvider="MemoryLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="MemoryLoggingProvider"
    ///                 type="CuttingEdge.Logging.MemoryLoggingProvider, CuttingEdge.Logging"
    ///                 threshold="Debug"
    ///                 description="Memory logging provider"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// The following example shows how to use the class:
    /// <code>
    /// Logger.Log(LoggingEventType.Error, "This is an error");
    /// 
    /// MemoryLoggingProvider memoryLogger = (MemoryLoggingProvider)Logger.Provider;
    /// 
    /// foreach (MemoryLoggingEvent loggedEvent in memoryLogger.GetLoggedEvents())
    /// {
    ///     Console.WriteLine(loggedEvent);
    /// }
    /// </code>
    /// </example>
    public class MemoryLoggingProvider : LoggingProviderBase
    {
        private readonly object locker = new object();
        private readonly List<MemoryLoggingEvent> loggingEvents = new List<MemoryLoggingEvent>();

        /// <summary>
        /// Gets a copy of the internal cache of <see cref="MemoryLoggingEvent"/> objects.
        /// </summary>
        /// <returns>An array of <see cref="MemoryLoggingEvent"/> objects.</returns>
        public MemoryLoggingEvent[] GetLoggedEvents()
        {
            lock (this.locker)
            {
                return this.loggingEvents.ToArray();
            }
        }

        /// <summary>Clears the internal cache of <see cref="MemoryLoggingEvent"/> objects.</summary>
        public void Clear()
        {
            lock (this.locker)
            {
                this.loggingEvents.Clear();
            }
        }

        /// <summary>Initializes the provider.</summary>
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
                config.Add("description", "Memory logging provider");
            }

            // Call initialize first.
            base.Initialize(name, config);

            // Performing implementation-specific provider initialization here.

            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">An optional source where the event occured.</param>
        /// <returns>
        /// The index of the inserted event in the internal cache.
        /// </returns>
        protected override object LogInternal(LoggingEventType severity, string message, Exception exception,
            string source)
        {
            MemoryLoggingEvent e = new MemoryLoggingEvent(severity, message, source, exception);

            lock (this.locker)
            {
                this.loggingEvents.Add(e);
                
                return this.loggingEvents.Count - 1;
            }
        }
    }
}
