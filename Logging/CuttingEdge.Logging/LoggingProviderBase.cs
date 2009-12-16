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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Reflection;

namespace CuttingEdge.Logging
{
    // Code generated with .NET Junkie's provider template. 
    // source: http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=26

    /// <summary>
    /// Defines the contract that is implemented to provide Logging services using custom Logging providers. 
    /// </summary>
    /// <remarks>
    /// <para>Logging is designed to enable you to easily use a number of different Logging providers for your 
    /// applications. You can use the supplied Logging providers that are included with this assembly, or you 
    /// can implement your own provider.</para>
    /// <para>
    /// When implementing a custom Logging provider, you are required to inherit this abstract
    /// <b>LoggingProviderBase</b> class.</para>
    /// <para>
    /// The <b>LoggingProviderBase</b> abstract class inherits from the <see cref="ProviderBase"/> abstract 
    /// class. <b>LoggingProviderBase</b> implementations must also implement the required members of 
    /// <see cref="ProviderBase"/>.</para>
    /// <para>
    /// <b>Note: </b> Concrete implementations of this type should be thread-safe. The Logger services will
    /// instantiate a single instance of a configured type and the instance can be called from multiple
    /// threads. Also note that users are allowed to configure multiple multiple providers of the same type,
    /// even with exactly the same configuration. Implementations must be aware of this and need to take
    /// appropriate actions to ensure correctness.
    /// </para>
    /// </remarks>
    public abstract class LoggingProviderBase : ProviderBase, ILogger
    {
        private string fallbackProviderName;
        private LoggingEventType threshold;

        /// <summary>
        /// Gets the supplied fallback provider that the <see cref="Logger"/> class will use when logging
        /// failed on this logging provider. When no fallback provider is defined in the configuration file,
        /// <b>null</b> (Nothing in VB) is returned.
        /// </summary>
        /// <value>The fallback provider.</value>
        public LoggingProviderBase FallbackProvider { get; internal set; }

        /// <summary>
        /// Gets the <see cref="LoggingEventType"/> logging threshold. The threshold can be set through
        /// configuration and limits the number of event logged. The threshold can be defined as follows: 
        /// Debug &lt; Information &lt; Warning &lt; Error &lt; Fatal.
        /// i.e., When the threshold is set to 
        /// <see cref="LoggingEventType.Information">Information</see>, 
        /// <see cref="LoggingEventType.Debug">Debug</see> events will not be logged.
        /// </summary>
        /// <value>The threshold.</value>
        public LoggingEventType Threshold
        {
            get { return this.threshold; }
        }

        /// <summary>Gets the name of the fallback provider. This property is used by the <see cref="Logger"/>
        /// class to initialize the <see cref="FallbackProvider"/> property.</summary>
        internal string FallbackProviderName
        {
            get { return this.fallbackProviderName; }
        }

        /// <summary>Initializes the provider.</summary>
        /// <remarks>
        /// Inheritors should call <b>base.Initialize</b> before performing implementation-specific provider
        /// initialization and call <see cref="CheckForUnrecognizedAttributes"/> last.
        /// </remarks>
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

            if (string.IsNullOrEmpty(name))
            {
                name = this.GetType().Name;
            }

            // Perform the basic initialization.
            base.Initialize(name, config);

            // Perform feature-specific provider initialization.
            this.InitializeThreshold(config);

