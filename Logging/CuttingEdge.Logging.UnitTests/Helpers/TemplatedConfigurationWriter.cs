using System.Collections.Generic;
using System.IO;
using System.Text;

using NSandbox;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    public class TemplatedConfigurationWriter : IConfigurationWriter
    {
        public TemplatedConfigurationWriter(string configurationTemplate)
        {
            this.ConfigurationTemplate = configurationTemplate;
            this.Variables = new Dictionary<string, string>();
        }

        public string ConfigurationTemplate { get; private set; }

        public Dictionary<string, string> Variables { get; private set; }

        public void WriteConfiguration(string destinationFilename)
        {
            StringBuilder contents = new StringBuilder(this.ConfigurationTemplate);

            foreach (KeyValuePair<string, string> pair in this.Variables)
            {
                contents.Replace("$" + pair.Key + "$", pair.Value);
            }

            File.WriteAllText(destinationFilename, contents.ToString());
        }
    }
}
