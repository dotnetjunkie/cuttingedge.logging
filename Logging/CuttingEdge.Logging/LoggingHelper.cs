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
    /// <summary>Helper methods for logging framework.</summary>
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
                    throw new ArgumentException(SR.ArgumentMustNotBeNullOrEmptyString(), "source");
                }
                else
                {
                    throw new ArgumentNullException("source", SR.ArgumentMustNotBeNullOrEmptyString());
                }
            }
        }

        internal static string BuildMethodName(MethodBase method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            int initialCapacity = EstimateInitialCapacity(method, parameters);

            StringBuilder methodName = new StringBuilder(initialCapacity);

            methodName
                 .Append(method.DeclaringType.FullName)
                 .Append(".")
                 .Append(method.Name)
                 .Append("(");

            BuildParameters(methodName, parameters);

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
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            StringBuilder message = new StringBuilder(256);

            message.AppendLine(entry.Message);

            message.Append("Severity: ").AppendLine(entry.Severity.ToString());

            if (entry.Source != null)
            {
                message.Append("Source: ").AppendLine(entry.Source);
            }

            AppendExceptionInformation(entry.Exception, message);

            return message.ToString();
        }

        internal static string FormatEvent(LogEntry entry)
        {
            StringBuilder builder = new StringBuilder(256);

            builder.AppendLine("LoggingEvent:");
            builder.Append("Severity:\t").AppendLine(entry.Severity.ToString());
            builder.Append("Message:\t").AppendLine(entry.Message);

            if (entry.Source != null)
            {
                builder.Append("Source:\t").AppendLine(entry.Source);
            }

            if (entry.Exception != null)
            {
                builder.Append("Exception:\t").AppendLine(entry.Exception.Message);
                builder.AppendLine(entry.Exception.StackTrace);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static int EstimateInitialCapacity(MethodBase method, ParameterInfo[] parameters)
        {
            const int AverageLengthOfParameterName = 15;
            const int ExtraLengthJustToBeSure = 20;

            return method.DeclaringType.FullName.Length + method.Name.Length +
                (AverageLengthOfParameterName * parameters.Length) + ExtraLengthJustToBeSure;
        }

        private static void BuildParameters(StringBuilder methodName, ParameterInfo[] parameters)
        {
            for (int index = 0; index < parameters.Length; index++)
            {
                if (index > 0)
                {
                    methodName.Append(", ");
                }

                ParameterInfo parameter = parameters[index];

                BuildParameter(methodName, parameter);
            }
        }

        private static void BuildParameter(StringBuilder methodName, ParameterInfo parameter)
        {
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

        private static void AppendExceptionInformation(Exception exception, StringBuilder message)
        {
            while (exception != null)
            {
                message.AppendLine();

                message
                    .Append("Exception: ").AppendLine(exception.GetType().FullName)
                    .Append("Message: ").AppendLine(exception.Message)
                    .AppendLine("StackTrace:")
                    .AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }
        }
    }
}
