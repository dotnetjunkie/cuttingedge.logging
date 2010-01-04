using System.Reflection;

using CuttingEdge.Logging.Tests.Common;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Unit.Helpers
{
    public sealed class UnitTestAppDomainManager : LoggingAppDomainManager<UnitTestRemoteSandbox>
    {
        public UnitTestAppDomainManager(IConfigurationWriter configuration) : base(configuration)
        {
        }

        protected override LocalSandbox<UnitTestRemoteSandbox> CreateLocalSandbox(string name, 
            IConfigurationWriter configuration)
        {
            var localSandbox = base.CreateLocalSandbox(name, configuration);

            string currentAssemblyName = MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

            localSandbox.DependentFiles.Add(currentAssemblyName + ".dll");

            return localSandbox;
        }
    }
}
