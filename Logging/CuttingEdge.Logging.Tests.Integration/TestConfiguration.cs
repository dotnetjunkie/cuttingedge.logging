using System;
using System.Configuration;

namespace CuttingEdge.Logging.Tests.Integration
{
    public static class TestConfiguration
    {
        public static string DatabaseName
        {
            get
            {
                string databaseName = ConfigurationManager.AppSettings["DatabaseName"];

                if (string.IsNullOrEmpty(databaseName))
                {
                    throw new InvalidOperationException(
                        "'DatabaseName' name missing from <appSettings> in app.config");
                }

                return databaseName;
            }
        }

        public static string ConnectionString
        {
            get
            {
                string connectionStringTemplate = GetConnectionStringTemplate();

                return string.Format(connectionStringTemplate, DatabaseName);
            }
        }

        private static string GetConnectionStringTemplate()
        {
            string connectionStringTemplate =
                ConfigurationManager.ConnectionStrings["ConnectionStringTemplate"].ConnectionString;

            if (string.IsNullOrEmpty(connectionStringTemplate))
            {
                throw new InvalidOperationException(
                    "'ConnectionStringTemplate' name missing from <connectionStrings> in app.config");
            }

            return connectionStringTemplate;
        }
    }
}
