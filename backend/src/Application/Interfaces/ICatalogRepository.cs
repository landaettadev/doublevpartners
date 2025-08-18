using Application.Dtos;

namespace Application.Interfaces;

public interface ICatalogRepository
{
    Task<IEnumerable<ClientDto>> GetClientsAsync();
    Task<ClientDto?> GetClientByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(ProductCreateDto productDto);
    Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto productDto);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> ToggleProductStatusAsync(int id);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<bool> ProductExistsAsync(int id);
    Task<bool> ProductNameExistsAsync(string name, int? excludeId = null);
}
