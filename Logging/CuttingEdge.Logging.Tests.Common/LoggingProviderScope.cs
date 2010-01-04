using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Common
{
    public enum ScopeOption
    {
        /// <summary>Default option. Does not do any checking.</summary>
        None = 0,

        /// <summary>Tells the LoggingProviderScope that it should fail when a second LogEntry is logged.</summary>
        AllowOnlyASingleEntryToBeLogged = 1,
    }

    /// <summary>
    /// Allows the default implementation of the <see cref="UnitTestingLoggingProvider"/> to use the
    /// supplied <typeparamref name="TLogger"/> in the context of the current thread.
    /// </summary>
    /// <typeparam name="TLogger">The type of logger that will be created and registered.</typeparam>
    public sealed class LoggingProviderScope : IDisposable
    {
        private readonly ReadOnlyCollection<LogEntry> loggedEntries;
        private readonly ScopeOption option;

        /// <summary>Initializes a new instance of the <see cref="LoggingProviderScope"/> class.</summary>
        /// <param name="option">The scope option.</param>
        public LoggingProviderScope(ScopeOption option)
            : this()
        {
            this.option = option;
        }

        /// <summary>Initializes a new instance of the <see cref="LoggingProviderScope"/> class.</summary>
        public LoggingProviderScope()
        {
            var entries = new List<LogEntry>();
            this.loggedEntries = new ReadOnlyCollection<LogEntry>(entries);
            var logger = new ActionLoggingProvider((entry) =>
            {
                entries.Add(entry);
                ValidateEntries(entries);
            });

            UnitTestingLoggingProvider.RegisterThreadStaticLogger(logger);
        }

        /// <summary>Gets the collection of logged entries.</summary>
        /// <value>The read only collection of logged entries.</value>
        public ReadOnlyCollection<LogEntry> LoggedEntries
        {
            get { return this.loggedEntries; }
        }

        /// <summary>
        /// Gets the provider that is set up in the configuration file that can be used to log to.
        /// </summary>
        /// <returns>The <see cref="LoggingProviderBase"/> that can be used to log to in the context of the
        /// LoggingProviderScope.</returns>
        public static LoggingProviderBase GetProviderFromConfiguration()
        {
            var provider = Logger.Providers.OfType<UnitTestingLoggingProvider>().FirstOrDefault();

            if (provider == null)
            {
                Assert.Fail("There is no UnitTestingLoggingProvider configured.");
            }

            return provider;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged 
        /// resources.
        /// </summary>
        public void Dispose()
        {
            UnitTestingLoggingProvider.UnregisterThreadStaticLogger();
        }

        private void ValidateEntries(List<LogEntry> entries)
        {
            if (this.option == ScopeOption.AllowOnlyASingleEntryToBeLogged)
            {
                if (entries.Count >= 2)
                {
                    Assert.Fail("A second LogEntry was logged, while only one entry was expected. " +
                        "LogEntry: " + entries.Last().ToString());
                }
            }
        }

        /// <summary>An logging provider that calls the supplied delegate on logging.</summary>
        private sealed class ActionLoggingProvider : LoggingProviderBase
        {
            private readonly Action<LogEntry> log;

            internal ActionLoggingProvider(Action<LogEntry> log)
            {
                this.log = log;
            }

            protected override object LogInternal(LogEntry entry)
            {
                this.log(entry);
                return null;
            }
        }
    }
}
