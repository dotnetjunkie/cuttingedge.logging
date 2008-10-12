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
using System.Globalization;
using System.Reflection;
using System.Text;
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
                    SectionName));
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
                throw new ConfigurationErrorsException(SR.GetString(SR.NoDefaultLoggingProviderFound, 
                    SectionName));
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
            Validator.ValidateExceptionIsNotNull(exception);

            return provider.Log(EventType.Error, GetExceptionMessage(exception), null, exception);
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
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateExceptionIsNotNull(exception);

            return provider.Log(EventType.Error, message, null, exception);
        }

        /// <summary>Logs an error event to the default <see cref="Provider"/>.</summary>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <param name="source">A source where the event occured.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate for the current logging provider.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the supplied <paramref name="exception"/> or
        /// the <paramref name="source"/> are null references (Nothing in VB).</exception>
        public static object Log(Exception exception, MethodBase source)
        {
            Validator.ValidateExceptionIsNotNull(exception);
            Validator.ValidateSourceIsNotNull(source);

            return provider.Log(EventType.Error, GetExceptionMessage(exception), BuildMethodName(source), 
                exception);
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
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateExceptionIsNotNull(exception);
            Validator.ValidateSourceIsNotNull(source);

            return provider.Log(EventType.Error, message, BuildMethodName(source), exception);
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
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateExceptionIsNotNull(exception);
            Validator.ValidateSourceNotNullOrEmpty(source);

            return provider.Log(EventType.Error, message, source, exception);
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
            Validator.ValidateTypeInValidRange(type);
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateExceptionIsNotNull(exception);
            Validator.ValidateSourceIsNotNull(source);

            return provider.Log(type, message, BuildMethodName(source), exception);
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
            Validator.ValidateTypeInValidRange(type);
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateExceptionIsNotNull(exception);
            Validator.ValidateSourceNotNullOrEmpty(source);

            return provider.Log(type, message, source, exception);
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
            Validator.ValidateMessageNotNullOrEmpty(message);

            return provider.Log(EventType.Information, message, null, null);
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
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateTypeInValidRange(type);

            return provider.Log(type, message, null, null);
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
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateSourceIsNotNull(source);

            return provider.Log(EventType.Information, message, BuildMethodName(source), null);
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
            Validator.ValidateTypeInValidRange(type);
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateSourceIsNotNull(source);

            return provider.Log(type, message, BuildMethodName(source), null);
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
            Validator.ValidateMessageNotNullOrEmpty(message);
            Validator.ValidateTypeInValidRange(type);
            Validator.ValidateSourceNotNullOrEmpty(source);

            return provider.Log(type, message, source, null);
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

        private static string BuildMethodName(MethodBase method)
        {
            if (method == null)
            {
                return null;
            }

            ParameterInfo[] parameters = method.GetParameters();

            int initialCapacity =
                method.DeclaringType.FullName.Length +
                method.Name.Length + (15 * parameters.Length) +
                20;

            StringBuilder methodName = new StringBuilder(initialCapacity);

           methodName
                .Append(method.DeclaringType.FullName)
                .Append(".")
                .Append(method.Name)
                .Append("(");

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];

                if (index > 0)
                {
                    methodName.Append(", ");
                }

                if (parameter.IsOut == true)
                {
                    methodName.Append("out ");
                }

                if (parameter.IsRetval == true)
                {
                    methodName.Append("ret ");
                    methodName.Insert(0, " ");
                    methodName.Insert(0, parameter.ParameterType.Name);
                }
                else
                {
                    methodName.Append(parameter.ParameterType.Name);
                }
            }

            methodName.Append(")");

            return methodName.ToString();
        }

        private static string GetExceptionMessage(Exception exception)
        {
            string message = exception.Message;

            return String.IsNullOrEmpty(message) ? exception.GetType().Name : message;
        }

        /// <summary>Validates arguments.</summary>
        private static class Validator
        {
            internal static void ValidateExceptionIsNotNull(Exception exception)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("exception");
                }
            }

            internal static void ValidateMessageNotNullOrEmpty(string message)
            {
                if (String.IsNullOrEmpty(message))
                {
                    if (message != null)
                    {
                        throw new ArgumentException(SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString), 
                            "message");
                    }
                    else
                    {
                        throw new ArgumentNullException("message", 
                            SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString));
                    }
                }
            }

            internal static void ValidateTypeInValidRange(EventType type)
            {
                int eventType = (int)type;
                if (eventType < 0 || eventType > 2)
                {
                    throw new InvalidEnumArgumentException("type", eventType, typeof(EventType));
                }
            }

            internal static void ValidateSourceIsNotNull(MethodBase source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
            }

            internal static void ValidateSourceNotNullOrEmpty(string source)
            {
                if (String.IsNullOrEmpty(source))
                {
                    if (source != null)
                    {
                        throw new ArgumentException(SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString), 
                            "source");
                    }
                    else
                    {
                        throw new ArgumentNullException("source", 
                            SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString));
                    }
                }
            }
        }
    }
}