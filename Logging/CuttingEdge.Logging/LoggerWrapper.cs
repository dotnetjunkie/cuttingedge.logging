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
using System.ComponentModel;
using System.Reflection;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Wraps an <see cref="ILogger"/> instance and adds convenient <b>Log</b> methods.
    /// </summary>
    /// <example>
    /// The <see cref="LoggerWrapper"/> can be used in situations where the <see cref="LoggingProviderBase"/>
    /// class isn't suitable. The folowing example shows a possible use of the <see cref="LoggerWrapper"/>
    /// class.
    /// <code>
    /// public class BusinessCommand
    /// {
    ///     public void Execute(ExecutionContext context, ILogger iLogger)
    ///     {
    ///         LoggerWrapper logger = new LoggerWrapper(iLogger);
    ///         logger.Log(LoggingEventType.Warning, "This is a warning.");
    ///     }
    /// }
    /// </code>
    /// </example>
    public class LoggerWrapper : ILogger
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerWrapper"/> class.
        /// </summary>
        /// <param name="logger">The wrapped logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is a null
        /// reference (Nothing in VB).</exception>
        public LoggerWrapper(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        /// <summary>Gets the wrapped logger.</summary>
        /// <value>The logger.</value>
        public ILogger Logger
        {
            get { return this.logger; }
        }

        /// <summary>Logs an error event to the default <see cref="Logger"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Throrn when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        public object Log(Exception exception)
        {
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            string message = LoggingHelper.GetExceptionMessageOrExceptionType(exception);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, null, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB) or the supplied <paramref name="message"/> is a null reference.</exception>
        public object Log(string message, Exception exception)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, null, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(Exception exception, MethodBase source)
        {
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string message = LoggingHelper.GetExceptionMessageOrExceptionType(exception);
            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, methodName, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
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

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, methodName, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, Exception exception, string source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, source, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is an
        /// empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, Exception exception, MethodBase source)
        {
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(severity, message, methodName, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/>,
        /// <paramref name="exception"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, Exception exception, string source)
        {
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            LogEntry entry = new LogEntry(severity, message, source, exception);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an information event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a 
        /// null reference.</exception>
        public object Log(string message)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            LogEntry entry = new LogEntry(LoggingEventType.Information, message, null, null);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message)
        {
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            LogEntry entry = new LogEntry(severity, message, null, null);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an information event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        public object Log(string message, MethodBase source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(LoggingEventType.Information, message, methodName, null);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, MethodBase source)
        {
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(severity, message, methodName, null);
            return this.logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, string source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            LogEntry entry = new LogEntry(severity, message, source, null);
            return this.logger.Log(entry);
        }

        /// <summary>Logs the specified entry to the wrapped <see cref="LoggerWrapper.Logger">Logger</see>.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>
        /// The id of the saved log (or null when an id is not appropriate for this type of logger.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="message"/> is an empty string.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when<paramref name="type"/> has an invalid value.</exception>
        object ILogger.Log(LogEntry entry)
        {
            return this.logger.Log(entry);
        }
    }
}
