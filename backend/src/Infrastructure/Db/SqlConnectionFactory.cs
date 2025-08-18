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
        // Intentar conexión principal y luego fallbacks si existen
        var tried = new List<string>();
        foreach (var connStr in EnumerateConnectionStrings())
        {
            try
            {
                var connection = new SqlConnection(connStr);
                connection.Open();
                return connection;
            }
            catch
            {
                tried.Add(connStr);
                // Intentar siguiente
            }
        }

        throw new InvalidOperationException($"No se pudo abrir conexión SQL. Intentados: {tried.Count}");
    }

    public async Task<SqlConnection> CreateConnectionAsync()
    {
        var tried = new List<string>();
        foreach (var connStr in EnumerateConnectionStrings())
        {
            try
            {
                var connection = new SqlConnection(connStr);
                await connection.OpenAsync();
                return connection;
            }
            catch
            {
                tried.Add(connStr);
            }
        }

        throw new InvalidOperationException($"No se pudo abrir conexión SQL (async). Intentados: {tried.Count}");
    }

    private IEnumerable<string> EnumerateConnectionStrings()
    {
        if (!string.IsNullOrWhiteSpace(_config.ConnectionString))
        {
            yield return _config.ConnectionString;
        }

        if (_config.FallbackConnectionStrings != null)
        {
            foreach (var cs in _config.FallbackConnectionStrings)
            {
                if (!string.IsNullOrWhiteSpace(cs))
                {
                    yield return cs;
                }
            }
        }
    }
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public string[]? FallbackConnectionStrings { get; set; }
}
