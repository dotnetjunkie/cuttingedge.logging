using System.Diagnostics;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Common
{
    [DebuggerDisplay("{Configuration}")]
    public sealed class ConfigurationBuilder
    {
        public ConfigurationBuilder()
        {
            this.Logging = new LoggingConfigurationBuilder();
        }

        public string Xml { get; set; }

        public LoggingConfigurationBuilder Logging { get; set; }

        // Used for debugging.
        public string Configuration
        {
            get
            {
                var writer = this.Build();

                var templateWriter = writer as TemplatedConfigurationWriter;

                if (templateWriter != null)
                {
                    return templateWriter.Configuration;
                }

                return base.ToString();
            }
        }

        public IConfigurationWriter Build()
        {
            return SandboxHelpers.CreateConfiguration(this.Logging.DefaultProvider,
                this.Logging.BuildProviderConfigurationLine(), this.Xml);
        }

        public override string ToString()
        {
            return this.Configuration;
        }
    }
}
