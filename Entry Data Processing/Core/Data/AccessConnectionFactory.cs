using System.Data;
using System.Data.OleDb;
using Entry_Data_Processing.Core.Configuration;

namespace Entry_Data_Processing.Core.Data
{
    public sealed class AccessConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public AccessConnectionFactory(AppConfig config)
        {
            _connectionString = config.ConnectionStrings.AccessDatabase;
        }

        public DatabaseProvider Provider => DatabaseProvider.Access;

        public IDbConnection CreateConnection()
        {
            var connection = new OleDbConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
