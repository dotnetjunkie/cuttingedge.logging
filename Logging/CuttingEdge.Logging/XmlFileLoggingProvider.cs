using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages writing of logging information in an XML format to a file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returning an identifier for the logged event is not appropriate for this provider. This provider will
    /// always return null (Nothing in VB) from the <see cref="ILogger.Log(LogEntry)">Log</see> method.
    /// </para>
    /// <para>
    /// This class is used by the <see cref="Logger"/> class to provide Logging services for an 
    /// application that can write to the file system.
    /// </para>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="XmlFileLoggingProvider"/>
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
    /// <item>
    ///     <attribute>path</attribute>
    ///     <description>
    ///         A relative or absolute path to a file on the file system that will be used to append logging
    ///         information to. The file will be created when it does not exist. If the file can not be created
    ///         the provider will fail to initialize. This attribute is mandatory.
    ///     </description>
    /// </item>
    /// </list>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.
    /// </para>
    /// <para>
    /// The <see cref="XmlFileLoggingProvider"/> checks upon creation whether the configured file can be created
    /// and written to. The provider will fail when this is not the case. When the provider is configured in the
    /// application's configuration file, it will invalidate the complete provider model. This enables quick
    /// detection of a system's misconfiguration.
    /// </para>
    /// <para>
    /// The <see cref="XmlFileLoggingProvider"/> does not hold a file lock during the entire life time of the
    /// application domain. Instead it will close the file directly after logging an event. This alows other
    /// applications to process that file more easily. This however, can lead to a failure when other processes
    /// are writing to this file. Writing to that file from outside the context of the 
    /// <see cref="XmlFileLoggingProvider"/> is discouraged. Do use the 
    /// <see cref="LoggingProviderBase.FallbackProvider">FallbackProvider</see> mechanism to prevent a failure
    /// to bubble up the application's call stack.
    /// </para>
    /// <para>
    /// The following table shows a list of scenario's and for each scenario the table shows whether this
    /// scenario can be executed without a change that the provider might fail to log the event.
    /// <list type="table">
    /// <listheader>
    ///     <operation>Operation</operation>
    ///     <safe>Safe?</safe>
    /// </listheader>
    /// <item>
    ///     <operation>Logging to one  <see cref="XmlFileLoggingProvider"/> instance from a single threaded 
    ///     application.</operation>
    ///     <safe>Yes</safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to one <see cref="XmlFileLoggingProvider"/> instance in a single AppDomain from 
    ///     multiple threads.</operation>
    ///     <safe>Yes</safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to multiple <see cref="XmlFileLoggingProvider"/> instances in a single AppDomain
    ///     that reference all diferent log files.</operation>
    ///     <safe></safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to multiple <see cref="XmlFileLoggingProvider"/> instances in a single AppDomain 
    ///     that all reference the same log file.</operation>
    ///     <safe>Yes</safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to mutliple <see cref="XmlFileLoggingProvider"/> instances in <b>different</b>
    ///     AppDomains that reference one single log file.</operation>
    ///     <safe><b>No</b></safe>
    /// </item>
    /// <item>
    ///     <operation>Writing to the log file outside the context of an <see cref="XmlFileLoggingProvider"/>
    ///     instance in that same AppDomain.</operation>
    ///     <safe><b>No</b></safe>
    /// </item>
    /// <item>
    ///     <operation>Writing to the log file from within another AppDomain in the same process.</operation>
    ///     <safe><b>No</b></safe>
    /// </item>
    /// <item>
    ///     <operation>Writing to the log file from another process.</operation>
    ///     <safe><b>No</b></safe>
    /// </item>
    /// </list>
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
    ///     <logging defaultProvider="XmlFileLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="XmlFileLoggingProvider"
    ///                 type="CuttingEdge.Logging.XmlFileLoggingProvider, CuttingEdge.Logging"
    ///                 description="XML file logging provider"
    ///                 threshold="Information"
    ///                 path="log.xml"
    ///             />
    ///         </providers>
    ///     </logging>
    /// </configuration>
    /// ]]></code>
    /// </example>
    /// <example>
    /// <para>
    /// The <see cref="XmlFileLoggingProvider"/> writes to the configured file by appending new entries to the
    /// file one by one, node by node. The <see cref="XmlFileLoggingProvider"/> will therefore produce a
    /// document with multiple root nodes. Because of this the produced document will not be a valid XML 
    /// document. This is a program when using this file for automatic analysis using some sort of XML parser.
    /// </para>
    /// <para>
    /// A workaround to this problem, a user can define a valid XML document that includes the log file by
    /// defining an <b>ENTITY</b> that references that file. This way most XML parsers can successfully parse
    /// that document.
    /// </para>
    /// <para>
    /// Below is an example of such a document. The document references a by the 
    /// <see cref="XmlFileLoggingProvider"/> produced log file called 'log.xml'.
    /// </para>
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <!DOCTYPE LogEntries [ <!ENTITY Entries SYSTEM "log.xml"> ]>
    /// <LogEntries>
    ///     &Entries;
    /// </LogEntries>
    /// ]]></code>
    /// </example>
    /// <example>
    /// The <see cref="XmlFileLoggingProvider"/> allows to be extended by inheriting from it and overriding
    /// the <see cref="WriteEntryInnerElements"/> method. This allows extra information to be writing to the
    /// log file or original information to be replaced entirely. Here is an example of a provider that adds
    /// a single &lt;MachineName&gt; element to each entry.
    /// <code><![CDATA[
    /// public class MachineNameXmlFileLoggingProvider : XmlFileLoggingProvider
    /// {
    ///    protected override void WriteEntryInnerElements(XmlWriter xmlWriter, LogEntry entry)
    ///    {
    ///        base.WriteEntryInnerElements(xmlWriter, entry);
    ///
    ///        xmlWriter.WriteStartElement("MachineName");
    ///        xmlWriter.WriteValue(Environment.MachineName);
    ///        xmlWriter.WriteEndElement();
    ///    }
    /// }
    /// ]]></code>
    /// </example>
    public class XmlFileLoggingProvider : LoggingProviderBase
    {
        // Collection of lock objects for all paths used by XmlFileLoggingProvider instances in the current
        // AppDomain. We assume all paths are case insensitive (although we know this is not completely correct).
        private static readonly Dictionary<string, object> PathLockers =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileLoggingProvider"/> class.
        /// </summary>
        public XmlFileLoggingProvider()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="XmlFileLoggingProvider"/> class.</summary>
        /// <param name="threshold">The <see cref="LoggingEventType"/> logging threshold. The threshold limits
        /// the number of event logged. <see cref="LoggingProviderBase.Threshold">Threshold</see> for more information.</param>
        /// <param name="path">The path of the file to log to. Both relative and rooted paths can be used.</param>
        /// <param name="fallbackProvider">The optional fallback provider.</param>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="threshold"/> has an
        /// invalid value.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is an empty string.</exception>
        /// <exception cref="ProviderException">Thrown when there was an error initializing the provider.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification =
            "Method 'CheckAuthorizationsByCreatingFile' uses the virtual property 'Name' for building the " +
            "exception message, which is not a big problem.")]
        public XmlFileLoggingProvider(LoggingEventType threshold, string path,
            LoggingProviderBase fallbackProvider)
            : base(threshold, fallbackProvider)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Length == 0)
            {
                throw new ArgumentException(SR.ValueShouldNotBeAnEmptyString(), "path");
            }

            this.path = GetFullCanonicalPath(path);

            this.CheckAuthorizationsByCreatingFile();
        }

        /// <summary>Gets the rooted canonical path provided with this provider.</summary>
        /// <value>The path to write to.</value>
        public string Path
        {
            get { return this.path; }
        }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name of the provider is null or when the
        /// <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the name of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an attempt is made to call Initialize on a
        /// provider after the provider has already been initialized.</exception>
        /// <exception cref="ProviderException">Thrown when the <paramref name="config"/> contains
        /// unrecognized attributes or the provider could not initialized correctly.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            LoggingHelper.SetDescriptionWhenMissing(config, "XML file logging provider");

            // Call initialize first.
            base.Initialize(name, config);

            // Performing implementation-specific provider initialization here.
            this.InitializePath(config);

            // Check if the configuration is valid, before testing file authorizations.
            this.CheckForUnrecognizedAttributes(name, config);

            this.CheckAuthorizationsByCreatingFile();
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>This logger returns null, because returning an id is inappropriate.</returns>
        protected override object LogInternal(LogEntry entry)
        {
            // This lock prevents multiple instances from simultaneously writing to the same path (within the
            // same AppDomain).
            var locker = this.GetLockObjectForCurrentPath();

            lock (locker)
            {
                this.WriteEntryToDisk(entry);
            }

            // Returning an ID is inappropriate for this type of logger.
            return null;
        }

        /// <summary>Writes the entry's properties to the <paramref name="xmlWriter"/>.</summary>
        /// <param name="xmlWriter">The XML writer to write to.</param>
        /// <param name="entry">The entry.</param>
        protected virtual void WriteEntryInnerElements(XmlWriter xmlWriter, LogEntry entry)
        {
            WriteElement(xmlWriter, "EventTime", DateTime.Now);
            WriteElement(xmlWriter, "Severity", entry.Severity.ToString());
            WriteElement(xmlWriter, "Message", entry.Message);
            WriteElement(xmlWriter, "Source", entry.Source);
            
            if (entry.Exception != null)
            {
                WriteException(xmlWriter, "Exception", entry.Exception);
            }
        }

        private object GetLockObjectForCurrentPath()
        {
            object pathLocker;

            lock (PathLockers)
            {
                if (!PathLockers.TryGetValue(this.Path, out pathLocker))
                {
                    pathLocker = new object();
                    PathLockers.Add(this.Path, pathLocker);
                }
            }

            return pathLocker;
        }

        private void WriteEntryToDisk(LogEntry entry)
        {
            using (var file = OpenOrCreateFileForAppending(this.Path))
            {
                using (var xml = CreateXmlWriter(file))
                {
                    xml.WriteStartElement("LogEntry");

                    this.WriteEntryInnerElements(xml, entry);

                    xml.WriteEndElement();
                }

                // By writing a new line we 
                WriteNewLine(file);
            }
        }

        private static Stream OpenOrCreateFileForAppending(string path)
        {
            return new FileStream(path, FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write);
        }

        private void InitializePath(NameValueCollection config)
        {
            const string PathAttribute = "path";

            string path = config[PathAttribute];

            // Throw exception when no connectionStringName is provided
            if (string.IsNullOrEmpty(path))
            {
                throw new ProviderException(SR.MissingConnectionStringAttribute(this.Name));
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove(PathAttribute);

            this.path = GetFullCanonicalPath(path);
        }

        private void CheckAuthorizationsByCreatingFile()
        {
            try
            {
                // This lock is needed at this point, because this instance could be created at a time that
                // other instances are already writing to that same log file. (This can only happen when
                // instances are manually created and not when the they are configured in the app.config).
                lock (this.GetLockObjectForCurrentPath())
                {
                    File.AppendAllText(this.Path, string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw new ProviderException(SR.MissingPathAttribute(this.Name, this.Path, ex.Message), ex);
            }
        }

        private static string GetFullCanonicalPath(string path)
        {
            string fullPath;

            if (System.IO.Path.IsPathRooted(path))
            {
                fullPath = path;
            }
            else
            {
                fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return GetCanonicalPath(fullPath);
        }

        private static string GetCanonicalPath(string fullPath)
        {
            // The canonical name is the path in its simplest presentation. Determining the canonical name is
            // important, because it allows writing to the same file by multiple instances thread-safe.
            // For instance that path 'c:\test\..\foo.xml' and 'c:\test\bla\..\..\foo.xml' have the same
            // canonical representation 'c:\foo.xml'. Path.GetFullPath gets the canonical name.
            return System.IO.Path.GetFullPath(fullPath);
        }

        private static XmlWriter CreateXmlWriter(Stream source)
        {
            var settings = new XmlWriterSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                Indent = true,
                IndentChars = "\t"
            };

            return XmlWriter.Create(source, settings);
        }

        private static void WriteException(XmlWriter xmlWriter, string elementName, Exception exception)
        {
            xmlWriter.WriteStartElement(elementName);

            WriteElement(xmlWriter, "Message", exception.Message);
            WriteElement(xmlWriter, "StackTrace", exception.StackTrace);

            // Recursive call:
            WriteInnerExceptions(xmlWriter, exception);

            xmlWriter.WriteEndElement();
        }

        private static void WriteInnerExceptions(XmlWriter xmlWriter, Exception parentException)
        {
            var innerExceptions = LoggingHelper.GetInnerExceptions(parentException);

            if (innerExceptions.Length > 0)
            {
                xmlWriter.WriteStartElement("InnerExceptions");

                foreach (var innerException in innerExceptions)
                {
                    // Recursive call:
                    WriteException(xmlWriter, "InnerException", innerException);
                }

                xmlWriter.WriteEndElement();
            }
        }

        private static void WriteElement(XmlWriter xmlWriter, string elementName, object value)
        {
            xmlWriter.WriteStartElement(elementName);
            xmlWriter.WriteValue(value ?? string.Empty);
            xmlWriter.WriteEndElement();
        }

        private static void WriteNewLine(Stream source)
        {
            using (var writer = new StreamWriter(source))
            {
                writer.WriteLine();
            }
        }
    }
}