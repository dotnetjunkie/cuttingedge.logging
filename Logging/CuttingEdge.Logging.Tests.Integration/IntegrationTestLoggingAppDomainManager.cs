using System.Reflection;

using CuttingEdge.Logging.Tests.Common;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Integration
{
    /// <summary>
    /// This class starts an transaction in the remote domain and rolls it back during dispose.
    /// </summary>
    public sealed class IntegrationTestLoggingAppDomainManager 
        : LoggingAppDomainManager<IntegrationTestingRemoteSandbox>
    {
        public IntegrationTestLoggingAppDomainManager(IConfigurationWriter configuration) : base(configuration)
        {
            this.DomainUnderTest.BeginTransactionScope();
        }

        protected override LocalSandbox<IntegrationTestingRemoteSandbox> CreateLocalSandbox(string name, 
            IConfigurationWriter configuration)
        {
            var localSandbox = base.CreateLocalSandbox(name, configuration);

            string currentAssemblyName = MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

            localSandbox.DependentFiles.Add(currentAssemblyName + ".dll");

            return localSandbox;
        }

        protected override void Dispose(bool disposing)
        {
            this.DomainUnderTest.DisposeTransactionScope();

            base.Dispose(disposing);
        }
    }
}
