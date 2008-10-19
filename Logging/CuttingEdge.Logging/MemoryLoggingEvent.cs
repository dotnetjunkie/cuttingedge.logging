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
using System.Diagnostics;
using System.Globalization;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Container for event data for the <see cref="MemoryLoggingProvider"/>.
    /// </summary>
    [DebuggerDisplay("LoggingEvent (Severity: {Severity}, Message: {Message}, Source: {Source})")]
    public class MemoryLoggingEvent
    {
        private readonly LoggingEventType severity;
        private readonly string message;
        private readonly string source;
        private readonly Exception exception;

        /// <summary>Initializes a new instance of the <see cref="MemoryLoggingEvent"/> class.</summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="exception">The exception.</param>
        public MemoryLoggingEvent(LoggingEventType severity, string message, string source, Exception exception)
        {
            LoggingHelper.ValidateSeverityInValidRange(severity);
            LoggingHelper.ValidateMessageNotNullOrEmpty(message);

            this.severity = severity;
            this.message = message;
            this.source = source;
            this.exception = exception;
        }

        /// <summary>Gets the severity of the event.</summary>
        /// <value>The severity.</value>
        public LoggingEventType Severity
        {
            get { return this.severity; }
        }

        /// <summary>Gets the message of the event.</summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return this.message; }
        }

        /// <summary>Gets the optional event source.</summary>
        /// <value>The source.</value>
        public string Source
        {
            get { return this.source; }
        }

        /// <summary>Gets the optional exception.</summary>
        /// <value>The exception.</value>
        public Exception Exception
        {
            get { return this.exception; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="MemoryLoggingEvent"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="MemoryLoggingEvent"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Severity: {0}, Message: {1}, Source: {2}", 
                this.severity, this.message, this.source);
        }
    }
}
