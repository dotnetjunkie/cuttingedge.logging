using System;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    /// <summary>
    /// Logging provider for unit tests.
    /// </summary>
    internal sealed class UnitTestingLoggingProvider : LoggingProviderBase
    {
        [ThreadStatic]
        private static ILogger internalThreadStaticLogger;

        internal static void RegisterThreadStaticLogger(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            internalThreadStaticLogger = logger;
        }

        internal static void UnregisterThreadStaticLogger(ILogger logger)
        {
            if (internalThreadStaticLogger != logger)
            {
                throw new InvalidOperationException(
                    "The supplied provider doesn't match the registered provider.");
            }

            internalThreadStaticLogger = null;
        }

        /// <summary>
        /// Implements the functionality to log the event.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>
        /// The id of the logged event or null when an id is inappropriate.
        /// </returns>
        protected override object LogInternal(LogEntry entry)
        {
            if (internalThreadStaticLogger != null)
            {
                return internalThreadStaticLogger.Log(entry);
            }

            return null;
        }
    }
}
