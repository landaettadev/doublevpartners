using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Db;
using System.Data;
using Microsoft.Data.SqlClient;

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
                Image = new ProductImageDto
                {
                    FileName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Url = string.Empty, // Se llenará en el servicio
                    ThumbnailUrl = string.Empty, // Se llenará en el servicio
                    AltText = reader.IsDBNull(4) ? "Imagen no disponible" : reader.GetString(1),
                    FileSize = 0,
                    ContentType = "image/jpeg",
                    Width = 0,
                    Height = 0
                },
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
                Image = new ProductImageDto
                {
                    FileName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Url = string.Empty, // Se llenará en el servicio
                    ThumbnailUrl = string.Empty, // Se llenará en el servicio
                    AltText = reader.IsDBNull(4) ? "Imagen no disponible" : reader.GetString(1),
                    FileSize = 0,
                    ContentType = "image/jpeg",
                    Width = 0,
                    Height = 0
                },
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            };
        }

        return null;
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto productDto)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_CreateProduct", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@Name", productDto.Name);
        command.Parameters.AddWithValue("@Description", productDto.Description);
        command.Parameters.AddWithValue("@Price", productDto.Price);
        command.Parameters.AddWithValue("@ImageUrl", productDto.Image?.FileName ?? string.Empty);
        command.Parameters.AddWithValue("@IsActive", productDto.IsActive);

        var productIdParam = command.Parameters.Add("@ProductId", SqlDbType.Int);
        productIdParam.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync();
        var productId = (int)productIdParam.Value;

        return await GetProductByIdAsync(productId) ?? 
            throw new InvalidOperationException("Error al crear el producto.");
    }

    public async Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto productDto)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_UpdateProduct", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ProductId", id);
        command.Parameters.AddWithValue("@Name", (object?)productDto.Name ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)productDto.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", (object?)productDto.Price ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImageUrl", (object?)productDto.Image?.FileName ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", (object?)productDto.IsActive ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();

        return await GetProductByIdAsync(id) ?? 
            throw new InvalidOperationException("Error al actualizar el producto.");
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_DeleteProduct", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ProductId", id);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ToggleProductStatusAsync(int id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_ToggleProductStatus", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@ProductId", id);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_GetActiveProducts", connection)
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
                Image = new ProductImageDto
                {
                    FileName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Url = string.Empty,
                    ThumbnailUrl = string.Empty,
                    AltText = reader.IsDBNull(4) ? "Imagen no disponible" : reader.GetString(1),
                    FileSize = 0,
                    ContentType = "image/jpeg",
                    Width = 0,
                    Height = 0
                },
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            });
        }

        return products;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("sp_SearchProducts", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@SearchTerm", searchTerm);

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
                Image = new ProductImageDto
                {
                    FileName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Url = string.Empty,
                    ThumbnailUrl = string.Empty,
                    AltText = reader.IsDBNull(4) ? "Imagen no disponible" : reader.GetString(1),
                    FileSize = 0,
                    ContentType = "image/jpeg",
                    Width = 0,
                    Height = 0
                },
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7)
            });
        }

        return products;
    }

    public async Task<bool> ProductExistsAsync(int id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var command = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @ProductId", connection);
        command.Parameters.AddWithValue("@ProductId", id);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> ProductNameExistsAsync(string name, int? excludeId = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM Products WHERE Name = @Name";
        if (excludeId.HasValue)
        {
            sql += " AND Id != @ExcludeId";
        }
        
        var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", name);
        if (excludeId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
        }
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }
}
