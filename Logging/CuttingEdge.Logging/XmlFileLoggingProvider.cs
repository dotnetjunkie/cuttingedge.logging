using System;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
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
    /// <see cref="XmlFileLoggingProvider"/> or any other type that inherits from the 
    /// <see cref="FileLoggingProviderBase"/> is discouraged. Do use the 
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
    public class XmlFileLoggingProvider : FileLoggingProviderBase
    {
        private static readonly byte[] NewLine = new UTF8Encoding(false, true).GetBytes(Environment.NewLine);

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
            : base(threshold, path, fallbackProvider)
        {
        }

        internal virtual DateTime CurrentTime
        {
            get { return DateTime.Now; }
        }

        /// <summary>
        /// Serializes the log entry to a XML string representation that that can be appended to the
        /// configured <see cref="Path"/>.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>The string representation of the supplied <paramref name="entry"/>.</returns>
        protected override string SerializeLogEntry(LogEntry entry)
        {
            int initialCapacity = EstimateCapacity(entry);

            using (var stream = new MemoryStream(initialCapacity))
            {
                this.SerializeLogEntryToXml(entry, stream);

                // By writing a new line we ensure pretty formatting.
                AppendWithNewLine(stream);

                return ConvertStreamToString(stream);
            }
        }

        /// <summary>Writes the entry's properties to the <paramref name="writer"/>.</summary>
        /// <param name="writer">The XML writer to write to.</param>
        /// <param name="entry">The entry.</param>
        protected virtual void WriteEntryInnerElements(XmlWriter writer, LogEntry entry)
        {
            WriteElement(writer, "EventTime", this.CurrentTime);
            WriteElement(writer, "Severity", entry.Severity.ToString());
            WriteElement(writer, "Message", entry.Message);
            WriteElement(writer, "Source", entry.Source);
            
            if (entry.Exception != null)
            {
                WriteExceptionRecursive(writer, "Exception", entry.Exception);
            }
        }

        private void SerializeLogEntryToXml(LogEntry entry, Stream stream)
        {
            using (XmlWriter writer = CreateXmlWriter(stream))
            {
                writer.WriteStartElement("LogEntry");

                this.WriteEntryInnerElements(writer, entry);

                writer.WriteEndElement();
            }
        }

        private static int EstimateCapacity(LogEntry entry)
        {
            // This is a pretty naive capicity estimation. We might need some 
            return entry.Exception != null ? 4096 : 512;
        }

        private static string ConvertStreamToString(Stream stream)
        {
            stream.Position = 0;

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
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

        private static void WriteExceptionRecursive(XmlWriter writer, string elementName, Exception exception)
        {
            writer.WriteStartElement(elementName);

            WriteElement(writer, "Message", exception.Message);
            WriteElement(writer, "StackTrace", exception.StackTrace);

            WriteInnerExceptionsRecursive(writer, exception);

            writer.WriteEndElement();
        }

        private static void WriteInnerExceptionsRecursive(XmlWriter writer, Exception parentException)
        {
            var innerExceptions = LoggingHelper.GetInnerExceptions(parentException);

            if (innerExceptions.Length > 0)
            {
                writer.WriteStartElement("InnerExceptions");

                foreach (var innerException in innerExceptions)
                {
                    WriteExceptionRecursive(writer, "InnerException", innerException);
                }

                writer.WriteEndElement();
            }
        }

        private static void WriteElement(XmlWriter writer, string elementName, object value)
        {
            writer.WriteStartElement(elementName);
            writer.WriteValue(value ?? string.Empty);
            writer.WriteEndElement();
        }

        private static void AppendWithNewLine(Stream source)
        {
            source.Write(NewLine, 0, NewLine.Length);
        }
    }
}