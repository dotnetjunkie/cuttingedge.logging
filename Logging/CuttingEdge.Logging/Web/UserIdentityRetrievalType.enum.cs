using System;
using System.Collections.Generic;
using System.Text;

namespace CuttingEdge.Logging.Web
{
    /// <summary>
    /// Defines the retrieval type of the user identity for the <see cref="AspNetSqlLoggingProvider"/>.
    /// </summary>
    public enum UserIdentityRetrievalType
    {
        /// <summary>No user name will be stored during logging.</summary>
        None = 0,

        /// <summary>The user name will be retrieved using the <see cref="Membership"/> model.</summary>
        Membership = 1,

        /// <summary>The user name will be retrieved using the <see cref="WindowsIdentity"/> system.</summary>
        WindowsIdentity = 2,
    }
}
