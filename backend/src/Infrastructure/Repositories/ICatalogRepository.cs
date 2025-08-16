using Application.Dtos;

namespace Infrastructure.Repositories;

public interface ICatalogRepository
{
    Task<IEnumerable<ClientDto>> GetClientsAsync();
    Task<ClientDto?> GetClientByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
}
