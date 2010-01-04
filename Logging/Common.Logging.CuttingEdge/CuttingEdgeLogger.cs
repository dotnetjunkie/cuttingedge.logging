using System;
using System.ComponentModel;
using System.Diagnostics;

using Common.Logging.Factory;

using CuttingEdge.Logging;

namespace Common.Logging.CuttingEdge
{
    /// <summary>
    /// Concrete implementation of <see cref="ILog"/> interface specific to CuttingEdge.Logging.
    /// </summary>
    [DebuggerDisplay("CuttingEdgeLogger (Name: {provider.Name}, Threshold: {provider.threshold})")]
    public class CuttingEdgeLogger : AbstractLogger
    {
        private const string NullMessage = "No message supplied";

        private readonly LoggingProviderBase provider;

        /// <summary>Initializes a new instance of the <see cref="CuttingEdgeLogger"/> class.</summary>
        /// <param name="provider">The provider.</param>
        public CuttingEdgeLogger(LoggingProviderBase provider)
        {
            this.provider = provider;
        }

        /// <summary>Gets the name of the underlying logging provider.</summary>
        /// <value>The name of the underlying logging provider.</value>
        public string Name
        {
            get { return this.provider.Name; }
        }

        /// <summary>Checks if this logger is enabled for the <see cref="LogLevel.Debug"/> level.</summary>
        /// <value>A boolean indicating whether debug messages will be logged.</value>
        public override bool IsDebugEnabled
        {
            get { return this.provider.Threshold <= LoggingEventType.Debug; }
        }

        /// <summary>Checks if this logger is enabled for the <see cref="LogLevel.Error"/> level.</summary>
        /// <value>A boolean indicating whether error messages will be logged.</value>
        public override bool IsErrorEnabled
        {
            get { return this.provider.Threshold <= LoggingEventType.Error; }
        }

        /// <summary>Checks if this logger is enabled for the <see cref="LogLevel.Fatal"/> level.</summary>
        /// <value>A boolean indicating whether fatal messages will be logged.</value>
        public override bool IsFatalEnabled
        {
            get { return this.provider.Threshold <= LoggingEventType.Critical; }
        }

        /// <summary>Checks if this logger is enabled for the <see cref="LogLevel.Info"/> level.</summary>
        /// <value>A boolean indicating whether info messages will be logged.</value>
        public override bool IsInfoEnabled
        {
            get { return this.provider.Threshold <= LoggingEventType.Information; }
        }

        /// <summary>Checks if this logger is enabled for the <see cref="LogLevel.Trace"/> level.</summary>
        /// <value>A boolean indicating whether trace messages will be logged.</value>
        public override bool IsTraceEnabled
        {
            get { return this.provider.Threshold <= LoggingEventType.Debug; }
        }

        /// <summary>Checks if this logger is enabled for the <see cref="LogLevel.Warn"/> level.</summary>
        /// <value>A boolean indicating whether warn messages will be logged.</value>
        public override bool IsWarnEnabled
        {
            get { return this.provider.Threshold <= LoggingEventType.Warning; }
        }

        /// <summary>Actually sends the message to the underlying log system.</summary>
        /// <param name="level">The level of this log event.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log (may be null).</param>
        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            LogEntry entry = CreateLogEntry(level, message, exception);

            ILogger logger = this.provider;

            logger.Log(entry);
        }

        private static LogEntry CreateLogEntry(LogLevel level, object messageObject, Exception exception)
        {
            LoggingEventType eventType = ConvertToLoggingEventType(level);
            string message = CreateMessage(messageObject);

            return new LogEntry(eventType, message, null, exception);
        }

        private static LoggingEventType ConvertToLoggingEventType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Off:
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return LoggingEventType.Debug;

                case LogLevel.Warn:
                    return LoggingEventType.Warning;

                case LogLevel.Info:
                    return LoggingEventType.Information;

                case LogLevel.Error:
                    return LoggingEventType.Error;

                case LogLevel.Fatal:
                case LogLevel.All:
                    return LoggingEventType.Critical;

                default:
                    throw new InvalidEnumArgumentException("level", (int)level, typeof(LogLevel));
            }
        }

        private static string CreateMessage(object messageObject)
        {
            if (messageObject == null)
            {
                return NullMessage;
            }

            string message = messageObject.ToString();

            return string.IsNullOrEmpty(message) ? NullMessage : message;
        }
    }
}
