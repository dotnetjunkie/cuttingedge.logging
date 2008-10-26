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
    /// <list>
    /// <item>1. Add a &lt;section&gt; to the &lt;configSections&gt; section pointing at the 
    /// <see cref="LoggingSection"/> class.</item>
    /// <item>Add a &lt;logging&gt; section to the &lt;configuration&gt; to configer the logging providers to
    /// use.</item>
    /// <item>Add the <see cref="AspNetExceptionLoggingModule"/> to the &lt;system.web&gt;/&lt;httpModules&gt; 
    /// section. This enables a global 'catch all' the logs unhandeld exceptions in your web application using
    /// the default logging provider you defined in step 2.</item>
    /// </list>
    /// The following snippet shows an example of how your web.config might look like.
    /// <code lang="xml">
    /// &lt;?xml version="1.0"?&gt;
    /// &lt;configuration&gt;
    ///     &lt;configSections&gt;
    ///         &lt;section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" /&gt;
    ///     &lt;/configSections&gt;
    ///     &lt;logging defaultProvider="WindowsEventLogLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="WindowsEventLogLoggingProvider"
    ///                 type="CuttingEdge.Logging.WindowsEventLogLoggingProvider, CuttingEdge.Logging"
    ///                    threshold="Information"
    ///                 source="MyWebApplication"
    ///                 logName="MyWebApplication"
    ///                 description="Windows event log logging provider"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    ///     &lt;system.web&gt;
    ///         &lt;httpModules&gt;
    ///             &lt;add name="ExceptionLogger" 
    ///                 type="CuttingEdge.Logging.Web.AspNetExceptionLoggingModule, CuttingEdge.Logging"/&gt;
    ///         &lt;/httpModules&gt;
    ///     &lt;/system.web&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </example>
    public class AspNetExceptionLoggingModule : IHttpModule
    {
        /// <summary>
        /// Initializes static members of the AspNetExceptionLoggingModule class.
        /// </summary>
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

        /// <summary>
        /// Initializes a module and prepares it to handle requests. 
        /// </summary>
        /// <param name="context">The <see cref="HttpApplication"/>.</param>
        public void Init(HttpApplication context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Error += this.LogException;
        }

        private void LogException(object sender, EventArgs e)
        {
            HttpApplication context = (HttpApplication)sender;

            Exception exception = context.Server.GetLastError();

            // Don't log if we're experiencing a Thread Abort. Thread aborts are pretty normal in ASP.NET
            // applications and are usually caused by a Response.End or Response.Redirect calls.
            if (exception != null && !(exception is ThreadAbortException))
            {
                // Often exceptions are wrapped by ASP.NET in a HttpUnhandledException. When this is the
                // case, we use the wrapped exception.
                if (exception is HttpUnhandledException && exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }

                // We use the default LoggingProvider and we simply expect logging to succeed.
                Logger.Log(exception);
            }
        }
    }
}