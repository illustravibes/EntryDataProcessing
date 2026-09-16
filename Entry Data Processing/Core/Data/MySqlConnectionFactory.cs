using System.Data;
using MySqlConnector;
using Entry_Data_Processing.Core.Configuration;

namespace Entry_Data_Processing.Core.Data
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(AppConfig config)
        {
            _connectionString = config.ConnectionStrings.WambDatabase;
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
