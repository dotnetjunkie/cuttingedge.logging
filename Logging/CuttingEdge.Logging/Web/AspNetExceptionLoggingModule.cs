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
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Threading;
using System.Web;

namespace CuttingEdge.Logging.Web
{
    /// <summary>
    /// HttpModule to enable logging of unhandled exceptions in web applications.
    /// </summary>
    /// <example>
    /// This example demonstrates how to configure the provider in a web.config to enable logging.
    /// You should add the following parts to the web.config:
    /// <list type="number">
    ///     <item>Add a &lt;section&gt; to the &lt;configSections&gt; section pointing at the 
    ///         <see cref="LoggingSection"/> class.</item>
    ///     <item>Add a &lt;logging&gt; section to the &lt;configuration&gt; section to configure the logging
    ///         providers to use.</item>
    ///     <item>Add the <see cref="AspNetExceptionLoggingModule"/> to the &lt;httpModules&gt; section of the 
    ///         &lt;system.web&gt; section. This enables a global 'catch all' and logs unhandled exceptions in
    ///          your web application using the default logging provider you defined in step 2.</item>
    /// </list>
    /// The following snippet shows an example of how your web.config might look like.
    /// <code lang="xml"><![CDATA[
    /// <?xml version="1.0"?>
    /// <configuration>
    ///     <configSections>
    ///         <section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" />
    ///     </configSections>
    ///     <logging defaultProvider="WindowsEventLogLoggingProvider">
    ///         <providers>
    ///             <add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                    threshold="Information"
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
    public class AspNetExceptionLoggingModule : IHttpModule
    {
        /// <summary>Initializes static members of the AspNetExceptionLoggingModule class.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "logger",
            Justification = "Logger.Provider has to be called. Local can not be removed.")]
        static AspNetExceptionLoggingModule()
        {
            // Ensure that the Logger configuration is loaded and correct. An exception will be thrown
            // when the logger hasn't been configured properly.
            // We have to do this in the static constructor to prevent exception from being thrown in the 
            // LogException event. When an exception if thrown from that event it will be lost and it is 
            // impossible to detect why logging failed.
            ILogger logger = Logger.Provider;
        }

        /// <summary>Nothing to dispose.</summary>
        public void Dispose()
        {
            // nothing to dispose
        }

        /// <summary>Initializes a module and prepares it to handle requests.</summary>
        /// <param name="context">The <see cref="HttpApplication"/>.</param>
        public void Init(HttpApplication context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.RegisterHttpApplicationError(context);
            this.RegisterAppDomainUnhandledException();
        }

        internal void RegisterHttpApplicationError(HttpApplication context)
        {
            // Route any unhandled exceptions within a request to the logging infrastructure.
            context.Error += this.Log;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "The possible security exception is caught.")]
        internal virtual void RegisterAppDomainUnhandledException()
        {
            try
            {
                // Route any unhandled exceptions within the app domain to the logging infrastructure.
                AppDomain.CurrentDomain.UnhandledException += this.LogUnhandledException;
            }
            catch (SecurityException)
            {
                // This event can only be set when the application has the ControlAppDomain SecurityPermission.
                // When the application doesn't have this permission (mainly when not running in full trust),
                // we simply skip this. The logging system does not depend on this.
            }
        }

        internal virtual void Log(object sender, EventArgs e)
        {
            HttpApplication context = (HttpApplication)sender;

            Exception exception = context.Server.GetLastError();

            bool shouldLog = IsLoggableException(exception);

            if (shouldLog)
            {
                LogException(exception, LoggingEventType.Error);
            }
        }

        internal void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception != null)
            {
                LogException(exception, LoggingEventType.Critical);
            }
        }

        private static bool IsLoggableException(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            // Don't log if we're experiencing a Thread Abort. Thread aborts are pretty normal in ASP.NET
            // applications and are usually caused by a Response.End or Response.Redirect calls.
            return !(exception is ThreadAbortException);
        }

        private static void LogException(Exception exception, LoggingEventType severity)
        {
            Exception exceptionToLog = ExtractUsefulExceptionToLog(exception);

            string message = LoggingHelper.GetExceptionMessageOrExceptionType(exception);

            // We use the default LoggingProvider and we simply expect logging to succeed.
            Logger.Log(new LogEntry(severity, message, null, exceptionToLog));
        }

        private static Exception ExtractUsefulExceptionToLog(Exception exception)
        {
            // Often exceptions are wrapped by ASP.NET in a HttpUnhandledException. When this is the case, we
            // use the wrapped exception.
            if (exception.InnerException != null && exception is HttpUnhandledException)
            {
                return exception.InnerException;
            }
            else
            {
                return exception;
            }
        }
    }
}