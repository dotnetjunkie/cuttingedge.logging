using System.Transactions;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Integration
{
    // We use NSandbox: http://thevalerios.net/matt/2008/10/nsandbox-an-introduction/
    public sealed class IntegrationTestingRemoteSandbox : RemoteSandboxBase
    {
        private TransactionScope transaction;

        public void InitializeLoggingSystem()
        {
            // Calling Logger.Provider starts the initialization of the logging provider system.
            LoggingProviderBase provider = Logger.Provider;
        }

        public void Log(LogEntry entry)
        {
            ILogger logger = Logger.Provider;
            logger.Log(entry);
        }

        public void BeginTransactionScope()
        {
            this.transaction = new TransactionScope();
        }

        public void DisposeTransactionScope()
        {
            this.transaction.Dispose();
        }
    }
}
