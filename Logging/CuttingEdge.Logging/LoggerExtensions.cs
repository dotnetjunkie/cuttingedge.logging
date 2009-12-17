using System;
using System.ComponentModel;
using System.Reflection;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Extension methods for the <see cref="ILogger"/> interface.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>Logs an error event to the default <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB) or the supplied <paramref name="logger"/> is a null reference.
        /// </exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, Exception exception)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            string message = LoggingHelper.GetExceptionMessageOrExceptionType(exception);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, null, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="exception"/>,
        /// the <paramref name="message"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, string message, Exception exception)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, null, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="exception"/>,
        /// the <paramref name="source"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, Exception exception, MethodBase source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string message = LoggingHelper.GetExceptionMessageOrExceptionType(exception);
            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, methodName, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="exception"/>, the <paramref name="message"/>,
        /// <paramref name="source"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, string message, Exception exception, MethodBase source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, methodName, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="exception"/>, the <paramref name="message"/>,
        /// <paramref name="source"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, string message, Exception exception, string source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            LogEntry entry = new LogEntry(LoggingEventType.Error, message, source, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is an
        /// empty string.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="exception"/>, the <paramref name="message"/>,
        /// <paramref name="source"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>        
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, LoggingEventType severity, string message, 
            Exception exception, MethodBase source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(severity, message, methodName, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="message"/>, <paramref name="exception"/>, 
        /// <paramref name="source"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, LoggingEventType severity, string message, 
            Exception exception, string source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateExceptionIsNotNull(exception);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            LogEntry entry = new LogEntry(severity, message, source, exception);
            return logger.Log(entry);
        }

        /// <summary>Logs an information event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="logger"/> are null references.</exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, string message)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);

            LogEntry entry = new LogEntry(LoggingEventType.Information, message, null, null);
            return logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="message"/> or <paramref name="logger"/> are null 
        /// references.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, LoggingEventType severity, string message)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateSeverityInValidRange(severity);

            LogEntry entry = new LogEntry(severity, message, null, null);
            return logger.Log(entry);
        }

        /// <summary>Logs an information event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/>,
        /// <paramref name="source"/> or <paramref name="logger"/> are null references (Nothing in VB).
        /// </exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, string message, MethodBase source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(LoggingEventType.Information, message, methodName, null);
            return logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="message"/>, <paramref name="source"/> or 
        /// <paramref name="logger"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, LoggingEventType severity, string message, 
            MethodBase source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateSourceIsNotNull(source);

            string methodName = LoggingHelper.BuildMethodName(source);

            LogEntry entry = new LogEntry(severity, message, methodName, null);
            return logger.Log(entry);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the supplied <paramref name="message"/>, <paramref name="source"/> or
        /// <paramref name="logger"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="severity"/>
        /// has an unexpected value.</exception>
        /// <exception cref="Exception">Thrown when the logging provider failed to log the event. The 
        /// exact type of exception thrown depends on the actual provider implementation. See documentation
        /// of the <see cref="LoggingProviderBase.LogInternal">LogInternal</see> method of the used logging
        /// provider for more information.</exception>
        public static object Log(this ILogger logger, LoggingEventType severity, string message, string source)
        {
            LoggingHelper.ValideLoggerIsNotNull(logger);
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateSourceNotNullOrEmpty(source);

            LogEntry entry = new LogEntry(severity, message, source, null);
            return logger.Log(entry);
        }
    }
}
