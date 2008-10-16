using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace CuttingEdge.Logging
{
    /// <summary>Validates arguments.</summary>
    internal static class LoggingHelper
    {
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

        internal static void ValidateTypeInValidRange(LoggingEventType severity)
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

        internal static string GetExceptionMessage(Exception exception)
        {
            string message = exception.Message;

            return String.IsNullOrEmpty(message) ? exception.GetType().Name : message;
        }
    }
}
