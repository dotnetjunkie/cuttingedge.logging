using System;
using System.Collections.Generic;
using System.Text;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    /// <summary>
    /// Allows the default implementation of the <see cref="UnitTestingLoggingProvider"/> to use the
    /// supplied <typeparamref name="TLogger"/> in the context of the current thread.
    /// </summary>
    /// <typeparam name="TLogger">The type of logger that will be created and registered.</typeparam>
    internal sealed class LoggingProviderTestingScope<TLogger> : IDisposable
        where TLogger : class, ILogger, new()
    {
        private TLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingProviderTestingScope"/> class.
        /// </summary>
        public LoggingProviderTestingScope()
        {
            this.logger = new TLogger();

            UnitTestingLoggingProvider.RegisterThreadStaticLogger(this.logger);
        }

        /// <summary>Gets the logger.</summary>
        /// <value>The logger.</value>
        public TLogger Logger
        {
            get { return this.logger; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged 
        /// resources.
        /// </summary>
        public void Dispose()
        {
            if (this.logger != null)
            {
                UnitTestingLoggingProvider.UnregisterThreadStaticLogger(this.logger);
                this.logger = null;
            }
        }
    }
}
