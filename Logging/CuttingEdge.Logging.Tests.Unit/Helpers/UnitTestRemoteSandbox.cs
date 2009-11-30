using NSandbox;

namespace CuttingEdge.Logging.Tests.Unit.Helpers
{
    // We use NSandbox: http://thevalerios.net/matt/2008/10/nsandbox-an-introduction/
    public sealed class UnitTestRemoteSandbox : RemoteSandboxBase
    {
        public void InitializeLoggingSystem()
        {
            // Calling Logger.Provider starts the initialization of the logging provider system.
            LoggingProviderBase provider = Logger.Provider;
        }
    }
}
