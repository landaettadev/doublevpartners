using Microsoft.Data.SqlClient;

namespace Infrastructure.Db;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
    Task<SqlConnection> CreateConnectionAsync();
}
