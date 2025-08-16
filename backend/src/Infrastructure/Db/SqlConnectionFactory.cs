using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Infrastructure.Db;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseConfig _config;

    public SqlConnectionFactory(IOptions<DatabaseConfig> config)
    {
        _config = config.Value;
    }

    public SqlConnection CreateConnection()
    {
        var connection = new SqlConnection(_config.ConnectionString);
        connection.Open();
        return connection;
    }

    public async Task<SqlConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_config.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
}
