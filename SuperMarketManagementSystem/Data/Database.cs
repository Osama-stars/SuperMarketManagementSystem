using System.Data.SqlClient;

namespace SupermarketManagementSystem.Data
{
    public static class Database
    {
        private static string connectionString =
            @"Data Source=localhost;Initial Catalog=SupermarketDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}