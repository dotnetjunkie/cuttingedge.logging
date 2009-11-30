using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuttingEdge.Logging.Tests.Common
{
    public sealed class ProviderConfigLine
    {
        public ProviderConfigLine()
        {
        }

        public ProviderConfigLine(string providerName, Type providerType)
        {
            this.Name = providerName;
            this.Type = providerType;
        }

        public ProviderConfigLine(string providerName, Type providerType, string customAttributes)
            : this(providerName, providerType)
        {
            this.CustomAttributes = customAttributes;
        }

        public string Name { get; set; }

        public Type Type { get; set; }

        public string CustomAttributes { get; set; }
    }
}
