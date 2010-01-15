#region Copyright (c) 2008 S. van Deursen
/* The CuttingEdge.Logging library allows developers to plug a logging mechanism into their web- and desktop
 * applications.
 * 
 * Copyright (C) 2008 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// String Resource helper class.
    /// </summary>
    internal static class SR
    {
        private static readonly ResourceManager resource =
            new ResourceManager(typeof(SR).Namespace + ".LoggingExceptionMessages", typeof(SR).Assembly);

        // General messages
        internal static string ArgumentMustNotBeNullOrEmptyString()
        {
            return GetString("ArgumentMustNotBeNullOrEmptyString");
        }

        // Exception messages for Logger class
        internal static string LoggingSectionMissingFromConfigSettings(string sectionName, Type type)
        {
            return GetString("LoggingSectionMissingFromConfigSettings", sectionName, type.FullName, 
                type.Assembly.GetName().Name);
        }

        internal static string NoDefaultLoggingProviderFound(string sectionName)
        {
            return GetString("NoDefaultLoggingProviderFound", sectionName);
        }

        internal static string CircularReferenceInLoggingSection(string sectionName, string providerName)
        {
            return GetString("CircularReferenceInLoggingSection", sectionName, providerName);
        }

        internal static string TypeNameMustBeSpecifiedForThisProvider(string providerName)
        {
            return GetString("TypeNameMustBeSpecifiedForThisProvider", providerName);
        }

        internal static string ProviderMustInheritFromType(string providerName, Type actualType,
            Type expectedBaseType)
        {
            return GetString("ProviderMustInheritFromType", providerName, actualType.FullName, 
                expectedBaseType.FullName);
        }

        internal static string TypeNameCouldNotBeResolvedForProvider(string providerName, string typeName,
            string exceptionMessage)
        {
            return GetString("TypeNameCouldNotBeResolvedForProvider", providerName, typeName,
                exceptionMessage);
        }

        internal static string TypeCouldNotBeCreatedForProvider(string providerName, Type providerType,
            string exceptionMessage)
        {
            return GetString("TypeCouldNotBeCreatedForProvider", providerName, providerType.FullName,
                exceptionMessage);
        }

        // Exception messages for LoggingProviderBase class
        internal static string InvalidFallbackProviderPropertyInConfig(string sectionName, 
            LoggingProviderBase provider)
        {
            return GetString("InvalidFallbackProviderPropertyInConfig", sectionName, 
                provider.GetType().FullName, provider.Name, provider.FallbackProviderName);
        }

        internal static string EventCouldNotBeLoggedWithX(string providerName)
        {
            return GetString("EventCouldNotBeLoggedWithX", providerName);
        }

        internal static string UnrecognizedAttributeInProviderConfiguration(string providerName, 
            string attributeName)
        {
            return GetString("UnrecognizedAttributeInProviderConfiguration", providerName, 
                attributeName);
        }

        internal static string InvalidThresholdValueInProviderConfiguration(string providerName)
        {
            string values = GetEventTypeValuesAsString();
            return GetString("InvalidThresholdValueInProviderConfiguration", providerName, values);
        }

        internal static string InvalidBooleanAttribute(string value, string attributeName, string providerName)
        {
            return GetString("InvalidBooleanAttribute", value, attributeName, providerName);
        }

        // Exception messages for LoggingProviderCollection class
        internal static string ProviderParameterMustBeOfTypeX(Type argumentType)
        {
            return GetString("ProviderParameterMustBeOfTypeX", argumentType.Name);
        }

        // Exception messages for WindowsEventLogLoggingProvider class
        internal static string EmptyOrMissingPropertyInConfiguration(string propertyName, string providerName)
        {
            return GetString("EmptyOrMissingPropertyInConfiguration", propertyName, providerName);
        }

        // Exception messages for SqlLoggingProvider
        internal static string MissingConnectionStringAttribute(string connectionStringName)
        {
            return GetString("MissingConnectionStringAttribute", connectionStringName);
        }

        internal static string MissingConnectionStringInConfig(string connectionStringName)
        {
            return GetString("MissingConnectionStringInConfig", connectionStringName);
        }

        internal static string SqlLoggingProviderSchemaScripts()
        {
            const string ScriptResource = "CuttingEdge.Logging.SqlLoggingProviderScripts.sql";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ScriptResource))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static string InitializationOfDatabaseSchemaFailed(string providerName, 
            string exceptionMessage)
        {
            return GetString("InitializationOfDatabaseSchemaFailed", providerName, exceptionMessage);
        }

        internal static string SqlProviderAlreadyInitialized(string providerName)
        {
            return GetString("SqlProviderAlreadyInitialized", providerName);
        }

        internal static string AspNetSqlLoggingProviderSchemaScripts()
        {
            const string ScriptResource = "CuttingEdge.Logging.Web.AspNetSqlLoggingProviderScripts.sql";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ScriptResource))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        // Exception messages for LoggingWebEventProvider
        internal static string MissingLoggingProviderInConfig(string loggingProviderName, 
            string loggingWebEventProviderName, string sectionName)
        {
            return GetString("MissingLoggingProviderInConfig", loggingProviderName, sectionName,
                loggingWebEventProviderName);
        }

        internal static string InvalidMailAddressAttribute(string attributeValue, string attributeName,
            string providerName)
        {
            return GetString("InvalidMailAddressAttribute", attributeValue, attributeName, 
                providerName);
        }

        internal static string InvalidFormatStringAttribute(string attributeValue, string attributeName,
            string providerName, string exceptionMessage)
        {
            return GetString("InvalidFormatStringAttribute", attributeValue, attributeName,
                providerName) + " " + exceptionMessage;
        }

        internal static string MissingAttributeInMailSettings(string providerName, string attributeName,
            string sectionName)
        {
            return GetString("MissingAttributeInMailSettings", providerName, attributeName, 
                sectionName);
        }

        internal static string NoPermissionsToAccessSmtpServers(string providerName, string exceptionMessage)
        {
            return GetString("NoPermissionsToAccessSmtpServers", providerName, exceptionMessage);
        }

        internal static string ExampleMailConfigurationSettings()
        {
            return GetString("ExampleMailConfigurationSettings");
        }

        internal static string PossibleInvalidMailConfigurationInConfigFile(Type messageType, 
            string exceptionMessage)
        {
            return GetString("PossibleInvalidMailConfigurationInConfigFile", messageType.FullName) +
                " " + exceptionMessage;
        }

        // Exception messages for AspNetSqlLoggingProvider
        internal static string EmptyOrMissingApplicationNameAttributeInConfig(string providerName)
        {
            return GetString("EmptyOrMissingApplicationNameAttributeInConfig", providerName);
        }

        internal static string ApplicationNameAttributeInConfigTooLong(string providerName, 
            int maximumNumberOfCharacters)
        {
            return GetString("ApplicationNameAttributeInConfigTooLong", providerName, 
                maximumNumberOfCharacters);
        }

        internal static string InvalidUseNameRetrievalTypeAttributeInConfig(string providerName)
        {
            string values = GetUserIdentityRetrievalTypeAsString();
            return GetString("InvalidUseNameRetrievalTypeAttributeInConfig", providerName, values);
        }

        internal static string MissingUserNameRetrievalTypeAttributeInConfig(string providerName)
        {
            string values = GetUserIdentityRetrievalTypeAsString();
            return GetString("MissingUserNameRetrievalTypeAttributeInConfig", providerName, values);
        }

        // Exception messages for CompositeLoggingProvider class
        internal static string LoggingFailed(string extraInformation)
        {
            return GetString("LoggingFailed", extraInformation);
        }

        internal static string ProviderHasNotBeenInitializedCorrectlyCallInitializeFirst(
            CompositeLoggingProvider provider)
        {
            string providerTypeName = GetShortTypeNameForOwnTypes(provider.GetType());

            // Note that this message is returned in the case that initialize hasn't been called. Therefore
            // the provider's Name property will be null and supplying it to the message is useless.
            return GetString("ProviderHasNotBeenInitializedCorrectlyCallInitializeFirst", providerTypeName);
        }
      
        internal static string ReferencedProviderDoesNotExist(CompositeLoggingProvider provider, 
            string missingProviderName)
        {
            string providerTypeName = GetShortTypeNameForOwnTypes(provider.GetType());
                        
            return GetString("ReferencedProviderDoesNotExist", providerTypeName, provider.Name, 
                missingProviderName);
        }

        internal static string ProviderReferencedMultipleTimes(CompositeLoggingProvider provider, 
            string doubleReferencedProviderName)
        {
            string providerTypeName = GetShortTypeNameForOwnTypes(provider.GetType());
          
            return GetString("ProviderReferencedMultipleTimes", providerTypeName, provider.Name, 
                doubleReferencedProviderName);
        }

        internal static string CompositeLoggingProviderDoesNotReferenceAnyProviders(
            CompositeLoggingProvider provider)
        {
            string providerTypeName = GetShortTypeNameForOwnTypes(provider.GetType());

            return GetString("CompositeLoggingProviderDoesNotReferenceAnyProviders", providerTypeName, 
                provider.Name);
        }

        private static string GetShortTypeNameForOwnTypes(Type type)
        {
            Assembly currentAssembly = MethodBase.GetCurrentMethod().DeclaringType.Assembly;

            if (type.Assembly == currentAssembly)
            {
                // The type is defined in this assembly, so let's return the short name
                return type.Name;
            }
            else
            {
                // The type is defined outside of this assembly (probably by the user), so let's return the
                // full name.
                return type.FullName;
            }
        }

        private static string GetString(string name, params object[] args)
        {
            string format = resource.GetString(name, CultureInfo.CurrentUICulture);

            if (args == null || args.Length == 0)
            {
                return format;
            }

            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        private static string GetEventTypeValuesAsString()
        {
            string values = String.Empty;

            Array types = Enum.GetValues(typeof(LoggingEventType));

            int lastIndex = types.Length - 1;
            for (int index = 0; index < types.Length; index++)
            {
                if (index > 0)
                {
                    values += index == lastIndex ? " or " : ", ";
                }

                values += types.GetValue(index).ToString();
            }

            return values;
        }

        private static string GetUserIdentityRetrievalTypeAsString()
        {
            string values = String.Empty;

            Array types = Enum.GetValues(typeof(CuttingEdge.Logging.Web.UserIdentityRetrievalType));

            int lastIndex = types.Length - 1;
            for (int index = 0; index < types.Length; index++)
            {
                if (index > 0)
                {
                    values += index == lastIndex ? " or " : ", ";
                }

                values += types.GetValue(index).ToString();
            }

            return values;
        }
    }
}