using System;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;

namespace CuttingEdge.Logging
{
    /// <summary>
    /// Helper methods for Logging providers that communicate with a SQL Server database.
    /// </summary>
    internal static class SqlLoggingHelper
    {
        internal static SqlParameter AddParameter(SqlCommand command, string parameterName, SqlDbType type,
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

        internal static void ThrowWhenSchemaAlreadyHasBeenInitialized(SqlLoggingProvider provider)
        {
            using (SqlConnection connection = new SqlConnection(provider.ConnectionString))
            {
                connection.Open();

                const string Query = @"
                    SELECT  CONVERT(int, count(*))
                    FROM    sysobjects
                    WHERE   name IN (
                                'logging_EventTypes', 
                                'logging_Events', 
                                'logging_Exceptions', 
                                'logging_AddEvent', 
                                'logging_AddException'
                            );";

                using (SqlCommand command = new SqlCommand(Query, connection))
                {
                    int sysObjectCount = (int)command.ExecuteScalar();

                    if (sysObjectCount > 0)
                    {
                        throw new ProviderException(SR.SqlProviderAlreadyInitialized(provider.Name));
                    }
                }
            }
        }

        internal static void CreateTablesAndStoredProcedures(SqlLoggingProvider provider, 
            string[] createScripts)
        {
            using (var connection = new SqlConnection(provider.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (string script in createScripts)
                    {
                        using (var command = new SqlCommand(script, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        internal static bool ParseBoolConfigValue(string providerName, string attributeName, string value,
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
            catch (FormatException)
            {
                throw new ProviderException(SR.InvalidBooleanAttribute(value, attributeName, providerName));
            }
        }
    }
}
