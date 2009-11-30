using System.Reflection;

using CuttingEdge.Logging.Tests.Common;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Unit.Helpers
{
    public sealed class UnitTestAppDomainManager : LoggingAppDomainManager<UnitTestRemoteSandbox>
    {
        public UnitTestAppDomainManager(IConfigurationWriter configuration)
            : base(configuration, ThisTestAssemblyFileName)
        {
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
    }
}
