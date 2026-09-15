using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entry_Data_Processing.Data
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MySqlConnection CreateConnection() => new MySqlConnection(_connectionString);
    }
}
