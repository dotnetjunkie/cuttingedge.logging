using System.Reflection;

using CuttingEdge.Logging.Tests.Common;

using NSandbox;

namespace Common.Logging.CuttingEdge.Tests.Unit
{
    /// <summary>
    /// This class starts remote domain and loads the needed assemblies.
    /// </summary>
    public sealed class CommonLoggingTestLoggingAppDomainManager 
        : LoggingAppDomainManager<CommonLoggingTestingRemoteSandbox>
    {
        public CommonLoggingTestLoggingAppDomainManager(IConfigurationWriter configuration)
            : base(configuration)
        {
        }

        protected override LocalSandbox<CommonLoggingTestingRemoteSandbox> CreateLocalSandbox(string name, 
            IConfigurationWriter configuration)
        {
            var localSandbox = base.CreateLocalSandbox(name, configuration);

            localSandbox.DependentFiles.Add("Common.Logging.dll");
            localSandbox.DependentFiles.Add("Common.Logging.CuttingEdge.dll");

            string currentAssemblyName = MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name;

            localSandbox.DependentFiles.Add(currentAssemblyName + ".dll");
            
            return localSandbox;
        }
    }
}
