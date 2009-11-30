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
        public IntegrationTestLoggingAppDomainManager(IConfigurationWriter configuration)
            : base(configuration, ThisTestAssemblyFileName)
        {
            this.DomainUnderTest.BeginTransactionScope();
        }

        private static string ThisTestAssemblyFileName
        {
            get
            {
                string currentAssemblyName =
                    MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

                return currentAssemblyName + ".dll";
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.DomainUnderTest.DisposeTransactionScope();

            base.Dispose(disposing);
        }
    }
}
