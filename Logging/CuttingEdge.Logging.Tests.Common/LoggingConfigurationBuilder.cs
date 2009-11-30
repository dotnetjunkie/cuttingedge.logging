using System.Collections.ObjectModel;
using System.Linq;

namespace CuttingEdge.Logging.Tests.Common
{
    public sealed class LoggingConfigurationBuilder
    {
        private Collection<ProviderConfigLine> configurationLines = new Collection<ProviderConfigLine>();

        public string DefaultProvider { get; set; }

        public Collection<ProviderConfigLine> Providers
        {
            get { return this.configurationLines; }
        }

        public string BuildProviderConfigurationLine()
        {
            var providerConfigurationLines =
                from c in this.Providers
                select SandboxHelpers.BuildProviderConfigurationLine(c.Name, c.Type, c.CustomAttributes);

            return string.Concat(providerConfigurationLines.ToArray());
        }
    }
}
