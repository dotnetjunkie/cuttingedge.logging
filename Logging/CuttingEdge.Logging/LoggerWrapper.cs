using System;
using System.ComponentModel;
using System.Reflection;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Wraps an <see cref="ILogger"/> instance and adds convenient <b>Log</b> methods.
    /// </summary>
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
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentNullException">Throrn when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        public object Log(Exception exception)
        {
            LoggingHelper.ValidateExceptionIsNotNull(exception);

            string message = LoggingHelper.GetExceptionMessage(exception);
            return this.logger.Log(EventType.Error, message, null, exception);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
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

            return this.logger.Log(EventType.Error, message, null, exception);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
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
            return this.logger.Log(EventType.Error, message, methodName, exception);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
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
            return this.logger.Log(EventType.Error, message, methodName, exception);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
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

            return this.logger.Log(EventType.Error, message, source, exception);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
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
            return this.logger.Log(type, message, methodName, exception);
        }

        /// <summary>Logs an error event to the wrapped <see cref="Logger"/>.</summary>
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

            return this.logger.Log(type, message, source, exception);
        }

        /// <summary>Logs an information event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging this.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a 
        /// null reference.</exception>
        public object Log(string message)
        {
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            return this.logger.Log(EventType.Information, message, null, null);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
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

            return this.logger.Log(type, message, null, null);
        }

        /// <summary>Logs an information event to the wrapped <see cref="Logger"/>.</summary>
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
            return this.logger.Log(EventType.Information, message, methodName, null);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
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
            return this.logger.Log(type, message, methodName, null);
        }

        /// <summary>Logs an event to the wrapped <see cref="Logger"/>.</summary>
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

            return this.logger.Log(type, message, source, null);
        }

        /// <summary>Adds a event to the wrapped <see cref="Logger"/>.</summary>
        /// <param name="type">The type of event to log.</param>
        /// <param name="message">The message of the event.</param>
        /// <param name="source">The optional source to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <returns>
        /// The id of the saved log (or null when an id is not appropriate for this type of logger.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="message"/> is an empty string.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when<paramref name="type"/> has an invalid value.</exception>
        object ILogger.Log(EventType type, string message, string source, Exception exception)
        {
            return this.logger.Log(type, message, source, exception);
        }
    }
}
