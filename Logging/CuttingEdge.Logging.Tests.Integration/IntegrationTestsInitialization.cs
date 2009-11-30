using System;
using System.Data.SqlClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Integration
{
    [TestClass]
    public class IntegrationTestsInitialization
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            TestDatabaseConnection(TestConfiguration.ConnectionString);
        }

        private static void TestDatabaseConnection(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Could not connect to database with following connection string: " + connectionString +
                    " " + ex.Message, ex);
            }
        }
    }
}
