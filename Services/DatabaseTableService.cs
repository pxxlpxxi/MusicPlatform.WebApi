using MusicPlatform.WebApi.Models;
using Npgsql;
using System.Data;

namespace MusicPlatform.WebApi.Services;

public class DatabaseTableService
{
    private readonly string _connectionString;

    public DatabaseTableService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MusicPlatform")
            ?? throw new InvalidOperationException(
                "Connection string 'MusicPlatform' was not found.");
    }

    public List<string> GetTableNames()
    {
        const string sql = """
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_type = 'BASE TABLE'
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
        if (!GetTableNames().Contains(tableName))
        {
            return null;
        }

        var table = new TableData
        {
            Name = tableName,
            Columns = GetColumns(tableName)
        };

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(
            $"SELECT * FROM \"{tableName}\" LIMIT 500;",
            connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var row = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(
                    reader.IsDBNull(i)
                        ? "NULL"
                        : reader.GetValue(i)?.ToString() ?? string.Empty);
            }

            table.Rows.Add(row);
        }

        return table;
    }
    private List<TableColumn> GetColumns(string tableName)
    {
        const string sql = """
        SELECT
            c.column_name,
            c.data_type,
            c.is_nullable,
            c.is_identity,
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu
                        ON tc.constraint_name = kcu.constraint_name
                        AND tc.table_schema = kcu.table_schema
                        AND tc.table_name = kcu.table_name
                    WHERE tc.constraint_type = 'PRIMARY KEY'
                      AND tc.table_schema = 'public'
                      AND tc.table_name = c.table_name
                      AND kcu.column_name = c.column_name
                )
                THEN true
                ELSE false
            END AS is_primary_key
        FROM information_schema.columns c
        WHERE c.table_schema = 'public'
          AND c.table_name = @tableName
        ORDER BY c.ordinal_position;
        """;

        var columns = new List<TableColumn>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tableName", tableName);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string columnName = reader.GetString(0);
            string dataType = reader.GetString(1);
            bool isNullable = reader.GetString(2) == "YES";

            // is_identity kan være NULL i PostgreSQL
            bool isIdentity =
                !reader.IsDBNull(3) &&
                reader.GetString(3) == "YES";

            bool isPrimaryKey = reader.GetBoolean(4);

            columns.Add(new TableColumn
            {
                Name = columnName,
                DataType = dataType,
                IsPrimaryKey = isPrimaryKey,
                IsIdentity = isIdentity,
                IsNullable = isNullable,

                //primærnøgler og identity-kolonner skal ikke redigeres
                IsEditable = !isPrimaryKey && !isIdentity
            });
        }

        return columns;
    }
    public int InsertRow(
    string tableName,
    Dictionary<string, string?> values)
    {
        var columns = GetColumns(tableName);

        var editableColumns = columns
            .Where(c => c.IsEditable)
            .ToList();

        var suppliedColumns = values.Keys
            .Select(key => new
            {
                Key = key,
                Column = editableColumns.FirstOrDefault(c =>
                    c.Name.Equals(
                        key,
                        StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.Column != null)
            .ToList();

        if (suppliedColumns.Count == 0)
        {
            throw new ArgumentException(
                "No editable columns were supplied.");
        }

        string columnList = string.Join(
            ", ",
            suppliedColumns.Select(x => $"\"{x.Column!.Name}\""));

        string parameterList = string.Join(
            ", ",
            suppliedColumns.Select(
                (_, index) => $"@p{index}"));

        string sql = $"""
        INSERT INTO "{tableName}" ({columnList})
        VALUES ({parameterList})
        RETURNING 1;
        """;

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);

        for (int i = 0; i < suppliedColumns.Count; i++)
        {
            string? value = values[suppliedColumns[i].Key];

            command.Parameters.AddWithValue(
                $"@p{i}",
                value ?? (object)DBNull.Value);
        }

        return Convert.ToInt32(command.ExecuteScalar());
    }


    private object ConvertValue(
    string? value,
    string dataType)
    {
        if (value == null)
        {
            return DBNull.Value;
        }

        return dataType switch
        {
            "integer" => int.Parse(value),
            "bigint" => long.Parse(value),
            "smallint" => short.Parse(value),

            "numeric" => decimal.Parse(value),
            "real" => float.Parse(value),
            "double precision" => double.Parse(value),

            "boolean" => bool.Parse(value),

            "date" => DateOnly.Parse(value),

            "timestamp without time zone" =>
                DateTime.Parse(value),

            "timestamp with time zone" =>
                DateTime.Parse(value),

            "uuid" => Guid.Parse(value),

            _ => value
        };
    }

    public void UpdateRow(
    string tableName,
    string primaryKeyValue,
    Dictionary<string, string?> values)
    {
        var columns = GetColumns(tableName);

        TableColumn? primaryKey = columns
            .FirstOrDefault(c => c.IsPrimaryKey);

        if (primaryKey == null)
        {
            throw new InvalidOperationException(
                "The table does not have a primary key.");
        }

        var editableColumns = columns
            .Where(c => c.IsEditable)
            .Where(c =>
                values.Keys.Any(key =>
                    key.Equals(
                        c.Name,
                        StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (editableColumns.Count == 0)
        {
            throw new ArgumentException(
                "No editable columns were supplied.");
        }

        string setClause = string.Join(
            ", ",
            editableColumns.Select(
                (column, index) =>
                    $"\"{column.Name}\" = @p{index}"));

        string sql = $"""
        UPDATE "{tableName}"
        SET {setClause}
        WHERE "{primaryKey.Name}" = @primaryKey;
        """;

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);

        for (int i = 0; i < editableColumns.Count; i++)
        {
            TableColumn column = editableColumns[i];

            string key = values.Keys.First(k =>
                k.Equals(
                    column.Name,
                    StringComparison.OrdinalIgnoreCase));

            string? value = values[key];

            command.Parameters.AddWithValue(
                $"@p{i}",
                ConvertValue(value, column.DataType));
        }

        command.Parameters.AddWithValue(
            "@primaryKey",
            ConvertValue(
                primaryKeyValue,
                primaryKey.DataType));

        command.ExecuteNonQuery();
    }
    public void DeleteRow(
    string tableName,
    string primaryKeyValue)
    {
        var columns = GetColumns(tableName);

        TableColumn? primaryKey = columns
            .FirstOrDefault(c => c.IsPrimaryKey);

        if (primaryKey == null)
        {
            throw new InvalidOperationException(
                "The table does not have a primary key.");
        }

        string sql = $"""
        DELETE FROM "{tableName}"
        WHERE "{primaryKey.Name}" = @primaryKey;
        """;

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@primaryKey",
            ConvertValue(
                primaryKeyValue,
                primaryKey.DataType));

        command.ExecuteNonQuery();
    }
}