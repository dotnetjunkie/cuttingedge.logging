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
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Globalization;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Collection of <see cref="LoggingProviderBase"/> objects.
    /// </summary>
    public sealed class LoggingProviderCollection : ProviderCollection, ICollection<LoggingProviderBase>
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value>True when readonly; false otherwise.</value>
        /// <returns>True if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool ICollection<LoggingProviderBase>.IsReadOnly
        {
            get { return false; }
        }

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
                throw new ArgumentException(SR.GetString(SR.ProviderParameterMustBeOfTypeX, 
                    typeof(LoggingProviderBase).Name), "provider");
            }

            base.Add(provider);
        }

        /// <summary>
        /// Copies the contents of the collection to the given array starting at the specified index. 
        /// </summary>
        /// <param name="array">The array to copy the elements of the collection to.</param>
        /// <param name="arrayIndex">The index of the collection item at which to start the copying process.
        /// </param>
        public void CopyTo(LoggingProviderBase[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>Adds the specified provider.</summary>
        /// <param name="provider">The provider.</param>
        void ICollection<LoggingProviderBase>.Add(LoggingProviderBase provider)
        {
            this.Add((ProviderBase)provider);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// True if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(LoggingProviderBase item)
        {
            if (item == null)
            {
                return false;
            }

            return this[item.Name] == item;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// True if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Remove(LoggingProviderBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            bool contains = ((ICollection<LoggingProviderBase>)this).Contains(item);

            if (contains)
            {
                this.Remove(item.Name);
            }

            return contains;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public new IEnumerator<LoggingProviderBase> GetEnumerator()
        {
            ICollection c = (ICollection)this;

            foreach (LoggingProviderBase provider in c)
            {
                yield return provider;
            }
        }
    }
}