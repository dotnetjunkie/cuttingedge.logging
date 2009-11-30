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
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Globalization;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Collection of <see cref="LoggingProviderBase"/> objects.
    /// </summary>
    public sealed class LoggingProviderCollection : ProviderCollection, IEnumerable<LoggingProviderBase>
    {
        /// <summary>Represents the collection of <see cref="LoggingProviderBase"/> objects.</summary>
        /// <param name="name">The key by which the provider is identified.</param>
        /// <returns>The provider with the specified name.</returns>
        public new LoggingProviderBase this[string name]
        {
            get { return (LoggingProviderBase)base[name]; }
        }

        /// <summary>Adds a provider to the collection. </summary>
        /// <param name="provider">The provider to be added.</param>
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider"); 
            }

            if (!(provider is LoggingProviderBase))
            {
                throw new ArgumentException(SR.ProviderParameterMustBeOfTypeX(typeof(LoggingProviderBase)), 
                    "provider");
            }

            base.Add(provider);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<LoggingProviderBase> IEnumerable<LoggingProviderBase>.GetEnumerator()
        {
            ICollection collection = this;

            foreach (object provider in collection)
            {
                yield return (LoggingProviderBase)provider;
            }
        }
    }
}