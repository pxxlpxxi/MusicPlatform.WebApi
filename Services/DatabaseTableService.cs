using MusicPlatform.WebApi.Models;
using Npgsql;

namespace MusicPlatform.WebApi.Services
{
    public class DatabaseTableService
    {
        private readonly string _connectionString;

        public DatabaseTableService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MusicPlatform")
                ?? throw new InvalidOperationException("Connection string 'MusicPlatform' was not found.");
        }

        //henter alle brugertabeller i public-schemaet fra PostgreSQL's information_schema
        public List<string> GetTableNames()
        {
            const string sql = """
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
                ORDER BY table_name;
                """;

            var tables = new List<string>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            return tables;
        }

        public TableData? GetTable(string tableName)
        {
            // Tabelnavnet valideres mod listen fra databasen for at undgå SQL injection
            if (!GetTableNames().Contains(tableName))
            {
                return null;
            }

            var table = new TableData { Name = tableName };
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand($"SELECT * FROM \"{tableName}\" LIMIT 500;", connection);
            using var reader = command.ExecuteReader();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                table.Columns.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                var row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? string.Empty);
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
