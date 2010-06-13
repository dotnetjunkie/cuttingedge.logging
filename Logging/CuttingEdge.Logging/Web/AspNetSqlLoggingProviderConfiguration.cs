using System;
using System.ComponentModel;
using System.Web;

namespace CuttingEdge.Logging.Web
{
    /// <summary>
    /// Defines configuration for initializing a new <see cref="AspNetSqlLoggingProvider"/>.
    /// </summary>
    public class AspNetSqlLoggingProviderConfiguration
    {
        private readonly string applicationName;
        private UserIdentityRetrievalType retrievalType;
        private bool logQueryString = true;
        private bool logFormData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AspNetSqlLoggingProviderConfiguration"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application to log with the request. This allows you
        /// to use a single logging store for multiple applications.</param>
        public AspNetSqlLoggingProviderConfiguration(string applicationName)
        {
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }

            if (applicationName.Length == 0)
            {
                throw new ArgumentException(SR.ValueShouldNotBeAnEmptyString(), "applicationName");
            }

            if (applicationName.Length > AspNetSqlLoggingProvider.MaxApplicationNameLength)
            {
                throw new ArgumentException(SR.ValueTooLong(AspNetSqlLoggingProvider.MaxApplicationNameLength), 
                    "applicationName");
            }

            this.applicationName = applicationName;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the the provider will write the
        /// <see cref="HttpRequest.QueryString">query string</see> of the current <see cref="HttpContext"/> to
        /// the database. This value is <b>true</b> by default.
        /// </summary>
        /// <value><c>True</c> when the provider will log the query string; otherwise, <c>false</c>.</value>
        public bool LogQueryString
        {
            get { return this.logQueryString; }
            set { this.logQueryString = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the provider will write the 
        /// <see cref="HttpRequest.Form">form data</see> of the current <see cref="HttpContext"/> to the 
        /// database. This value is <b>false</b> by default.
        /// </summary>
        /// <value><c>True</c> when the provider will log the form data; otherwise, <c>false</c>.</value>
        public bool LogFormData
        {
            get { return this.logFormData; }
            set { this.logFormData = value; }
        }

        /// <summary>Gets or sets the retrieval type.</summary>
        /// <value>The type of the retrieval.</value>
        /// <exception cref="InvalidEnumArgumentException">Thrown when an invalid enum value is set.</exception>
        public UserIdentityRetrievalType RetrievalType
        {
            get
            {
                return this.retrievalType;
            }

            set
            {
                if (value < UserIdentityRetrievalType.None || value > UserIdentityRetrievalType.WindowsIdentity)
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(UserIdentityRetrievalType));
                }

                this.retrievalType = value;
            }
        }

        /// <summary>Gets the name of the application to log with the request. This allows you to use a single
        /// logging store for multiple applications.</summary>
        /// <value>The name of the application.</value>
        public string ApplicationName
        {
            get { return this.applicationName; }
        }
    }
}