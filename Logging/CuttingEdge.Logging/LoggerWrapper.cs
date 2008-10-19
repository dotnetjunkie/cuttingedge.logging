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

            string message = LoggingHelper.GetExceptionMessage(exception);
            return this.logger.Log(LoggingEventType.Error, message, null, exception);
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

            return this.logger.Log(LoggingEventType.Error, message, null, exception);
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

            string message = LoggingHelper.GetExceptionMessage(exception);
            string methodName = LoggingHelper.BuildMethodName(source);
            return this.logger.Log(LoggingEventType.Error, message, methodName, exception);
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
            return this.logger.Log(LoggingEventType.Error, message, methodName, exception);
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

            return this.logger.Log(LoggingEventType.Error, message, source, exception);
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
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, Exception exception, MethodBase source)
        {
            LoggingHelper.ValidateTypeInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);
            return this.logger.Log(severity, message, methodName, exception);
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
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, Exception exception, string source)
        {
            LoggingHelper.ValidateTypeInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            return this.logger.Log(severity, message, source, exception);
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

            return this.logger.Log(LoggingEventType.Information, message, null, null);
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
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message)
        {
            LoggingHelper.ValidateTypeInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            return this.logger.Log(severity, message, null, null);
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
            return this.logger.Log(LoggingEventType.Information, message, methodName, null);
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
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, MethodBase source)
        {
            LoggingHelper.ValidateTypeInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);
            return this.logger.Log(severity, message, methodName, null);
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
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public object Log(LoggingEventType severity, string message, string source)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);
            LoggingHelper.ValidateTypeInValidRange(severity);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            return this.logger.Log(severity, message, source, null);
        }

        /// <summary>Adds a event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The message of the event.</param>
        /// <param name="source">The optional source to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="message"/> is an empty string.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when<paramref name="type"/> has an invalid value.</exception>
        object ILogger.Log(LoggingEventType severity, string message, string source, Exception exception)
        {
            return this.logger.Log(severity, message, source, exception);
        }
    }
}
