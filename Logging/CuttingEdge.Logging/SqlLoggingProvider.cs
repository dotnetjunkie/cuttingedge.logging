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
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Manages storage of logging information in a SQL Server database.
    /// </summary>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the 
    /// Logging section, which can also be accessed as members of the
    /// <see cref="LoggingSection"/> class. The following configuration file example shows
    /// how to specify values declaratively for the Logging section.
    /// <code>
    /// &lt;?xml version="1.0"?&gt;
    /// &lt;configuration&gt;
    ///     &lt;configSections&gt;
    ///         &lt;section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" /&gt;
    ///     &lt;/configSections&gt;
    ///     &lt;connectionStrings&gt;
    ///         &lt;add name="SqlLogging" 
    ///             connectionString="Data Source=localhost;Integrated Security=SSPI;Initial Catalog=Logging;" /&gt;
    ///     &lt;/connectionStrings&gt;
    ///     &lt;logging defaultProvider="SqlLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="SqlLoggingProvider"
    ///                 connectionStringName="SqlLogging"
    ///                 type="CuttingEdge.Logging.SqlLoggingProvider, CuttingEdge.Logging"
    ///                 description="SQL logging provider"
    ///             /&gt;
    ///         &lt;/providers&gt;
    ///     &lt;/logging&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </example>
    public class SqlLoggingProvider : LoggingProviderBase
    {
        private string connectionString;

        /// <summary>Gets the connection string provided with this provider.</summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return this.connectionString; }
        }

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the name of the provider has a length of zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown wen an attempt is made to call Initialize on a
        /// provider after the provider has already been initialized.</exception>
        /// <exception cref="ProviderException">Thrown when the <paramref name="config"/> contains
        /// unrecognized attributes or when the connectionStringName attribute is not configured properly.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // The contract of the base type is relexed. When the name is null, we give it the name of the
            // current type.
            if (string.IsNullOrEmpty(name))
            {
                name = this.GetType().Name;
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "SQL logging provider");
            }

            this.InitializeConnectionString(name, config);

            // We call base last, because base.Initialize will throw when it has found unused config arguments.
            base.Initialize(name, config);
        }

        /// <summary>Implements the functionality to log the event.</summary>
        /// <param name="type">The <see cref="EventType"/> of the event.</param>
        /// <param name="message">The description of the event.</param>
        /// <param name="source">An optional source where the event occured.</param>
        /// <param name="exception">The exception that has to be logged.</param>
        /// <returns>The id of the logged event or null when an id is inappropriate.</returns>
        protected override object LogInternal(EventType type, string message, string source,
            Exception exception)
        {
            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    // Log the message
                    int eventId = this.SaveEventToDatabase(transaction, type, message, source);

                    this.SaveExceptionChainToDatabase(transaction, exception, eventId);

                    transaction.Commit();

                    return eventId;
                }
            }
        }

        /// <summary>Saves the event to database.</summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="type">The event type.</param>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <returns>The database's primary key of the saved event.</returns>
        protected virtual int SaveEventToDatabase(SqlTransaction transaction, EventType type, 
            string message, string source)
        {
            using (SqlCommand command =
                new SqlCommand("dbo.logging_AddEvent", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                AddParameter(command, "EventTypeId", SqlDbType.Int, (int)type);
                AddParameter(command, "Message", SqlDbType.NText, message);
                AddParameter(command, "Source", SqlDbType.NText, source);

                return (int)command.ExecuteScalar();
            }
        }

        /// <summary>Saves the exception to database.</summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="parentEventId">The parent event id.</param>
        /// <param name="parentExceptionId">The parent exception id.</param>
        /// <returns>The database's primary key of the saved exception.</returns>
        protected virtual int SaveExceptionToDatabase(SqlTransaction transaction, Exception exception,
            int parentEventId, int? parentExceptionId)
        {
            using (SqlCommand command =
                new SqlCommand("dbo.logging_AddException", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                AddParameter(command, "EventId", SqlDbType.Int, parentEventId);
                AddParameter(command, "ParentExceptionId", SqlDbType.Int, parentExceptionId);
                AddParameter(command, "ExceptionType", SqlDbType.NVarChar, exception.GetType().Name);
                AddParameter(command, "Message", SqlDbType.NText, exception.Message);
                AddParameter(command, "StackTrace", SqlDbType.NText, exception.StackTrace);

                return (int)command.ExecuteNonQuery();
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

        private void SaveExceptionChainToDatabase(SqlTransaction transaction, Exception exception,
            int eventId)
        {
            int? parentExceptionId = null;

            while (exception != null)
            {
                parentExceptionId =
                    this.SaveExceptionToDatabase(transaction, exception, eventId, parentExceptionId);

                exception = exception.InnerException;
            }
        }

        private void InitializeConnectionString(string name, NameValueCollection config)
        {
            string connectionStringName = config["connectionStringName"];

            config.Remove("connectionStringName");

            // Throw exception when no connectionStringName is provided
            if (string.IsNullOrEmpty(connectionStringName) == true)
            {
                throw new ProviderException(SR.GetString(SR.MissingConnectionStringAttribute, name));
            }

            string connectionString =
                ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            // Throw exception when connection string is missing from the <connectionStrings> section.
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ProviderException(SR.GetString(SR.MissingConnectionStringInConfig,
                    connectionStringName));
            }

            this.connectionString = connectionString;
        }
    }
}