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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web.Configuration;

namespace CuttingEdge.Logging
{
    // Code generated with .NET Junkie's provider template. 
    // source: http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=26

    /// <summary>
    /// Manages logging in an application. This class cannot be inherited.
    /// </summary>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the 
    /// Logging section, which can also be accessed as members of the
    /// <see cref="LoggingSection"/> class. The following configuration file example shows
    /// how to specify values declaratively for the Logging section.
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" />
    ///     </configSections>
    ///     <connectionStrings>
    ///         <add name="SqlLogging" 
    ///             connectionString="Data Source=localhost;Integrated Security=SSPI;Initial Catalog=Logging;" />
    ///     </connectionStrings>
    ///     <logging defaultProvider="SqlLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="SqlLoggingProvider"
    ///                 type="CuttingEdge.Logging.SqlLoggingProvider, CuttingEdge.Logging"
    ///                 fallbackProvider="WindowsEventLogLoggingProvider"
    ///                 threshold="Information"
    ///                 connectionStringName="SqlLogging"
    ///                 description="SQL logging provider"
    ///             />
    ///             <add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                 threshold="Warning"
    ///                 source="MyWebApplication"
    ///                 logName="MyWebApplication"
    ///                 description="Windows event log logging provider"
    ///             />
    ///         </providers>
    ///     </logging>
    ///     <system.web>
    ///         <httpModules>
    ///             <add name="ExceptionLogger" 
    ///                 type="CuttingEdge.Logging.Web.AspNetExceptionLoggingModule, CuttingEdge.Logging"/>
    ///         </httpModules>
    ///     </system.web>
    /// </configuration>
    /// ]]></code>
    /// </example>
    public static class Logger
    {
        private const string DefaultSectionName = "logging";

        private static readonly LoggingProviderCollection providers;
        private static readonly LoggingProviderBase provider;

        // When the initialization of the Logger class failed, this field will have a non-null value.
        private static readonly Exception InitializationException;

