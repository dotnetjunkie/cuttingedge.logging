using System;
using System.Collections.Generic;
using System.Text;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    internal class StubLoggingProvider : LoggingProviderBase
    {
        protected override object LogInternal(EventType type, string message, string source, 
            Exception exception)
        {
            return null;
        }
    }
}
