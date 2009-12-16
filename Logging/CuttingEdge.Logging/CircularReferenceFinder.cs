using System.Collections.Generic;
using System.Configuration;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Enabled searching for circular references in a chain of logging providers.
    /// </summary>
    internal static class CircularReferenceFinder
    {
        /// <summary>Validates the provided <paramref name="providers"/> and throws an 
        /// <see cref="ConfigurationErrorsException"/> when a circular reference is found.</summary>
        /// <param name="providers">The providers.</param>
        /// <exception cref="ConfigurationErrorsException">Thrown when circular reference is found.</exception>
        internal static void Validate(LoggingProviderCollection providers)
        {
            var chain = LoggingProviderChain.Empty;

            foreach (LoggingProviderBase provider in providers)
            {
                ValidateProvider(provider, chain);
            }
        }

        private static void ValidateProvider(LoggingProviderBase provider, LoggingProviderChain chain)
        {
            ThrowWhenProviderIsInChain(provider, chain);

            // Note that LoggingProviderChain is immutable. Adding involves creating a new instance.
            var chainWithProvider = chain.Add(provider);

            CheckAllReferencedProviders(provider.GetReferencedProviders(), chainWithProvider);
        }

        private static void ThrowWhenProviderIsInChain(LoggingProviderBase providerToValidate, 
            LoggingProviderChain providersInChain)
        {
            // Check whether the supplied provider is already directly or indirectly referencing itself.
            bool providerIsSelfReferenced = providersInChain.Contains(providerToValidate);

            if (providerIsSelfReferenced)
            {
                throw new ConfigurationErrorsException(
                    SR.CircularReferenceInLoggingSection(Logger.SectionName, providerToValidate.Name));
            }
        }

        private static void CheckAllReferencedProviders(IEnumerable<LoggingProviderBase> referencedProviders, 
            LoggingProviderChain chain)
        {
            // Most of the time referenced providers are simply fallback providers.
            foreach (var provider in referencedProviders)
            {
                ValidateProvider(provider, chain);
            }
        }

        /// <summary>
        /// Immutable collection of <see cref="LoggingProviderBase"/> objects. By using an immutable list, the
        /// <see cref="CircularReferenceFinder"/> can easily provide the collection to the recursive method
        /// call, without having to make a copy of that list on each call.
        /// </summary>
        private sealed class LoggingProviderChain
        {
            internal static readonly LoggingProviderChain Empty = new LoggingProviderChain(null, null);

            private readonly LoggingProviderBase provider;
            private readonly LoggingProviderChain list;

            private LoggingProviderChain(LoggingProviderBase provider, LoggingProviderChain list)
            {
                this.provider = provider;
                this.list = list;
            }

            internal bool IsEmpty
            {
                get { return this.list == null; }
            }

            internal LoggingProviderChain Add(LoggingProviderBase provider)
            {
                return new LoggingProviderChain(provider, this);
            }

            internal bool Contains(LoggingProviderBase provider)
            {
                if (this.IsEmpty)
                {
                    return false;
                }

                if (this.provider == provider)
                {
                    return true;
                }

                return this.list.Contains(provider);
            }
        }
    }
}
