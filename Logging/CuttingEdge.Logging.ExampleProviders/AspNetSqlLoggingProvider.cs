#region Copyright (c) 2008 S. van Deursen
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
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;

namespace CuttingEdge.Logging.ExampleProviders
{
    /// <summary>
    /// Manages storage of logging information for ASP.NET web applications in a SQL Server database.
    /// </summary>
    public class AspNetSqlLoggingProvider : SqlLoggingProvider
    {
        private bool logQueryString = true;
        private bool logFormData = true;

        /// <summary>
        /// Gets a value indicating whether the query string should be logged.
        /// </summary>
        /// <value><c>True</c> if the query string should be logged; otherwise, <c>false</c>.</value>
        public bool LogQueryString
        {
            get { return this.logQueryString; }
        }

        /// <summary>
        /// Gets a value indicating whether the form data should be logged.
        /// </summary>
        /// <value><c>True</c> if the form data should be logged; otherwise, <c>false</c>.</value>
        public bool LogFormData
        {
            get { return this.logFormData; }
        }

        /// <summary>Overridden from base.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific 
        /// attributes specified in the configuration for this provider.</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            this.logQueryString = ParseBoolConfigValue(name, "logQueryString", config["logQueryString"], true);
            this.logFormData = ParseBoolConfigValue(name, "logFormData", config["logFormData"], false);

            config.Remove("logQueryString");
            config.Remove("logFormData");
            config.Remove("applicationName");

            base.Initialize(name, config);
        }

        /// <summary>Saves the event to database.</summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="type">The event type.</param>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The database's primary key of the saved event.
        /// </returns>
        protected override int SaveEventToDatabase(SqlTransaction transaction, 
            LoggingEventType type, string message, string source)
        {
            RequestLogData requestLogData = 
                new RequestLogData(HttpContext.Current, this.logQueryString, this.logFormData);

            using (SqlCommand command =
                new SqlCommand("dbo.logging_AddEvent", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                AddParameter(command, "EventTypeId", SqlDbType.Int, (int)type);
                AddParameter(command, "Message", SqlDbType.NText, message);
                AddParameter(command, "Source", SqlDbType.NText, source);

                AddParameter(command, "UserName", SqlDbType.NText, GetCurrentUserName());
                AddParameter(command, "QueryString", SqlDbType.NText, requestLogData.QueryString);
                AddParameter(command, "FormData", SqlDbType.NText, requestLogData.Form);
          
                return (int)command.ExecuteScalar();
            }
        }

        private static string GetCurrentUserName()
        {
             MembershipUser user = Membership.GetUser();

            if (user != null)
            {
                return user.UserName;
            }

            return null;
        }

        private static bool ParseBoolConfigValue(string providerName, string propertyName, string value,
            bool defaultWhenMissing)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultWhenMissing;
            }

            try
            {
                return bool.Parse(value);
            }
            catch (FormatException ex)
            {
                const string ExceptionMessage = "Invalid value '{0}' in '{1}' property in provider {2} in " +
                    "config file. Should be a boolean.";

                throw new ProviderException(String.Format(CultureInfo.InvariantCulture, ExceptionMessage, 
                    value, propertyName, providerName), ex);
            }
        }

        private static SqlParameter AddParameter(SqlCommand command, string parameterName, SqlDbType type,
            object value)
        {
            SqlParameter parameter = command.CreateParameter();

            parameter.IsNullable = true;
            parameter.SqlDbType = type;
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Encapsulates the <see cref="HttpContext"/> to easy access the request data.
        /// </summary>
        private class RequestLogData
        {
            internal readonly string QueryString;

            internal readonly string Form;

            internal RequestLogData(HttpContext context, bool logQueryString, bool logFormData)
            {
                if (context != null && context.Request != null)
                {
                    HttpRequest request = context.Request;

                    if (logQueryString)
                    {
                        this.QueryString = request.QueryString.ToString();
                    }

                    if (logFormData)
                    {
                        this.Form = request.Form.ToString();
                    }
                }
            }
        }
    }
}
