using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    /// <summary>
    /// A logging provider that logs the events in memory. The logged events can be accessed with the
    /// <see cref="LoggedEvents"/> property.
    /// </summary>
    /// <remarks>This type is not thread-safe.</remarks>
    internal sealed class InMemoryLogger : ILogger
    {
        private readonly List<LoggingEvent> loggedMessages = new List<LoggingEvent>();
        private ReadOnlyCollection<LoggingEvent> readonlyLoggedMessages;

        /// <summary>Gets the logged messages.</summary>
        /// <value>The logged messages.</value>
        public ReadOnlyCollection<LoggingEvent> LoggedEvents
        {
            get
            {
                if (this.readonlyLoggedMessages == null)
                {
                    this.readonlyLoggedMessages = 
                        new ReadOnlyCollection<LoggingEvent>(this.loggedMessages);
                }

                return this.readonlyLoggedMessages;
            }
        }

        /// <summary>Gets the first logged event or null when no event has been logged.</summary>
        /// <value>The first logged event.</value>
        public LoggingEvent FirstLoggedEvent
        {
            get
            {
                if (this.LoggedEvents.Count == 0)
                {
                    return null;
                }

                return this.LoggedEvents[0];
            }
        }

        /// <summary>Gets the last logged event or null when no event has been logged.</summary>
        /// <value>The last logged event.</value>
        public LoggingEvent LastLoggedEvent
        {
            get
            {
                if (this.LoggedEvents.Count == 0)
                {
                    return null;
                }

                return this.LoggedEvents[this.LoggedEvents.Count - 1];
            }
        }

        /// <summary>
        /// Adds a event to the log.
        /// </summary>
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
        public object Log(LoggingEventType type, string message, string source, Exception exception)
        {
            LoggingEvent loggingMessage = new LoggingEvent(type, message, source, exception);
            this.loggedMessages.Add(loggingMessage);
            return null;
        }
    }
}
