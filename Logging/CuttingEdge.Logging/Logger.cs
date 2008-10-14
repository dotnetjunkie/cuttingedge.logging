#region Copyright (c) 2008 S. van Deursen
/* The CuttingEdge.Logging library allows developers to plug a logging mechanism into their web- and desktop
 * applications.
 * 
 * Copyright (C) 2008 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ 
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Reflection;
using System.Web.Configuration;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages Logging in an application. This class cannot be inherited.
    /// </summary>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the 
    /// Logging section, which can also be accessed as members of the
    /// <see cref="LoggingSection"/> class. The following configuration file example shows
    /// how to specify values declaratively for the Logging section.
    /// <code>
    /// &lt;?xml version="1.0"?&gt;
    /// &lt;configuration&gt;
    ///     &lt;configSections&gt;
    ///         &lt;section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" /&gt;
    ///     &lt;/configSections&gt;
    ///     &lt;connectionStrings&gt;
    ///         &lt;add name="SqlLogging" 
    ///             connectionString="Data Source=localhost;Integrated Security=SSPI;Initial Catalog=Logging;" /&gt;
    ///     &lt;/connectionStrings&gt;
    ///     &lt;logging defaultProvider="SqlLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="SqlLoggingProvider"
    ///                 fallbackProvider="WindowsEventLogLoggingProvider"
    ///                 connectionStringName="SqlLogging"
    ///                 type="CuttingEdge.Logging.SqlLoggingProvider, CuttingEdge.Logging"
    ///                 description="SQL logging provider"
    ///             /&gt;
    ///             &lt;add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 source="MyWebApplication"
    ///                 logName="MyWebApplication"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                 description="Windows event log logging provider"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    ///     &lt;system.web&gt;
    ///         &lt;httpModules&gt;
    ///             &lt;add name="ExceptionLogger" 
    ///                 type="CuttingEdge.Logging.AspNetExceptionLoggingModule, CuttingEdge.Logging"/&gt;
    ///         &lt;/httpModules&gt;
    ///     &lt;/system.web&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </example>
    public static class Logger
    {
        private const string SectionName = "logging";

        private static readonly LoggingProviderCollection providerCollection;
        private static readonly LoggingProviderBase provider;

        /// <summary>Initializes static members of the Logger class.</summary>
        static Logger()
        {
            // Get the feature's configuration info
            object section = ConfigurationManager.GetSection(SectionName);

            // Throw exception when null
            if (section == null)
            {
                throw new ProviderException(SR.GetString(SR.LoggingSectionMissingFromConfigSettings,
                    typeof(LoggingSection).FullName, typeof(LoggingSection).Assembly.GetName().Name));
            }

            LoggingSection config = section as LoggingSection;

            // Throw exception when invalid cast
            if (config == null)
            {
                throw new ProviderException(SR.GetString(SR.SectionIsNotOfCorrectType, SectionName, 
                    typeof(LoggingSection).FullName, section.GetType().FullName));
            }

            // Instantiate the providers
            providerCollection = new LoggingProviderCollection();

            ProvidersHelper.InstantiateProviders(config.Providers, providerCollection,
                typeof(LoggingProviderBase));

            providerCollection.SetReadOnly();

            provider = providerCollection[config.DefaultProvider];

            if (provider == null)
            {
                PropertyInformation property = config.ElementInformation.Properties["defaultProvider"];

                throw new ConfigurationErrorsException(SR.GetString(SR.NoDefaultLoggingProviderFound, 
                    SectionName), property.Source, property.LineNumber);
            }

            InitializeFallbackProviders(providerCollection);

            ValidateProviders(providerCollection);
        }

        /// <summary>Gets the default configured <see cref="LoggingProviderBase"/> instance.</summary>
        /// <value>The provider.</value>
        public static LoggingProviderBase Provider
        {
            get { return provider; }
        }

        /// <summary>Gets the collection of configured <see cref="LoggingProviderBase"/> instances.</summary>
        /// <value>The providers.</value>
        public static LoggingProviderCollection Providers
        {
            get { return providerCollection; }
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentNullException">Throrn when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        public static object Log(Exception exception)
        {
            return provider.Log(exception);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB) or the supplied <paramref name="message"/> is a null reference.</exception>
        public static object Log(string message, Exception exception)
        {
            return provider.Log(message, exception);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(Exception exception, MethodBase source)
        {
            return provider.Log(exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(string message, Exception exception, MethodBase source)
        {
            return provider.Log(message, exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(string message, Exception exception, string source)
        {
            return provider.Log(message, exception, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is an
        /// empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(EventType type, string message, Exception exception, MethodBase source)
        {
            return provider.Log(type, message, exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/>,
        /// <paramref name="exception"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(EventType type, string message, Exception exception, string source)
        {
            return provider.Log(type, message, exception, source);
        }

        /// <summary>Logs an information event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a 
        /// null reference.</exception>
        public static object Log(string message)
        {
            return provider.Log(message);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(EventType type, string message)
        {
            return provider.Log(type, message);
        }

        /// <summary>Logs an information event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(string message, MethodBase source)
        {
            return provider.Log(message, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(EventType type, string message, MethodBase source)
        {
            return provider.Log(type, message, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(EventType type, string message, string source)
        {
            return provider.Log(type, message, source);
        }

        // The fallback providers must be initialized from within the this Logger class. It can't be done
        // from within the LoggingProviderBase.Initialize() method. At the time of calling 
        // LoggingProviderBase.Initialize() the collection of Providers is not yet initialized.
        private static void InitializeFallbackProviders(LoggingProviderCollection providers)
        {
            foreach (LoggingProviderBase provider in providers)
            {
                if (provider.FallbackProviderName != null)
                {
                    // Fetch the fallback provider with the defined name from the providers collection
                    LoggingProviderBase fallbackProvider = providers[provider.FallbackProviderName];

                    // Throw an exception when that provider could not be found.
                    if (fallbackProvider == null)
                    {
                        throw new ProviderException(SR.GetString(SR.InvalidFallbackProviderPropertyInConfig,
                            SectionName, provider.GetType().FullName, provider.Name, 
                            provider.FallbackProviderName));
                    }

                    // Initialize the provider's fallback provider with the found provider.
                    provider.FallbackProvider = fallbackProvider;
                }
            }
        }

        private static void ValidateProviders(LoggingProviderCollection providers)
        {
            foreach (LoggingProviderBase provider in providers)
            {
                bool circularReferenceFound = ProviderContainsACircularReference(provider);

                if (circularReferenceFound)
                {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.CircularReferenceInLoggingSection, SectionName, provider.Name));
                }
            }
        }

        private static bool ProviderContainsACircularReference(LoggingProviderBase provider)
        {
            if (provider.FallbackProvider == null)
            {
                return false;
            }

            // A HashSet<LoggingProviderBase> would be nicer, but we are targetting .NET 2.0.
            Dictionary<LoggingProviderBase, object> referencedProviders =
                new Dictionary<LoggingProviderBase, object>();

            do
            {
                if (referencedProviders.ContainsKey(provider))
                {
                    // Circular reference found in the chain of providers that are directly or indirectly
                    // linked by the given provider.
                    return true;
                }

                referencedProviders.Add(provider, null);

                // Move to the next provider
                provider = provider.FallbackProvider;
            }
            while (provider != null);

            // post: No circular reference found.
            return false;
        }
    }
}