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
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace CuttingEdge.Logging
{
    /// <summary>Validates arguments.</summary>
    internal static class LoggingHelper
    {
        internal static void ValideLoggerIsNotNull(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
        }

        internal static void ValidateExceptionIsNotNull(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
        }

        internal static void ValidateMessageNotNullOrEmpty(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                if (message != null)
                {
                    throw new ArgumentException(SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString),
                        "message");
                }
                else
                {
                    throw new ArgumentNullException("message",
                        SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString));
                }
            }
        }

        internal static void ValidateSeverityInValidRange(LoggingEventType severity)
        {
            if (severity < LoggingEventType.Debug || severity > LoggingEventType.Critical)
            {
                throw new InvalidEnumArgumentException("severity", (int)severity, typeof(LoggingEventType));
            }
        }

        internal static void ValidateSourceIsNotNull(MethodBase source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
        }

        internal static void ValidateSourceNotNullOrEmpty(string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (source != null)
                {
                    throw new ArgumentException(SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString),
                        "source");
                }
                else
                {
                    throw new ArgumentNullException("source",
                        SR.GetString(SR.ArgumentMustNotBeNullOrEmptyString));
                }
            }
        }

        internal static string BuildMethodName(MethodBase method)
        {
            if (method == null)
            {
                return null;
            }

            ParameterInfo[] parameters = method.GetParameters();

            int initialCapacity =
                method.DeclaringType.FullName.Length +
                method.Name.Length + (15 * parameters.Length) +
                20;

            StringBuilder methodName = new StringBuilder(initialCapacity);

            methodName
                 .Append(method.DeclaringType.FullName)
                 .Append(".")
                 .Append(method.Name)
                 .Append("(");

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];

                if (index > 0)
                {
                    methodName.Append(", ");
                }

                if (parameter.IsOut == true)
                {
                    methodName.Append("out ");
                }

                if (parameter.IsRetval == true)
                {
                    methodName.Append("ret ");
                    methodName.Insert(0, " ");
                    methodName.Insert(0, parameter.ParameterType.Name);
                }
                else
                {
                    methodName.Append(parameter.ParameterType.Name);
                }
            }

            methodName.Append(")");

            return methodName.ToString();
        }

        internal static string GetExceptionMessageOrExceptionType(Exception exception)
        {
            string message = exception.Message;

            return String.IsNullOrEmpty(message) ? exception.GetType().Name : message;
        }

        internal static string BuildMessageFromLogEntry(LogEntry entry)
        {
            StringBuilder message = new StringBuilder(256);

            message.AppendLine(entry.Message);

            message.Append("Severity: ").AppendLine(entry.Severity.ToString());

            if (entry.Source != null)
            {
                message.Append("Source: ").AppendLine(entry.Source);
            }

            Exception exception = entry.Exception;

            while (exception != null)
            {
                message.AppendLine();

                message
                    .Append("Exception: ").AppendLine(exception.GetType().FullName)
                    .Append("Message: ").AppendLine(exception.Message)
                    .AppendLine("Stacktrace:")
                    .AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return message.ToString();
        }
    }
}
