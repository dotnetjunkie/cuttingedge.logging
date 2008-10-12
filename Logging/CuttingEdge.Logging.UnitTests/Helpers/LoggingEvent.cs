using System;
using System.Collections.Generic;
using System.Text;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    /// <summary>A logging event.</summary>
    internal sealed class LoggingEvent
    {
        internal LoggingEvent(EventType type, string message, string source, Exception exception)
        {
            this.Type = type;
            this.Message = message;
            this.Source = source;
            this.Exception = exception;
        }

        internal EventType Type { get; private set; }

        internal string Message { get; private set; }

        internal string Source { get; private set; }

        internal Exception Exception { get; private set; }
    }
}
