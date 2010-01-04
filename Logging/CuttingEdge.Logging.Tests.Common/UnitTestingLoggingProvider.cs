using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuttingEdge.Logging.Tests.Common
{
    /// <summary>
    /// Logging provider for unit tests.
    /// </summary>
    internal sealed class UnitTestingLoggingProvider : LoggingProviderBase
    {
        [ThreadStatic]
        private static ILogger internalThreadStaticLogger;

        public static void RegisterThreadStaticLogger(ILogger logger)
        {
            internalThreadStaticLogger = logger;
        }

        public static void UnregisterThreadStaticLogger()
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
