using System;
using System.Reflection;
using System.Threading;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Common
{
    /// <summary>
    /// Manager for creating a <see cref="LoggerSandbox"/> using the <b>NSandbox</b> infrastructure.
    /// </summary>
    /// <typeparam name="TRemoteSandbox">The type of the remote sandbox.</typeparam>
    public abstract class LoggingAppDomainManager<TRemoteSandbox> : IDisposable
        where TRemoteSandbox : RemoteSandboxBase
    {
        private readonly LocalSandbox<TRemoteSandbox> sandbox;
        private static int id;
        private SandboxManager sandboxManager;

        protected LoggingAppDomainManager(IConfigurationWriter configuration, string testAssemblyFileName)
        {
            string name = "sandbox" + GetNextId();

            this.sandbox = CreateLocalSandbox(name, configuration, testAssemblyFileName);

            this.sandboxManager = new SandboxManager();

            this.sandboxManager.Sandboxes.Add(this.sandbox);

            this.sandboxManager.SetupAllSandboxes();
        }

        public TRemoteSandbox DomainUnderTest
        {
            get { return this.sandbox.RemoteSandbox.As<TRemoteSandbox>(); }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            SandboxManager manager = this.sandboxManager;

            if (manager != null)
            {
                // Prevent TeardownAllSandboxes from being called twice.
                this.sandboxManager = null;

                manager.TeardownAllSandboxes();
            }
        }

        private static LocalSandbox<TRemoteSandbox> CreateLocalSandbox(string name,
            IConfigurationWriter configuration, string testAssemblyFileName)
        {
            LocalSandbox<TRemoteSandbox> localSandbox =
                new LocalSandbox<TRemoteSandbox>(name, configuration);

            string currentAssemblyName = MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

            localSandbox.DependentFiles.Add("CuttingEdge.Logging.dll");
            localSandbox.DependentFiles.Add(currentAssemblyName + ".dll");
            localSandbox.DependentFiles.Add(testAssemblyFileName);
            localSandbox.DependentFiles.Add("NSandbox.dll");

            return localSandbox;
        }

        private static int GetNextId()
        {
            return Interlocked.Increment(ref id);
        }
    }
}
