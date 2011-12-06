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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Provider;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages storage of logging information into memory. Use the <see cref="GetLoggedEntries"/> to retrieve
    /// a copy of the internal cache and <see cref="Clear"/> to clear the internal cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the <b>MemoryLoggingProvider</b> succeeds storing the event in the internal list, the
    /// <see cref="ILogger.Log(LogEntry)">Log</see> method will return an <see cref="Int32"/> with the
    /// index of the logging event in the internal collection. Note that when <see cref="Clear">Clear</see> is
    /// called, counting will start over. Please note that the provider will return null when the event was
    /// not stored. This happens in the following situations:
    /// <list type="bullet">
    ///     <item>The provider's <see cref="LoggingProviderBase.Threshold">Threshold</see> was set higher than
    ///     the event's <see cref="LogEntry.Severity">Severity</see>.</item>
    ///     <item>There was an exception logging the event, but it was logged successfully to the
    ///     <see cref="LoggingProviderBase.FallbackProvider">FallbackProvider</see>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This class is used by the <see cref="Logger"/> class to provide Logging services that logs to an
    /// in-memory cache.
    /// </para>
    /// <para>
    /// <b>WARNING:</b> This class is only suitable for debugging purposes. The internal cache that the 
    /// <see cref="MemoryLoggingProvider"/> will hold is not cleared automatically and will grow unlimited.
    /// This could lead to <see cref="OutOfMemoryException"/>s in production environments.
    /// </para>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="MemoryLoggingProvider"/>
    /// configuration:
    /// <list type="table">  
    /// <listheader>
    ///     <attribute>Attribute</attribute>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <attribute>name</attribute>
    ///     <description>
    ///         The name of the provider. This attribute is mandatory.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>description</attribute>
    ///     <description>
    ///         A description of the provider. This attribute is optional.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>fallbackProvider</attribute>
    ///     <description>
    ///         A fallback provider that this provider will use when logging failed. The value must contain 
    ///         the name of an existing logging provider. This attribute is optional.
    ///     </description>
    /// </item>  
    /// <item>
    ///     <attribute>threshold</attribute>
    ///     <description>
    ///         The logging threshold. The threshold limits the number of events logged. The threshold can be
    ///         defined as follows: Debug &lt; Information &lt; Warning &lt; Error &lt; Critical. i.e., When
    ///         the threshold is set to Information, events with a severity of Debug  will not be logged. When
    ///         no value is specified, all events are logged. This attribute is optional.
    ///      </description>
    /// </item>
    /// </list>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the
    /// Logging section, which can also be accessed as members of the <see cref="LoggingSection"/> class.
    /// The following configuration file example shows how to specify values declaratively for the
    /// Logging section.
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging" />
    ///     </configSections>
    ///     <logging defaultProvider="MemoryLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="MemoryLoggingProvider"
    ///                 type="CuttingEdge.Logging.MemoryLoggingProvider, CuttingEdge.Logging"
    ///                 description="Memory logging provider"
    ///                 threshold="Debug"
    ///             />
    ///         </providers>
    ///     </logging>
    /// </configuration>
    /// ]]></code>
    /// The following example shows how to use the class:
    /// <code lang="cs"><![CDATA[
    /// Logger.Log(LoggingEventType.Error, "This is an error");
    /// 
    /// MemoryLoggingProvider memoryLogger = (MemoryLoggingProvider)Logger.Provider;
    /// 
    /// foreach (LogEntry entry in memoryLogger.GetLoggedEntries())
    /// {
    ///     Console.WriteLine(entry);
    /// }
    /// 
    /// // Clear the cache
    /// memoryLogger.Clear();
    /// ]]></code>
    /// </example>
    public class MemoryLoggingProvider : LoggingProviderBase
    {
        private readonly object locker = new object();
        private readonly List<LogEntry> loggingEvents = new List<LogEntry>();

        /// <summary>Initializes a new instance of the <see cref="MemoryLoggingProvider"/> class.</summary>
        public MemoryLoggingProvider()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MemoryLoggingProvider"/> class.</summary>
        /// <param name="threshold">The <see cref="LoggingEventType"/> logging threshold. The threshold limits
        /// the number of event logged. <see cref="LoggingProviderBase.Threshold">Threshold</see> for more 
        /// information.</param>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="threshold"/> has an
        /// invalid value.</exception>
        public MemoryLoggingProvider(LoggingEventType threshold)
            : base(threshold, null)
        {
        }

        /// <summary>
        /// Gets a copy of the internal cache of <see cref="LogEntry"/> objects.
        /// </summary>
        /// <returns>An array of <see cref="LogEntry"/> objects.</returns>
        public LogEntry[] GetLoggedEntries()
        {
            lock (this.locker)
            {
                return this.loggingEvents.ToArray();
            }
        }

        /// <summary>Clears the internal cache of <see cref="LogEntry"/> objects.</summary>
        public void Clear()
        {
            lock (this.locker)
            {
                this.loggingEvents.Clear();
            }
        }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
        /// <remarks>
        /// Inheritors should call <b>base.Initialize</b> before performing implementation-specific provider
        /// initialization and call <see cref="LoggingProviderBase.CheckForUnrecognizedAttributes"/> last.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the name of the provider is null or when the
        /// <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the name of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an attempt is made to call Initialize on a
        /// provider after the provider has already been initialized.</exception>
        /// <exception cref="ProviderException">Thrown when the <paramref name="config"/> contains
        /// unrecognized attributes.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            LoggingHelper.SetDescriptionWhenMissing(config, "Memory logging provider");

            // Call initialize first.
            base.Initialize(name, config);

            // Always call this method last
            this.CheckForUnrecognizedAttributes(name, config);
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>An <see cref="Int32"/> with the index of the inserted event in the internal cache.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is a null (Nothing
        /// in VB) reference.</exception>
        protected override object LogInternal(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            lock (this.locker)
            {
                this.loggingEvents.Add(entry);
                
                return this.loggingEvents.Count - 1;
            }
        }
    }
}