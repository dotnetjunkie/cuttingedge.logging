﻿#region Copyright (c) 2008 S. van Deursen
/* The CuttingEdge.Logging library allows developers to plug a logging mechanism into their web- and desktop
 * applications.
 * 
 * Copyright (C) 2008 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ 
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
using System.Globalization;
using System.Reflection;

namespace CuttingEdge.Logging
{
    // Code generated with the provider template. 
    // source: http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=26

    /// <summary>
    /// Defines the contract that is implemented to provide Logging services using custom Logging providers. 
    /// </summary>
    /// <remarks>
    /// <p>Logging is designed to enable you to easily use a number of different Logging providers for your 
    /// applications. You can use the supplied Logging providers that are included with this assembly, or you 
    /// can implement your own provider.</p>
    /// <p>
    /// When implementing a custom Logging provider, you are required to inherit this LoggingProvider abstract
    /// class.</p>
    /// <p>
    /// The <b>LoggingProviderBase</b> abstract class inherits from the <see cref="ProviderBase"/> abstract 
    /// class. <b>LoggingProviderBase</b> implementations must also implement the required members of 
    /// <see cref="ProviderBase"/>.</p>
    /// </remarks>
    public abstract class LoggingProviderBase : ProviderBase, ILogger
    {
        private LoggingProviderBase fallbackProvider;
        private string fallbackProviderName;

        /// <summary>
        /// Gets the supplied fallback provider that the <see cref="Logger"/> class will use when logging
        /// failed on this logging provider. When no fallback provider is defined in the config file,
        /// <b>null</b> (Nothing in VB) is returned.
        /// </summary>
        /// <value>The fallback provider.</value>
        public LoggingProviderBase FallbackProvider
        {
            get
            {
                return this.fallbackProvider;
            }

            internal set
            {
                if (this.fallbackProvider != null)
                {
                    throw new InvalidOperationException();
                }

                this.fallbackProvider = value;
            }
        }

        internal string FallbackProviderName
        {
            get { return this.fallbackProviderName; }
        }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific 
        /// attributes specified in the configuration for this provider.</param>
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

            // Let ProviderBase perform the basic initialization
            base.Initialize(name, config);

            this.InitializeFallbackProvider(config);

            CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentNullException">Throrn when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        public object Log(Exception exception)
        {
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            string message = LoggingHelper.GetExceptionMessage(exception);
            return ((ILogger)this).Log(EventType.Error, message, null, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB) or the supplied <paramref name="message"/> is a null reference.</exception>
        public object Log(string message, Exception exception)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            return ((ILogger)this).Log(EventType.Error, message, null, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(Exception exception, MethodBase source)
        {
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string message = LoggingHelper.GetExceptionMessage(exception);
            string methodName = LoggingHelper.BuildMethodName(source);
            return ((ILogger)this).Log(EventType.Error, message, methodName, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, Exception exception, MethodBase source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);
            return ((ILogger)this).Log(EventType.Error, message, methodName, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, Exception exception, string source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            return ((ILogger)this).Log(EventType.Error, message, source, exception);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is an
        /// empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(EventType type, string message, Exception exception, MethodBase source)
        {
            LoggingHelper.ValidateTypeInValidRange(type);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);
            return ((ILogger)this).Log(type, message, methodName, exception);
        }

        /// <summary>Logs an error event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/>,
        /// <paramref name="exception"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(EventType type, string message, Exception exception, string source)
        {
            LoggingHelper.ValidateTypeInValidRange(type);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            return ((ILogger)this).Log(type, message, source, exception);
        }

        /// <summary>Logs an information event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a 
        /// null reference.</exception>
        public object Log(string message)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            return ((ILogger)this).Log(EventType.Information, message, null, null);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(EventType type, string message)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateTypeInValidRange(type);

            return ((ILogger)this).Log(type, message, null, null);
        }

        /// <summary>Logs an information event.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, MethodBase source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);
            return ((ILogger)this).Log(EventType.Information, message, methodName, null);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(EventType type, string message, MethodBase source)
        {
            LoggingHelper.ValidateTypeInValidRange(type);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);
            return ((ILogger)this).Log(type, message, methodName, null);
        }

        /// <summary>Logs an event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(EventType type, string message, string source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateTypeInValidRange(type);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            return ((ILogger)this).Log(type, message, source, null);
        }

        /// <summary>Adds a event to the log. When logging fails, the event is forwarded to the
        /// <see cref="FallbackProvider"/>, if any.</summary>
        /// <param name="type">The type of event to log.</param>
        /// <param name="message">The message of the event.</param>
        /// <param name="source">The optional source to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <returns>
        /// The id of the saved log (or null when an id is not appropriate for this type of logger).
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="message"/> is an empty string.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when<paramref name="type"/> has an invalid value.</exception>
        object ILogger.Log(EventType type, string message, string source, Exception exception)
        {
            LoggingHelper.ValidateTypeInValidRange(type);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            try
            {
                return this.LogInternal(type, message, exception, source);
            }
            catch (Exception ex)
            {
                // When logging failed and there is no fallback provider, we'll throw the caught exception.
                if (this.FallbackProvider == null)
                {
                    throw;
                }

                // When there is a fallback provider, we let the fallback provider log the event.
                this.LogToFallbackProvider(type, message, source, exception);

                // We also log the failure of the current provider
                this.LogProviderFailureToFallbackProvider(ex);

                // We return null, because the fallback provider returns a different type of id, then the
                // user would expect.
                return null;
            }
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">An optional source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate.</returns>
        protected abstract object LogInternal(EventType type, string message, Exception exception, 
            string source);

        private static void CheckForUnrecognizedAttributes(string name, NameValueCollection config)
        {
            // The config argument should contain no elements.
            if (config.Count > 0)
            {
                throw new ProviderException(SR.GetString(SR.UnrecognizedAttributeInProviderConfiguration,
                    name, config.GetKey(0)));
            }
        }

        private void InitializeFallbackProvider(NameValueCollection config)
        {
            string fallbackProviderName = config["fallbackProvider"];

            config.Remove("fallbackProvider");

            if (!String.IsNullOrEmpty(fallbackProviderName))
            {
                this.fallbackProviderName = fallbackProviderName;
            }

            // We don't throw an exception when there is no provider, because this attribute is optional.
        }

        private void LogToFallbackProvider(EventType type, string message, string source, Exception exception)
        {
            ILogger fallbackProvider = this.FallbackProvider;
            fallbackProvider.Log(type, message, source, exception);
        }

        private void LogProviderFailureToFallbackProvider(Exception exception)
        {
            string failureMessage = SR.GetString(SR.EventCouldNotBeLoggedWithX, this.GetType().Name);
            ILogger fallbackProvider = this.FallbackProvider;
            fallbackProvider.Log(EventType.Error, failureMessage, this.GetType().FullName, exception);
        }
    }
}