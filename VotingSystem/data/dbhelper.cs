using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace VotingSystem.Data
{
    public class DBHelper
    {
        private readonly string _connectionString;

        public DBHelper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            _connectionString = config.GetConnectionString("MySqlConnection");
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