        /// <summary>Initializes static members of the Logger class.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "Static constructor is needed to be able to store the thrown exception.")]
        static Logger()
        {
            try
            {
                LoggingSection section = GetConfigurationSection();

                LoggingProviderCollection providerCollection = LoadAndInitializeProviderCollection(section);

                LoggingProviderBase defaultProvider = GetDefaultProvider(section, providerCollection);

                InitializeFallbackProviders(providerCollection);              

                CompleteInitialization(providerCollection, defaultProvider);

                CircularReferenceFinder.Validate(providerCollection);

                Logger.providers = providerCollection;
                Logger.provider = defaultProvider;
            }
            catch (ProviderException pex)
            {
                // When a ProviderException or ConfigurationException is thrown, we store those and throw them
                // when one of the public methods of Logger is called. This way the original exceptions are 
                // thrown and not a TypeInitializeException that wraps the original.
                InitializationException = pex;
            }
            catch (ConfigurationException ceex)
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
        
        /// <summary>Gets the name of the logging section as it is currently configured by the user.</summary>
        /// <value>The name of the logging section.</value>
        internal static string SectionName
        {
            get { return GetConfigurationSection().SectionInformation.Name; }
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="entry">The entry that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="entry"/> is
        /// a null reference (Nothing in VB).</exception>
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(LogEntry entry)
        {
            ILogger logger = Logger.Provider;
            
            return logger.Log(entry);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> is
        /// a null reference (Nothing in VB).</exception>
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(string message, Exception exception)
        {
            return Logger.Provider.Log(message, exception);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
        /// <returns>The id of the logged event or null in one of the following reasons:
        /// The event hasn't been logged, because of the current 
        /// <see cref="LoggingProviderBase.Threshold">Threshold</see> level;
        /// Returning an id is not supported by the current implementation;
        /// The event has been logged to a fallback provider, because of an error in the current implementation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(Exception exception, MethodBase source)
        {
            return Logger.Provider.Log(exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(string message, Exception exception, MethodBase source)
        {
            return Logger.Provider.Log(message, exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(string message, Exception exception, string source)
        {
            return Logger.Provider.Log(message, exception, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(LoggingEventType severity, string message, Exception exception, MethodBase source)
        {
            return Logger.Provider.Log(severity, message, exception, source);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(LoggingEventType severity, string message)
        {
            return Logger.Provider.Log(severity, message);
        }

        /// <summary>Logs an information event to the default <see cref="Provider"/>.</summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(string message, MethodBase source)
        {
            return Logger.Provider.Log(message, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(LoggingEventType severity, string message, MethodBase source)
        {
            return Logger.Provider.Log(severity, message, source);
        }

        /// <summary>Logs an event to the default <see cref="Provider"/>.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">A source where the event occurred.</param>
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
        /// <exception cref="Exception">Thrown when the <b>Logger</b> failed to log to the underlying logging
        /// provider. The exact type of exception thrown depends on the actual provider implementation.
        /// </exception>
        public static object Log(LoggingEventType severity, string message, string source)
        {
            return Logger.Provider.Log(severity, message, source);
        }

        // Throws a ProviderException on failure.
        private static LoggingSection GetConfigurationSection()
        {
            Configuration configuration;

            try
            {
                configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (ArgumentException)
            {
                // ArgumentException is thrown when configuration could not be opened, because application is
                // not a stand alone exe. Next best guess is that we're dealing with a web application.
                configuration = WebConfigurationManager.OpenWebConfiguration("~");
            }

            var section = FindLoggingSection(configuration.Sections, configuration.SectionGroups);

            if (section != null)
            {
                return section;
            }

            throw new ProviderException(
                SR.LoggingSectionMissingFromConfigSettings(DefaultSectionName, typeof(LoggingSection)));
        }

        private static LoggingSection FindLoggingSection(ConfigurationSectionCollection sections,
            ConfigurationSectionGroupCollection sectionGroups)
        {
            foreach (var section in sections)
            {
                var loggingSection = section as LoggingSection;

                if (loggingSection != null)
                {
                    return loggingSection;
                }
            }

            foreach (ConfigurationSectionGroup sectionGroup in sectionGroups)
            {
                var section = FindLoggingSection(sectionGroup.Sections, sectionGroup.SectionGroups);

                if (section != null)
                {
                    return section;
                }
            }

            return null;
        }

        // Throws a ConfigurationException (or a descendant) on failure.
        private static LoggingProviderCollection LoadAndInitializeProviderCollection(LoggingSection section)
        {
            LoggingProviderCollection providerCollection = new LoggingProviderCollection();

            foreach (ProviderSettings settings in section.Providers)
            {
                LoggingProviderBase loggingProvider = InstantiateLoggingProvider(settings);

                providerCollection.Add(loggingProvider);
            }

            providerCollection.SetReadOnly();

            return providerCollection;
        }

        // Throws a ConfigurationErrorsException (descendant of ConfigurationException) on failure.
        private static LoggingProviderBase GetDefaultProvider(LoggingSection loggingSection,
            LoggingProviderCollection providerCollection)
        {
            LoggingProviderBase defaultProvider = providerCollection[loggingSection.DefaultProvider];

            if (defaultProvider == null)
            {
                PropertyInformation property = loggingSection.ElementInformation.Properties["defaultProvider"];

                throw new ConfigurationErrorsException(
                    SR.NoDefaultLoggingProviderFound(SectionName), property.Source, property.LineNumber);
            }

            return defaultProvider;
        }

        // The fallback providers must be initialized from within the this Logger class. It can't be done
        // from within the LoggingProviderBase.Initialize() method. At the time of calling 
        // LoggingProviderBase.Initialize() the collection of Providers is not yet initialized.
        // Throws ProviderException on failure.
        private static void InitializeFallbackProviders(LoggingProviderCollection providers)
        {
            foreach (LoggingProviderBase provider in providers)
            {
                InitializeFallbackProvider(provider, providers);
            }
        }

        private static void InitializeFallbackProvider(LoggingProviderBase provider,
            LoggingProviderCollection providers)
        {
            if (provider.FallbackProviderName != null)
            {
                // Fetch the fallback provider with the defined name from the providers collection
                LoggingProviderBase fallbackProvider = providers[provider.FallbackProviderName];

                // Throw an exception when that provider could not be found.
                if (fallbackProvider == null)
                {
                    throw new ProviderException(
                        SR.InvalidFallbackProviderPropertyInConfig(SectionName, provider));
                }

                // Initialize the provider's fallback provider with the found provider.
                provider.FallbackProvider = fallbackProvider;
            }
        }

        // Throws a ConfigurationException (or a descendant) on failure.
        private static LoggingProviderBase InstantiateLoggingProvider(ProviderSettings settings)
        {
            try
            {
                var provider = CreateNewProviderInstance(settings);

                InitializeProvider(provider, settings);

                return provider;
            }
            catch (Exception ex)
            {
                throw BuildMoreExpressiveException(ex, settings);
            }
        }

        private static LoggingProviderBase CreateNewProviderInstance(ProviderSettings settings)
        {
            Type providerType = GetProviderType(settings);

            try
            {
                return (LoggingProviderBase)Activator.CreateInstance(providerType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    SR.TypeCouldNotBeCreatedForProvider(settings.Name, providerType, ex.Message), ex);
            }
        }

        private static void InitializeProvider(LoggingProviderBase provider, ProviderSettings settings)
        {
            string providerName = settings.Name;

            NameValueCollection configuration = BuildProviderConfiguration(settings);

            provider.Initialize(providerName, configuration);
        }

        private static Exception BuildMoreExpressiveException(Exception thrownException, 
            ProviderSettings settings)
        {
            PropertyInformation providerInfo = settings.ElementInformation.Properties["type"];

            if (thrownException is ArgumentException)
            {
                // The exception is thrown from within this method. Therefore we do not supply an inner
                // exception.
                return new ConfigurationErrorsException(thrownException.Message, 
                    thrownException.InnerException, providerInfo.Source, providerInfo.LineNumber);
            }
            else
            {
                return new ConfigurationErrorsException(thrownException.Message, thrownException, 
                    providerInfo.Source, providerInfo.LineNumber);
            }
        }

        private static Type GetProviderType(ProviderSettings settings)
        {
            string typeName = GetNonEmptyTypeName(settings);

            Type providerType;

            try
            {
                const bool ThrowOnError = true;
                const bool IgnoreCase = true;

                providerType = Type.GetType(typeName, ThrowOnError, IgnoreCase);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    SR.TypeNameCouldNotBeResolvedForProvider(settings.Name, typeName, ex.Message), ex);
            }

            if (!typeof(LoggingProviderBase).IsAssignableFrom(providerType))
            {
                throw new ArgumentException(
                    SR.ProviderMustInheritFromType(settings.Name, providerType, typeof(LoggingProviderBase)));
            }

            return providerType;
        }

        private static string GetNonEmptyTypeName(ProviderSettings settings)
        {
            string typeName = (settings.Type == null) ? null : settings.Type.Trim();

            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException(SR.TypeNameMustBeSpecifiedForThisProvider(settings.Name));
            }

            return typeName;
        }

        private static NameValueCollection BuildProviderConfiguration(ProviderSettings settings)
        {
            NameValueCollection parameters = settings.Parameters;

            var providerConfiguration = new NameValueCollection(parameters.Count, StringComparer.Ordinal);

            foreach (string parameter in parameters)
            {
                providerConfiguration[parameter] = parameters[parameter];
            }

            return providerConfiguration;
        }

        private static void CompleteInitialization(LoggingProviderCollection providers,
            LoggingProviderBase defaultProvider)
        {
            foreach (LoggingProviderBase provider in providers)
            {
                provider.CompleteInitialization(providers, defaultProvider);
            }
        }
    }
}