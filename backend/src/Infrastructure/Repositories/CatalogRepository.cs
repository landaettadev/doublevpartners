using Application.Dtos;
using Infrastructure.Db;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogRepository(IDbConnectionFactory connectionFactory)
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
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString("Email"),
                Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString("Phone"),
                Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
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
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString("Email"),
                Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString("Phone"),
                Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
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
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull("Description") ? string.Empty : reader.GetString("Description"),
                Price = reader.GetDecimal("Price"),
                ImageUrl = reader.IsDBNull("ImageUrl") ? string.Empty : reader.GetString("ImageUrl"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
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
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull("Description") ? string.Empty : reader.GetString("Description"),
                Price = reader.GetDecimal("Price"),
                ImageUrl = reader.IsDBNull("ImageUrl") ? string.Empty : reader.GetString("ImageUrl"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt")
            };
        }

        return null;
    }
}
