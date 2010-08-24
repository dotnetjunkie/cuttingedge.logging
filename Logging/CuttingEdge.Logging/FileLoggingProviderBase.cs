using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Abstract base class for manages writing of logging information to a file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returning an identifier for the logged event is not appropriate for this provider. This provider will
    /// always return null (Nothing in VB) from the <see cref="ILogger.Log(LogEntry)">Log</see> method.
    /// </para>
    /// <para>
    /// This class is abstract and can not be created directly. See the <see cref="XmlFileLoggingProvider"/>
    /// for a concrete implementation. Implementers can inherit from this class to create a specific logger 
    /// that allows writing to a file. Implementers should override the <see cref="SerializeLogEntry"/> method.
    /// </para>
    /// <para>
    /// The <see cref="FileLoggingProviderBase"/> checks upon creation whether the configured file can be 
    /// created and written to. The provider will fail when this is not the case. When the provider is 
    /// configured in the application's configuration file, it will invalidate the complete provider model. 
    /// This enables quick detection of a system's misconfiguration.
    /// </para>
    /// <para>
    /// The <see cref="FileLoggingProviderBase"/> does not hold a file lock during the entire life time of the
    /// application domain. Instead it will close the file directly after logging a single event. This allows
    /// other applications to process that file more easily. This however, can lead to a failure when other 
    /// processes are writing to this same file. Writing to that file from outside the context of the 
    /// <see cref="FileLoggingProviderBase"/> or one of it's implementations is discouraged. Configer a
    /// <see cref="LoggingProviderBase.FallbackProvider">FallbackProvider</see> to prevent a failure to bubble
    /// up the call stack.
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
    ///     <operation>Logging to one <see cref="FileLoggingProviderBase"/> instance (or one of its 
    ///     decendants) from a single threaded application.</operation>
    ///     <safe>Yes</safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to one <see cref="FileLoggingProviderBase"/> instance (or one of its 
    ///     decendants) in a single AppDomain from multiple threads.</operation>
    ///     <safe>Yes</safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to multiple <see cref="FileLoggingProviderBase"/> instances (or its decendants) 
    ///     in a single AppDomain that reference all diferent log files.</operation>
    ///     <safe></safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to multiple <see cref="FileLoggingProviderBase"/> instances (or its decendants) 
    ///     in a single AppDomain that all reference the same log file.</operation>
    ///     <safe>Yes</safe>
    /// </item>
    /// <item>
    ///     <operation>Logging to mutliple <see cref="FileLoggingProviderBase"/> instances (or its decendants) 
    ///     in <b>different</b> AppDomains that reference one single log file.</operation>
    ///     <safe><b>No</b></safe>
    /// </item>
    /// <item>
    ///     <operation>Writing to the log file outside the context of an <see cref="FileLoggingProviderBase"/> 
    ///     instance (or one of its decendants) in that same AppDomain.</operation>
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
    public abstract class FileLoggingProviderBase : LoggingProviderBase
    {
        // Collection of lock objects for all paths used by XmlFileLoggingProvider instances in the current
        // AppDomain. We assume all paths are case insensitive (although we know this is not completely correct).
        private static readonly Dictionary<string, object> PathLockers =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private string path;

        /// <summary>Initializes a new instance of the <see cref="FileLoggingProviderBase"/> class.</summary>
        protected FileLoggingProviderBase()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FileLoggingProviderBase"/> class.</summary>
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
        protected FileLoggingProviderBase(LoggingEventType threshold, string path,
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

        // This method is virtual to allow the file system to be replaced for testing.
        internal virtual void AppendAllText(string contents)
        {
            // Opens or creates a file and appends the contents to it.
            File.AppendAllText(this.Path, contents);
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>This logger returns null, because returning an id is inappropriate.</returns>
        protected override object LogInternal(LogEntry entry)
        {
            // This lock prevents multiple instances from simultaneously writing to the same path (within the
            // same AppDomain).
            var locker = this.GetLockObjectForCurrentPath();

            // We lock after this call. Not only does this make the lock much shorter, we don't want to make
            // a virtual call while holding a lock.
            string contents = this.SerializeLogEntry(entry);

            lock (locker)
            {
                this.AppendAllText(contents);
            }

            // Returning an ID is inappropriate for this type of logger.
            return null;
        }

        /// <summary>
        /// Serializes the log entry to a string representation that that can be appended to the configured
        /// <see cref="Path"/>.
        /// </summary>
        /// <param name="entry">The entry to log.</param>
        /// <returns>The string representation of the supplied <paramref name="entry"/>.</returns>
        protected abstract string SerializeLogEntry(LogEntry entry);

        private void InitializePath(NameValueCollection config)
        {
            const string PathAttribute = "path";

            string path = config[PathAttribute];

            // Throw exception when no connectionStringName is provided
            if (string.IsNullOrEmpty(path))
            {
                throw new ProviderException(SR.MissingAttribute(PathAttribute, this.Name));
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove(PathAttribute);

            this.path = GetFullCanonicalPath(path);
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

        private void CheckAuthorizationsByCreatingFile()
        {
            try
            {
                // This lock is needed at this point, because this instance could be created at a time that
                // other instances are already writing to that same log file. (This can only happen when
                // instances are manually created and not when the they are configured in the app.config).
                lock (this.GetLockObjectForCurrentPath())
                {
                    this.AppendAllText(string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw new ProviderException(SR.ErrorCreatingOrWritingToFile(this.Name, this.Path, ex.Message),
                    ex);
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
    }
}