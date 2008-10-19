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

namespace CuttingEdge.Logging
{
    /// <summary>
    /// The type of the event.
    /// </summary>
    public enum LoggingEventType
    {
        /// <summary>
        /// A debug event. This indicates a verbose event, usefull during development.
        /// </summary>
        Debug = 0,

        /// <summary>
        /// An information event. This indicates a significant, successful operation.
        /// </summary>
        Information = 1,

        /// <summary>
        /// A warning event. This indicates a problem that is not immediately significant, but that may 
        /// signify conditions that could cause future problems.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// An error event. This indicates a significant problem the user should know about; usually a loss of
        /// functionality or data.
        /// </summary>
        Error = 3,

        /// <summary>
        /// A critical event. This indicates a fatal error or application crash.
        /// </summary>
        Critical = 4,
    }
}
