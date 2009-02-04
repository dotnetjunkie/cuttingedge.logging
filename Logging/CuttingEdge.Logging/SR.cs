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
using System.Resources;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// String Resource helper class.
    /// </summary>
    internal static class SR
    {
        // General messages
        internal const string ArgumentMustNotBeNullOrEmptyString = "ArgumentMustNotBeNullOrEmptyString";

        // Exception messages for Logger class
        internal const string LoggingSectionMissingFromConfigSettings = "LoggingSectionMissingFromConfigSettings";
        internal const string SectionIsNotOfCorrectType = "SectionIsNotOfCorrectType";
        internal const string NoDefaultLoggingProviderFound = "NoDefaultLoggingProviderFound";
        internal const string CircularReferenceInLoggingSection = "CircularReferenceInLoggingSection";

        // Exception messages for LoggingProviderBase class
        internal const string InvalidFallbackProviderPropertyInConfig = "InvalidFallbackProviderPropertyInConfig";
        internal const string DuplicateLoggingProviderInConfig = "DuplicateLoggingProviderInConfig";
        internal const string UnrecognizedAttributeInProviderConfiguration = "UnrecognizedAttributeInProviderConfiguration";
        internal const string InvalidThresholdValueInProviderConfiguration = "InvalidThresholdValueInProviderConfiguration";
        internal const string InvalidBooleanAttribute = "InvalidBooleanAttribute";

        // Exception messages for LoggingProviderCollection class
        internal const string ProviderParameterMustBeOfTypeX = "ProviderParameterMustBeOfTypeX";

        // Exception messages for WindowsEventLogLoggingProvider class
        internal const string EmptyOrMissingPropertyInConfiguration = "EmptyOrMissingPropertyInConfiguration";

        // Exception messages for SqlLoggingProvider
        internal const string MissingConnectionStringAttribute = "MissingConnectionStringAttribute";
        internal const string MissingConnectionStringInConfig = "MissingConnectionStringInConfig";
        internal const string EventCouldNotBeLoggedWithX = "EventCouldNotBeLoggedWithX";
        internal const string SqlLoggingProviderSchemaScripts = "SqlLoggingProviderSchemaScripts";
        internal const string InitializationOfDatabaseSchemaFailed = "InitializationOfDatabaseSchemaFailed";
        internal const string SqlProviderAlreadyInitialized = "SqlProviderAlreadyInitialized";
        internal const string AspNetSqlLoggingProviderSchemaScripts = "AspNetSqlLoggingProviderSchemaScripts";

        // Exception messages for LoggingWebEventProvider
        internal const string MissingLoggingProviderInConfig = "MissingLoggingProviderInConfig";

        // Exception messages for MailLoggingProvider
        internal const string InvalidIntegerAttribute = "InvalidIntegerAttribute";
        internal const string InvalidMailAddressAttribute = "InvalidMailAddressAttribute";
        internal const string InvalidFormatStringAttribute = "InvalidFormatStringAttribute";
        internal const string MissingAttributeInMailSettings = "MissingAttributeInMailSettings";
        internal const string NoPermissionsToAccessSmtpServers = "NoPermissionsToAccessSmtpServers";

        private static readonly ResourceManager resource =
            new ResourceManager(typeof(SR).Namespace + ".LoggingExceptionMessages", typeof(SR).Assembly);

        // Returns a string from the resource.
        internal static string GetString(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return GetStringInternal(name, null);
        }

        // Returns a string from the resource and formats it with the given args in a culture-specific way.
        internal static string GetString(string name, params object[] args)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return GetStringInternal(name, args);
        }

        private static string GetStringInternal(string name, params object[] args)
        {
            string format = resource.GetString(name, CultureInfo.CurrentUICulture);

            if (format == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The supplied string '{0}' could not be found in the resource.", name), "name");
            }

            if (args == null)
            {
                return format;
            }

            return string.Format(CultureInfo.CurrentCulture, format, args);
        }
    }
}
