﻿#region Copyright (c) 2008 S. van Deursen
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
using System.ComponentModel;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Defines a method that logs a given message with type and optional source and exception to the logging
    /// store.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Adds a event to the log.</summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="message">The message of the event.</param>
        /// <param name="source">The optional source to log.</param>
        /// <param name="exception">An optional exception to log.</param>
        /// <returns>The id of the saved log (or null when an id is not appropriate for this type of logger.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the given <paramref name="message"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="message"/> is an empty string.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown when<paramref name="type"/> has an invalid value.</exception>
        object Log(LoggingEventType severity, string message, string source, Exception exception);
    }
}
