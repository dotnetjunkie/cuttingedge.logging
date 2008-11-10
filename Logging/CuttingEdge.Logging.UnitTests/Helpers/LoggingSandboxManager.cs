using System;
using System.Threading;

using NSandbox;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    /// <summary>
    /// Manager for creating a <see cref="LoggerSandbox"/> using the <b>NSandbox</b> infrastructure.
    /// </summary>
    public sealed class LoggingSandboxManager : IDisposable
    {
        private static int id;
        private SandboxManager sandboxManager;
        private LocalSandbox<LoggerSandbox> sandbox;

        public LoggingSandboxManager(IConfigurationWriter configuration) // LocalSandbox<T> sandbox
            : this(null, null, configuration)
        {
        }

        private LoggingSandboxManager(string rootDirectory, string name, IConfigurationWriter configuration)
        {
            if (name == null)
            {
                name = "sandbox" + GetNextId();
            }

            this.sandbox = CreateLocalSandbox(name, configuration);

            if (rootDirectory == null)
            {
                this.sandboxManager = new SandboxManager();
            }
            else
            {
                this.sandboxManager = new SandboxManager(rootDirectory);
            }

            this.sandboxManager.Sandboxes.Add(this.sandbox);

            this.sandboxManager.SetupAllSandboxes();
        }

        public string RootDirectory
        {
            get { return this.sandboxManager.RootDirectory; }
        }

        public LoggerSandbox Logger
        {
            get { return this.sandbox.RemoteSandbox.As<LoggerSandbox>(); }
        }

        public void Dispose()
        {
            SandboxManager manager = this.sandboxManager;

            if (manager != null)
            {
                this.sandboxManager = null;

                manager.TeardownAllSandboxes();
            }
        }

        private static LocalSandbox<LoggerSandbox> CreateLocalSandbox(string name, IConfigurationWriter configuration)
        {
            LocalSandbox<LoggerSandbox> localSandbox =
                new LocalSandbox<LoggerSandbox>(name, configuration);

            localSandbox.DependentFiles.Add("CuttingEdge.Logging.dll");
            localSandbox.DependentFiles.Add("CuttingEdge.Logging.UnitTests.dll");
            localSandbox.DependentFiles.Add("NSandbox.dll");

            return localSandbox;
        }

        private static int GetNextId()
        {
            return Interlocked.Increment(ref id);
        }
    }
}
