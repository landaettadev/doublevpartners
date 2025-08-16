using Application.Dtos;
using Infrastructure.Db;
using Microsoft.Data.SqlClient;
using Application.Interfaces;

namespace Infrastructure.Repositories;

public class CatalogRepo : ICatalogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogRepo(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ClientDto>> GetClientsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_GetClients", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        var clients = new List<ClientDto>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            clients.Add(new ClientDto
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Phone = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Address = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                UpdatedAt = reader.GetDateTime(6)
            });
        }

        return clients;
    }

    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_GetClientById", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        
        command.Parameters.AddWithValue("@ClientId", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ClientDto
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Phone = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Address = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                UpdatedAt = reader.GetDateTime(6)
            };
        }

        return null;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_GetProducts", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        var products = new List<ProductDto>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            products.Add(new ProductDto
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            });
        }

        return products;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_GetProductById", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        
        command.Parameters.AddWithValue("@ProductId", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ProductDto
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            };
        }

        return null;
    }
}
