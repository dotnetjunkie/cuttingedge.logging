using System;
using System.Globalization;

using NSandbox;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    public static class SandboxHelpers
    {
        private const string TemplatedConfiguration = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
            <configuration>
	            <configSections>
		            <section name=""logging"" type=""CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"" allowDefinition=""MachineToApplication"" />
	            </configSections>
	            <logging defaultProvider=""$DefaultProvider$"">
		            <providers>
			            $Providers$
		            </providers>
	            </logging>
                $OtherConfigurations$
            </configuration>";

        public static IConfigurationWriter CreateConfiguration(string defaultProviderName,
            string providerConfigurationLines)
        {
            return CreateConfiguration(defaultProviderName, providerConfigurationLines, string.Empty);
        }

        public static IConfigurationWriter CreateConfiguration(string defaultProviderName,
            string providerConfigurationLines, string otherConfigurations)
        {
            TemplatedConfigurationWriter writer = new TemplatedConfigurationWriter(TemplatedConfiguration);

            writer.Variables.Add("DefaultProvider", defaultProviderName);
            writer.Variables.Add("Providers", providerConfigurationLines);
            writer.Variables.Add("OtherConfigurations", otherConfigurations);

            return writer;
        }

        public static string BuildProviderConfigurationLine(Type type)
        {
            return BuildProviderConfigurationLine(type.Name, type, null);
        }

        public static string BuildProviderConfigurationLine(Type type, string customAttributes)
        {
            return BuildProviderConfigurationLine(type.Name, type, customAttributes);
        }

        public static string BuildProviderConfigurationLine(string name, Type type)
        {
            return BuildProviderConfigurationLine(name, type, null);
        }

        public static string BuildProviderConfigurationLine(string name, Type type, string customAttributes)
        {
            string typeName = string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);

            return BuildProviderConfigurationLine(name, typeName, customAttributes);
        }

        public static string BuildProviderConfigurationLine(string name, string type, string customAttributes)
        {
            return string.Format(CultureInfo.InvariantCulture, @"<add name=""{0}"" type=""{1}"" {2} />",
                name, type, customAttributes);
        }
    }
}
