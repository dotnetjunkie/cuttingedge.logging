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
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace CuttingEdge.Logging.Web
{
    /// <summary>
    /// Manages storage of logging information for ASP.NET web applications in a SQL Server database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The table below shows the list of valid attributes for the <see cref="AspNetSqlLoggingProvider"/>:
    /// <list type="table">  
    /// <listheader>
    ///     <attribute>Attribute</attribute>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <attribute>fallbackProvider</attribute>
    ///     <description>
    ///         A fallback provider that the Logger class will use when logging failed on this logging 
    ///         provider. The value must contain the name of an existing logging provider. This attribute is
    ///         optional.
    ///     </description>
    /// </item>  
    /// <item>
    ///     <attribute>threshold</attribute>
    ///     <description>
    ///         The logging threshold. The threshold limits the number of event logged. The threshold can be
    ///         defined as follows: Debug &lt; Information &lt; Warning &lt; Error &lt; Fatal. i.e., When the 
    ///         threshold is set to Information, Debug events will not be logged. When no value is specified
    ///         all events are logged. This attribute is optional.
    ///      </description>
    /// </item>  
    /// <item>
    ///     <attribute>connectionStringName</attribute>
    ///     <description>
    ///         The the connection string provided with this provider. This attribute is mandatory.
    ///     </description>
    /// </item>  
    /// <item>
    ///     <attribute>initializeSchema</attribute>
    ///     <description>
    ///         When this boolean attribute is set to true, the provider will try to create the needed tables 
    ///         and stored spocedures in the database. This attribute is optional and false by default.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>applicationName</attribute>
    ///     <description>
    ///         Specifies the name of the application to log with the request. This allows you to use a single
    ///         logging store for multiple applications. This attribute is mandatory.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>userNameRetrievalType</attribute>
    ///     <description>
    ///         The userNameRetrievalType attribute allows you to configure the source from which the provider
    ///         tries to retrieve the user name of the current request. The options are 
    ///         <see cref="UserIdentityRetrievalType.None">None</see>, 
    ///         <see cref="UserIdentityRetrievalType.Membership">Membership</see> and
    ///         <see cref="UserIdentityRetrievalType.WindowsIdentity">WindowsIdentity</see>;
    ///         Use <b>Membership</b> when a <see cref="MembershipProvider"/> is configured using the
    ///         <see cref="Membership"/> model. Use <b>WindowsIdentity</b> when you use windows authentication;
    ///         Use <b>None</b> when you don't want any user information to be logged. This attribute is
    ///         mandatory.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>logQueryString</attribute>
    ///     <description>
    ///         When this boolean attribute is set to true, the provider will write the 
    ///         <see cref="HttpRequest.QueryString">query string</see> of the current <see cref="HttpContext"/>
    ///         to the database. This attribute is optional and <b>true</b> by default.
    ///     </description>
    /// </item>
    /// <item>
    ///     <attribute>logFormData</attribute>
    ///     <description>
    ///         When this boolean attribute is set to true, the provider will write the 
    ///         <see cref="HttpRequest.Form">form data</see> of the current <see cref="HttpContext"/>
    ///         to the database. This attribute is optional and <b>false</b> by default.
    ///     </description>
    /// </item>
    /// </list>
    /// The attributes can be specified within the provider configuration. See the example below on how to
    /// use.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example demonstrates how to specify values declaratively for several attributes of the
    /// Logging section, which can also be accessed as members of the <see cref="LoggingSection"/> class.
    /// The following configuration file example shows how to specify values declaratively for the
    /// Logging section.
    /// <code lang="xml">
    /// &lt;?xml version="1.0"?&gt;
    /// &lt;configuration&gt;
    ///     &lt;configSections&gt;
    ///         &lt;section name="logging" type="CuttingEdge.Logging.LoggingSection, CuttingEdge.Logging"
    ///             allowDefinition="MachineToApplication" /&gt;
    ///     &lt;/configSections&gt;
    ///     &lt;connectionStrings&gt;
    ///         &lt;add name="SqlLogging" 
    ///             connectionString="Data Source=.;Integrated Security=SSPI;Initial Catalog=Logging;" /&gt;
    ///     &lt;/connectionStrings&gt;
    ///     &lt;logging defaultProvider="AspNetSqlLoggingProvider"&gt;
    ///         &lt;providers&gt;
    ///             &lt;add 
    ///                 name="AspNetSqlLoggingProvider"
    ///                 type="CuttingEdge.Logging.Web.AspNetSqlLoggingProvider, CuttingEdge.Logging"
    ///                 description="ASP.NET SQL logging provider example"
    ///                 connectionStringName="SqlLogging"
    ///                 threshold="Information"
    ///                 initializeSchema="True"
    ///                 applicationName="MyWebApplication"
    ///                 userNameRetrievalType="Membership"
    ///                 logQueryString="True"
    ///                 logFormData="False"
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
    public class AspNetSqlLoggingProvider : SqlLoggingProvider
    {
        private bool logQueryString = true;
        private bool logFormData = true;
        private string applicationName;
        private UserIdentityRetrievalType retrievalType;

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

        /// <summary>Gets the name of the application.</summary>
        /// <value>The name of the application.</value>
        public string ApplicationName
        {
            get { return this.applicationName; }
        }

        /// <summary>Gets the retrieval type of the user identity to log.</summary>
        /// <value>The type of the retrieval.</value>
        public UserIdentityRetrievalType RetrievalType
        {
            get { return this.retrievalType; }
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

            // Retrieve and remove values from config. We do this before calling base.Initialize(), 
            // because the base class checks for unrecognizedd attributes.
            bool logQueryString = GetLogQueryStringFromConfig(name, config);
            bool logFormData = GetLogFormDataFromConfig(name, config);
            UserIdentityRetrievalType retrievalType = GetRetrievalTypeFromConfig(name, config);
            string applicationName = GetApplicationNameFromConfig(name, config);

            // Then call initialize (base.Initialize checks for unrecognized attributes)
            base.Initialize(name, config);

            // Set fields after base.Initialize  (this prevents the provider from being altered afterwards)
            this.logQueryString = logQueryString;
            this.logFormData = logFormData;
            this.retrievalType = retrievalType;
            this.applicationName = applicationName;
        }

        /// <summary>Initializes the database schema.</summary>
        protected override void InitializeDatabaseSchema()
        {
            try
            {
                SqlLoggingHelper.ThrowWhenSchemaAlreadyHasBeenInitialized(this);

                string createScript = SR.GetString(SR.AspNetSqlLoggingProviderSchemaScripts);

                string[] createScripts = createScript.Split(new string[] { "GO" }, StringSplitOptions.None);

                SqlLoggingHelper.CreateTablesAndStoredProcedures(this, createScripts);
            }
            catch (SqlException sex)
            {
                throw new ProviderException(SR.GetString(SR.InitializationOfDatabaseSchemaFailed, this.Name,
                    sex.Message), sex);
            }
        }

        /// <summary>Saves the event to database.</summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="severity">The event severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <returns>The database's primary key of the saved event.</returns>
        protected override int SaveEventToDatabase(SqlTransaction transaction, LoggingEventType severity,
            string message, string source)
        {
            RequestLogData requestLogData =
                new RequestLogData(HttpContext.Current, this.logQueryString, this.logFormData);

            using (SqlCommand command =
                new SqlCommand("dbo.logging_AddEvent", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlLoggingHelper.AddParameter(command, "EventTypeId", SqlDbType.Int, (int)severity);
                SqlLoggingHelper.AddParameter(command, "Message", SqlDbType.NText, message);
                SqlLoggingHelper.AddParameter(command, "Source", SqlDbType.NText, source);

                SqlLoggingHelper.AddParameter(command, "MachineName", SqlDbType.NVarChar, Environment.MachineName);
                SqlLoggingHelper.AddParameter(command, "ApplicationName", SqlDbType.NVarChar, this.ApplicationName);
                SqlLoggingHelper.AddParameter(command, "UserName", SqlDbType.NVarChar, this.GetCurrentUserName());
                SqlLoggingHelper.AddParameter(command, "IpAddress", SqlDbType.NVarChar, requestLogData.IpAddress);
                SqlLoggingHelper.AddParameter(command, "QueryString", SqlDbType.NText, requestLogData.QueryString);
                SqlLoggingHelper.AddParameter(command, "FormData", SqlDbType.NText, requestLogData.Form);

                return (int)command.ExecuteScalar();
            }
        }

        /// <summary>Saves the exception to database.</summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="parentEventId">The parent event id.</param>
        /// <param name="parentExceptionId">The parent exception id.</param>
        /// <returns>
        /// The database's primary key of the saved exception.
        /// </returns>
        protected override int SaveExceptionToDatabase(SqlTransaction transaction, Exception exception,
            int parentEventId, int? parentExceptionId)
        {
            using (SqlCommand command =
                new SqlCommand("dbo.logging_AddException", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlLoggingHelper.AddParameter(command, "EventId", SqlDbType.Int, parentEventId);
                SqlLoggingHelper.AddParameter(command, "ParentExceptionId", SqlDbType.Int, parentExceptionId);
                SqlLoggingHelper.AddParameter(command, "ExceptionType", SqlDbType.NVarChar, exception.GetType().Name);
                SqlLoggingHelper.AddParameter(command, "Message", SqlDbType.NText, exception.Message);
                SqlLoggingHelper.AddParameter(command, "StackTrace", SqlDbType.NText, exception.StackTrace);

                return (int)command.ExecuteScalar();
            }
        }

        private static bool GetLogQueryStringFromConfig(string name, NameValueCollection config)
        {
            const bool DefaultValueWhenMissing = true;

            bool logQueryString = 
                SqlLoggingHelper.ParseBoolConfigValue(name, "logQueryString", config["logQueryString"],
                DefaultValueWhenMissing);

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove("logQueryString");
            
            return logQueryString;
        }

        private static bool GetLogFormDataFromConfig(string name, NameValueCollection config)
        {
            const bool DefaultValueWhenMissing = false;

            bool logFormData =
                SqlLoggingHelper.ParseBoolConfigValue(name, "logFormData", config["logFormData"],
                DefaultValueWhenMissing);

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove("logFormData");
            
            return logFormData;
        }

        private static UserIdentityRetrievalType GetRetrievalTypeFromConfig(string name,
            NameValueCollection config)
        {
            string userNameRetrievalType = config["userNameRetrievalType"];

            // Throw exception when no userNameRetrievalType is provided
            if (string.IsNullOrEmpty(userNameRetrievalType))
            {
                string exceptionMessage =
                    "Empty or missing userNameRetrievalType attribute in provider '{0}' in config file. " +
                    "Please supply one of the following values: {1}.";

                throw new ProviderException(String.Format(CultureInfo.InvariantCulture, exceptionMessage,
                    name, GetUserIdentityRetrievalTypeAsString()));
            }

            UserIdentityRetrievalType retrievalType;
            try
            {
                retrievalType = (UserIdentityRetrievalType)
                    Enum.Parse(typeof(UserIdentityRetrievalType), userNameRetrievalType, true);
            }
            catch (ArgumentException)
            {
                string exceptionMessage = "Invalid userNameRetrievalType attribute in provider '{0}' in " +
                    "the config file. Please supply one of the following values: {1}.";

                throw new ProviderException(String.Format(CultureInfo.InvariantCulture, exceptionMessage,
                    name, GetUserIdentityRetrievalTypeAsString()));
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove("userNameRetrievalType");

            return retrievalType;
        }

        private static string GetUserIdentityRetrievalTypeAsString()
        {
            string values = String.Empty;

            Array types = Enum.GetValues(typeof(UserIdentityRetrievalType));

            int lastIndex = types.Length - 1;
            for (int index = 0; index < types.Length; index++)
            {
                if (index > 0)
                {
                    values += index == lastIndex ? " or " : ", ";
                }

                values += types.GetValue(index).ToString();
            }

            return values;
        }

        private static string GetApplicationNameFromConfig(string name, NameValueCollection config)
        {
            string applicationName = config["applicationName"];

            // Throw exception when no applicationName is provided
            if (string.IsNullOrEmpty(applicationName))
            {
                string exceptionMessage =
                    "Empty or missing applicationName attribute in provider '{0}' in config file.";

                throw new ProviderException(
                    String.Format(CultureInfo.InvariantCulture, exceptionMessage, name));
            }

            if (applicationName.Length > 255)
            {
                string exceptionMessage =
                    "The supplied applicationName attribute in provider '{0}' in config file is longer " +
                    "than 255 characters.";

                throw new ProviderException(
                    String.Format(CultureInfo.InvariantCulture, exceptionMessage, name));
            }

            // Remove this attribute from the config. This way the provider can spot unrecognized attributes
            // after the initialization process.
            config.Remove("applicationName");

            return applicationName;
        }

        private string GetCurrentUserName()
        {
            switch (this.retrievalType)
            {
                case UserIdentityRetrievalType.Membership:
                    MembershipUser user = Membership.GetUser();
                    return user == null ? null : user.UserName;

                case UserIdentityRetrievalType.WindowsIdentity:
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    return identity == null ? null : identity.Name;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Encapsulates the <see cref="HttpContext"/> to easy access the request data.
        /// </summary>
        private class RequestLogData
        {
            internal readonly string QueryString;

            internal readonly string Form;

            internal readonly string IpAddress;

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

                    this.IpAddress = request.ServerVariables["REMOTE_ADDR"];
                }
            }
        }
    }
}