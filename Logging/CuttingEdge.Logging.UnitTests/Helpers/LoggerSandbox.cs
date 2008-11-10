using NSandbox;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    // We use NSandbox: http://thevalerios.net/matt/2008/10/nsandbox-an-introduction/
    public class LoggerSandbox : RemoteSandboxBase
    {
        public void Initialize()
        {
            LoggingProviderBase provider = Logger.Provider;
        }
    }
}