            this.InitializeFallbackProvider(config);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        public object Log(Exception exception)
        {
            return LoggerExtensions.Log(this, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB) or the supplied <paramref name="message"/> is a null reference.</exception>
        public object Log(string message, Exception exception)
        {
            return LoggerExtensions.Log(this, message, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(Exception exception, MethodBase source)
        {
            return LoggerExtensions.Log(this, exception, source);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, Exception exception, MethodBase source)
        {
            return LoggerExtensions.Log(this, message, exception, source);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, Exception exception, string source)
        {
            return LoggerExtensions.Log(this, message, exception, source);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is an
        /// empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, Exception exception, MethodBase source)
        {
            return LoggerExtensions.Log(this, severity, message, exception, source);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/>,
        /// <paramref name="exception"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, Exception exception, string source)
        {
            return LoggerExtensions.Log(this, severity, message, exception, source);
        }

        /// <summary>Logs an information event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a 
        /// null reference.</exception>
        public object Log(string message)
        {
            return LoggerExtensions.Log(this, message);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message)
        {
            return LoggerExtensions.Log(this, severity, message);
        }

        /// <summary>Logs an information event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, MethodBase source)
        {
            return LoggerExtensions.Log(this, message, source);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, MethodBase source)
        {
            return LoggerExtensions.Log(this, severity, message, source);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, string source)
        {
            return LoggerExtensions.Log(this, severity, message, source);
        }

        /// <summary>Adds a event to the log. When logging fails, the event is forwarded to the
        /// <see cref="FallbackProvider"/>, if any.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current <see cref="Threshold"/> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="entry"/> is a null reference.</exception>
        object ILogger.Log(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            // Only log when the event is equal to or above the specified threshold.
            if (entry.Severity < this.threshold)
            {
                return null;
            }

            try
            {
                return this.LogInternal(entry);
            }
            catch (Exception ex)
            {
                // When logging failed and there is no fallback provider, we'll throw the caught exception.
                if (this.FallbackProvider == null)
                {
                    throw;
                }

                this.LogToFallbackProvider(entry);

                this.LogProviderFailureToFallbackProvider(ex);

                // We return null, because the fallback provider returns a different type of id, then the
                // user would expect.
                return null;
            }
        }

        internal virtual void CompleteInitialization(LoggingProviderCollection configuredProviders,
            LoggingProviderBase defaultProvider)
        {
            // Default implementation is empty.
        }

        /// <summary>
        /// Returns a list of providers that this provider is referencing. Normally this method returns one
        /// single element when the provider contains a <see cref="FallbackProvider"/> and no elements when
        /// there is no <see cref="FallbackProvider"/>. Other implementations however, can override this
        /// implementation. The <see cref="CompositeLoggingProvider"/> does this. The <see cref="Logger"/>
        /// class uses this information to detect circular references during initialization.
        /// </summary>
        /// <returns>A list of referenced <see cref="LoggingProviderBase"/> instances.</returns>
        internal virtual List<LoggingProviderBase> GetReferencedProviders()
        {
            var referencedProviders = new List<LoggingProviderBase>();

            if (this.FallbackProvider != null)
            {
                referencedProviders.Add(this.FallbackProvider);
            }

            return referencedProviders;
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <remarks>Implementations of this method must guarantee it to be thread safe.</remarks>
        /// <param name="entry">The entry to log.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate.</returns>
        protected abstract object LogInternal(LogEntry entry);

        /// <summary>Checks for unrecognized attributes and throws an <see cref="ProviderException"/> when
        /// the <paramref name="config"/> contains values.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific 
        /// attributes specified in the configuration for this provider.</param>
        /// <exception cref="ProviderException">Thrown when the <paramref name="config"/> collection is not empty.</exception>
        protected void CheckForUnrecognizedAttributes(string name, NameValueCollection config)
        {
            // The config argument should contain no elements.
            if (config != null && config.Count > 0)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = this.GetType().Name;
                }

                string attribute = config.GetKey(0);

                throw new ProviderException(SR.UnrecognizedAttributeInProviderConfiguration(name, attribute));
            }
        }

        private void InitializeThreshold(NameValueCollection config)
        {
            const string ThresholdAttribute = "threshold";

            string thresholdConfigValue = config[ThresholdAttribute];

            // The threshold attribute is optional.
            if (!string.IsNullOrEmpty(thresholdConfigValue))
            {
                this.threshold = this.ConvertToLoggingEventType(thresholdConfigValue);
            }
            else
            {
                // When no threshold is specified, we set 'Debug' as threshold.
                this.threshold = LoggingEventType.Debug;
            }

            // Remove this attribute from the configuration. This way the provider can spot unrecognized 
            // attributes after the initialization process.
            config.Remove(ThresholdAttribute);
        }

        private LoggingEventType ConvertToLoggingEventType(string value)
        {
            try
            {
                const bool IgnoreCase = true;

                // .NET 2.0 does not have a convenient Enum.TryParse method.
                return (LoggingEventType)Enum.Parse(typeof(LoggingEventType), value, IgnoreCase);
            }
            catch (ArgumentException)
            {
                throw new ProviderException(SR.InvalidThresholdValueInProviderConfiguration(this.Name));
            }
        }

        private void InitializeFallbackProvider(NameValueCollection config)
        {
            string fallbackProviderName = config["fallbackProvider"];

            // Remove this attribute from the configuration. This way the provider can spot unrecognized 
            // attributes after the initialization process.
            config.Remove("fallbackProvider");

            if (!String.IsNullOrEmpty(fallbackProviderName))
            {
                this.fallbackProviderName = fallbackProviderName;
            }

            // We don't throw an exception when there is no provider, because this attribute is optional.
        }

        private void LogToFallbackProvider(LogEntry entry)
        {
            ILogger fallbackProvider = this.FallbackProvider;
            fallbackProvider.Log(entry);
        }

        private void LogProviderFailureToFallbackProvider(Exception exception)
        {
            string failureMessage = SR.EventCouldNotBeLoggedWithX(this.Name);

            string source = this.GetType().FullName;

            var entry = new LogEntry(LoggingEventType.Error, failureMessage, source, exception);

            ILogger fallbackProvider = this.FallbackProvider;

            fallbackProvider.Log(entry);
        }
    }
}