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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Reflection;
using System.Web.Configuration;

namespace CuttingEdge.Logging
{
    // Code generated with .NET Junkie's provider template. 
    // source: http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=26

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
    ///                 type="CuttingEdge.Logging.SqlLoggingProvider, CuttingEdge.Logging"
    ///                 fallbackProvider="WindowsEventLogLoggingProvider"
    ///                 threshold="Information"
    ///                 connectionStringName="SqlLogging"
    ///                 description="SQL logging provider"
    ///             /&gt;
    ///             &lt;add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                 threshold="Warning"
    ///                 source="MyWebApplication"
    ///                 logName="MyWebApplication"
    ///                 description="Windows event log logging provider"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    ///     &lt;system.web&gt;
    ///         &lt;httpModules&gt;
    ///             &lt;add name="ExceptionLogger" 
    ///                 type="CuttingEdge.Logging.Web.AspNetExceptionLoggingModule, CuttingEdge.Logging"/&gt;
    ///         &lt;/httpModules&gt;
    ///     &lt;/system.web&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </example>
    public static class Logger
    {
        private const string SectionName = "logging";

        private static readonly LoggingProviderCollection providers;
        private static readonly LoggingProviderBase provider;

        // When the initialization of the Logger class failed, this field will have a non-null value.
        private static readonly Exception InitializationException;

        /// <summary>Initializes static members of the Logger class.</summary>
        static Logger()
        {
            try
            {
                LoggingSection section = GetConfigurationSection();

                LoggingProviderCollection providerCollection = LoadProviderCollection(section);

                LoggingProviderBase defaultProvider = GetDefaultProvider(section, providerCollection);

                InitializeFallbackProviders(providerCollection);

                ValidateProviders(providerCollection);

                Logger.providers = providerCollection;
                Logger.provider = defaultProvider;
            }
            catch (ProviderException pex)
            {
                InitializationException = pex;
            }
            catch (ConfigurationErrorsException ceex)
            {
                InitializationException = ceex;
            }
        }

        /// <summary>Gets a reference to the default Logging provider for the application.</summary>
        /// <value>The default Logging provider for the application exposed using the 
        /// <see cref="LoggingProviderBase"/> abstract base class.</value>
        public static LoggingProviderBase Provider
        {
            get
            {
                if (Logger.InitializationException != null)
                {
                    throw Logger.InitializationException;
                }

                return provider;
            }
        }

        /// <summary>Gets a collection of the Logging providers for the application.</summary>
        /// <value>A <see cref="LoggingProviderCollection"/> of the Logging providers configured for the 
        /// application.</value>
        public static LoggingProviderCollection Providers
        {
            get
            {
                if (Logger.InitializationException != null)
                {
                    throw Logger.InitializationException;
                }

                return providers;
            }
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Throrn when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        public static object Log(Exception exception)
        {
            return Logger.Provider.Log(exception);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB) or the supplied <paramref name="message"/> is a null reference.</exception>
        public static object Log(string message, Exception exception)
        {
            return Logger.Provider.Log(message, exception);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(Exception exception, MethodBase source)
        {
            return Logger.Provider.Log(exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(string message, Exception exception, MethodBase source)
        {
            return Logger.Provider.Log(message, exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(string message, Exception exception, string source)
        {
            return Logger.Provider.Log(message, exception, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is an
        /// empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/>,
        /// <paramref name="message"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(LoggingEventType severity, string message, Exception exception, MethodBase source)
        {
            return Logger.Provider.Log(severity, message, exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/>,
        /// <paramref name="exception"/> or <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(LoggingEventType severity, string message, Exception exception, string source)
        {
            return Logger.Provider.Log(severity, message, exception, source);
        }

        /// <summary>Logs an information event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a 
        /// null reference.</exception>
        public static object Log(string message)
        {
            return Logger.Provider.Log(message);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(LoggingEventType severity, string message)
        {
            return Logger.Provider.Log(severity, message);
        }

        /// <summary>Logs an information event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="exception"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(string message, MethodBase source)
        {
            return Logger.Provider.Log(message, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> is
        /// an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(LoggingEventType severity, string message, MethodBase source)
        {
            return Logger.Provider.Log(severity, message, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the supplied <paramref name="message"/> or 
        /// <paramref name="source"/> are empty strings.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="message"/> or
        /// <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when the supplied <paramref name="type"/>
        /// has an unexpected value.</exception>
        public static object Log(LoggingEventType severity, string message, string source)
        {
            return Logger.Provider.Log(severity, message, source);
        }

        private static LoggingSection GetConfigurationSection()
        {
            // Get the feature's configuration info
            object section = ConfigurationManager.GetSection(SectionName);

            // Throw exception when null
            if (section == null)
            {
                throw new ProviderException(SR.GetString(SR.LoggingSectionMissingFromConfigSettings,
                    typeof(LoggingSection).FullName, typeof(LoggingSection).Assembly.GetName().Name));
            }

            LoggingSection loggingSection = section as LoggingSection;

            // Throw exception when invalid cast
            if (loggingSection == null)
            {
                throw new ProviderException(SR.GetString(SR.SectionIsNotOfCorrectType, SectionName,
                    typeof(LoggingSection).FullName, section.GetType().FullName));
            }

            return loggingSection;
        }

        private static LoggingProviderCollection LoadProviderCollection(LoggingSection section)
        {
            LoggingProviderCollection providerCollection = new LoggingProviderCollection();

            ProvidersHelper.InstantiateProviders(section.Providers, providerCollection,
                typeof(LoggingProviderBase));

            providerCollection.SetReadOnly();

            return providerCollection;
        }

        private static LoggingProviderBase GetDefaultProvider(LoggingSection loggingSection,
            LoggingProviderCollection providerCollection)
        {
            LoggingProviderBase defaultProvider = providerCollection[loggingSection.DefaultProvider];

            if (defaultProvider == null)
            {
                PropertyInformation property = loggingSection.ElementInformation.Properties["defaultProvider"];

                throw new ConfigurationErrorsException(SR.GetString(SR.NoDefaultLoggingProviderFound,
                    SectionName), property.Source, property.LineNumber);
            }

            return defaultProvider;
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