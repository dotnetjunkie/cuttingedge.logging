using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using NSandbox;

namespace CuttingEdge.Logging.Tests.Common
{
    [DebuggerDisplay("{Configuration}")]
    public class TemplatedConfigurationWriter : IConfigurationWriter
    {
        public TemplatedConfigurationWriter(string configurationTemplate)
        {
            this.ConfigurationTemplate = configurationTemplate;
            this.Variables = new Dictionary<string, string>();
        }

        public string ConfigurationTemplate { get; private set; }

        public Dictionary<string, string> Variables { get; private set; }

        public string Configuration
        {
            get
            {
                return this.BuildConfiguration();
            }
        }

        public void WriteConfiguration(string destinationFilename)
        {
            File.WriteAllText(destinationFilename, this.BuildConfiguration());
        }

        private string BuildConfiguration()
        {
            StringBuilder contents = new StringBuilder(this.ConfigurationTemplate);

            foreach (KeyValuePair<string, string> pair in this.Variables)
            {
                contents.Replace("$" + pair.Key + "$", pair.Value);
            }

            return contents.ToString();
        }
    }
}
