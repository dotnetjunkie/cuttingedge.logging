using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using Common.Logging.Factory;

using CuttingEdge.Logging;

namespace Common.Logging.CuttingEdge
{
    /// <summary>
    /// Adapts the CuttingEdge.Logging logging system to Common.Logging.
    /// </summary>
    /// <remarks>
    /// <para>The adapter has no configuration properties.</para>
    /// <para>
    /// This adapter for Common.Logging adds the notion of hierarchical loggers to CuttingEdge.Logging.
    /// This is a feature CuttingEdge.Logging doesn't support natively. Naming logging providers as
    /// namespaces (such as 'Company.Product.Layer') allows the <b>CuttingEdgeLoggerFactoryAdapter</b> to
    /// resolve loggers in an hierarchical manner.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following snippet shows how to configure CuttingEdge.Logging logging for Common.Logging:
    /// <code><![CDATA[
    /// <configuration>
    ///   <configSections>
    ///     <sectionGroup name="common">
    ///       <section name="logging" type="Common.Logging.ConfigurationSectionHandler, Common.Logging" />  
    ///     </sectionGroup>
    ///     <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging" />
    ///   </configSections>
    ///   <common>
    ///     <logging>
    ///       <factoryAdapter
    ///         type="Common.Logging.CuttingEdge.CuttingEdgeLoggerFactoryAdapter, Common.Logging.CuttingEdge"
    ///       />
    ///     </logging>
    ///   </common>
    ///   <logging defaultProvider="...">
    ///     <providers>
    ///       <-- configure CuttingEdge.Logging here -->
    ///     </providers>
    ///   </logging>
    /// </configuration>
    /// ]]></code>
    /// </example>
    public class CuttingEdgeLoggerFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {
        private const bool CaseSensitiveLoggerCache = false;

        // Calling Logger.Provider will initialize logging system. The Logger will throw an exception when
        // initialization failed. It is important to let the logging system fail as soon as possible when the
        // system is configured incorrectly.
        private readonly LoggingProviderBase DefaultProvider = Logger.Provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CuttingEdgeLoggerFactoryAdapter"/> class.
        /// </summary>
        public CuttingEdgeLoggerFactoryAdapter() : base(CaseSensitiveLoggerCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CuttingEdgeLoggerFactoryAdapter"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public CuttingEdgeLoggerFactoryAdapter(NameValueCollection properties)
            : base(CaseSensitiveLoggerCache)
        {
        }

        /// <summary>Create the specified named logger instance.</summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>An instance (instances are cached).</returns>
        protected override ILog CreateLogger(string name)
        {
            LoggingProviderBase provider = this.FindProviderByExactName(name);

            if (provider == null)
            {
                provider = this.FindClosestHierarchicalParentProvider(name);

                if (provider == null)
                {
                    provider = this.DefaultProvider;
                }
            }

            return new CuttingEdgeLogger(provider);
        }

        private LoggingProviderBase FindProviderByExactName(string providerName)
        {
            // Searching is case insensitive.
            return Logger.Providers[providerName];
        }

        private LoggingProviderBase FindClosestHierarchicalParentProvider(string providerName)
        {
            // example: A provider with name "My.Ya" is a parent of name "My.Ya.Foo.Bar". When the list of
            // providers contains a provider named "My" and a provider named "My.Ya", "My.Ya" will be returned
            // when the supplied providerName equals to "My.Ya.Foo.Bar" because "My.Ya" is hierarchical closer
            // to "My.Ya.Foo.Bar" than "My" is.
            var parents = new List<LoggingProviderBase>();

            foreach (LoggingProviderBase provider in Logger.Providers)
            {
                if (providerName.StartsWith(provider.Name + "."))
                {
                    parents.Add(provider);
                }
            }

            // All the names of the parents in the list have a name that is the starting part of the
            // providerName. Therefore, the parent with the longest name will be the closest parent.
            parents.Sort((x, y) => y.Name.Length - x.Name.Length);

            var closestParent = parents.Count > 0 ? parents[0] : null;

            return closestParent;
        }
    }
}
