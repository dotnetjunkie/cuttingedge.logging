using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.Helpers
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
            internalThreadStaticLogger = logger;
        }

        internal static void UnregisterThreadStaticLogger()
        {
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
